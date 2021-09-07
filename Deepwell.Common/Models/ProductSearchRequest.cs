using Deepwell.Common.Enum;

namespace Deepwell.Common.Models
{
    public class ProductSearchRequest
    {
        public string ProductNumber { get; set; }
        public string ProductName { get; set; }
        public TaxableOptions Taxable { get; set; }
        public TypeOfProduct ProductType { get; set; }
        public IsActiveOptions Active { get; set; }
        public int Page { get; set; } = 1;
        public int LocationId { get; set; }
    }
}