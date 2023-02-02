using Common;
using Interfaces;
using SecurityManager;
using ServiceManager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.ServiceModel;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Client
{
    class Program
    {
        public static byte[] serverPublicKey = null;
        static void Main(string[] args)
        {
            NetTcpBinding binding = new NetTcpBinding();
            string address = "net.tcp://localhost:8888/WCFService";


            binding.Security.Mode = SecurityMode.Transport;
            binding.Security.Transport.ClientCredentialType = TcpClientCredentialType.Windows;
            binding.Security.Transport.ProtectionLevel = System.Net.Security.ProtectionLevel.EncryptAndSign;

            Console.WriteLine("Korisnik koji je pokrenuo klijenta je : " + WindowsIdentity.GetCurrent().Name);

            EndpointAddress endpointAddress = new EndpointAddress(new Uri(address), EndpointIdentity.CreateUpnIdentity("wcfServer"));

            bool connected = false;

            string opcija = "";
         
            Console.WriteLine("============ MENU ============");
            Console.WriteLine("[ 1 ] Connect");
            Console.Write("Unesite da ako zelite da se konektujete: ");
            opcija = Console.ReadLine();
            opcija = opcija.ToLower();

            if (opcija == "da") 
            {
                using (ClientProxy proxy = new ClientProxy(binding, endpointAddress))
                {

                    //connect
                    ExcangeKey clientDiffieHellman = new ExcangeKey();
                   

                    if(Connect(clientDiffieHellman, proxy))
                    {
                        connected = true;
                        while (true)
                        {
                            switch (Izbor())
                            {
                                case 1:
                                    StartNewService(proxy, clientDiffieHellman);
                                    break;
                                case 2:
                                    StopService(proxy, clientDiffieHellman);
                                    break;

                                case 3:
                                    BanPort(proxy);
                                    break;
                                case 4:
                                    BanProtocol(proxy);
                                    break;
                                case 5:
                                    RemovePortFromBL(proxy);
                                    break;
                                case 6:
                                    RemoveProtocolFromBL(proxy);
                                    break;
                                //TODO 7
                                default:
                                    break;
                            }
                        }
                        Console.ReadLine();
                    }     
                }
            }
            else
            {
                Console.WriteLine("Gasenje klijenta");
            }
        }

        public static int Izbor()
        {
            int option = -1;
            bool valid = false;

            do
            {
                Console.WriteLine("============ MENU ============");
                Console.WriteLine("[ 1 ] Run service");
                Console.WriteLine("[ 2 ] Stop service");
                Console.WriteLine("[ 3 ] Add port to blacklit");
                Console.WriteLine("[ 4 ] Add protocol to blacklit");
                Console.WriteLine("[ 5 ] Remove port from blacklit");
                Console.WriteLine("[ 6 ] Remove protocol from blacklit");
                Console.WriteLine("[ 7 ] DoS Attack - Test");
                Console.WriteLine("==============================");

                Console.Write("Choose option: ");
                valid = Int32.TryParse(Console.ReadLine(), out option);
            } while (option < 1 || option > 7 && !valid);

            return option;
        }

        public static bool Connect(ExcangeKey clientDiffieHellman,ClientProxy proxy)
        {
            serverPublicKey = proxy.Connect(clientDiffieHellman.PublicKey, clientDiffieHellman.IV);

            if (serverPublicKey != null)
                return true;

            return false;
        }


        public static void StartNewService(ClientProxy proxy, ExcangeKey clientDiffieHellman)
        {
            Console.Write("Unesite IP adresu      : ");
            string ip = Console.ReadLine().Trim();
            Console.Write("Unesite port    : ");
            string port = Console.ReadLine().Trim();
            Console.Write("Unesite protokol: ");
            string protocol = Console.ReadLine().Trim();

            bool validRun = proxy.RunService(clientDiffieHellman.Encrypt(serverPublicKey, ip),
                                clientDiffieHellman.Encrypt(serverPublicKey, port),
                                clientDiffieHellman.Encrypt(serverPublicKey, protocol));

            if (validRun)
            {
                Console.WriteLine("[ CLIENT ] Service runned successfully!\n");
            }
            else
            {
                Console.WriteLine("[ CLIENT ] Service run falied!\n");
            }


        }


        public static void StopService(ClientProxy proxy, ExcangeKey clientDiffieHellman)
        {
            Console.Write("Enter IP      : ");
            string stopIp = Console.ReadLine().Trim();
            Console.Write("Enter PORT    : ");
            string stopPort = Console.ReadLine().Trim();
            Console.Write("Enter PROTOCOL: ");
            string stopProtocol = Console.ReadLine().Trim();

            bool validStop = proxy.StopService(clientDiffieHellman.Encrypt(serverPublicKey, stopIp),
                                clientDiffieHellman.Encrypt(serverPublicKey, stopPort),
                                clientDiffieHellman.Encrypt(serverPublicKey, stopProtocol));
            if (validStop)
            {
                Console.WriteLine("[ CLIENT ] Service stopped successfully!\n");
            }
            else
            {
                Console.WriteLine("[ CLIENT ] Service falied to stop!\n");
            }
        }


        public static void BanPort(ClientProxy proxy)
        {
            Console.Write("Port to ban:");
            string portBan = Console.ReadLine().Trim();

            if (proxy.AddPortToBlackList(portBan))
            {
                Console.WriteLine($"Port: {portBan} is banned successfyly.\n");
            }
            else
            {
                Console.WriteLine($"Port ban failed.\n");
            }
        }


        public static void BanProtocol(ClientProxy proxy)
        {
            Console.Write("Protocol to ban:");
            string protocolBan = Console.ReadLine().Trim();
            if (proxy.AddProtocolToBlackList(protocolBan))
            {
                Console.WriteLine($"Protocol: {protocolBan} is banned successfyly.\n");
            }
            else
            {
                Console.WriteLine($"Protocol ban failed.\n");
            }
        }


        public static void RemovePortFromBL(ClientProxy proxy)
        {
            Console.Write("Port to remove from blacklist:");
            string portToRemove = Console.ReadLine().Trim();
            if (proxy.RemovePortFromBlackList(portToRemove))
            {
                Console.WriteLine($"Port: {portToRemove} is removed from blacklist successfyly.\n");
            }
            else
            {
                Console.WriteLine($"Port is not removed.\n");
            }
        }


        public static void RemoveProtocolFromBL(ClientProxy proxy)
        {
            Console.Write("Protocol to remove from blacklist:");
            string protocolToRemove = Console.ReadLine().Trim();
            if (proxy.RemoveProtocolFromBlackList(protocolToRemove))
            {
                Console.WriteLine($"Protocol: {protocolToRemove} is removed from blacklist successfyly.\n");
            }
            else
            {
                Console.WriteLine($"Protocol is not removed.\n");
            }
        }




    }
}


