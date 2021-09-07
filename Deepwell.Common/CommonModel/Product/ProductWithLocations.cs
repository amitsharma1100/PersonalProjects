using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Deepwell.CommonModels
{
    public class ProductWithLocations
    {
        public int InventoryId { get; set; }
        public int ProductId { get; set; }
        public string ProductNumber { get; set; }
        public string ProductName { get; set; }
        public decimal Price { get; set; }
        public bool IsTaxable { get; set; }
        public int LocationId { get; set; }
        public int Quantity { get; set; }
        public bool IsMudProduct { get; set; }
        public string UnitOfMeasure { get; set; }
    }
}
