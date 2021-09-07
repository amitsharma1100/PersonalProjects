using Deepwell.Data.Interfaces;
using System.Collections.Generic;
using System.Linq;

namespace Deepwell.Data.Repository
{
    public class LogRepository : ILogRepository
    {
        private DeepwellContext _deepwellContext;

        public LogRepository()
        {
            _deepwellContext = new DeepwellContext();
        }

        public LogRepository(DeepwellContext deepwellContext)
        {
            _deepwellContext = deepwellContext;
        }

        public IEnumerable<Log4NetLog> GetLogs()
        {
            return _deepwellContext
                .Log4NetLog
                .OrderByDescending(l => l.Date);
        }
    }
}
