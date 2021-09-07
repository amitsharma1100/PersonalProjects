using Deepwell.Common.Enum;

namespace Deepwell.Common.CommonModel
{
    public class CustomerSearchRequest
    {
        public string CustomerNumber { get; set; }
        public string Name { get; set; }
        public CustomerStatus Status { get; set; }
        public CustomerType Type { get; set; }
        public string Taxable { get; set; }
    }
}
