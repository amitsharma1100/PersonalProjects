using Deepwell.Common.CommonModel.PriceTier;
using Deepwell.Common.Enum;
using Deepwell.Common.Extensions;
using Deepwell.Front.Helpers;
using Deepwell.Front.Models.PriceTier;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace Deepwell.Front.Controllers
{
    public partial class PriceTierController
    {
        [HttpPost]
        public JsonResult AddProducts(int[] inventoryIds)
        {
            var selectedProducts = _productRepository.GetProductsByInventoryIds(inventoryIds).Select(p => new PriceTierProduct
            {
                Price = p.Price,
                ProductId = p.ProductId,
                ProductNumber = p.ProductNumber,
                ProductStatus = PriceTierProductStatus.New,
            }).ToList();

            PriceTierHelper.AddItem(selectedProducts);

            return this.Json(new
            {
                result = this.RenderPartialViewToString("_ProductsSection", PriceTierHelper.PriceTierItems)
            });
        }

        [HttpPost]
        public JsonResult RemoveProduct(int id = 0)
        {
            if (id != 0)
            {
                PriceTierHelper.RemoveItem(id);
            }

            return this.Json(new
            {
                result = this.RenderPartialViewToString("_ProductsSection", PriceTierHelper.PriceTierItems),
            });
        }

        [HttpPost]
        public JsonResult UpdatePrice(int ProductId, decimal Price)
        {
            bool response = false;
            if (ProductId > 0)
            {
                response = PriceTierHelper.UpdatePrice(ProductId, Price);
            }

            return this.Json(new
            {
                message = response
                ? "Price successfully updated"
                : "Could not update price."
            });
        }

        private IEnumerable<TierProduct> GetTierProductsFromSession()
        {
            var response = new List<TierProduct>();
            var sessionItems = PriceTierHelper.PriceTierItems;
            sessionItems.ForEach(i =>
            {
                response.Add(new TierProduct
                {
                    Price = i.Price,
                    ProductId = i.ProductId,
                    ProductNumber = i.ProductNumber,
                    Status = i.ProductStatus,
                });
            });

            return response;
        }
    }
}