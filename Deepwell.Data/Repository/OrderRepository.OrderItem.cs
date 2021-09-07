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

    public partial class OrderRepository
    {
        public OrderDetail GetOrderDetail(int orderDetailId)
        {
            return _deepwellContext
                .OrderDetails
                .FirstOrDefault(od => od.OrderDetailId == orderDetailId);
        }

        public ResponseResult UpdateOrderItemQuantity(int orderDetailId, int quantity)
        {
            var response = new ResponseResult();

            var orderDetailItem = _deepwellContext.OrderDetails.FirstOrDefault(od => od.OrderDetailId == orderDetailId);
            _deepwellContext.Entry(orderDetailItem).State = System.Data.Entity.EntityState.Modified;
            orderDetailItem.Quantity = quantity;

            response.Success = _deepwellContext.SaveChanges() > 0;
            return response;
        }

        public ResponseResult UpdateOrderItem(UpdateOrderItemRequest request)
        {
            var response = new ResponseResult();

            switch (request.ItemAction)
            {
                case OrderItemAction.Ship:
                    {
                        response = this.ShipOrderItem(request);
                        break;
                    }
                case OrderItemAction.Invoice:
                    {
                        response = this.InvoiceOrderItem(request);
                        break;
                    }
                case OrderItemAction.Return:
                    {
                        response = this.ReturnOrderItem(request);
                        break;
                    }
                case OrderItemAction.Cancel:
                    {
                        this.CancelOrderItem(request);
                        break;
                    }
            }

            return response;
        }

        public ResponseResult ProcessItems(OrderProcessRequest request)
        {
            var response = new ResponseResult();

            int orderProcessId = request.Items.FirstOrDefault().IsNotNull()
                ? request.Items.FirstOrDefault().OrderProcessId
                : 0;

            var isShipped = request.Source == "ship";

            if (orderProcessId > 0)
            {
                // update
                var itemToUpdate = _deepwellContext.OrderProcesses.FirstOrDefault(op => op.OrderProcessId == orderProcessId);
                if (itemToUpdate.IsNotNull())
                {
                    if (isShipped)
                    {
                        itemToUpdate.DateShipped = DateTime.Now;
                        itemToUpdate.PoNumber = request.PoNumber;
                        itemToUpdate.ShippingVia = request.ShippingVia;
                        itemToUpdate.TrackingId = request.TrackingId;
                        itemToUpdate.IsShipped = true;
                    }
                    else
                    {
                        itemToUpdate.DateInvoiced = DateTime.Now;
                        itemToUpdate.IsInvoiced = true;
                        itemToUpdate.Comments = request.Comments;
                    }

                    this.UpdateOrderDetails(request.Items, isShipped, true);

                    if (isShipped)
                    {
                        response = this.UpdateInventoryByOrderDetailId(request);
                    }
                }
                else
                {
                    response.Success = false;
                    response.ErrorMessage = $"Not able to find the Order process batch with Id: {orderProcessId}.";
                }
            }
            else
            {
                // insert
                OrderProcess process = new OrderProcess
                {
                    IsInvoiced = isShipped == false,
                    IsShipped = isShipped,
                    OrderId = request.OrderId,
                    Owner = SessionHelper.UserId,
                    TaxPercent = request.TaxPercent,
                };

                if (isShipped)
                {
                    process.DateShipped = DateTime.Now;
                    process.PoNumber = request.PoNumber;
                    process.ShippingVia = request.ShippingVia;
                    process.TrackingId = request.TrackingId;
                }
                else
                {
                    process.DateInvoiced = DateTime.Now;
                    process.Comments = request.Comments;
                }

                foreach (var item in request.Items)
                {
                    process.OrderProcessDetails.Add(new OrderProcessDetail
                    {
                        OriginalOrderDetailId = item.OrderDetailId,
                        DateCreated = DateTime.Now,
                        DateModified = DateTime.Now,
                        OrderDetailId = item.OrderDetailId,
                        OrderProcessId = process.OrderProcessId,
                    });
                }

                var orderDetails = this.UpdateOrderDetails(request.Items, isShipped);
                process.Total = orderDetails.Sum(od => od.Quantity * od.Price);

                if (isShipped)
                {
                    response = this.UpdateInventoryByOrderDetailId(request);
                }

                _deepwellContext.OrderProcesses.Add(process);
            }

            if (response.Success)
            {
                response.Success = _deepwellContext.SaveChanges() > 0;
            }

            return response;
        }

        private List<OrderDetail> UpdateOrderDetails(List<OrderProcessItem> requestItems, bool isShipped, bool isProcessed = false)
        {
            List<int> detailIds = requestItems.Select(i => i.OrderDetailId).ToList();

            var orderDetails = _deepwellContext
                .OrderDetails
                .Where(od => detailIds.Contains(od.OrderDetailId)).ToList();

            var status = isProcessed
                ? OrderStatus.ShippedAndInvoiced
                : isShipped
                    ? OrderStatus.Shipped
                    : OrderStatus.Invoiced;

            foreach (var item in orderDetails)
            {
                item.StatusId = (short)status;

                int quantityReturned = requestItems.Where(ri => ri.OrderDetailId == item.OrderDetailId).FirstOrDefault()?.QuantityReturned ?? 0;
                if ((status.Equals(OrderStatus.Invoiced) || status.Equals(OrderStatus.ShippedAndInvoiced)) && item.InvoicedQuantity.IsNull())
                {
                    item.InvoicedQuantity = item.Quantity - quantityReturned;
                }

                if ((status.Equals(OrderStatus.Shipped) || status.Equals(OrderStatus.ShippedAndInvoiced)) && item.ShippedQuantity.IsNull())
                {
                    item.ShippedQuantity = item.Quantity - quantityReturned;
                }
            }

            return orderDetails;
        }

        private ResponseResult CancelOrderItem(UpdateOrderItemRequest request)
        {
            var response = new ResponseResult();

            this.UpdateOrderItemStatus(request.OrderDetailId, OrderStatus.Cancelled, request.ItemAction);

            var productInventory = this.GetInventoryByProductAndLocationId(request.ProductId, request.LocationId);
            this.UpdateProductInventory(productInventory, request.Quantity, OrderItemAction.Cancel);

            this.LogInventoryChange(InventoryAction.Cancelled, InventoryChangeType.Cancelled, productInventory.InventoryId,
                   request.LocationId, request.ProductId, request.Quantity, request.OrderId);

            this.UpdateOrderTotals(request.OrderId);

            response.Success = _deepwellContext.SaveChanges() > 0;

            return response;
        }

        private ResponseResult ReturnOrderItem(UpdateOrderItemRequest request)
        {
            var response = new ResponseResult();

            if (request.QuantityToUpdate > request.Quantity)
            {
                response.Success = false;
                response.Message = $"Requested quantity {request.QuantityToUpdate} is greater than returned quantity {request.Quantity}. ";
                return response;
            }

            var productInventory = this.GetInventoryByProductAndLocationId(request.ProductId, request.LocationId);
            this.UpdateProductInventory(productInventory, request.QuantityToUpdate, OrderItemAction.Return);

            this.LogInventoryChange(InventoryAction.Returned, InventoryChangeType.Returned, productInventory.InventoryId,
                   request.LocationId, request.ProductId, request.QuantityToUpdate, request.OrderId);

            if (request.QuantityToUpdate <= request.Quantity && request.QuantityToUpdate > 0)
            {
                // Partial return, Create a new line item for remaining items not shipped. 
                var orderItemToCopy = this.GetOrderDetail(request.OrderDetailId);

                var maxLineNumber = _deepwellContext
                    .OrderDetails
                    .Where(od => od.OrderId == request.OrderId)
                    .Max(od => od.LineNumber);

                int orderDetailIdForReturnedItem = 0;
                if (orderItemToCopy.IsNotNull())
                {
                    var orderLineRequest = new OrderDetail
                    {
                        OrderId = request.OrderId,
                        LineNumber = Convert.ToSByte(++maxLineNumber),
                        ListPrice = orderItemToCopy.ListPrice,
                        ProductId = orderItemToCopy.ProductId,
                        ProductName = orderItemToCopy.ProductName,
                        ProductNumber = orderItemToCopy.ProductNumber,
                        ProductTaxable = orderItemToCopy.ProductTaxable,
                        LocationId = orderItemToCopy.LocationId,
                        Quantity = request.QuantityToUpdate,
                        Price = orderItemToCopy.Price,
                        StatusId = Convert.ToSByte(OrderStatus.Returned),
                        DateCreated = DateTime.Now,
                        DateModified = DateTime.Now,
                    };

                    _deepwellContext.OrderDetails.Add(orderLineRequest);
                    orderDetailIdForReturnedItem = orderLineRequest.OrderDetailId;
                }

                if (request.CurrentStatus.Equals(OrderStatus.Invoiced))
                {
                    this.AddCreditMemo(request, orderDetailIdForReturnedItem, request.OrderDetailId);
                }
            }

            this.UpdateOrderTotals(request.OrderId);

            response.Success = _deepwellContext.SaveChanges() > 0;

            return response;
        }

        public ResponseResult ReturnMultipleItems(List<CreditMemoItemModel> itemsToReturn, decimal taxAmount, int ownerId)
        {
            var response = new ResponseResult();
            if (itemsToReturn.Any() == false)
            {
                return response;
            }

            int orderProcessId = itemsToReturn.First().OrderProcessId;

            var batchDetails = _deepwellContext.OrderProcesses
                .Where(o => o.OrderProcessId == orderProcessId)
                .FirstOrDefault();

            var creditMemoBatch = new OrderProcess();

            if (batchDetails.IsInvoiced || batchDetails.IsShipped)
            {
                creditMemoBatch.OrderId = batchDetails.OrderId;
                creditMemoBatch.TaxPercent = taxAmount;
                creditMemoBatch.Total = 0M;
                creditMemoBatch.Owner = ownerId;
                creditMemoBatch.IsReturned = true;
                creditMemoBatch.DateReturned = DateTime.Now;
                creditMemoBatch.IsInvoiced = batchDetails.IsInvoiced;

                _deepwellContext.OrderProcesses.Add(creditMemoBatch);
            }

            itemsToReturn.ForEach(i =>
            {
                if (i.QuantityToReturn <= i.Quantity && i.QuantityToReturn > 0)
                {
                    // Partial return, Create a new line item for remaining items not shipped. 
                    var orderItemToCopy = this.GetOrderDetail(i.OrderDetailId);

                    var maxLineNumber = _deepwellContext
                        .OrderDetails
                        .Where(od => od.OrderId == batchDetails.OrderId)
                        .Max(od => od.LineNumber);

                    int orderDetailIdForReturnedItem = 0;
                    if (orderItemToCopy.IsNotNull())
                    {
                        var orderLineRequest = new OrderDetail
                        {
                            OrderId = batchDetails.OrderId,
                            LineNumber = Convert.ToSByte(++maxLineNumber),
                            ListPrice = orderItemToCopy.ListPrice,
                            ProductId = orderItemToCopy.ProductId,
                            ProductName = orderItemToCopy.ProductName,
                            ProductNumber = orderItemToCopy.ProductNumber,
                            ProductTaxable = orderItemToCopy.ProductTaxable,
                            LocationId = orderItemToCopy.LocationId,
                            Quantity = i.QuantityToReturn,
                            Price = orderItemToCopy.Price,
                            StatusId = Convert.ToSByte(OrderStatus.Returned),
                            DateCreated = DateTime.Now,
                            DateModified = DateTime.Now,
                        };

                        _deepwellContext.OrderDetails.Add(orderLineRequest);
                        _deepwellContext.SaveChanges();

                        if (batchDetails.IsInvoiced || batchDetails.IsShipped)
                        {
                            var orderProcessDetail = new OrderProcessDetail
                            {
                                OriginalOrderDetailId = i.OrderDetailId,
                                OrderDetailId = orderLineRequest.OrderDetailId,
                                OrderProcessId = creditMemoBatch.OrderProcessId,
                                DateCreated = DateTime.Now,
                                DateModified = DateTime.Now,
                            };

                            _deepwellContext.OrderProcessDetails.Add(orderProcessDetail);
                        }
                    }
                }

                // Update product inventory with returned items
                if (batchDetails.IsShipped)
                {
                    var productInventory = this.GetInventoryByProductAndLocationId(i.ProductId, i.LocationId);
                    this.UpdateProductInventory(productInventory, i.QuantityToReturn, OrderItemAction.Return);

                    this.LogInventoryChange(InventoryAction.Returned, InventoryChangeType.Returned, productInventory.InventoryId,
                           i.LocationId, i.ProductId, i.QuantityToReturn, batchDetails.OrderId);
                }
            });

            response.Success = _deepwellContext.SaveChanges() > 0;

            return response;
        }

        private ResponseResult InvoiceOrderItem(UpdateOrderItemRequest request)
        {
            var response = new ResponseResult();
            this.UpdateOrderItemStatus(request.OrderDetailId, OrderStatus.Invoiced, request.ItemAction);

            response.Success = _deepwellContext.SaveChanges() > 0;

            return response;
        }

        private ResponseResult ShipOrderItem(UpdateOrderItemRequest request)
        {
            var response = new ResponseResult();
            var productInventory = this.GetInventoryByProductAndLocationId(request.ProductId, request.LocationId);
            int currentInventory = this.GetQuantityFromInventory(productInventory);

            if (request.QuantityToUpdate > request.Quantity)
            {
                response.Success = false;
                response.Message = $"Requested quantity {request.QuantityToUpdate} is greater than shipped quantity {request.Quantity}. ";
                return response;
            }

            if (request.QuantityToUpdate > currentInventory)
            {
                response.Success = false;
                response.Message = $"Requested quantity {request.QuantityToUpdate} is not available. " +
                    $"Current available invnetory is: {currentInventory}";
                return response;
            }

            this.UpdateOrderItemStatus(request.OrderDetailId, OrderStatus.Shipped, request.ItemAction, request.QuantityToUpdate);

            this.UpdateProductInventory(productInventory, request.QuantityToUpdate, request.ItemAction);

            this.LogInventoryChange(InventoryAction.Shipped, InventoryChangeType.Ordered, productInventory.InventoryId,
                request.LocationId, request.ProductId, request.QuantityToUpdate, request.OrderId);

            if (request.QuantityToUpdate < request.Quantity && request.QuantityToUpdate > 0)
            {
                // Partial shipping, Create a new line item for remaining items not shipped. 
                var remainingQuantity = request.Quantity - request.QuantityToUpdate;
                var orderItemToCopy = this.GetOrderDetail(request.OrderDetailId);

                var maxLineNumber = _deepwellContext
                    .OrderDetails
                    .Where(od => od.OrderId == request.OrderId)
                    .Max(od => od.LineNumber);

                if (orderItemToCopy.IsNotNull())
                {
                    var orderLineRequest = new OrderDetail
                    {
                        OrderId = request.OrderId,
                        LineNumber = Convert.ToSByte(++maxLineNumber),
                        ListPrice = orderItemToCopy.ListPrice,
                        ProductId = orderItemToCopy.ProductId,
                        ProductName = orderItemToCopy.ProductName,
                        ProductNumber = orderItemToCopy.ProductNumber,
                        ProductTaxable = orderItemToCopy.ProductTaxable,
                        LocationId = orderItemToCopy.LocationId,
                        Quantity = remainingQuantity,
                        Price = orderItemToCopy.Price,
                        StatusId = Convert.ToSByte(OrderStatus.Active),
                        DateCreated = DateTime.Now,
                        DateModified = DateTime.Now,
                    };

                    _deepwellContext.OrderDetails.Add(orderLineRequest);
                }
            }

            this.UpdateOrderTotals(request.OrderId);

            response.Success = _deepwellContext.SaveChanges() > 0;

            return response;
        }

        private void UpdateOrderItemStatus(int orderDetailId, OrderStatus status, OrderItemAction action, int quantityAffected = -1)
        {
            var orderDetailItem = _deepwellContext.OrderDetails.FirstOrDefault(od => od.OrderDetailId == orderDetailId);
            _deepwellContext.Entry(orderDetailItem).State = System.Data.Entity.EntityState.Modified;

            switch (action)
            {
                case OrderItemAction.Cancel:
                    {
                        orderDetailItem.Quantity = 0;
                        break;
                    }
                case OrderItemAction.Ship:
                    {
                        orderDetailItem.Quantity = quantityAffected;
                        break;
                    }
            }

            orderDetailItem.StatusId = (short)status;
        }

        private void UpdateProductInventory(ProductInventory productInventory, int quantity, OrderItemAction action)
        {
            _deepwellContext.Entry(productInventory).State = System.Data.Entity.EntityState.Modified;

            switch (action)
            {
                case OrderItemAction.Ship:
                    productInventory.Quantity -= quantity;
                    break;
                case OrderItemAction.Cancel:
                case OrderItemAction.Return:
                    productInventory.Quantity += quantity;
                    break;
            }
        }

        public ProductInventory GetInventoryByProductAndLocationId(int productId, int locationId)
        {
            return _deepwellContext
                .ProductInventories
                .FirstOrDefault(pi => pi.ProductId == productId && pi.LocationId == locationId);
        }

        public ResponseResult UpdateInventoryByOrderDetailId(OrderProcessRequest request)
        {
            var result = new ResponseResult();
            List<OrderProcessItem> requestItems = request.Items;
            List<int> inventoryids = requestItems.Select(i => i.InventoryId).ToList();

            var productInventories = _deepwellContext
                .ProductInventories
                .Where(pi => inventoryids.Contains(pi.InventoryId))
                .AsEnumerable();

            var qty = 0;
            foreach (var item in productInventories)
            {
                _deepwellContext.Entry(item).State = System.Data.Entity.EntityState.Modified;
                qty = requestItems.FirstOrDefault(i => i.InventoryId == item.InventoryId).Quantity;

                if (item.Quantity < qty)
                {
                    result.Success = false;
                    result.ErrorMessage = "One or more products dont have enough Inventory to ship.";
                    return result;
                }

                item.Quantity = item.Quantity - qty;
            }

            var logs = new List<InventoryLog>();

            request.Items.ForEach(i =>
            {
                logs.Add(new InventoryLog
                {
                    Action = InventoryAction.Shipped.ToString(),
                    ChangeType = InventoryChangeType.Ordered.ToString(),
                    DateCreated = DateTime.Now,
                    InventoryId = i.InventoryId,
                    LocationId = SessionHelper.LocationId,
                    OrderId = request.OrderId,
                    ProductId = i.ProductId,
                    QuantityAffected = i.Quantity,
                    Remarks = this.GetRemarks(InventoryChangeType.Ordered, request.OrderId),
                    Source = InventorySource.Order.ToString(),
                    UserId = SessionHelper.UserId
                });
            });

            _deepwellContext.InventoryLogs.AddRange(logs);

            return result;
        }

        public int GetInventoryQuantityByProductAndLocationId(int productId, int locationId)
        {
            int inventory = 0;

            var productInventory = this.GetInventoryByProductAndLocationId(productId, locationId);

            if (productInventory.IsNotNull())
            {
                inventory = productInventory.Quantity.Value;
            }

            return inventory;
        }

        private int GetQuantityFromInventory(ProductInventory productInventory)
        {
            int inventory = 0;

            if (productInventory.IsNotNull())
            {
                inventory = productInventory.Quantity.Value;
            }

            return inventory;
        }

        private void LogInventoryChange(InventoryAction action, InventoryChangeType changeType, int inventoryId,
            int locationId, int productId, int quantity, int orderId)
        {
            var inventoryLog = new InventoryLog
            {
                Action = action.ToString(),
                ChangeType = changeType.ToString(),
                DateCreated = DateTime.Now,
                InventoryId = inventoryId,
                LocationId = locationId,
                OrderId = orderId,
                ProductId = productId,
                QuantityAffected = quantity,
                Remarks = this.GetRemarks(changeType, orderId),
                Source = InventorySource.Order.ToString(),
                UserId = SessionHelper.UserId
            };

            _deepwellContext.InventoryLogs.Add(inventoryLog);
        }

        private string GetRemarks(InventoryChangeType changeType, int orderId)
        {
            string remarks = string.Empty;
            switch (changeType)
            {
                case InventoryChangeType.Ordered:
                    remarks = $"Inventory shipped for Order No: {orderId}";
                    break;
                case InventoryChangeType.Returned:
                    remarks = $"Inventory returned from Order No: {orderId}";
                    break;
                case InventoryChangeType.Cancelled:
                    remarks = $"Inventory cancelled from Order No: {orderId}";
                    break;
            }

            return remarks;
        }

        public ResponseResult InvoiceShippedOrderItems(int orderId, decimal taxAmount)
        {
            var response = new ResponseResult();

            short shippedStatus = (short)OrderStatus.Shipped;
            short inovicedStatus = (short)OrderStatus.Invoiced;
            var shippedItems = _deepwellContext.OrderDetails.Where(od => od.OrderId == orderId && od.StatusId == shippedStatus);
            //_deepwellContext.Entry(shippedItems).State = System.Data.Entity.EntityState.Modified;

            decimal nonTaxableTotal = shippedItems.FirstOrDefault(si => si.ProductTaxable == false).IsNotNull()
                ? shippedItems.Where(si => si.ProductTaxable == false).Sum(si => si.Price * si.Quantity)
                : 0M;

            decimal taxableTotal = shippedItems.FirstOrDefault(si => si.ProductTaxable == true).IsNotNull()
                ? shippedItems.Where(si => si.ProductTaxable == true).Sum(si => si.Price * si.Quantity)
                : 0M;


            var invoice = new Invoice
            {
                GrandTotal = nonTaxableTotal + taxableTotal,
                InvoiceDate = DateTime.Now,
                OrderId = orderId,
                PlacedBy = SessionHelper.UserId,
                TaxAmount = taxAmount,
            };

            foreach (var item in shippedItems)
            {
                invoice.InvoiceDetails.Add(new InvoiceDetail
                {
                    DateCreated = DateTime.Now,
                    DateModified = DateTime.Now,
                    InvoiceId = invoice.InvoiceId,
                    OrderDetailId = item.OrderDetailId,
                    OriginalOrderDetailId = item.OrderDetailId,
                });
            }

            _deepwellContext.Invoices.Add(invoice);


            foreach (var item in shippedItems)
            {
                item.StatusId = inovicedStatus;
            }

            response.Success = _deepwellContext.SaveChanges() > 0;

            return response;
        }

        public OrderProcess GetOrderProcessById(int invoiceId)
        {
            return _deepwellContext
                .OrderProcesses
                .Where(id => id.OrderProcessId == invoiceId)
                .FirstOrDefault();
        }

        public IEnumerable<OrderProcess> GetInvoices(int orderId)
        {
            return _deepwellContext
                .OrderProcesses
                .Where(i => i.OrderId == orderId && i.IsInvoiced == true && i.IsReturned == false)
                .OrderByDescending(i => i.DateInvoiced);
        }

        public IEnumerable<OrderProcess> GetShipping(int orderId)
        {
            return _deepwellContext
                .OrderProcesses
                .Where(i => i.OrderId == orderId && i.IsShipped == true)
                .OrderByDescending(i => i.DateShipped);
        }

        public IEnumerable<OrderProcess> GetCreditMemo(int orderId)
        {
            return _deepwellContext
                .OrderProcesses
                .Where(i => i.OrderId == orderId && i.IsReturned == true && i.IsInvoiced == true)
                .OrderByDescending(i => i.DateReturned);
        }
    }
}
