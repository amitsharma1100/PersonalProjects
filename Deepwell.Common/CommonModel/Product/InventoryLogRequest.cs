using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Deepwell.Common.Enum;

namespace Deepwell.Common.CommonModel.Product
{
    public class InventoryLogRequest
    {
        public int ProductId { get; set; }

        public InventoryChangeType ChangeType { get; set; }

        public InventoryLocation Location { get; set; }

        public DateTime FromDate { get; set; }

        public DateTime ToDate { get; set; }
    }
}
