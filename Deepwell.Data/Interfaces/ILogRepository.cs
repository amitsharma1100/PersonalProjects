using System.Collections.Generic;

namespace Deepwell.Data.Interfaces
{
    interface ILogRepository
    {
        IEnumerable<Log4NetLog> GetLogs();
    }
}
