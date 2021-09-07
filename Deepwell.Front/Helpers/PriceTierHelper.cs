using Deepwell.Common.Enum;
using Deepwell.Common.Extensions;
using Deepwell.Front.Models.PriceTier;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.SessionState;

namespace Deepwell.Front.Helpers
{
    public partial class PriceTierHelper
    {
        private const string PRICE_TIER_PRODUCTS_KEY = "PriceTierProducts";
        private static HttpSessionState sessionState => HttpContext.Current.Session;

        public static List<PriceTierProduct> PriceTierItems
        {
            get
            {
                var priceTierItems = sessionState[PRICE_TIER_PRODUCTS_KEY];
                if (priceTierItems.IsNull())
                {
                    priceTierItems = new List<PriceTierProduct>();
                }

                return (priceTierItems as List<PriceTierProduct>).OrderBy(i => i.ProductId).ToList();
            }
            set
            {
                sessionState[PRICE_TIER_PRODUCTS_KEY] = value;
            }
        }

        public static void AddItem(PriceTierProduct item)
        {
            if (PriceTierItems.Contains(item))
            {
                UpdatePrice(item.ProductId, item.Price);
            }
            else
            {
                PriceTierItems.Add(item);
            }
        }

        public static void AddItem(List<PriceTierProduct> items)
        {
            var existingItems = PriceTierItems;
            var itemsToAdd = new List<PriceTierProduct>();

            items.ForEach(i =>
            {
                var matchedItem = existingItems.FirstOrDefault(e => e.ProductId == i.ProductId);
                if (matchedItem.IsNull())
                {
                    itemsToAdd.Add(i);
                }
            });

            existingItems.AddRange(itemsToAdd);
            PriceTierItems = existingItems;
        }

        public static void RemoveItem(int productId)
        {
            var sessionList = PriceTierItems;
            var itemToRemove = sessionList.FirstOrDefault(ti => ti.ProductId == productId);
            if (itemToRemove.IsNotNull())
            {
                switch (itemToRemove.ProductStatus)
                {
                    case PriceTierProductStatus.New:
                        {
                            sessionList.Remove(itemToRemove);
                            break;
                        }

                    case PriceTierProductStatus.Existing:
                        {
                            sessionList.Remove(itemToRemove);
                            itemToRemove.ProductStatus = PriceTierProductStatus.Removed;
                            sessionList.Add(itemToRemove);
                            break;
                        }
                }

                PriceTierItems = sessionList;
            }
        }

        public static bool UpdatePrice(int productId, decimal price)
        {
            bool response = false;
            var itemToUpdate = PriceTierItems.FirstOrDefault(ti => ti.ProductId == productId);
            if (itemToUpdate.IsNotNull())
            {
                itemToUpdate.Price = price;
                if (itemToUpdate.ProductStatus == PriceTierProductStatus.Existing)
                {
                    itemToUpdate.ProductStatus = PriceTierProductStatus.Updated;
                }

                response = true;
            }

            return response;
        }

        public static void EmptySessionItems()
        {
            PriceTierSessionItems = new Dictionary<int, decimal>();
            PriceTierItems = new List<PriceTierProduct>();
        }
    }
}