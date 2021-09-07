using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Deepwell.Common.Enum;

namespace Deepwell.Common.CommonModel.Order
{
    public class UpdateOrderItemRequest
    {
        public int OrderId { get; set; }

        public int OrderDetailId { get; set; }

        public int LocationId { get; set; }

        public int ProductId { get; set; }

        public int OrderProcessId { get; set; }

        public OrderItemAction ItemAction { get; set; }

        public int Quantity { get; set; }

        public int QuantityToUpdate { get; set; }       
        
        public OrderStatus CurrentStatus { get; set; }

        public decimal TaxAmount { get; set; }

        public decimal Price { get; set; }
    }
}
