using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Deepwell.Common.Models
{
    public class OrderProcessRequest
    {
        public int OrderId { get; set; }

        public string Source { get; set; }

        public string PoNumber { get; set; }

        public string ShippingVia { get; set; }

        public decimal TaxPercent { get; set; }

        public string TrackingId { get; set; }

        public string Comments { get; set; }

        public List<OrderProcessItem> Items { get; set; }
    }
}
