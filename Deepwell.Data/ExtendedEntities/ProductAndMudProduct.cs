using Deepwell.Common.Enum;

namespace Deepwell.Data.ExtendedEntities
{
    public class ProductAndMudProduct
    {
        public int ProductId { get; set; }
        public string ProductNumber { get; set; }
        public string ProductName { get; set; }
        public bool IsActive { get; set; }
        public TypeOfProduct ProductType { get; set; }
        public bool IsAssociatedWithAnyOrder { get; set; }
        public decimal Price { get; set; }
    }
}
