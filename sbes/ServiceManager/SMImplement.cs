﻿using Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Security.Principal;
using System.ServiceModel;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SecurityManager;
using Common;
using System.Security.Permissions;
using Audit;

namespace ServiceManager
{
    public class SMImplement : IServiceManager
    {
        public byte[] ClientPublicKey { get; set; }
        public byte[] ClientIV { get; set; }
        public ExcangeKey excangeKey = new ExcangeKey();
        public Dictionary<string, ServiceHost> hosts = new Dictionary<string, ServiceHost>();





        [PrincipalPermission(SecurityAction.Demand, Role = "Modify")]
        public bool AddPortToBlackList(string value, string group)
        {
           
            bool blackListUpddate = false;

            foreach (var item in BlackListManager.blackListPort)
            {
                if (item.Group == group && item.Value == value)
                {
                    Console.WriteLine("This item is alredy on black list");
                    return false;
                }
            }

            BlackListItem blackListItem = new BlackListItem(group, value);
            BlackListManager.blackListPort.Add(blackListItem);
            blackListUpddate = BlackListManager.UpdateBlackList();

            if (blackListUpddate)
            {
                Console.WriteLine("Port :" + value + " is banned");
            }


            return blackListUpddate;

        }

        [PrincipalPermission(SecurityAction.Demand, Role = "Modify")]
        public bool AddProtocolToBlackList(string value, string group)
        {
            bool blackListUpddate = false;
            foreach (var item in BlackListManager.blackListProtocol)
            {
                if (item.Group == group && item.Value == value) {
                    Console.WriteLine("This item is alredy on black list");
                    return false;
                }
            }

            BlackListItem blackListItem = new BlackListItem(group, value);
            BlackListManager.blackListProtocol.Add(blackListItem);
            blackListUpddate = BlackListManager.UpdateBlackList();

            if (blackListUpddate)
            {
                Console.WriteLine("Protocol :" + value + " is banned");
            }
            
            
            return blackListUpddate;
        }



        [PrincipalPermission(SecurityAction.Demand, Role = "Modify")]
        public bool RemoveProtocolFromBlackList(string value,string group)
        {
            bool blackListUpddate = false;

           foreach(var item in BlackListManager.blackListProtocol)
           {
                if (item.Group == group && item.Value == value)
                {
                    BlackListManager.blackListProtocol.Remove(item);

                    blackListUpddate = BlackListManager.UpdateBlackList();

                    if (blackListUpddate)
                    {
                        Console.WriteLine("Protocol :" + value + " is not banned anymore");
                    }
                    break;
                }
            }
            return blackListUpddate;
        }



        [PrincipalPermission(SecurityAction.Demand, Role = "Modify")]
        public bool RemovePortFromBlackList(string value, string group)
        {
            bool blackListUpddate = false;
            foreach (var item in BlackListManager.blackListPort)
            {
                if (item.Group == group && item.Value == value)
                {
                    BlackListManager.blackListPort.Remove(item);
                    blackListUpddate = BlackListManager.UpdateBlackList();

                    if (blackListUpddate)
                    {
                        Console.WriteLine("Port :" + value + " is not banned anymore");
                    }
                    break;
                }
            }
            return blackListUpddate;
        }


        [PrincipalPermission(SecurityAction.Demand, Role = "ExchangeSessionKey")]
        public byte[] Connect(byte[] publicKey, byte[] iv)
        {
            ClientPublicKey = publicKey;
            ClientIV = iv;

            Console.WriteLine("[ CLIENT CONNECTED ]\n");
            WindowsIdentity windowsIdentity = Thread.CurrentPrincipal.Identity as WindowsIdentity;
            string username = Formatter.ParseName(windowsIdentity.Name);
            Program.auditProxy.LogEvent((int)AuditEventTypes.ConnectSuccess, username);

            return excangeKey.PublicKey;
        }



        [PrincipalPermission(SecurityAction.Demand, Role = "RunService")]
        public bool RunService(byte[] ip, byte[] port, byte[] protocol)
        {
            Console.WriteLine("RUN");
            WindowsIdentity windowsIdentity = Thread.CurrentPrincipal.Identity as WindowsIdentity;
            string username = Formatter.ParseName(windowsIdentity.Name);

            string decryptedIp = excangeKey.Decrypt(ClientPublicKey, ip, ClientIV);
            string decryptedPort = excangeKey.Decrypt(ClientPublicKey, port, ClientIV);
            string decryptedProtocol = excangeKey.Decrypt(ClientPublicKey, protocol, ClientIV);
            string protocol2 = decryptedProtocol.ToLower();
            if (decryptedProtocol.ToLower().Equals("tcp"))
                decryptedProtocol = "net.tcp";

            if (decryptedIp.ToLower().Equals("localhost"))
                decryptedIp = "127.0.0.1";

            NetTcpBinding binding = new NetTcpBinding();
            string address = $"net.tcp://{decryptedIp}:{decryptedPort}/SMImplement";

            //get group of client
          

            if (hosts.ContainsKey(address) || BlackListManager.ItemIsOnBlacklist(decryptedPort, protocol2))
            {
                Program.auditProxy.LogEvent((int)AuditEventTypes.RunServiceFailure, username);
                Console.WriteLine("Service faild to run ...");
                return false;
            }
          

            binding.Security.Mode = SecurityMode.Transport;
            binding.Security.Transport.ClientCredentialType = TcpClientCredentialType.Windows;
            binding.Security.Transport.ProtectionLevel = System.Net.Security.ProtectionLevel.EncryptAndSign;

            ServiceHost host = new ServiceHost(typeof(SMImplement));

            host.AddServiceEndpoint(typeof(IServiceManager), binding, address);

            host.Open();
            Program.auditProxy.LogEvent((int)AuditEventTypes.RunServiceSuccess, username);
            hosts.Add(address, host);

            Console.WriteLine("Service run on port " + decryptedPort);
            return true;
        }



        [PrincipalPermission(SecurityAction.Demand, Role = "RunService")]
        public bool StopService(byte[] ip, byte[] port, byte[] protocol)
        {
            WindowsIdentity windowsIdentity = Thread.CurrentPrincipal.Identity as WindowsIdentity;
            string username = Formatter.ParseName(windowsIdentity.Name);

            string decryptedIp = excangeKey.Decrypt(ClientPublicKey, ip, ClientIV);
            string decryptedPort = excangeKey.Decrypt(ClientPublicKey, port, ClientIV);
            string decryptedProtocol = excangeKey.Decrypt(ClientPublicKey, protocol, ClientIV);

            if (decryptedProtocol.ToLower().Equals("tcp"))
                decryptedProtocol = "net.tcp";
            else
                return false;

            if (decryptedIp.ToLower().Equals("localhost"))
                decryptedIp = "127.0.0.1";

            string address = $"{decryptedProtocol}://{decryptedIp}:{decryptedPort}/SMImplement";

            
            if (hosts.ContainsKey(address))
            {
                hosts[address].Close();
                hosts.Remove(address);
                Console.WriteLine("Service stopped on port " + decryptedPort);
                Program.auditProxy.LogEvent((int)AuditEventTypes.StopServiceSuccess, username);
                return true;
            }

            Program.auditProxy.LogEvent((int)AuditEventTypes.StopServiceFailure, username);
            return false;
        }



        public void TestConnection()
        {
            Console.WriteLine("[ CONNECTION WORKING ] This is a test message.\n");
        }

    }
}
