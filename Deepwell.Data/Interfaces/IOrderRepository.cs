using Deepwell.Common.CommonModel;
using Deepwell.Common.CommonModel.Order;
using Deepwell.Common.Models;
using System.Collections.Generic;

namespace Deepwell.Data.Interfaces
{
    public interface IOrderRepository
    {
        IEnumerable<OrderItemStatu> GetOrderStatus();
        IEnumerable<OrderSearchResponse> Search(OrderSearchRequest request);
        OrderHeader GetOrder(int orderId);
        bool SaveOrder(CreateOrderRequest request);
        ResponseResult UpdateOrderItem(UpdateOrderItemRequest request);
        ProductInventory GetInventoryByProductAndLocationId(int productId, int locationId);
        int GetInventoryQuantityByProductAndLocationId(int productId, int locationId);
        bool EditOrder(EditOrderRequest request);

        OrderDetail GetOrderDetail(int orderDetailId);

        ResponseResult UpdateOrderItemQuantity(int orderDetailId, int quantity);

        int GetNewOrderId();

        ResponseResult InvoiceShippedOrderItems(int orderId, decimal taxAmount);
        IEnumerable<OrderDetail> GetOrderItems(int orderId);

        ResponseResult UpdateOrderTotalsToDB(int orderId);
        OrderProcess GetOrderProcessById(int invoiceId);
        IEnumerable<OrderProcess> GetInvoices(int orderId);
        IEnumerable<OrderProcessDetail> GetOrderDetailsByOriginalOrderDetailId(int orderDetailId);
        ProcOrderStatsGet GetStats();
    }
}