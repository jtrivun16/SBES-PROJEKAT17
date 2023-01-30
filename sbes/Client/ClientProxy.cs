﻿using Interfaces;
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

        public ClientProxy(Binding binding, EndpointAddress remoteAddress) : base(binding, remoteAddress)
        {
            factory = this.CreateChannel();
        }

        public void AddItemToBlackList(string type, string value)
        {
            throw new NotImplementedException();
        }

        public byte[] Connect(byte[] publicKey, byte[] iv)
        {
            byte[] serverPublicKey = null;
            try
            {
                serverPublicKey = factory.Connect(publicKey, iv);
                Console.WriteLine("Connect allowed!\n");
            }
            catch (Exception e)
            {
                Console.WriteLine("Error: {0}", e.Message);
            }

            return serverPublicKey;
        }

        public bool RunService(byte[] ip, byte[] port, byte[] protocol)
        {
            throw new NotImplementedException();
        }

        public bool StopService(byte[] ip, byte[] port, byte[] protocol)
        {
            throw new NotImplementedException();
        }
    }
}
