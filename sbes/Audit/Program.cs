using Certificates;
using Common;
using Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Security.Principal;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.ServiceModel.Security;
using System.Text;
using System.Threading.Tasks;

namespace Audit
{
    class Program
    {
        static void Main(string[] args)
        {
			string srvCertCN = Formatter.ParseName(WindowsIdentity.GetCurrent().Name);
			//string srvCertCN = "auditcert";
			NetTcpBinding binding = new NetTcpBinding();
			binding.Security.Transport.ClientCredentialType = TcpClientCredentialType.Certificate;

			string address = "net.tcp://localhost:8200/Audit";
			ServiceHost host = new ServiceHost(typeof(AuditFunctions));
			host.AddServiceEndpoint(typeof(IAuditFunctions), binding, address);

			host.Description.Behaviors.Remove(typeof(ServiceDebugBehavior));
			host.Description.Behaviors.Add(new ServiceDebugBehavior() { IncludeExceptionDetailInFaults = true });

			///Custom validation mode enables creation of a custom validator - CustomCertificateValidator
			host.Credentials.ClientCertificate.Authentication.CertificateValidationMode = X509CertificateValidationMode.Custom;
			host.Credentials.ClientCertificate.Authentication.CustomCertificateValidator = new ServerCertValidator();

			///If CA doesn't have a CRL associated, WCF blocks every client because it cannot be validated
			host.Credentials.ClientCertificate.Authentication.RevocationMode = X509RevocationMode.NoCheck;

			///Set appropriate service's certificate on the host. Use CertManager class to obtain the certificate based on the "srvCertCN"
			host.Credentials.ServiceCertificate.Certificate = CertManager.GetCertificateFromStorage(StoreName.My, StoreLocation.LocalMachine, srvCertCN);

			ServiceSecurityAuditBehavior newAudit = new ServiceSecurityAuditBehavior();
			newAudit.AuditLogLocation = AuditLogLocation.Application;
			newAudit.ServiceAuthorizationAuditLevel = AuditLevel.SuccessOrFailure;

			host.Description.Behaviors.Remove<ServiceSecurityAuditBehavior>();
			host.Description.Behaviors.Add(newAudit);


			try
			{
				host.Open();
				Console.WriteLine("Audit process run by user: " + WindowsIdentity.GetCurrent().Name);
				Console.WriteLine("Press <enter> to stop ...");
				Console.ReadLine();
			}
			catch (Exception e)
			{
				Console.WriteLine("[ERROR] {0}", e.Message);
				Console.WriteLine("[StackTrace] {0}", e.StackTrace);
			}
			finally
			{
				host.Close();
			}
		}
	}
}
