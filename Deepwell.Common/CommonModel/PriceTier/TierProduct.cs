using Deepwell.Common.Enum;

namespace Deepwell.Common.CommonModel.PriceTier
{
    public class TierProduct
    {
        public int ProductId { get; set; }
        public string ProductNumber { get; set; }
        public decimal Price { get; set; }
        public PriceTierProductStatus Status { get; set; }
    }
}
