using System.Collections.Generic;

namespace Deepwell.Common.CommonModel.PriceTier
{
    public class PriceTierModel
    {
        public int TierId { get; set; }
        public string Title { get; set; }
        public bool IsActive { get; set; }
        public IEnumerable<TierProduct> TierProducts { get; set; }
    }
}
