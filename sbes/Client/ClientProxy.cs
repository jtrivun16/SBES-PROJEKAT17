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
            try
            {
                return factory.RunService(ip, port, protocol);
            }
            catch (Exception e)
            {
                Console.WriteLine("Error: {0}", e.Message);
                return false;
            }
        }

        public bool StopService(byte[] ip, byte[] port, byte[] protocol)
        {
            try
            {
                return factory.StopService(ip, port, protocol);
            }
            catch (Exception e)
            {
                Console.WriteLine("Error: {0}", e.Message);
                return false;
            }
        }

        public bool AddPortToBlackList(string value, string group)
        {
            try
            {
                return factory.AddPortToBlackList(value, group);
            }
            catch (Exception e)
            {
                Console.WriteLine("Error: {0}", e.Message);
                return false;
            }
        }

        public bool AddProtocolToBlackList(string value, string group)
        {
            try
            {
                return factory.AddProtocolToBlackList(value, group);
            }
            catch (Exception e)
            {
                Console.WriteLine("Error: {0}", e.Message);
                return false;
            }
        }

        public bool RemoveProtocolFromBlackList(string value, string group)
        {
            try
            {
                return factory.RemoveProtocolFromBlackList(value, group);
            }
            catch (Exception e)
            {
                Console.WriteLine("Error: {0}", e.Message);
                return false;
            }
        }

        public bool RemovePortFromBlackList(string value, string group)
        {
            try
            {
                return factory.RemovePortFromBlackList(value, group);
            }
            catch (Exception e)
            {
                Console.WriteLine("Error: {0}", e.Message);
                return false;
            }
        }

        public void TestConnection()
        {
            try
            {
                factory.TestConnection();
            }
            catch (Exception e)
            {
                Console.WriteLine("Error: {0}", e.Message);
            }
        }
    }
}
