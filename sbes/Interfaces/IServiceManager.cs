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
        void AddItemToBlackList(string type, string value);


    }
}
