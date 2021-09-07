namespace Deepwell.Data.Repository
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Deepwell.Common.CommonModel;
    using Deepwell.Common.CommonModel.Order;
    using Deepwell.Common.Enum;
    using Deepwell.Common.Extensions;
    using Deepwell.Common.Helpers;
    using Deepwell.Common.Models;
    using Deepwell.Data.Interfaces;
    using CustomerAddress = Common.CommonModel.Customer;

    public partial class OrderRepository : IOrderRepository
    {
        private DeepwellContext _deepwellContext;

        public OrderRepository()
        {
            _deepwellContext = new DeepwellContext();
        }

        public IEnumerable<OrderItemStatu> GetOrderStatus()
        {
            return _deepwellContext.OrderItemStatus;
        }

        public IEnumerable<OrderSearchResponse> Search(OrderSearchRequest request)
        {
            IEnumerable<OrderSearchResponse> response = this.GetOrders();

            if (request.OrderId > 0)
            {
                response = response.Where(o => o.OrderId == request.OrderId);
            }

            if (request.ProductNumber.HasValue())
            {
                response = response.Where(o => o.ProductNumber.ToLower().Contains(request.ProductNumber.ToLower()));
            }

            if (request.CustomerNumber.HasValue())
            {
                response = response.Where(o => o.CustomerNumber.ToLower().Contains(request.CustomerNumber.ToLower()));
            }

            if (request.CustomerName.HasValue())
            {
                response = response.Where(p => p.CustomerName.ToLower().Contains(request.CustomerName.ToLower()));
            }

            if (request.ProductName.HasValue())
            {
                response = response.Where(p => p.ProductName.ToLower().Contains(request.ProductName.ToLower()));
            }

            if (request.OrderStatus != 0)
            {
                response = response.Where(p => p.OrderStatus == request.OrderStatus);
            }

            response = response.Where(p => p.OrderDate.Date >= request.OrderDateFrom.Date && p.OrderDate.Date <= request.OrderDateTo.Date);

            return response.OrderByDescending(p => p.OrderDate);
        }

        public IEnumerable<OrderSearchResponse> GetOrders()
        {
            return (from oh in _deepwellContext.OrderHeaders
                    join od in _deepwellContext.OrderDetails on oh.OrderId equals od.OrderId
                    select new OrderSearchResponse
                    {
                        OrderId = oh.OrderId,
                        CustomerName = oh.CustomerName,
                        OrderDate = oh.DateCreated,
                        CustomerId = oh.CustomerId,
                        CustomerNumber = oh.Customer.CustomerNumber,
                        OrderStatus = od.StatusId,
                        ProductId = od.ProductId,
                        ProductNumber = od.ProductNumber,
                        ProductName = od.ProductName,
                        LocationId = od.LocationId,
                    }).OrderBy(o => o.OrderId);
        }

        public OrderHeader GetOrder(int orderId)
        {
            return _deepwellContext
                 .OrderHeaders
                 .FirstOrDefault(oh => oh.OrderId == orderId);
        }

        public IEnumerable<OrderDetail> GetOrderItems(int orderId)
        {
            return _deepwellContext
                 .OrderDetails
                 .Where(od => od.OrderId == orderId);
        }

        public bool SaveOrder(CreateOrderRequest request)
        {
            try
            {
                var newBillingAddress = this.AddAddress(request.BillingAddress, AddressType.Billing);
                var newShippingAddress = this.AddAddress(request.ShippingAddress, AddressType.Shipping);
                _deepwellContext.SaveChanges();

                var orderHeaderRequest = new OrderHeader
                {
                    CustomerId = request.CustomerId,
                    CustomerName = request.CustomerName,
                    CustomerEmail = request.CustomerEmail,
                    CustomerPhone = request.Phone,
                    BillingAddressId = newBillingAddress.AddressId,
                    ShippingAddressId = newShippingAddress.AddressId,
                    TaxableTotal = request.TaxableTotal,
                    NonTaxableTotal = request.NonTaxableTotal,
                    GrandTotal = request.GrandTotal,
                    PlacedBy = request.PlacedById,
                    StatusId = Convert.ToSByte(request.OrderStatus),
                    DateCreated = request.OrderDate,
                    DateModified = request.OrderDate,
                };

                if (request.PriceTierId > 0)
                {
                    orderHeaderRequest.TierId = request.PriceTierId;
                }

                _deepwellContext.OrderHeaders.Add(orderHeaderRequest);

                request.Items.ToList().ForEach(i =>
                {
                    var orderLineRequest = new OrderDetail
                    {
                        OrderId = orderHeaderRequest.OrderId,
                        LineNumber = Convert.ToSByte(i.LineNumber),
                        ProductId = i.ProductId,
                        ProductName = i.ProductName,
                        ProductNumber = i.ProductNumber,
                        ProductTaxable = i.IsTaxable,
                        LocationId = Convert.ToSByte(i.LocationId),
                        Quantity = i.Quantity,
                        Price = i.SellPrice,
                        ListPrice = i.ListPrice,
                        StatusId = Convert.ToSByte(i.Status),
                        DateCreated = request.OrderDate,
                        DateModified = request.OrderDate,
                    };

                    _deepwellContext.OrderDetails.Add(orderLineRequest);
                });

                return _deepwellContext.SaveChanges() > 0;

            }
            catch (Exception e)
            {
                return false;
            }
        }

        public bool EditOrder(EditOrderRequest request)
        {
            if (request.IsNotNull())
            {
                try
                {
                    var order = this.GetOrder(request.OrderId);
                    var isOrderHeaderEditable = order.OrderDetails.IsNotNull() && order.OrderDetails.All(i => i.StatusId == (int)Common.Enum.OrderStatus.Active);
                    if (isOrderHeaderEditable)
                    {
                        var orderBillingAddress = order.Address;
                        var updatedBillingAddress = request.BillingAddress;
                        var orderShippingAddress = order.Address1;
                        var updatedShippingAddress = request.ShippingAddress;

                        this.UpdateAddress(orderBillingAddress, request.BillingAddress);
                        this.UpdateAddress(orderShippingAddress, request.ShippingAddress);
                    }

                    order.TaxableTotal = request.TaxableTotal;
                    order.NonTaxableTotal = request.NonTaxableTotal;
                    order.GrandTotal = request.GrandTotal;
                    order.DateModified = DateTime.Now;

                    _deepwellContext.Entry(order).State = System.Data.Entity.EntityState.Modified;

                    request.Items.Where(oi => oi.OrderDetailId == 0).ToList().ForEach(i =>
                    {
                        var orderLineRequest = new OrderDetail
                        {
                            OrderId = request.OrderId,
                            LineNumber = Convert.ToSByte(i.LineNumber),
                            ProductId = i.ProductId,
                            ProductName = i.ProductName,
                            ProductTaxable = i.IsTaxable,
                            ProductNumber = i.ProductNumber,
                            LocationId = Convert.ToSByte(i.LocationId),
                            Quantity = i.Quantity,
                            Price = i.SellPrice,
                            ListPrice = i.ListPrice,
                            StatusId = Convert.ToSByte(i.Status),
                            DateCreated = DateTime.Now,
                            DateModified = DateTime.Now,
                        };

                        _deepwellContext.OrderDetails.Add(orderLineRequest);
                    });

                    return _deepwellContext.SaveChanges() > 0;
                }
                catch (Exception e)
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        public Address AddAddress(CustomerAddress.Address addressRequest, AddressType type)
        {
            var address = this.DataAddressFromRequestAddress(addressRequest, type);
            _deepwellContext.Addresses.Add(address);

            return address;
        }

        public void UpdateAddress(Address address, CustomerAddress.Address updateAddress)
        {
            address.WellName = updateAddress.WellName;
            address.City = updateAddress.City;
            address.County = updateAddress.County;
            address.StateId = updateAddress.StateId;
            address.PostalCode = updateAddress.PostalCode;

            _deepwellContext.Entry(address).State = System.Data.Entity.EntityState.Modified;
        }

        public int GetNewOrderId()
        {
            var orderHeader = _deepwellContext.OrderDetails;

            if (orderHeader.Count() > 0)
            {
                return orderHeader.Max(oh => oh.OrderId) + 1;
            }
            else
            {
                return 1;
            }
        }

        public ResponseResult UpdateOrderTotalsToDB(int orderId)
        {
            var response = new ResponseResult();
            this.UpdateOrderTotals(orderId);
            response.Success = _deepwellContext.SaveChanges() > 0;
            return response;
        }

        public IEnumerable<OrderProcessDetail> GetOrderDetailsByOriginalOrderDetailId(int orderDetailId)
        {
            return _deepwellContext.OrderProcessDetails
                .Where(opd => opd.OriginalOrderDetailId == orderDetailId && opd.OrderDetail.StatusId == (short)OrderStatus.Returned);
        }

        public ProcOrderStatsGet GetStats()
        {
            var result = new ProcOrderStatsGet();
            var sqlResponse = _deepwellContext.OrderStatsGet().FirstOrDefault();
            if (sqlResponse.IsNotNull())
            {
                result.OrderCount = sqlResponse.OrderCount.Value;
                result.OrderTotal = sqlResponse.OrderTotal.IsNotNull()
                    ? sqlResponse.OrderTotal.Value
                    : 0m;
            }

            return result;
        }
        private void UpdateOrderTotals(int orderId)
        {
            var orderToUpdate = this.GetOrder(orderId);

            if (orderToUpdate.IsNotNull())
            {
                var taxableTotal = orderToUpdate.OrderDetails.Where(i => i.ProductTaxable == true).Select(i => i.Quantity * i.Price).Sum();
                var nonTaxableTotal = orderToUpdate.OrderDetails.Where(i => i.ProductTaxable == false).Select(i => i.Quantity * i.Price).Sum();
                orderToUpdate.TaxableTotal = taxableTotal;
                orderToUpdate.NonTaxableTotal = nonTaxableTotal;
                orderToUpdate.GrandTotal = taxableTotal + nonTaxableTotal;
                orderToUpdate.DateModified = DateTime.Now;

                _deepwellContext.Entry(orderToUpdate).State = System.Data.Entity.EntityState.Modified;
            }
        }

        private void AddCreditMemo(UpdateOrderItemRequest request, int orderDetailIdForReturnedItem, int originalOrderDetailId)
        {
            var creditMemo = new Invoice
            {
                InvoiceDate = DateTime.Now,
                OrderId = request.OrderId,
                PlacedBy = SessionHelper.UserId,
                TaxAmount = -request.TaxAmount,
                GrandTotal = -(request.TaxAmount + (request.Price * request.QuantityToUpdate)),
            };

            creditMemo.InvoiceDetails = new List<InvoiceDetail>
                {
                    new InvoiceDetail
                    {
                        InvoiceId = creditMemo.InvoiceId,
                        OrderDetailId = orderDetailIdForReturnedItem,
                        DateCreated = DateTime.Now,
                        DateModified = DateTime.Now,
                        OriginalOrderDetailId = originalOrderDetailId,
                    }
                };

            _deepwellContext.Invoices.Add(creditMemo);
        }

        private Address DataAddressFromRequestAddress(CustomerAddress.Address addressRequest, AddressType type)
        {
            return new Address
            {
                AddressId = addressRequest.AddressId,
                WellName = addressRequest.WellName,
                City = addressRequest.City,
                County = addressRequest.County,
                StateId = addressRequest.StateId,
                PostalCode = addressRequest.PostalCode,
                AddressType = type.ToString(),
                DateCreated = DateTime.Now,
                DateModified = DateTime.Now,
            };
        }
    }
}
