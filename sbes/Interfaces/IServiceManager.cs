using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace Interfaces
{
    [ServiceContract]
    public interface IServiceManager
    {
        [OperationContract]
        //[FaultContract(typeof(SecurityException))]
        byte[] Connect(byte[] publicKey, byte[] iv);

        [OperationContract]
        bool RunService(byte[] ip, byte[] port, byte[] protocol);

        [OperationContract]
        bool StopService(byte[] ip, byte[] port, byte[] protocol);

        [OperationContract]
        bool AddPortToBlackList(string value);


        [OperationContract]
        bool AddProtocolToBlackList(string value);

        [OperationContract]
        bool RemoveProtocolFromBlackList(string value);

        [OperationContract]
        bool RemovePortFromBlackList(string value);

        [OperationContract]
        void TestConnection();


    }
}
