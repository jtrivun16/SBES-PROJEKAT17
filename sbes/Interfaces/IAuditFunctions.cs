using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace Interfaces
{
    [ServiceContract]

    public interface IAuditFunctions
    {
        [OperationContract]
        void LogEvent(int code, string username);

        [OperationContract]
        void DoSTrackerDetection();

    }
}
