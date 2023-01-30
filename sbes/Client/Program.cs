using Common;
using Interfaces;
using ServiceManager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.ServiceModel;
using System.Text;
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


                    int option = -1;
                        bool valid = false;

                    do
                    {
                        Console.WriteLine("============ MENU ============");          
                        Console.WriteLine("[ 1 ] Run service");
                        Console.WriteLine("[ 2 ] Stop service");
                        Console.WriteLine("[ 3 ] DoS Attack - Test");
                        Console.WriteLine("==============================");

                        Console.Write("Choose option: ");
                        valid = Int32.TryParse(Console.ReadLine(), out option);

                    } while (option < 1 || option > 6 && !valid);


                    while (true)
                    {
                        switch (option)
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
                                option = -1;
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
                                option = -1;
                                break;
                            default:
                                option = -1;
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
    }
}
