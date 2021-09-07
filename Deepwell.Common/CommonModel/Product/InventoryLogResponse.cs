using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Deepwell.Common.CommonModel.Product
{
    public class InventoryLogResponse
    {
        public string ChangeType { get; set; }
        public DateTime DateCreated { get; set; }
        public int LocationId { get; set; }
        public string Location { get; set; }
        public int QuantityAffected { get; set; }
        public string Remarks { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; }
    }
}
