using Deepwell.Common.Enum;
using System;

namespace Deepwell.Common.CommonModel
{
    public class CustomerInformation
    {
        public int UserId { get; set; }
        public string CustomerNumber { get; set; }
        public string IdentityId { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string CustomerType { get; set; }
        public string PhoneNumber { get; set; }
        public string BillingType { get; set; }
        public int BillingAddressId { get; set; }
        public bool IsActive { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime DateModified { get; set; }
        public string WellName { get; set; }
        public string County { get; set; }
        public int StateId { get; set; }
        public string PostalCode { get; set; }
        public string City { get; set; }
        public bool IsTaxable { get; set; }
        public CustomerPricingSetting PricingSetting { get; set; }
    }
}
