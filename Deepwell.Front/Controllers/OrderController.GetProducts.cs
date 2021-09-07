namespace Deepwell.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Web.Mvc;
    using Deepwell.Common.CommonModel.Order;
    using Deepwell.Common.Extensions;
    using Deepwell.Common.Helpers;
    using Deepwell.Common.Models;
    using Deepwell.CommonModels;
    using Deepwell.Front.Helpers;
    using Deepwell.Front.Models.Constants;
    using Deepwell.Front.Models.Order;
    using PagedList;

    public partial class OrderController
    {
        public ActionResult GetProducts(ProductSearchRequest request)
        {
            int pageNumber = request.Page;

            request.LocationId = SessionHelper.LocationId;

            IEnumerable<ProductWithLocationsViewModel> model = productRepository
                .GetProductsWithLocations(request)
                .Select(p => new ProductWithLocationsViewModel
                {
                    InventoryId = p.InventoryId,
                    IsTaxable = p.IsTaxable,
                    LocationId = p.LocationId,
                    Price = p.Price,
                    ProductId = p.ProductId,
                    ProductNumber = p.ProductNumber,
                    ProductName = p.ProductName,
                    Quantity = p.Quantity,
                    QuantityAvailable = p.Quantity,
                }).ToPagedList(pageNumber, DeepwellConstants.PAGESIZE);

            return this.PartialView("_ProductSelectionPopup", model);
        }

        [HttpPost]
        public JsonResult AddProducts(int[] inventoryIds)
        {
            var selectedProducts = productRepository.GetProductsByInventoryIds(inventoryIds).ToList();

            OrderItemsHelper.AddItem(selectedProducts);

            return this.Json(new
            {
                result = this.RenderPartialViewToString("_OrderDetailItemsAdvanced", OrderItemsHelper.OrderDetailItems),
                orderSummary = this.GetOrderSummaryHtml(),
            });
        }
    }
}