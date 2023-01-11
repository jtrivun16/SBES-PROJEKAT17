using Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Text;
using System.Threading.Tasks;

namespace Client
{
    public class ClientProxy : ChannelFactory<IServiceManager>, IServiceManager, IDisposable
    {
        IServiceManager factory;

        public ClientProxy(Binding binding, string remoteAddress) : base(binding, remoteAddress)
        {
            factory = this.CreateChannel();
        }

        public bool Connect(byte[] encryptedSessionKey)
        {
            bool connected = false;
            try
            {
                connected = factory.Connect(encryptedSessionKey);

            }
            catch (FaultException<SecurityException> sec)
            {
                Console.WriteLine(sec.Message);

            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            return connected;
        }

    }
}
