using Deepwell.Common;
using Deepwell.Common.CommonModel.PriceTier;
using Deepwell.Common.Enum;
using Deepwell.Common.Extensions;
using Deepwell.Common.Helpers;
using Deepwell.Common.Models;
using Deepwell.Data;
using Deepwell.Data.Repository;
using Deepwell.Front.CustomFilters;
using Deepwell.Front.Helpers;
using Deepwell.Front.Models.Constants;
using Deepwell.Front.Models.Order;
using Deepwell.Front.Models.PriceTier;
using log4net;
using log4net.Core;
using PagedList;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Deepwell.Front.Controllers
{
    [Authorize, CustomerAuthorizationFilter]
    public partial class PriceTierController : Controller
    {
        private PriceTierRepository _priceTierRepository;
        private ProductRepository _productRepository;
        private ILog _logger;


        public PriceTierController()
        {
            if (_priceTierRepository.IsNull())
            {
                _priceTierRepository = new PriceTierRepository();
            }

            if (_productRepository.IsNull())
            {
                _productRepository = new ProductRepository();
            }

            _logger = Logger.Log;

        }

        // GET: PriceTier
        public ActionResult Index(int Page = 1)
        {
            IEnumerable<Tier> response = _priceTierRepository.GetTiers();
            var tiers = response.Select(tier =>
                new PricingTierViewModel
                {
                    TierId = tier.TierId,
                    TierName = tier.Title,
                    EditUrl = this.Url.Action("Details", new { id = tier.TierId }),
                    IsActive = tier?.IsActive ?? false,
                    IsActiveDisplay = tier?.IsActive ?? false
                        ? "Yes"
                        : "No",
                    IsAdministrator = SessionHelper.IsUserAnAdministrator(),
                }
           ).ToPagedList(Page, DeepwellConstants.PAGESIZE);

            if (TempData["Message"].IsNotNull())
            {
                ViewBag.Message = TempData["Message"].ToString();
            }

            return View(tiers);
        }

        [HttpGet, AdminAccessFilter]
        public ActionResult Details(int id = 0)
        {
            IEnumerable<Product> allActiveProducts = _productRepository.GetProducts()
                .Where(p => p.IsActive == true);
            var model = new PricingTierViewModel
            {
                TierId = id,
                Products = allActiveProducts
                .Select(p => new SelectListItem
                {
                    Text = $"{p.ProductNumber} - {p.ProductName}",
                    Value = p.ProductId.ToString()
                }),
                IsActive = true,
                SelectedProducts = new List<PriceTierProduct>(),
            };

            PriceTierHelper.EmptySessionItems();

            if (id != 0)
            {
                this.FillProducts(model);
            }

            return View(model);
        }

        public ActionResult SaveTierDetails(PricingTierViewModel model)
        {
            if (ModelState.IsValid)
            {
                bool response = model.IsEditMode
                     ? this.EditTier(model)
                     : this.AddNewTier(model);

                if (response)
                {
                    PriceTierHelper.EmptySessionItems();
                    TempData["Message"] = model.IsEditMode
                        ? DisplayText.PriceTier.PriceTierSuccessfullyUpdatedMessage
                        : DisplayText.PriceTier.PriceTierSuccessfullyCreatedMessage;
                }
                else
                {
                    TempData["Message"] = model.IsEditMode
                        ? DisplayText.PriceTier.PriceTierUpdateErrorMessage
                        : DisplayText.PriceTier.PriceTierCreateErrorMessage;
                }
            }

            return RedirectToAction("Index");
        }

        public ActionResult GetProducts(ProductSearchRequest request)
        {
            int pageNumber = request.Page;

            request.LocationId = SessionHelper.LocationId;

            IEnumerable<ProductWithLocationsViewModel> model = _productRepository
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

        [AdminAccessFilter]
        private bool AddNewTier(PricingTierViewModel model)
        {
            bool response = false;

            var request = new PriceTierModel
            {
                Title = model.TierName,
                IsActive = model.IsActive,
                TierProducts = this.GetTierProductsFromSession(),
            };

            try
            {
                response = _priceTierRepository.Add(request);
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
            }

            return response;
        }

        [AdminAccessFilter]
        private bool EditTier(PricingTierViewModel model)
        {
            var request = new PriceTierModel
            {
                TierId = model.TierId,
                Title = model.TierName,
                IsActive = model.IsActive,
                TierProducts = this.GetTierProductsFromSession(),
            };

            try
            {
                return _priceTierRepository.Edit(request);
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
                return false;
            }
        }

        private void FillProducts(PricingTierViewModel model)
        {
            Tier tier = _priceTierRepository.GetById(model.TierId);
            if (tier.IsNotNull())
            {
                model.TierName = tier.Title;
                model.SelectedProducts = PriceTierHelper.PriceTierItems = tier.TierDetails.Select(p => new PriceTierProduct
                {
                    Price = p.Price,
                    ProductId = p.ProductId,
                    ProductNumber = p.ProductNumber,
                    ProductTitle = p.Product?.ProductName ?? string.Empty,
                    ProductStatus = PriceTierProductStatus.Existing,
                }).ToList();
                model.SelectedProductIds = tier.TierDetails.Select(td => td.ProductId.ToString());
                model.IsActive = tier?.IsActive ?? false;
            }
        }
    }
}