using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Deepwell.Common;
using Deepwell.Common.CommonModel.Order;
using Deepwell.Common.Enum;
using Deepwell.Common.Extensions;
using Deepwell.Data;
using Deepwell.Data.Repository;
using Deepwell.Front.CustomFilters;
using Deepwell.Front.Helpers;
using Deepwell.Front.Models.Constants;
using Deepwell.Front.Models.Customer;
using Deepwell.Front.Models.Order;
using log4net;
using PagedList;

namespace Deepwell.Controllers
{
    [Authorize, CustomerAuthorizationFilter]
    public partial class OrderController : Controller
    {
        private OrderRepository orderRepository;
        private CustomerRepository customerRepository;
        private ProductRepository productRepository;
        private PriceTierRepository priceTierRepository;
        private ILog logger;

        public OrderController()
        {
            orderRepository = new OrderRepository();
            customerRepository = new CustomerRepository();
            productRepository = new ProductRepository();
            priceTierRepository = new PriceTierRepository();
            logger = Logger.Log;
        }

        // GET: Order
        public ActionResult Index()
        {
            var model = new OrderSearchRequestModel
            {
                OrderStatusList = this.GetOrderStatusList(),
            };

            OrderItemsHelper.EmptySessionItems();

            return View(model);
        }

        [HttpGet]
        public ActionResult OrderDetail(int id = 0)
        {
            var model = new OrderDetailViewModel
            {
                BillingAddress = new CustomerAddress
                {
                    States = this.GetStates(),
                },
                CustomersList = this.GetCustomersList(),
                OrderId = id,
                OrderDate = DateTime.Now,
                ShippingAddress = new CustomerAddress
                {
                    States = this.GetStates(),
                },
                PriceTierList = this.GetTiersList(),
            };

            OrderItemsHelper.EmptySessionItems();
            PriceTierHelper.EmptySessionItems();
            OrderItemsHelper.EmptyCreditMemoSessionItems();

            if (id != 0)
            {
                this.FillOrderDetailData(model);
            }
            else
            {
                model.NewOrderId = orderRepository.GetNewOrderId();
            }

            return View(model);
        }

        private void FillOrderDetailData(OrderDetailViewModel model)
        {
            var order = orderRepository.GetOrder(model.OrderId);
            if (order.IsNotNull())
            {
                model.OrderDate = order.DateCreated;
                model.CustomerId = order.CustomerId;
                model.CustomerName = order.CustomerName;
                model.CustomerEmail = order.CustomerEmail;
                model.Phone = order.CustomerPhone;
                model.BillingAddress = this.ModelAddressFromOrderAddress(order.Address);
                model.ShippingAddress = this.ModelAddressFromOrderAddress(order.Address1);
                model.OrderStatus = order.StatusId;
                model.TierId = order?.TierId ?? 0;
                model.InvoicesList = this.GetInvoicesList(model.OrderId);
                model.ShipmentList = this.GetShipmentList(model.OrderId);
                model.CreditMemoList = this.GetCreditMemoList(model.OrderId);

                // TODO:
                model.IsTaxable = true;

                OrderItemsHelper.EmptySessionItems();
                OrderItemsHelper.AddItem(order.OrderDetails.Select(od => new OrderDetailItemViewModel
                {
                    Inventory = orderRepository.GetInventoryByProductAndLocationId(od.ProductId, od.LocationId),
                    QuantityAvailable = orderRepository.GetInventoryQuantityByProductAndLocationId(od.ProductId, od.LocationId),
                    IsTaxable = od.ProductTaxable,
                    LineNumber = od.LineNumber,
                    LocationId = od.LocationId,
                    OrderDetailId = od.OrderDetailId,
                    OrderProcessId = od.OrderProcessDetails.FirstOrDefault(op => op.OrderDetailId == od.OrderDetailId).IsNotNull()
                        ? od.OrderProcessDetails.FirstOrDefault(op => op.OrderDetailId == od.OrderDetailId).OrderProcessId
                        : 0,
                    IsInvoiced = od.OrderProcessDetails.FirstOrDefault(op => op.OrderDetailId == od.OrderDetailId).IsNotNull()
                        ? od.OrderProcessDetails.FirstOrDefault(op => op.OrderDetailId == od.OrderDetailId).OrderProcess?.IsInvoiced ?? false
                        : false,
                    ListPrice = od.ListPrice,
                    SoldPrice = od.Price,
                    ProductId = od.ProductId,
                    ProductName = od.ProductName,
                    Quantity = od.Quantity,
                    UnitOfMeasure = od.Product.UOM,
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

                model.Items = OrderItemsHelper.OrderDetailItems;

                PriceTierHelper.EmptySessionItems();
                this.UpdatePriceTierInSession(model.TierId);
            }
        }

        [HttpGet]
        public ActionResult GetCustomerDetails(int customerId)
        {
            Customer customer = customerRepository.GetById(customerId);
            if (customer.IsNotNull())
            {
                var customerAddress = customer.Address;

                var response = new
                {
                    customerName = customer.Name,
                    customerEmail = customer.AspNetUser.Email,
                    phoneNumber = customer.PhoneNumber,
                    wellName = customerAddress.WellName,
                    city = customerAddress.City,
                    county = customerAddress.County,
                    stateId = customerAddress.StateId,
                    zipcode = customerAddress.PostalCode,
                    success = true,
                };

                return Json(response, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(new { errorMessage = "Could not fetch the customer details", success = false }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public JsonResult UpdatePriceTierInSession(int tierId = 0)
        {
            bool success = false;
            var resultString = string.Empty;
            var errorMessage = string.Empty;

            try
            {
                if (tierId == 0)
                {
                    PriceTierHelper.EmptyList();
                    success = true;
                    resultString = this.RenderPartialViewToString("_OrderDetailItemsAdvanced", OrderItemsHelper.OrderDetailItems);
                }
                else
                {
                    Tier tier = priceTierRepository.GetById(tierId);
                    if (tier.IsNotNull())
                    {
                        var tierProducts = new Dictionary<int, decimal>();
                        tier.TierDetails.ToList().ForEach(td =>
                        {
                            tierProducts.Add(td.ProductId, td.Price);
                        });

                        PriceTierHelper.SetList(tierProducts);
                        success = true;
                        resultString = this.RenderPartialViewToString("_OrderDetailItemsAdvanced", OrderItemsHelper.OrderDetailItems);
                    }
                    else
                    {
                        errorMessage = "Could not fetch the tier details";
                    }
                }
            }
            catch (Exception ex)
            {
                errorMessage = "Error fetching the tier details";
                logger.Error(ex);
            }

            return this.Json(new
            {
                success = success,
                result = resultString,
                orderSummary = this.GetOrderSummaryHtml(),
                errorMessage = errorMessage,
            });
        }

        [HttpGet]
        public PartialViewResult Search(OrderSearchRequest searchRequest)
        {
            try
            {
                if (searchRequest.OrderDateFrom.IsNull())
                {
                    searchRequest.OrderDateFrom = DateTime.MinValue;
                }

                if (searchRequest.OrderDateTo.IsNull() || searchRequest.OrderDateTo == DateTime.MinValue)
                {
                    searchRequest.OrderDateTo = DateTime.MaxValue;
                }

                var orders = orderRepository.Search(searchRequest);

                var response = orders.Select(order =>
                    new OrderSearchResponseModel
                    {
                        ProductId = order.ProductId,
                        ProductNumber = order.ProductNumber,
                        ProductName = order.ProductName,
                        CustomerId = order.CustomerId,
                        CustomerNumber = order.CustomerNumber,
                        CustomerName = order.CustomerName,
                        LocationId = order.LocationId,
                        OrderDate = order.OrderDate.ToShortDateString(),
                        OrderId = order.OrderId,
                        OrderStatus = ((OrderStatus)order.OrderStatus).ToString(),
                    }
               ).ToPagedList(searchRequest.Page, DeepwellConstants.PAGESIZE);

                return PartialView("_Search", response);
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                AddErrors(new List<string> { "Could not fetch the orders list" });
                return PartialView("_Search");
            }
        }

        [HttpPost]
        public ActionResult SaveOrderDetails(OrderDetailViewModel order)
        {
            if (ModelState.IsValid)
            {
                bool response = order.IsEditMode
                ? this.SaveEditedOrder(order)
                : this.SaveNewOrder(order);

                if (response == false)
                {
                    return RedirectToAction("OrderDetail", order);
                }
                else
                {
                    OrderItemsHelper.EmptySessionItems();
                    PriceTierHelper.EmptySessionItems();
                    return RedirectToAction("Index");
                }
            }

            return View("OrderDetail", order);
        }

        private IEnumerable<SelectListItem> GetOrderStatusList()
        {
            var orderStatusList = new List<SelectListItem>
            {
                new SelectListItem
                {
                    Text = "All",
                    Value = "0",
                },
            };

            orderRepository.GetOrderStatus().ToList().ForEach(os =>
            {
                orderStatusList.Add(new SelectListItem
                {
                    Text = os.Title,
                    Value = os.OrderStatusId.ToString(),
                });
            });

            return orderStatusList;
        }

        private IEnumerable<SelectListItem> GetCustomersList()
        {
            return customerRepository.GetCustomersActive()
                .OrderBy(c => c.Name)
                .Select(c => new SelectListItem
                {
                    Text = $"{c.CustomerNumber} - {c.Name}",
                    Value = c.UserId.ToString()
                });
        }

        private IEnumerable<SelectListItem> GetTiersList()
        {
            var tierList = new List<SelectListItem>
            {
                new SelectListItem
                {
                    Text = "Select Tier",
                    Value = "0",
                }
            };

            priceTierRepository.GetAllActive()
                .OrderBy(t => t.Title).ToList()
                .ForEach(t =>
                {
                    tierList.Add(
                        new SelectListItem
                        {
                            Text = t.Title,
                            Value = t.TierId.ToString(),
                        });
                });

            return tierList;
        }

        private IEnumerable<SelectListItem> GetInvoicesList(int orderId)
        {
            List<SelectListItem> invoicesList = orderRepository.GetInvoices(orderId)
                .Select(i => new SelectListItem
                {
                    Text = $"Invoice ({i.OrderProcessId}) - {i.DateInvoiced?.ToString("MM/dd/yyyy")}",
                    Value = i.OrderProcessId.ToString(),
                }).ToList();

            if (invoicesList.Any())
            {
                invoicesList.Insert(0, new SelectListItem
                {
                    Text = "Select to view",
                    Value = "0",
                });
            }

            return invoicesList;
        }

        private IEnumerable<SelectListItem> GetShipmentList(int orderId)
        {
            List<SelectListItem> shipmentList = orderRepository.GetShipping(orderId)
                .Where(i => i.Total > 0)
                .Select(i => new SelectListItem
                {
                    Text = $"Shipment ({i.OrderProcessId}) - {i.DateShipped?.ToString("MM/dd/yyyy")}",
                    Value = i.OrderProcessId.ToString(),
                }).ToList();

            if (shipmentList.Any())
            {
                shipmentList.Insert(0, new SelectListItem
                {
                    Text = "Select to view",
                    Value = "0",
                });
            }

            return shipmentList;
        }

        private IEnumerable<SelectListItem> GetCreditMemoList(int orderId)
        {
            List<SelectListItem> creditMemoList = orderRepository.GetCreditMemo(orderId)
                .Select(i => new SelectListItem
                {
                    Text = $"Credit Memo ({i.OrderProcessId}) - {i.DateReturned?.ToString("MM/dd/yyyy")}",
                    Value = i.OrderProcessId.ToString(),
                }).ToList();

            if (creditMemoList.Any())
            {
                creditMemoList.Insert(0, new SelectListItem
                {
                    Text = "Select to view",
                    Value = "0",
                });
            }

            return creditMemoList;
        }

        private List<SelectListItem> GetStates()
        {
            var allStates = customerRepository.GetStates();
            var stateList = new List<SelectListItem>();
            foreach (State state in allStates)
            {
                stateList.Add(
                    new SelectListItem
                    {
                        Text = state.StateName,
                        Value = state.StateID.ToString(),
                    });
            }

            return stateList;
        }

        private int GetReturnedQuantity(int orderDetailId)
        {
            var response = orderRepository.GetOrderDetailsByOriginalOrderDetailId(orderDetailId);
            if (response.IsNotNull())
            {
                return response
                    .Sum(i => i.OrderDetail.Quantity);
            }

            return 0;
        }

        private void AddErrors(IEnumerable<string> result)
        {
            foreach (var error in result)
            {
                ModelState.AddModelError("", error);
            }
        }

        private CustomerAddress ModelAddressFromOrderAddress(Address orderAddress)
        {
            if (orderAddress.IsNotNull())
            {
                return new CustomerAddress
                {
                    WellName = orderAddress.WellName,
                    City = orderAddress.City,
                    County = orderAddress.County,
                    Zipcode = orderAddress.PostalCode,
                    StateId = orderAddress.StateId.ToString(),
                    States = this.GetStates(),
                };
            }
            else
            {
                return new CustomerAddress();
            }
        }
    }
}