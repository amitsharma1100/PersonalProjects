using Deepwell.Common.CommonModel.PriceTier;
using System.Collections.Generic;

namespace Deepwell.Data.Interfaces
{
    interface IPriceTier
    {
        Tier AddTier(Tier t);
        IEnumerable<Tier> GetTiers();
        Tier GetById(int id);
        IEnumerable<Tier> GetAllActive();
        bool Edit(PriceTierModel t);
        bool Add(PriceTierModel t);
        void AddProducts(int tierId, IEnumerable<TierProduct> products);
        TierDetail GetTierProductById(int id, int tierId);
    }
}
