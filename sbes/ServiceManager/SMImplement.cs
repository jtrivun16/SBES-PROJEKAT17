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

namespace ServiceManager
{
    public class SMImplement : IServiceManager
    {
        Dictionary<string, byte[]> UsersSessionKeys = new Dictionary<string, byte[]>();
        //static BlacklistManager BLM = BlacklistManager.Instance();

        public bool Connect(byte[] encryptedSessionKey)
        {
            CustomPrincipal principal = Thread.CurrentPrincipal as CustomPrincipal;
            string userName = Formatter.ParseName(principal.Identity.Name);

            //if (Thread.CurrentPrincipal.IsInRole("ExchangeSessionKey"))
            //{
            //    try
            //    {
            //        AuditClient.Instance().LogAuthorizationSuccess(userName, OperationContext.Current.IncomingMessageHeaders.Action);
            //    }
            //    catch (Exception e)
            //    {
            //        Console.WriteLine(e.Message);
            //    }
            //}
            //else
            //{
            //    try
            //    {
            //        AuditClient.Instance().LogAuthorizationFailed(userName, OperationContext.Current.IncomingMessageHeaders.Action, "Connect need ExchangeSessionKey permission");
            //    }
            //    catch (Exception e)
            //    {
            //        Console.WriteLine(e.Message);
            //    }
            //}

            //string serviceCert = Formatter.ParseName(WindowsIdentity.GetCurrent().Name);
            ////Console.WriteLine(serviceCert);
            ////string serviceCert = "Manager";

            //X509Certificate2 certificate = CertManager.GetCertificateFromStorage(StoreName.My, StoreLocation.LocalMachine, serviceCert);
            //byte[] sessionKey = SessionKeyHelper.DecryptSessionKey(certificate, encryptedSessionKey);

            ////SessionKeyHelper.PrintSessionKey(sessionKey);

            //UsersSessionKeys[userName] = sessionKey;

            //return true;
            throw  new NotImplementedException();
        }
    }
}
