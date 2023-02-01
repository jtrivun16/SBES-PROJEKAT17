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
                    byte[] serverPublicKey = null;

                    serverPublicKey = proxy.Connect(clientDiffieHellman.PublicKey, clientDiffieHellman.IV);
                    connected = true;

            
                    while (true)
                    {
                        
                            switch (Izbor())
                            {
                                case 1:
                                    if (!connected)
                                    {
                                        Console.WriteLine("Morate se konektovati!");
                                        break;
                                    }
                                    Console.Write("Unesite IP adresu      : ");
                                    string ip = Console.ReadLine().Trim();
                                    Console.Write("Unesite port    : ");
                                    string port = Console.ReadLine().Trim();
                                    Console.Write("Unesite protokol: ");
                                    string protocol = Console.ReadLine().Trim();

                                    bool validRun = proxy.RunService(clientDiffieHellman.Encrypt(serverPublicKey, ip),
                                                        clientDiffieHellman.Encrypt(serverPublicKey, port),
                                                        clientDiffieHellman.Encrypt(serverPublicKey, protocol));
                                    break;
                                case 2:
                                    if (!connected)
                                    {
                                        Console.WriteLine("Please connect first!");
                                        break;
                                    }
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
                                    break;

                            case 3:
                                if (!connected)
                                {
                                    Console.WriteLine("Please connect first!");
                                    break;
                                }
                                Console.Write("Port to ban:");
                                string portBan = Console.ReadLine().Trim();
                                proxy.AddPortToBlackList(portBan);
                                break;
                            case 4:
                                if (!connected)
                                {
                                    Console.WriteLine("Please connect first!");
                                    break;
                                }
                                Console.Write("Protocol to ban:");
                                string protocolBan = Console.ReadLine().Trim();
                                proxy.AddProtocolToBlackList(protocolBan);
                                break;
                            //TODO 6,7
                            default:
                                    break;
                            }
                        

                    }
                    
                }

                Console.ReadLine(); 
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

    
    }
}


