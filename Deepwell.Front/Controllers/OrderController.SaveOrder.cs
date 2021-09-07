namespace Deepwell.Controllers
{
    using System;
    using System.Collections.Generic;
    using Deepwell.Common.CommonModel.Customer;
    using Deepwell.Common.CommonModel.Order;
    using Deepwell.Common.Enum;
    using Deepwell.Common.Extensions;
    using Deepwell.Common.Helpers;
    using Deepwell.Front.Helpers;
    using Deepwell.Front.Models.Customer;
    using Deepwell.Front.Models.Order;

    public partial class OrderController
    {
        private bool SaveNewOrder(OrderDetailViewModel order)
        {
            var billingAddress = order.BillingAddress;
            var shippingAddress = order.ShippingAddress;

            var request = new CreateOrderRequest
            {
                BillingAddress = this.OrderAddressFromModelAddress(billingAddress),
                ShippingAddress = this.OrderAddressFromModelAddress(shippingAddress),
                CustomerId = order.CustomerId,
                CustomerName = order.CustomerName,
                CustomerEmail = order.CustomerEmail,
                OrderDate = DateTime.Now,
                OrderStatus = Convert.ToInt32(OrderStatus.Active),
                Phone = order.Phone,
                Items = this.GetOrderItems(),
                OrderId = 0,
                PlacedById = SessionHelper.UserId,
                GrandTotal = OrderItemsHelper.GetOrderTotal(),
                TaxableTotal = OrderItemsHelper.GetTaxableTotal(),
                NonTaxableTotal = OrderItemsHelper.GetNonTaxableTotal(),
                PriceTierId = order.TierId,
            };

            return orderRepository.SaveOrder(request);
        }

        private bool SaveEditedOrder(OrderDetailViewModel order)
        {
            var billingAddress = order.BillingAddress;
            var shippingAddress = order.ShippingAddress;

            var request = new EditOrderRequest
            {
                BillingAddress = this.OrderAddressFromModelAddress(billingAddress),
                ShippingAddress = this.OrderAddressFromModelAddress(shippingAddress),
                CustomerEmail = order.CustomerEmail,
                CustomerName = order.CustomerName,
                Phone = order.Phone,
                Items = this.GetOrderItems(),
                OrderId = order.OrderId,
                OrderStatus = order.OrderStatus,
                OrderDate = Convert.ToDateTime(order.OrderDate),
                GrandTotal = OrderItemsHelper.GetOrderTotal(),
                TaxableTotal = OrderItemsHelper.GetTaxableTotal(),
                NonTaxableTotal = OrderItemsHelper.GetNonTaxableTotal(),
            };

            return orderRepository.EditOrder(request);
        }

        private IEnumerable<OrderItemDetail> GetOrderItems()
        {
            var items = OrderItemsHelper.OrderDetailItems;
            var response = new List<OrderItemDetail>();
            if (items.IsNotNull())
            {
                items.ForEach(i =>
                {
                    response.Add(new OrderItemDetail
                    {
                        OrderDetailId = i.OrderDetailId,
                        LineNumber = i.LineNumber,
                        IsTaxable = i.IsTaxable,
                        SellPrice = i.SellPrice > 0
                            ? i.SellPrice
                            : i.ListPrice,
                        ListPrice = i.ListPrice,
                        ProductId = i.ProductId,
                        ProductNumber = i.ProductNumber,
                        ProductName = i.ProductName,
                        Quantity = i.Quantity,
                        Status = i.Status,
                        LocationId = i.LocationId,
                        CreatedDate = DateTime.Now,
                        ModifiedDate = DateTime.Now,
                    });
                });
            }

            return response;
        }

        private Address OrderAddressFromModelAddress(CustomerAddress modelAddress)
        {
            if (modelAddress.IsNotNull())
            {
                return new Address
                {
                    WellName = modelAddress.WellName,
                    City = modelAddress.City,
                    County = modelAddress.County,
                    PostalCode = modelAddress.Zipcode,
                    StateId = Convert.ToInt32(modelAddress.StateId),
                };
            }
            else
            {
                return new Address();
            }
        }
    }
}