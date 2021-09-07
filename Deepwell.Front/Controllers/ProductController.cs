using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Deepwell.Common;
using Deepwell.Common.Enum;
using Deepwell.Common.Extensions;
using Deepwell.Common.Helpers;
using Deepwell.Common.Models;
using Deepwell.Data.Repository;
using Deepwell.Front.Models.Product;
using Deepwell.Data;
using PagedList;
using Deepwell.Front.CustomFilters;
using Deepwell.Front.Models.Constants;
using Deepwell.Common.CommonModel;

namespace Deepwell.Controllers
{
    [Authorize, CustomerAuthorizationFilter]
    public partial class ProductController : Controller
    {
        private ProductRepository productRepository;

        public ProductController()
        {
            productRepository = new ProductRepository();
        }

        public ActionResult Index()
        {
            return RedirectToAction("ManageProducts");
        }

        [HttpGet, AdminAccessFilter]
        public ViewResult Add()
        {
            var model = new AddProductViewModel
            {
                IsActive = true,
                InventoryInformation = Utility.Locations.Select(l => new ProductInventoryViewModel
                {
                    LocationId = l.Key,
                    LocationName = l.Value,
                    IsCurrentLocation = l.Key == SessionHelper.Location
                }).ToList(),
                ProductType = TypeOfProduct.Individual,
                ViewTitle = "Add New Product",
                UnitOfMeasure = UnitOfMeasure.None,
                UnitOfMeasureOptions = GetUomOptions(),
            };

            return View(model);
        }

        [HttpGet, AdminAccessFilter]
        public ActionResult Edit(int id)
        {
            var model = new AddProductViewModel
            {
                ProductType = TypeOfProduct.Individual,
                ViewTitle = "Edit Product",
                UnitOfMeasureOptions = GetUomOptions(),
            };

            var product = productRepository.GetById(id);
            if (product != null)
            {
                model.IsActive = product.IsActive;
                model.Price = product.Price;
                model.ProductName = product.ProductName;
                model.ProductId = product.ProductId;
                model.ProductNumber = product.ProductNumber;
                model.UnitOfMeasure = product.UOM.ToEnum(UnitOfMeasure.None);
                model.InventoryInformation = product.ProductInventories.Select(pi => new ProductInventoryViewModel
                {
                    LocationId = (InventoryLocation)pi.LocationId,
                    LocationName = Utility.GetLocationName((InventoryLocation)pi.LocationId),
                    Quantity = pi.Quantity.Value,
                    ToLocationId = (InventoryLocation)pi.LocationId == InventoryLocation.One ? InventoryLocation.Two : InventoryLocation.One,
                    IsCurrentLocation = (InventoryLocation)pi.LocationId == SessionHelper.Location
                }).ToList();

                model.InventoryInformation.ForEach(item =>
                {
                    var logInitial = product.InventoryLogs
                    .FirstOrDefault(il => il.LocationId == (int)item.LocationId
                    && il.ProductId == product.ProductId
                    && il.ChangeType == InventoryChangeType.Created.ToString());

                    if (logInitial.IsNotNull())
                    {
                        item.InitialInventory = logInitial.QuantityAffected;
                    }

                    var logLastUpdated = product.InventoryLogs
                    .OrderByDescending(il => il.DateCreated)
                    .FirstOrDefault(il => il.LocationId == (int)item.LocationId
                    && il.ProductId == product.ProductId);

                    if (logLastUpdated.IsNotNull())
                    {
                        item.InventoryLastUpdatedDate = logLastUpdated.DateCreated.ToString();
                    }

                });
            }
            else
            {
                return RedirectToAction("ManageProducts");
            }

            return View("Add", model);
        }

        [HttpPost, AdminAccessFilter]
        public ActionResult Add(AddProductViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    if (this.AreInventoriesInValid(model.InventoryInformation, model.ChangeType))
                    {
                        ModelState.AddModelError("", "Calculated inventories are not valid. You cannot have inventories in negative values.");
                        return this.Edit(model.ProductId);
                    }

                    ResponseResult result = this.AddUpdateProduct(model);
                    if (result.Success)
                    {
                        TempData["Message"] = model.IsEditMode
                            ? "Product successfully updated."
                            : "Product successfully added.";

                        return RedirectToAction("ManageProducts");
                    }
                    else
                    {
                        ModelState.AddModelError("", $"Unable to create the product - {result.ErrorMessage}");
                    }
                }
                catch (Exception ex)
                {
                    Logger.Log.Error(ex);
                    ModelState.AddModelError("", $"Unable to create the product: {ex.Message}");
                }
            }

            return View(model);
        }

        [HttpGet, AdminAccessFilter]
        public JsonResult Delete(int id)
        {
            bool response = false;
            var message = string.Empty;

            if (id == 0)
            {
                message = "Could not delete invalid product";
            }
            else
            {
                try
                {
                    response = productRepository.Remove(id);
                }
                catch (Exception ex)
                {
                    Logger.Log.Error(ex);
                    message = "An error occured while deleting the product";
                }
            }

            return Json(new { message = message, success = response }, JsonRequestBehavior.AllowGet);
        }

        private ResponseResult AddUpdateProduct(AddProductViewModel model)
        {
            var product = new Product
            {
                DateModified = DateTime.Now,
                IsActive = model.IsActive,
                Price = model.Price,
                ProductId = model.ProductId,
                ProductNumber = model.ProductNumber,
                ProductName = model.ProductName,
                UOM = model.UnitOfMeasure.IsNull()
                    ? UnitOfMeasure.None.ToString()
                    : model.UnitOfMeasure.ToString(),
            };

            List<ProductInventory> productInventories;

            if (model.IsEditMode == false)
            {
                productInventories = AddProductInventories(model.InventoryInformation, product);
            }
            else
            {
                productInventories = UpdateProductInventories(product.ProductId, model.ChangeType, model.InventoryInformation);
            }

            List<InventoryLog> logs = this.LogInventoryChanges(product.ProductId, model, productInventories).ToList();

            return productRepository.AddProductAndInventory(product, productInventories, logs, model.IsEditMode);
        }

        private bool AreInventoriesInValid(List<ProductInventoryViewModel> inventoryInformation, InventoryChangeType changeType)
        {
            bool isInValid = false;

            if (inventoryInformation.Count > 0 && (changeType == InventoryChangeType.Decreased || changeType == InventoryChangeType.Transferred))
            {
                switch (changeType)
                {
                    case InventoryChangeType.Decreased:
                        isInValid = inventoryInformation.Any(ii => ii.Quantity - ii.QuantityDecreasedBy < 0);
                        break;
                    case InventoryChangeType.Transferred:
                        isInValid = inventoryInformation.Any(ii => ii.Quantity - ii.QuantityTransferred < 0);
                        break;
                }
            }

            return isInValid;
        }

        private static List<ProductInventory> AddProductInventories(List<ProductInventoryViewModel> inventoryInformation, Product product)
        {
            List<ProductInventory> productInventories;
            product.DateCreated = DateTime.Now;
            productInventories = inventoryInformation.Select(inv => new ProductInventory
            {
                DateCreated = DateTime.Now,
                DateModified = DateTime.Now,
                LocationId = (int)inv.LocationId,
                Quantity = inv.Quantity,
                Remarks = inv.Remarks
            }).ToList();

            return productInventories;
        }

        private void ReduceComponentInventories(
            List<ComponentItemViewModel> componentItems,
            int locationId , int mudProductId)
        {
            int[] productIds = componentItems.Select(ci => ci.ProductId).ToArray();
            var componentInventories = productRepository.GetInventoryByProductIdsAndLocation(productIds, locationId).ToList();

            componentInventories.ForEach(dbInv =>
            {
                dbInv.Quantity = dbInv.Quantity - componentItems.FirstOrDefault(ci => ci.ProductId == dbInv.ProductId).Quantity;
            });

            var inventoryLogsForComponents = componentItems
                                               .Select(pi => new InventoryLog
                                               {
                                                   Action = InventoryAction.Add.ToString(),
                                                   ChangeType = InventoryChangeType.AllocatedToMud.ToString(),
                                                   DateCreated = DateTime.Now,
                                                   LocationId = locationId,
                                                   ProductId = pi.ProductId,
                                                   QuantityAffected = pi.Quantity,
                                                   Remarks = string.Concat("Quantity allocated to Mud Product with Id:", mudProductId),
                                                   Source = InventorySource.Product.ToString(),
                                                   UserId = SessionHelper.UserId
                                               });


            productRepository.SaveInventoryLogs(inventoryLogsForComponents);

            productRepository.SaveChanges();
        }

        private static List<ProductInventory> AddProductInventoriesAndReduceComponentInventory(List<ProductInventoryViewModel> inventoryInformation, Product product)
        {
            List<ProductInventory> productInventories;
            product.DateCreated = DateTime.Now;
            productInventories = inventoryInformation.Select(inv => new ProductInventory
            {
                DateCreated = DateTime.Now,
                DateModified = DateTime.Now,
                LocationId = (int)inv.LocationId,
                Quantity = inv.Quantity,
                Remarks = inv.Remarks
            }).ToList();

            return productInventories;
        }

        private List<ProductInventory> UpdateProductInventories(int productId, InventoryChangeType changeType, List<ProductInventoryViewModel> inventoryInformation)
        {
            List<ProductInventory> productInventories = productRepository.GetInventoryByProductId(productId).ToList();

            if (changeType != InventoryChangeType.Transferred)
            {
                productInventories.ForEach(cpi =>
                {
                    var pi = inventoryInformation.FirstOrDefault(ii => (int)ii.LocationId == cpi.LocationId);
                    int updatedQuantity = this.GetUpdatedQuantity(changeType, pi);
                    if (updatedQuantity < 0)
                    {
                        ModelState.AddModelError($"Location {pi.LocationId} InvalidQuantity", "Final quantity cannot be less than 0");
                    }

                    if (pi.IsNotNull() && updatedQuantity >= 0)
                    {
                        cpi.Quantity = updatedQuantity;
                        cpi.Remarks = pi.Remarks;
                    }
                });
            }
            else
            {
                var fromTransferInventoryInfo = inventoryInformation.FirstOrDefault(ii => ii.QuantityTransferred > 0);

                if (fromTransferInventoryInfo.IsNotNull())
                {
                    InventoryLocation fromLocation = fromTransferInventoryInfo.LocationId;
                    InventoryLocation toLocation = fromTransferInventoryInfo.ToLocationId;
                    var toTransferInventoryInfo = inventoryInformation.FirstOrDefault(ii => ii.LocationId == toLocation);

                    int fromLocationQuantity = fromTransferInventoryInfo.Quantity - fromTransferInventoryInfo.QuantityTransferred;
                    int toLocationQuantity = toTransferInventoryInfo.Quantity + fromTransferInventoryInfo.QuantityTransferred;

                    if (fromLocationQuantity < 0)
                    {
                        ModelState.AddModelError($"Location {fromLocation} InvalidQuantity", "Final quantity cannot be less than 0");
                    }
                    else
                    {
                        productInventories.ForEach(cpi =>
                        {
                            if (cpi.LocationId == (int)fromLocation)
                            {
                                cpi.Quantity = fromLocationQuantity;
                            }

                            if (cpi.LocationId == (int)toLocation)
                            {
                                cpi.Quantity = toLocationQuantity;
                            }

                            cpi.Remarks = fromTransferInventoryInfo.Remarks;
                        });
                    }
                }
            }

            return productInventories;
        }

        private List<ProductInventory> UpdateMudProductInventories(int productId, List<ProductInventoryViewModel> inventoryInformation)
        {
            List<ProductInventory> productInventories = productRepository.GetInventoryByProductId(productId).ToList();

            var fromTransferInventoryInfo = inventoryInformation.FirstOrDefault(ii => ii.QuantityTransferred > 0);

            if (fromTransferInventoryInfo.IsNotNull())
            {
                InventoryLocation fromLocation = fromTransferInventoryInfo.LocationId;
                InventoryLocation toLocation = fromTransferInventoryInfo.ToLocationId;
                var toTransferInventoryInfo = inventoryInformation.FirstOrDefault(ii => ii.LocationId == toLocation);

                int fromLocationQuantity = fromTransferInventoryInfo.Quantity - fromTransferInventoryInfo.QuantityTransferred;
                int toLocationQuantity = toTransferInventoryInfo.Quantity + fromTransferInventoryInfo.QuantityTransferred;

                if (fromLocationQuantity < 0)
                {
                    ModelState.AddModelError($"Location {fromLocation} InvalidQuantity", "Final quantity cannot be less than 0");
                }
                else
                {
                    productInventories.ForEach(cpi =>
                    {
                        if (cpi.LocationId == (int)fromLocation)
                        {
                            cpi.Quantity = fromLocationQuantity;
                        }

                        if (cpi.LocationId == (int)toLocation)
                        {
                            cpi.Quantity = toLocationQuantity;
                        }

                        cpi.Remarks = fromTransferInventoryInfo.Remarks;
                    });
                }
            }

            return productInventories;
        }

        private int GetUpdatedQuantity(InventoryChangeType changeType, ProductInventoryViewModel inventoryViewModel)
        {
            int quantity = 0;
            switch (changeType)
            {
                case InventoryChangeType.Created:
                    break;
                case InventoryChangeType.Increased:
                    quantity = inventoryViewModel.Quantity + inventoryViewModel.QuantityIncreasedBy;
                    break;
                case InventoryChangeType.Decreased:
                    quantity = inventoryViewModel.Quantity - inventoryViewModel.QuantityDecreasedBy;
                    break;
                case InventoryChangeType.Transferred:
                    break;
                default:
                    break;
            }

            return quantity;
        }

        public ViewResult ManageProducts()
        {
            var model = new ProductSearchViewModel
            {
                ProductType = new List<SelectListItem>
                {
                    new SelectListItem{Text = "All"},
                    new SelectListItem{Text = "Individual"},
                    new SelectListItem{Text = "Mud"},
                },
                Taxable = new List<SelectListItem>
                {
                    new SelectListItem{Text = "All"},
                    new SelectListItem{Text = "Yes"},
                    new SelectListItem{Text = "No"},
                },
                IsAdministrator = SessionHelper.IsUserAnAdministrator(),
            };

            if (TempData["Message"].IsNotNull())
            {
                ViewBag.Message = TempData["Message"].ToString();
            }

            return View(model);
        }

        [HttpGet]
        public PartialViewResult Search(string ProductName, string Active, string ProductType, string ProductNumber, int page = 1)
        {
            try
            {
                int pageNumber = page;

                var productRepository = new ProductRepository();
                var searchRequest = new ProductSearchRequest
                {
                    ProductNumber = ProductNumber,
                    ProductName = ProductName,
                    Active = Active.ToEnum<IsActiveOptions>(IsActiveOptions.All),
                    ProductType = ProductType.ToEnum<TypeOfProduct>(TypeOfProduct.NotSet),
                };

                var products = productRepository.SearchProducts(searchRequest);
                bool isAdmin = SessionHelper.IsUserAnAdministrator();
                var response = products.Select(product =>
                    new ProductSearchResponse
                    {
                        ProductId = product.ProductId,
                        ProductNumber = product.ProductNumber,
                        ProductName = product.ProductName,
                        Active = product.IsActive ? "Yes" : "No",
                        IsAdministrator = isAdmin,
                        ProductType = product.ProductType == TypeOfProduct.Mud
                        ? "Mud Product"
                        : "Individual Product",
                        EditUrl = product.ProductType == TypeOfProduct.Mud
                        ? this.Url.Action("MudProduct", new { id = product.ProductId })
                        : this.Url.Action("Edit", new { id = product.ProductId }),
                        IsAssociatedWithOrder = product.IsAssociatedWithAnyOrder,
                        Price = product.Price.FormatCurrency(),
                    }
               ).ToPagedList(pageNumber, DeepwellConstants.PAGESIZE);

                return PartialView("_Search", response);
            }
            catch (Exception ex)
            {
                var logger = Logger.Log;
                logger.Error(ex);
                AddErrors(new List<string> { "Could not fetch the product list" });
                return PartialView("_Search");
            }
        }

        private void AddErrors(IEnumerable<string> result)
        {
            foreach (var error in result)
            {
                ModelState.AddModelError("", error);
            }
        }

        private IEnumerable<InventoryLog> LogInventoryChanges(int productId, AddProductViewModel model, List<ProductInventory> productInventories)
        {
            IEnumerable<InventoryLog> inventoryLogs = null;
            if (model.IsEditMode == false)
            {
                inventoryLogs = productInventories.Select(pi => new InventoryLog
                {
                    Action = InventoryAction.Add.ToString(),
                    ChangeType = InventoryChangeType.Created.ToString(),
                    DateCreated = DateTime.Now,
                    InventoryId = pi.InventoryId,
                    LocationId = pi.LocationId,
                    ProductId = productId,
                    QuantityAffected = pi.Quantity.Value,
                    Remarks = "Inventory Created",
                    Source = InventorySource.Product.ToString(),
                    UserId = SessionHelper.UserId
                });
            }
            else
            {
                var inventoryAction = InventoryAction.Edit.ToString();
                if (model.ChangeType != InventoryChangeType.Transferred)
                {
                    inventoryLogs = productInventories
                        .Where(pi => this.GetQuantityAffected(model.InventoryInformation, model.ChangeType, pi.LocationId) > 0)
                        .Select(pi => new InventoryLog
                        {
                            Action = inventoryAction,
                            ChangeType = model.ChangeType.ToString(),
                            DateCreated = DateTime.Now,
                            InventoryId = pi.InventoryId,
                            LocationId = pi.LocationId,
                            ProductId = productId,
                            QuantityAffected = this.GetQuantityAffected(model.InventoryInformation, model.ChangeType, pi.LocationId),
                            Remarks = pi.Remarks,
                            Source = InventorySource.Product.ToString(),
                            UserId = SessionHelper.UserId
                        });
                }
                else
                {
                    var fromTransferInventoryInfo = model.InventoryInformation.FirstOrDefault(ii => ii.QuantityTransferred > 0);
                    if (fromTransferInventoryInfo.IsNotNull())
                    {
                        var fromLocation = (int)fromTransferInventoryInfo.LocationId;
                        var toLocation = (int)fromTransferInventoryInfo.ToLocationId;
                        var toTransferInventoryInfo = model.InventoryInformation.FirstOrDefault(ii => ii.LocationId == (InventoryLocation)toLocation);
                        var quantityAffected = fromTransferInventoryInfo.QuantityTransferred;

                        inventoryLogs = productInventories
                            .Where(pi => pi.LocationId == fromLocation)
                            .Select(pi => new InventoryLog
                            {
                                Action = inventoryAction,
                                ChangeType = model.ChangeType.ToString(),
                                DateCreated = DateTime.Now,
                                FromLocationId = pi.LocationId == fromLocation ? fromLocation : toLocation,
                                InventoryId = pi.InventoryId,
                                LocationId = pi.LocationId,
                                ProductId = productId,
                                QuantityAffected = quantityAffected,
                                Remarks = this.GetRemarksForTransferInventory(pi.Remarks, pi, fromLocation, toLocation),
                                Source = InventorySource.Product.ToString(),
                                ToLocationId = pi.LocationId == fromLocation ? toLocation : fromLocation,
                                UserId = SessionHelper.UserId
                            });

                        //inventoryLogs = productInventories.Select(pi => new InventoryLog
                        //{
                        //    Action = inventoryAction,
                        //    ChangeType = model.ChangeType.ToString(),
                        //    DateCreated = DateTime.Now,
                        //    FromLocationId = pi.LocationId.Value == fromLocation ? fromLocation : toLocation,
                        //    InventoryId = pi.InventoryId,
                        //    LocationId = pi.LocationId.Value,
                        //    ProductId = p.ProductId,
                        //    QuantityAffected = pi.LocationId.Value == fromLocation ? -quantityAffected : quantityAffected,
                        //    Remarks =  this.GetRemarksForTransferInventory(pi.Remarks, pi, fromLocation, toLocation),
                        //    Source = InventorySource.Product.ToString(),
                        //    ToLocationId = pi.LocationId.Value == fromLocation ? toLocation : fromLocation,
                        //    UserId = SessionHelper.UserId                            
                        //});

                        //inventoryLogs = productInventories.Where(pi => pi.LocationId == (int)fromLocation).Select(pi => new InventoryLog
                        //{
                        //    Action = inventoryAction,
                        //    ChangeType = model.ChangeType.ToString(),
                        //    DateCreated = DateTime.Now,
                        //    FromLocationId = pi.LocationId,
                        //    InventoryId = pi.InventoryId,
                        //    LocationId = pi.LocationId.Value,                            
                        //    ProductId = p.ProductId,
                        //    QuantityAffected = this.GetQuantityAffected(model.InventoryInformation, model.ChangeType, pi.LocationId.Value),
                        //    Remarks = pi.Remarks,
                        //    Source = InventorySource.Product.ToString(),
                        //    ToLocationId = (int)fromTransferInventoryInfo.ToLocationId,                           
                        //    UserId = SessionHelper.UserId
                        //});
                    }
                }
            }

            return inventoryLogs;
        }

        private string GetRemarksForTransferInventory(string remarks, ProductInventory productInventory, int fromLocation, int toLocation)
        {
            string message = string.Empty;

            //if (productInventory.LocationId == fromLocation)
            //{
            //    message = $"Transferred to: {toLocation} - {remarks}";
            //}

            //if (productInventory.LocationId == toLocation)
            //{
            //    message = $"Transferred from: {fromLocation} - {remarks}";
            //}

            message = $"Transferred from Location: {fromLocation} to Location: {toLocation}";
            if (remarks.HasValue())
            {
                message += ($" - {remarks}");
            }

            return message;
        }

        private int GetQuantityAffected(List<ProductInventoryViewModel> inventoryInformation, InventoryChangeType changeType, int locationId)
        {
            int quantityAffected = 0;
            var inventory = inventoryInformation.FirstOrDefault(ii => ii.LocationId == (InventoryLocation)locationId);
            if (inventory.IsNotNull())
            {
                switch (changeType)
                {
                    case InventoryChangeType.Created:
                        quantityAffected = inventory.Quantity;
                        break;
                    case InventoryChangeType.Increased:
                        quantityAffected = inventory.QuantityIncreasedBy;
                        break;
                    case InventoryChangeType.Decreased:
                        quantityAffected = inventory.QuantityDecreasedBy;
                        break;
                    case InventoryChangeType.Transferred:
                        quantityAffected = inventory.QuantityTransferred;
                        break;
                }
            }

            return quantityAffected;
        }

        private IEnumerable<SelectListItem> GetUomOptions()
        {
            var uomOptions = new List<SelectListItem>();

            var uomList = Enum.GetValues(typeof(UnitOfMeasure)).Cast<UnitOfMeasure>();

            uomList.ToList().ForEach(os =>
            {
                uomOptions.Add(new SelectListItem
                {
                    Text = os.ToString(),
                    Value = os.ToString(),
                });
            });

            return uomOptions;
        }
    }
}