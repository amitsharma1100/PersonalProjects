using Deepwell.Common.Extensions;
using System.Collections.Generic;

namespace Deepwell.Front.Helpers
{
    public partial class PriceTierHelper
    {
        private const string PRICE_TIER_SESSION_KEY = "PriceTierSessionProducts";
        public static Dictionary<int, decimal> PriceTierSessionItems
        {
            get
            {
                var priceTierSessionItems = sessionState[PRICE_TIER_SESSION_KEY];
                if (priceTierSessionItems.IsNull())
                {
                    priceTierSessionItems = new Dictionary<int, decimal>();
                }

                return (priceTierSessionItems as Dictionary<int, decimal>);
            }
            set
            {
                sessionState[PRICE_TIER_SESSION_KEY] = value;
            }
        }

        public static void SetList(Dictionary<int, decimal> products)
        {
            PriceTierSessionItems = products;
        }

        public static void EmptyList()
        {
            PriceTierSessionItems = new Dictionary<int, decimal>();
        }
    }
}