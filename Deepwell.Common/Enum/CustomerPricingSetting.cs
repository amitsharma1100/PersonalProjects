using System.ComponentModel;

namespace Deepwell.Common.Enum
{
    public enum CustomerPricingSetting
    {
        [Description("List Price")]
        ListPrice,

        [Description("Customer Price")]
        CustomerPrice,

        [Description("No Price")]
        NoPrice
    }
}
