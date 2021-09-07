using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Deepwell.Front.Models
{
    public class InformationViewModel
    {
        public int OrderCount { get; set; }

        public decimal OrderTotal { get; set; }

        public decimal OrderRevenue { get; set; }
    }
}