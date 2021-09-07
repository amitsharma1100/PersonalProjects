using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.SessionState;
using Deepwell.Common.Enum;
using Deepwell.Common.Extensions;
using Deepwell.CommonModels;
using Deepwell.Data;
using Deepwell.Front.Models.Order;

namespace Deepwell.Front.Helpers
{
    public static partial class OrderItemsHelper
    {
        private const string ORDER_ITEMS_KEY = "OrderItems";
        private static HttpSessionState sessionState => HttpContext.Current.Session;

        public static List<OrderDetailItemViewModel> OrderDetailItems
        {
            get
            {
                var orderItems = sessionState[ORDER_ITEMS_KEY];
                if (orderItems.IsNull())
                {
                    orderItems = new List<OrderDetailItemViewModel>();
                }

                return (orderItems as List<OrderDetailItemViewModel>)
                    .OrderBy(i => i.OrderProcessId)
                    .ThenBy(i => i.LineNumber).ToList();
            }
            set
            {
                sessionState[ORDER_ITEMS_KEY] = value;
            }
        }

        public static void AddItem(OrderDetailItemViewModel item)
        {
            OrderDetailItems.Add(item);
        }

        public static void AddItem(List<OrderDetailItemViewModel> items)
        {
            OrderDetailItems = items;
        }

        public static void AddItem(List<ProductWithLocations> items)
        {
            var existingItems = OrderDetailItems;
            var itemsToAdd = new List<OrderDetailItemViewModel>();
            int lineNumber = existingItems.Count > 0 ? existingItems.Max(i => i.LineNumber) + 1 : 1;
            items.ForEach(i =>
            {
                var matchedItem = existingItems.FirstOrDefault(si => si.LocationId == i.LocationId && si.ProductId == i.ProductId && si.Status == OrderStatus.Active);
                if (matchedItem.IsNull())
                {
                    itemsToAdd.Add(new OrderDetailItemViewModel
                    {
                        IsTaxable = i.IsTaxable,
                        LineNumber = lineNumber++,
                        LocationId = i.LocationId,
                        ListPrice = i.Price,                        
                        ProductId = i.ProductId,
                        ProductNumber = i.ProductNumber,
                        ProductName = i.ProductName,
                        Quantity = 1,
                        QuantityAvailable = i.Quantity,
                        Status = OrderStatus.Active,
                        UnitOfMeasure = i.UnitOfMeasure,
                    });
                }
                else
                {
                    matchedItem.Quantity += 1;
                }
            });

            existingItems.AddRange(itemsToAdd);
            OrderDetailItems = existingItems;
        }

        public static void RemoveItem(int locationId, int productId)
        {
            var sessionList = OrderDetailItems;
            var itemToRemove = sessionList.FirstOrDefault(oi => oi.LocationId == locationId && oi.ProductId == productId);
            if (itemToRemove.IsNotNull())
            {
                sessionList.Remove(itemToRemove);
                OrderDetailItems = sessionList;
            }
        }

        public static void UpdateQuantity(int locationId, int productId, int quantity)
        {
            var itemToUpdate = OrderDetailItems.FirstOrDefault(oi => oi.LocationId == locationId && oi.ProductId == productId);
            if (itemToUpdate.IsNotNull())
            {
                itemToUpdate.Quantity = quantity;
            }
        }

        public static void UpdateItem(OrderDetail updatedItem)
        {
            var sessionList = OrderDetailItems;
            var existingItem = sessionList.FirstOrDefault(odi => odi.OrderDetailId == updatedItem.OrderDetailId);

            var newItem = new OrderDetailItemViewModel
            {
                IsTaxable = updatedItem.ProductTaxable,
                LineNumber = updatedItem.LineNumber,
                LocationId = updatedItem.LocationId,
                OrderDetailId = updatedItem.OrderDetailId,
                ListPrice = updatedItem.Price,
                ProductId = updatedItem.ProductId,
                ProductName = updatedItem.ProductName,
                Quantity = updatedItem.Quantity,
                QuantityAvailable = existingItem.QuantityAvailable,
                Status = (OrderStatus)updatedItem.StatusId
            };

            if (existingItem.IsNotNull())
            {
                sessionList.Remove(existingItem);
            }

            sessionList.Add(newItem);

            OrderDetailItems = sessionList;
        }

        public static void EmptySessionItems()
        {
            OrderDetailItems = new List<OrderDetailItemViewModel>();
        }

        public static decimal GetOrderTotal()
        {
            decimal grandTotal = 0;
            OrderDetailItems.ForEach(i => grandTotal += (i.SellPrice * i.Quantity));

            return grandTotal;
        }

        public static decimal GetTaxableTotal()
        {
            if (OrderDetailItems.IsNull())
            {
                return 0;
            }

            return OrderDetailItems.Where(i => i.IsTaxable == true).Select(i => i.Quantity * i.SellPrice).Sum();
        }

        public static decimal GetNonTaxableTotal()
        {
            if (OrderDetailItems.IsNull())
            {
                return 0;
            }

            return OrderDetailItems.Where(i => i.IsTaxable == false).Select(i => i.Quantity * i.SellPrice).Sum();
        }

        public static decimal GetTotal()
        {
            if (OrderDetailItems.IsNull())
            {
                return 0;
            }

            return OrderDetailItems.Select(i => i.Quantity * i.SellPrice).Sum();
        }

        internal static decimal GetProductPriceFromTier(int productId)
        {
            var tierProducts = PriceTierHelper.PriceTierSessionItems;
            if (tierProducts.Any())
            {
                var matchedItem = tierProducts
                    .Where(tp => tp.Key == productId)
                    .FirstOrDefault();
                return matchedItem.Value > 0
                    ? matchedItem.Value
                    : 0;
            }
            else
            {
                return 0;
            }
        }
    }
}