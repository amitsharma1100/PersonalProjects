namespace Deepwell.Controllers
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Web.Mvc;
    using Deepwell.Common.CommonModel.Order;
    using Deepwell.Common.Extensions;
    using Deepwell.Front.Helpers;
    using Deepwell.Front.Models.Order;
    using Deepwell.Common.Enum;
    using Deepwell.Common.CommonModel;
    using Deepwell.Common;
    using System.Xml.Linq;
    using System.Xml.Xsl;
    using Deepwell.Data;
    using SelectPdf;
    using System.Collections.Generic;
    using Deepwell.Common.Models;
    using Deepwell.Common.Helpers;

    public partial class OrderController
    {
        [HttpPost]
        public JsonResult RemoveItem(int locationId, int productId)
        {
            OrderItemsHelper.RemoveItem(locationId, productId);

            return this.Json(new
            {
                result = this.RenderPartialViewToString("_OrderDetailItemsAdvanced", OrderItemsHelper.OrderDetailItems)
            });
        }

        [HttpPost]
        public JsonResult UpdateQuantity(int orderId, int orderDetailId, int locationId, int productId, int quantity)
        {
            var response = new ResponseResult();

            if (orderDetailId == 0)
            {
                OrderItemsHelper.UpdateQuantity(locationId, productId, quantity);
            }
            else
            {
                response = orderRepository.UpdateOrderItemQuantity(orderDetailId, quantity);
                if (response.Success)
                {
                    orderRepository.UpdateOrderTotalsToDB(orderId);
                    this.UpdateSessionOrderItemsFromDB(orderId);
                }
            }

            return this.Json(new
            {
                success = response.Success,
                message = response.Message,
                result = this.RenderPartialViewToString("_OrderDetailItemsAdvanced", OrderItemsHelper.OrderDetailItems),
                orderSummaryHtml = this.GetOrderSummaryHtml()
            });
        }

        [HttpPost]
        public JsonResult UpdateOrderItem(UpdateOrderItemRequest request)
        {
            var response = new ResponseResult();
            try
            {
                if (request.ItemAction == OrderItemAction.Remove)
                {
                    OrderItemsHelper.RemoveItem(request.LocationId, request.ProductId);
                }
                else
                {
                    response = orderRepository.UpdateOrderItem(request);
                    if (response.Success)
                    {
                        this.UpdateSessionOrderItemsFromDB(request.OrderId);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Log.Error(ex.Message, ex);
            }

            return this.Json(new
            {
                success = response.Success,
                message = response.Message,
                result = this.RenderPartialViewToString("_OrderDetailItemsAdvanced", OrderItemsHelper.OrderDetailItems),
                isOrderHeaderEditable = OrderItemsHelper.OrderDetailItems.All(i => i.Status == OrderStatus.Active),
                orderSummaryHtml = this.GetOrderSummaryHtml(),
                invoicesList = this.GetInvoicesList(request.OrderId),
                shippedItemsList = this.GetShipmentList(request.OrderId),
            });
        }

        [HttpPost]
        public JsonResult AddItemForCreditMemo(UpdateOrderItemRequest request)
        {
            string message = "";
            bool success = false;

            if (request.IsNotNull())
            {
                if (request.QuantityToUpdate > request.Quantity)
                {
                    message = "Quantity to return is greater than maximum allowed.";
                }
                else if (OrderItemsHelper.CreditMemoItems.Where(i => i.OrderProcessId != request.OrderProcessId).Any())
                {
                    message = "Items only from a single batch can be returned.";
                }
                else
                {
                    var itemToReturn = new CreditMemoItemModel
                    {
                        ProductId = request.ProductId,
                        QuantityToReturn = request.QuantityToUpdate,
                        OrderProcessId = request.OrderProcessId,
                        LocationId = request.LocationId,
                        OrderDetailId = request.OrderDetailId,
                        Quantity = request.Quantity,
                    };

                    OrderItemsHelper.AddCreditMemoItem(itemToReturn);
                    success = true;
                    message = "Added to session for return.";
                }
            }
            else
            {
                message = "Invalid request received.";
            }

            return this.Json(new
            {
                success = success,
                message = message,
            });
        }

        [HttpPost]
        public JsonResult ProcessCreditMemo(decimal taxAmount = 0M)
        {
            bool success = false;
            string message = "";

            try
            {
                ResponseResult response = orderRepository.ReturnMultipleItems(OrderItemsHelper.CreditMemoItems, taxAmount, SessionHelper.UserId);
                if (response.Success)
                {
                    success = true;
                    OrderItemsHelper.EmptyCreditMemoSessionItems();
                }
                else
                {
                    message = "Could not return items at this time.";
                }
            }
            catch (Exception ex)
            {
                message = ex.Message;
                Logger.Log.Error(ex.Message, ex);
            }

            return this.Json(new
            {
                success = success,
                message = message,
            });
        }

        public string GetOrderSummaryHtml()
        {
            var orderSummayModel = new OrderSummaryViewModel
            {
                //TaxableItemsTotal = OrderItemsHelper.GetTaxableTotal(),
                //NonTaxableItemsTotal = OrderItemsHelper.GetNonTaxableTotal()
                OrderTotal = OrderItemsHelper.GetTotal()
            };

            return this.RenderPartialViewToString("_OrderSummary", orderSummayModel);
        }

        [HttpPost]
        public JsonResult ProcessOrderItems(OrderProcessRequest request)
        {
            var response = new ResponseResult();
            try
            {
                response = orderRepository.ProcessItems(request);

                if(response.Success)
                {
                    this.UpdateSessionOrderItemsFromDB(request.OrderId);
                }

            }
            catch (Exception ex)
            {
                Logger.Log.Error(ex.Message, ex);
            }

            return this.Json(new
            {
                success = response.Success,
                message = response.Message,
                errorMessage = response.ErrorMessage,
                result = this.RenderPartialViewToString("_OrderDetailItemsAdvanced", OrderItemsHelper.OrderDetailItems),
                isOrderHeaderEditable = OrderItemsHelper.OrderDetailItems.All(i => i.Status == OrderStatus.Active),
                orderSummaryHtml = this.GetOrderSummaryHtml(),
                invoicesList = this.GetInvoicesList(request.OrderId),
                shippedItemsList = this.GetShipmentList(request.OrderId),
            });
        }

        public string GetOrderItemsToProcess(int[] request)
        {
            var response = new List<UpdateOrderItemRequest>();
            var items = OrderItemsHelper.OrderDetailItems;
            response.AddRange(items
                .Where(oi => request.Contains(oi.OrderDetailId))
                .Select(oi => new UpdateOrderItemRequest
                {
                    OrderDetailId = oi.OrderDetailId,
                    LocationId = oi.LocationId,
                    Quantity = oi.Quantity,

                }));

            return this.RenderPartialViewToString("_OrderItemsProcessPopup", response);
        }

        [HttpPost]
        public JsonResult InvoiceItems(int orderId, decimal taxAmount = 0)
        {
            var response = new ResponseResult();

            if (orderId != 0)
            {
                response = orderRepository.InvoiceShippedOrderItems(orderId, taxAmount);
                this.UpdateSessionOrderItemsFromDB(orderId);
            }
            else
            {
                response.Success = false;
                response.Message = "Order Id must be greater than 0.";
            }

            return this.Json(new
            {
                success = response.Success,
                message = response.Message,
                result = this.RenderPartialViewToString("_OrderDetailItemsAdvanced", OrderItemsHelper.OrderDetailItems),
                orderSummaryHtml = this.GetOrderSummaryHtml(),
                invoicesList = this.GetInvoicesList(orderId),
                shippedItemsList = this.GetShipmentList(orderId),
            });
        }

        [HttpGet]
        public ActionResult GetInvoicedItems(int invoiceId)
        {
            try
            {
                OrderProcess response = orderRepository.GetOrderProcessById(invoiceId);
                return this.GenerateInvoicePDF(response);
            }
            catch (Exception ex)
            {
                logger.Error(ex);
            }

            return View();
        }

        [HttpGet]
        public ActionResult GetReturnedItems(int creditMemoId)
        {
            try
            {
                OrderProcess response = orderRepository.GetOrderProcessById(creditMemoId);
                return this.GenerateCreditMemoPDF(response);
            }
            catch (Exception ex)
            {
                logger.Error(ex);
            }

            return View();
        }

        [HttpGet]
        public ActionResult GetShippedItems(int invoiceId)
        {
            try
            {
                OrderProcess response = orderRepository.GetOrderProcessById(invoiceId);
                return this.GenerateShipmentPDF(response);
            }
            catch (Exception ex)
            {
                logger.Error(ex);
            }

            return View();
        }

        private void UpdateSessionOrderItemsFromDB(int orderId)
        {
            var orderItems = orderRepository.GetOrderItems(orderId);

            OrderItemsHelper.EmptySessionItems();
            OrderItemsHelper.AddItem(orderItems.Select(od => new OrderDetailItemViewModel
            {
                QuantityAvailable = orderRepository.GetInventoryQuantityByProductAndLocationId(od.ProductId, od.LocationId),
                IsTaxable = od.ProductTaxable,
                LineNumber = od.LineNumber,
                LocationId = od.LocationId,
                OrderDetailId = od.OrderDetailId,
                ListPrice = od.ListPrice,
                SoldPrice = od.Price,
                ProductId = od.ProductId,
                ProductName = od.ProductName,
                Quantity = od.Quantity,
                Status = od.OrderItemStatu.Title.ToEnum(OrderStatus.None),
                InvoiceId = od.InvoiceDetails.Where(id => id.OrderDetail.StatusId == (short)OrderStatus.Invoiced).FirstOrDefault().IsNotNull()
                        ? od.InvoiceDetails.Where(id => id.OrderDetail.StatusId == (short)OrderStatus.Invoiced).FirstOrDefault().InvoiceId
                        : 0,
                CreditMemoId = od.InvoiceDetails.Where(id => id.Invoice.GrandTotal < 0).FirstOrDefault().IsNotNull()
                        ? od.InvoiceDetails.Where(id => id.Invoice.GrandTotal < 0).FirstOrDefault().InvoiceId
                        : 0,
                QuantityReturned = od.StatusId == (short)OrderStatus.Shipped || od.StatusId == (short)OrderStatus.ShippedAndInvoiced || od.StatusId == (short)OrderStatus.Invoiced
                        ? this.GetReturnedQuantity(od.OrderDetailId)
                        : 0,
            }).ToList());
        }

        private ActionResult GenerateInvoicePDF(OrderProcess invoice)
        {
            HtmlToPdf HtmlToPdf = new HtmlToPdf();
            var xml = this.CreateInvoiceXML(invoice);
            string xsltFile = $"{AppDomain.CurrentDomain.BaseDirectory}Content\\XSL\\InvoicePDF.xslt";

            return GetPdfFromHtml(this.TransformXmlToString(xml, xsltFile), "Invoice");
        }

        private ActionResult GenerateCreditMemoPDF(OrderProcess creditMemo)
        {
            HtmlToPdf HtmlToPdf = new HtmlToPdf();
            var xml = this.CreateCreditMemoXML(creditMemo);
            string xsltFile = $"{AppDomain.CurrentDomain.BaseDirectory}Content\\XSL\\CreditMemoPDF.xslt";

            return GetPdfFromHtml(this.TransformXmlToString(xml, xsltFile), "Credit Memo");
        }

        private XElement CreateInvoiceXML(OrderProcess invoice)
        {
            var billingAddress = invoice.OrderHeader.Address;
            var shippingAddress = invoice.OrderHeader.Address1;
            decimal subTotal = invoice.Total ?? 0;
            decimal taxableTotal = ((invoice.TaxPercent ?? 0) * subTotal) / 100;
            decimal grandTotal = subTotal + taxableTotal;
            bool isDisplayTaxField = (invoice.TaxPercent ?? 0) > 0;

            CustomerPricingSetting customerPricingSetting = invoice.OrderHeader.Customer.PricingSetting.ToEnum(CustomerPricingSetting.ListPrice);

            return new XElement("InvoiceData",
                            new XElement("Date", invoice.DateInvoiced?.ToShortDateString()),
                            new XElement("InvoiceNumber", invoice.OrderProcessId),
                            new XElement("Comments", invoice.Comments),
                            new XElement("TaxPercentage", invoice.TaxPercent),
                            new XElement("IsPriceToBeDisplayed", customerPricingSetting.Equals(CustomerPricingSetting.NoPrice) == false),
                            new XElement("IsDisplayTaxField", isDisplayTaxField),
                            new XElement("Terms", "Net 30"),
                            new XElement("DueDate", invoice.DateInvoiced?.AddDays(30).ToShortDateString()),
                            new XElement("SubTotal", subTotal.FormatCurrency()),
                            new XElement("Tax", taxableTotal.FormatCurrency()),
                            new XElement("Total", grandTotal.FormatCurrency()),
                            new XElement("LogoUrl", $"{AppDomain.CurrentDomain.BaseDirectory}Content\\images\\dw_logo_new.png"),
                            new XElement("DeepwellAddress",
                                new XElement("WellName", "Deepwell Energy Services, LLC"),
                                new XElement("AddressLine2", "Drilling Fluids, USA"),
                                new XElement("AddressLine3", "4025 Highway 35"),
                                new XElement("AddressLine4", "Columbia, MS 39429")),
                            new XElement("BillToAddress",
                                new XElement("WellName", billingAddress.WellName),
                                new XElement("City", billingAddress.City),
                                new XElement("State", billingAddress.State.StateName),
                                new XElement("PostalCode", billingAddress.PostalCode),
                                new XElement("County", billingAddress.County)),
                            new XElement("ShipToAddress",
                                new XElement("WellName", shippingAddress.WellName),
                                new XElement("City", billingAddress.City),
                                new XElement("State", shippingAddress.State.StateName),
                                new XElement("PostalCode", shippingAddress.PostalCode),
                                new XElement("County", shippingAddress.County)),
                            new XElement("Invoices",
                            invoice.OrderProcessDetails.Select(i =>
                                new XElement("Invoice",
                                    new XElement("ProductNumber", i.OrderDetail.ProductNumber),
                                    new XElement("Description", i.OrderDetail.ProductName),
                                    new XElement("Quantity", i.OrderDetail.InvoicedQuantity),
                                    new XElement("Rate", this.GetPriceToDisplay(CustomerPricingSetting.CustomerPrice, i.OrderDetail).FormatCurrency()),
                                    new XElement("Uom", i.OrderDetail.Product.UOM),
                                    new XElement("Amount", (i.OrderDetail.Quantity * this.GetPriceToDisplay(CustomerPricingSetting.CustomerPrice, i.OrderDetail)).FormatCurrency()))
                            )));
        }

        private decimal GetPriceToDisplay(CustomerPricingSetting pricingSetting, OrderDetail orderDetail)
        {
            switch(pricingSetting)
            {
                case CustomerPricingSetting.ListPrice:
                    {
                        return orderDetail.ListPrice;
                    }

                case CustomerPricingSetting.CustomerPrice:
                    {
                        return orderDetail.Price;
                    }

                case CustomerPricingSetting.NoPrice:
                    {
                        return 0M;
                    }

                default:
                    {
                        return 0M;
                    }
            }
        }

        private ActionResult GenerateShipmentPDF(OrderProcess shipment)
        {
            var xml = this.CreateShipmentXML(shipment);
            string xsltFile = $"{AppDomain.CurrentDomain.BaseDirectory}Content\\XSL\\ShipmentPDF.xslt";

            return GetPdfFromHtml(this.TransformXmlToString(xml, xsltFile), "Delivery Ticket");
        }

        private XElement CreateShipmentXML(OrderProcess shipment)
        {
            CustomerPricingSetting customerPricingSetting = shipment.OrderHeader.Customer.PricingSetting.ToEnum(CustomerPricingSetting.ListPrice);

            var billingAddress = shipment.OrderHeader.Address;
            var shippingAddress = shipment.OrderHeader.Address1;
            decimal subTotal = GetSubtotalBasedOnPricingSetting(customerPricingSetting, shipment);
            decimal taxableTotal = subTotal * (shipment.TaxPercent ?? 0) / 100;
            decimal grandTotal = subTotal + taxableTotal;
            bool isDisplayTaxField = (shipment.TaxPercent ?? 0) > 0;

            return new XElement("ShipmentData",
                            new XElement("Date", shipment.DateShipped?.ToShortDateString()),
                            new XElement("OrderNumber", shipment.OrderId),
                            new XElement("ShipmentId", shipment.OrderProcessId),
                            new XElement("TaxPercentage", shipment.TaxPercent),
                            new XElement("SubTotal", subTotal.FormatCurrency()),
                            new XElement("Tax", taxableTotal.FormatCurrency()),
                            new XElement("Total", grandTotal.FormatCurrency()),
                            new XElement("IsPriceToBeDisplayed", customerPricingSetting.Equals(CustomerPricingSetting.NoPrice) == false),
                            new XElement("IsDisplayTaxField", isDisplayTaxField),
                            new XElement("PoNumber", shipment.PoNumber),
                            new XElement("ShippingVia", shipment.ShippingVia),
                            new XElement("TrackingInfo", shipment.TrackingId),
                            new XElement("LogoUrl", $"{AppDomain.CurrentDomain.BaseDirectory}Content\\images\\dw_logo_new.png"),
                            new XElement("DeepwellAddress",
                                new XElement("WellName", "Deepwell Energy Services, LLC"),
                                new XElement("AddressLine2", "Drilling Fluids, USA"),
                                new XElement("AddressLine3", "4025 Highway 35"),
                                new XElement("AddressLine4", "Columbia, MS 39429")),
                            new XElement("BillToAddress",
                                new XElement("WellName", billingAddress.WellName),
                                new XElement("City", billingAddress.City),
                                new XElement("State", billingAddress.State.StateName),
                                new XElement("PostalCode", billingAddress.PostalCode),
                                new XElement("County", billingAddress.County)),
                            new XElement("ShipToAddress",
                                new XElement("WellName", shippingAddress.WellName),
                                new XElement("City", billingAddress.City),
                                new XElement("State", shippingAddress.State.StateName),
                                new XElement("PostalCode", shippingAddress.PostalCode),
                                new XElement("County", shippingAddress.County)),
                            new XElement("Items",
                            shipment.OrderProcessDetails.Select(i =>
                                new XElement("Item",
                                    new XElement("ProductNumber", i.OrderDetail.ProductNumber),
                                    new XElement("Description", i.OrderDetail.ProductName),
                                    new XElement("Price", this.GetPriceToDisplay(customerPricingSetting, i.OrderDetail).FormatCurrency()),
                                    new XElement("Uom", i.OrderDetail.Product.UOM),
                                    new XElement("Shipped", i.OrderDetail.ShippedQuantity))
                            )));
        }

        private XElement CreateCreditMemoXML(OrderProcess creditMemo)
        {
            var billingAddress = creditMemo.OrderHeader.Address;
            var shippingAddress = creditMemo.OrderHeader.Address1;

            decimal subTotal = creditMemo.OrderProcessDetails.Sum(i => i.OrderDetail.Quantity * i.OrderDetail.Price);
            decimal taxableTotal = (creditMemo.TaxPercent ?? 0M) * subTotal / 100;
            decimal grandTotal = taxableTotal + subTotal;

            bool isDisplayTaxField = (creditMemo.TaxPercent ?? 0) > 0;

            CustomerPricingSetting customerPricingSetting = creditMemo.OrderHeader.Customer.PricingSetting.ToEnum(CustomerPricingSetting.ListPrice);

            return new XElement("InvoiceData",
                            new XElement("Date", creditMemo.DateReturned?.ToShortDateString()),
                            new XElement("InvoiceNumber", creditMemo.OrderProcessId),
                            new XElement("Terms", ""),
                            new XElement("DueDate", creditMemo.DateReturned?.ToShortDateString()),
                            new XElement("SubTotal", (subTotal*-1).FormatCurrency()),
                            new XElement("Tax", taxableTotal.FormatCurrency()),
                            new XElement("IsPriceToBeDisplayed", customerPricingSetting.Equals(CustomerPricingSetting.NoPrice) == false),
                            new XElement("IsDisplayTaxField", isDisplayTaxField),
                            new XElement("Total", (grandTotal*-1).FormatCurrency()),
                            new XElement("LogoUrl", $"{AppDomain.CurrentDomain.BaseDirectory}Content\\images\\dw_logo_new.png"),
                            new XElement("DeepwellAddress",
                                new XElement("WellName", "Deepwell Energy Services, LLC"),
                                new XElement("AddressLine2", "Drilling Fluids, USA"),
                                new XElement("AddressLine3", "4025 Highway 35"),
                                new XElement("AddressLine4", "Columbia, MS 39429")),
                            new XElement("BillToAddress",
                                new XElement("WellName", billingAddress.WellName),
                                new XElement("City", billingAddress.City),
                                new XElement("State", billingAddress.State.StateName),
                                new XElement("PostalCode", billingAddress.PostalCode),
                                new XElement("County", billingAddress.County)),
                            new XElement("ShipToAddress",
                                new XElement("WellName", shippingAddress.WellName),
                                new XElement("City", billingAddress.City),
                                new XElement("State", shippingAddress.State.StateName),
                                new XElement("PostalCode", shippingAddress.PostalCode),
                                new XElement("County", shippingAddress.County)),
                            new XElement("Invoices",
                            creditMemo.OrderProcessDetails.Select(i =>
                                new XElement("Invoice",
                                    new XElement("ProductNumber", i.OrderDetail.ProductNumber),
                                    new XElement("Description", i.OrderDetail.ProductName),
                                    new XElement("Quantity", i.OrderDetail.Quantity),
                                    new XElement("Rate", this.GetPriceToDisplay(customerPricingSetting, i.OrderDetail).FormatCurrency()),
                                    new XElement("Uom", i.OrderDetail.Product.UOM),
                                    new XElement("Amount", (i.OrderDetail.Quantity * i.OrderDetail.Price).FormatCurrency()))
                            )));
        }

        private string TransformXmlToString(XElement xml, string xsltFile)
        {
            string response = string.Empty;

            if (System.IO.File.Exists(xsltFile))
            {
                var xslt = new XslCompiledTransform();
                xslt.Load(xsltFile);

                var args = new XsltArgumentList();

                try
                {
                    using (var writer = new StringWriter())
                    {
                        xslt.Transform(xml.CreateReader(), args, writer);
                        response = writer.ToString();
                    }
                }
                catch (Exception ex)
                {
                    logger.Error(ex);
                }
            }

            return response;
        }

        private FileContentResult GetPdfFromHtml(string html, string title)
        {
            HtmlToPdf converter = new HtmlToPdf();
            PdfDocument doc = converter.ConvertHtmlString(html);

            using (MemoryStream memoryStream = new MemoryStream())
            {
                doc.DocumentInformation.Title = title;
                doc.Save(memoryStream);
                return new FileContentResult(memoryStream.ToArray(), "application/pdf");
            }
        }

        private decimal GetSubtotalBasedOnPricingSetting(CustomerPricingSetting  pricingSetting, OrderProcess order)
        {
            switch(pricingSetting)
            {
                case CustomerPricingSetting.ListPrice:
                    {
                        return order.OrderProcessDetails.Sum(i => (i.OrderDetail.ListPrice * (decimal)i.OrderDetail.ShippedQuantity));
                    }

                case CustomerPricingSetting.CustomerPrice:
                    {
                        return order.OrderProcessDetails.Sum(i => (i.OrderDetail.Price * (decimal)i.OrderDetail.ShippedQuantity));
                    }
                default:
                    {
                        return 0M;
                    }
            }
        }
    }
}