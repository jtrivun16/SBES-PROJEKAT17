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

        public bool RunService(byte[] ip, byte[] port, byte[] protocol)
        {
            Console.WriteLine("RUN");
            return true;
        }

        public bool StopService(byte[] ip, byte[] port, byte[] protocol)
        {
            Console.WriteLine("STOp");
            return true;
        }
    }
}
