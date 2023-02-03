using Certificates;
using Interfaces;
using SecurityManager;
using System;
using System.Collections.Generic;
using System.IdentityModel.Policy;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
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
        public static IAuditFunctions auditProxy = null;

        public static bool exitService = false;
        static void Main(string[] args)
        {
            BlackListManager blackListManager = new BlackListManager();
            blackListManager.IsBlackListCorrupted();

            auditProxy = ConnectAudit();

            NetTcpBinding binding = new NetTcpBinding();
            string address = "net.tcp://localhost:8888/WCFService";

            binding.Security.Mode = SecurityMode.Transport;
            binding.Security.Transport.ClientCredentialType = TcpClientCredentialType.Windows;
            binding.Security.Transport.ProtectionLevel = System.Net.Security.ProtectionLevel.EncryptAndSign;

            ServiceHost host = new ServiceHost(typeof(SMImplement));

            host.Description.Behaviors.Remove(typeof(ServiceDebugBehavior));
            host.Description.Behaviors.Add(new ServiceDebugBehavior() { IncludeExceptionDetailInFaults = true });

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
                    //Environment.Exit(0);
                    break;
                }

                Thread.Sleep(1000);
                
            }
            Console.ReadLine();

            Console.ReadLine();


        }


        static AuditProxy ConnectAudit()
        {
            /// Define the expected service certificate. It is required to establish cmmunication using certificates.
            string srvCertCN = "auditcert";

            NetTcpBinding binding = new NetTcpBinding();
            binding.Security.Transport.ClientCredentialType = TcpClientCredentialType.Certificate;

            /// Use CertManager class to obtain the certificate based on the "srvCertCN" representing the expected service identity.
            X509Certificate2 srvCert = CertManager.GetCertificateFromStorage(StoreName.TrustedPeople, StoreLocation.LocalMachine, srvCertCN);
            EndpointAddress address = new EndpointAddress(new Uri("net.tcp://localhost:8200/Audit"),
                                      new X509CertificateEndpointIdentity(srvCert));

            return new AuditProxy(binding, address);
        }
    }
}
