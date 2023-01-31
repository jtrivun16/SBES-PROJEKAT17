using Interfaces;
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

namespace ServiceManager
{
    public class SMImplement : IServiceManager
    {
        public byte[] ClientPublicKey { get; set; }
        public byte[] ClientIV { get; set; }
        public ExcangeKey excangeKey = new ExcangeKey();
        public Dictionary<string, ServiceHost> hosts = new Dictionary<string, ServiceHost>();

        public void AddItemToBlackList(string type, string value)
        {
            throw new NotImplementedException();
        }

        //static BlacklistManager BLM = BlacklistManager.Instance();


        [PrincipalPermission(SecurityAction.Demand, Role = "ExchangeSessionKey")]
        public byte[] Connect(byte[] publicKey, byte[] iv)
        {
            ClientPublicKey = publicKey;
            ClientIV = iv;

            Console.WriteLine("[ CLIENT CONNECTED ]\n");
            WindowsIdentity windowsIdentity = Thread.CurrentPrincipal.Identity as WindowsIdentity;
            string username = Formatter.ParseName(windowsIdentity.Name);
            //Program.auditProxy.LogEvent((int)AuditEventTypes.ConnectSuccess, username);

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

            if (decryptedProtocol.ToLower().Equals("tcp"))
                decryptedProtocol = "net.tcp";
            else
                return false;

            if (decryptedIp.ToLower().Equals("localhost"))
                decryptedIp = "127.0.0.1";

            NetTcpBinding binding = new NetTcpBinding();
            string address = $"{decryptedProtocol}://{decryptedIp}:{decryptedPort}/SMImplement";

            if (hosts.ContainsKey(address))
            {
                Console.WriteLine("Service faild to run ...");
                return false;
            }

            binding.Security.Mode = SecurityMode.Transport;
            binding.Security.Transport.ClientCredentialType = TcpClientCredentialType.Windows;
            binding.Security.Transport.ProtectionLevel = System.Net.Security.ProtectionLevel.EncryptAndSign;

            ServiceHost host = new ServiceHost(typeof(SMImplement));

            host.AddServiceEndpoint(typeof(IServiceManager), binding, address);

            host.Open();        
            hosts.Add(address, host);

            Console.WriteLine("Service run on port " + decryptedPort);
            return true;
        }

        [PrincipalPermission(SecurityAction.Demand, Role = "RunService")]
        public bool StopService(byte[] ip, byte[] port, byte[] protocol)
        {
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
                return true;
            }

            return false;
        }

        public void TestConnection()
        {
            Console.WriteLine("[ CONNECTION WORKING ] This is a test message.\n");
        }

    }
}
