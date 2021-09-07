using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Deepwell.Common.Models
{
    public class ProcOrderStatsGet
    {
        public int OrderCount { get; set; }

        public decimal OrderTotal { get; set; }

        public decimal OrderRevenue { get; set; }
    }
}
