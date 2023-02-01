using Interfaces;
using SecurityManager;
using System;
using System.Collections.Generic;
using System.IdentityModel.Policy;
using System.Linq;
using System.Security.Principal;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ServiceManager
{
    class Program
    {
        public static bool exitService = false;
        static void Main(string[] args)
        {
            NetTcpBinding binding = new NetTcpBinding();
            string address = "net.tcp://localhost:8888/WCFService";

            binding.Security.Mode = SecurityMode.Transport;
            binding.Security.Transport.ClientCredentialType = TcpClientCredentialType.Windows;
            binding.Security.Transport.ProtectionLevel = System.Net.Security.ProtectionLevel.EncryptAndSign;

            ServiceHost host = new ServiceHost(typeof(SMImplement));
            host.AddServiceEndpoint(typeof(IServiceManager), binding, address);


            // podesavamo da se koristi MyAuthorizationManager umesto ugradjenog
            host.Authorization.ServiceAuthorizationManager = new CustomAuthorizationManager();

            // podesavamo custom polisu, odnosno nas objekat principala
            host.Authorization.PrincipalPermissionMode = PrincipalPermissionMode.Custom;
            List<IAuthorizationPolicy> policies = new List<IAuthorizationPolicy>();
            policies.Add(new CustomAuthorizationPolicy());
            host.Authorization.ExternalAuthorizationPolicies = policies.AsReadOnly();

            host.Open();
            Console.WriteLine(WindowsIdentity.GetCurrent().Name);
            Console.WriteLine("Server is successfully opened");


            while (true)
            {
                // if file corrupted, shutdown server
                if (exitService)
                {
                    host.Close();
                    Console.WriteLine("Service shutdown...");
                    break;
                }

                Thread.Sleep(1000);
            }
            Console.ReadLine();

            Console.ReadLine();


        }
    }
}
