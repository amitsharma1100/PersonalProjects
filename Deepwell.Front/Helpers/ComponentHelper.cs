using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.SessionState;
using Deepwell.Common.Extensions;
using Deepwell.Front.Models.Product;

namespace Deepwell.Front.Helpers
{
    public static partial class ComponentHelper
    {
        private const string COMPONENT_ITEMS_KEY = "Components";
        private static HttpSessionState sessionState => HttpContext.Current.Session;

        public static List<ComponentItemViewModel> Items
        {
            get
            {
                var orderItems = sessionState[COMPONENT_ITEMS_KEY];
                if (orderItems.IsNull())
                {
                    orderItems = new List<ComponentItemViewModel>();
                }

                return (orderItems as List<ComponentItemViewModel>).OrderBy(i => i.ProductName).ToList();
            }
            set
            {
                sessionState[COMPONENT_ITEMS_KEY] = value;
            }
        }

        public static void AddItem(ComponentItemViewModel item)
        {
            Items.Add(item);
        }

        public static void AddItem(List<ComponentItemViewModel> items)
        {
            Items = items;
        }

        public static void AddToExistingItems(List<ComponentItemViewModel> items)
        {
            var compList = Items;
            foreach (var item in items)
            {
                if (compList.Any(i => i.ProductId == item.ProductId) == false)
                {
                    compList.Add(item);
                }
            }

            Items = compList;
        }

        public static void RemoveItem(int productId)
        {
            var sessionList = Items;
            var itemToRemove = sessionList.FirstOrDefault(oi => oi.ProductId == productId);
            if (itemToRemove.IsNotNull())
            {
                sessionList.Remove(itemToRemove);
                Items = sessionList;
            }
        }

        public static void UpdateQuantity(int productId, int quantity)
        {
            var itemToUpdate = Items.FirstOrDefault(oi => oi.ProductId == productId);
            if (itemToUpdate.IsNotNull())
            {
                itemToUpdate.Quantity = quantity;
            }
        }

        public static void EmptySessionItems()
        {
            Items = new List<ComponentItemViewModel>();
        }
    }
}