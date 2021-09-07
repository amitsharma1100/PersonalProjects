namespace Deepwell.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Web.Mvc;
    using Deepwell.Common;
    using Deepwell.Common.Enum;
    using Deepwell.Common.Extensions;
    using Deepwell.Common.Helpers;
    using Deepwell.Common.Models;
    using Deepwell.Data;
    using Deepwell.Front.CustomFilters;
    using Deepwell.Front.Helpers;
    using Deepwell.Front.Models.Constants;
    using Deepwell.Front.Models.Product;
    using PagedList;

    public partial class ProductController : Controller
    {
        [HttpGet, AdminAccessFilter]
        public ViewResult MudProduct(int id = 0)
        {
            ComponentHelper.EmptySessionItems();
            var model = new AddMudProductViewModel
            {
                ComponentInfo = new ComponentViewModel
                {
                    Components = new List<ComponentItemViewModel>()
                },
                ProductId = id,
                IsActive = true,
                InventoryInformation = Common.Helpers.Utility.Locations.Select(l => new ProductInventoryViewModel
                {
                    LocationId = l.Key,
                    LocationName = l.Value,
                    IsCurrentLocation = l.Key == SessionHelper.Location
                }).ToList(),
                ProductType = TypeOfProduct.Mud,
                ViewTitle = "Add New Mud Product",
                UnitOfMeasure = UnitOfMeasure.None,
                UnitOfMeasureOptions = this.GetUomOptions(),
            };

            if (id != 0)
            {
                this.FillMudProductData(model);
            }

            return View(model);
        }

        private AddMudProductViewModel GetDefaultModel(int id = 0)
        {
            return new AddMudProductViewModel
            {
                ComponentInfo = new ComponentViewModel
                {
                    Components = ComponentHelper.Items
                },
                ProductId = id,
                IsActive = true,
                InventoryInformation = Common.Helpers.Utility.Locations.Select(l => new ProductInventoryViewModel
                {
                    LocationId = l.Key,
                    LocationName = l.Value,
                    IsCurrentLocation = l.Key == SessionHelper.Location
                }).ToList(),
                ProductType = TypeOfProduct.Mud
            };
        }

        private bool ValidateMudProduct(AddMudProductViewModel model)
        {
            bool isValid = true;
            if (ComponentHelper.Items.Count < 1)
            {
                ModelState.AddModelError("", $"Please select atleast one componenet for Mud product.");
                isValid = false;
            }

            if(model.InventoryInformation.FirstOrDefault(i => i.LocationId == SessionHelper.Location).Quantity < 1)
            {
                ModelState.AddModelError("", $"Please provide valid inventory for the Mud product.");
                isValid = false;
            }
            
            return isValid;
        }

        [HttpPost, AdminAccessFilter]
        public ActionResult MudProduct(AddMudProductViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var currentDate = DateTime.Now;
                    Product mudProduct = null;

                    if (model.IsEditMode == false)
                    {
                        if (ValidateMudProduct(model) == false)
                        {                            
                            return View(GetDefaultModel());
                        }
                        else
                        {
                            mudProduct = new Product
                            {
                                DateCreated = currentDate
                            };

                            foreach (var item in ComponentHelper.Items)
                            {
                                mudProduct.ProductComponents.Add(new ProductComponent
                                {
                                    ComponentProductId = item.ProductId,
                                    DateCreated = DateTime.Now,
                                    DateModified = DateTime.Now,
                                    MudProductId = mudProduct.ProductId,
                                    Quantity = item.Quantity
                                });
                            }
                        }
                    }
                    else
                    {
                        mudProduct = productRepository.GetById(model.ProductId);
                    }

                    mudProduct.DateModified = currentDate;
                    mudProduct.IsActive = model.IsActive;
                    mudProduct.Price = model.Price;
                    mudProduct.ProductName = model.ProductName;
                    mudProduct.ProductNumber = model.ProductNumber;
                    mudProduct.IsMudProduct = true;
                    mudProduct.UOM = model.UnitOfMeasure.ToString();

                    List<ProductInventory> productInventories;

                    if (model.IsEditMode == false)
                    {
                        productInventories = AddProductInventories(model.InventoryInformation, mudProduct);
                    }
                    else
                    {
                        productInventories = UpdateProductInventories(mudProduct.ProductId, InventoryChangeType.Transferred, model.InventoryInformation);
                    }

                    List<InventoryLog> logs = this.LogInventoryChanges(mudProduct.ProductId, model, productInventories).ToList();

                    var result = productRepository.AddProductAndInventory(mudProduct, productInventories, logs, model.IsEditMode);

                    if (result.Success && model.IsEditMode == false)
                    {
                        ReduceComponentInventories(ComponentHelper.Items, SessionHelper.LocationId, mudProduct.ProductId);
                    }

                    ComponentHelper.EmptySessionItems();

                    if (result.Success)
                    {
                        TempData["Message"] = model.IsEditMode
                            ? "Mud Product successfully updated."
                            : "Mud Product successfully added.";

                        return RedirectToAction("ManageProducts");
                    }
                    else
                    {
                        ModelState.AddModelError("", $"Unable to create the product - {result.ErrorMessage}");
                        return this.MudProduct(model.ProductId);
                    }

                }
                catch (Exception ex)
                {
                    Logger.Log.Error(ex);
                    ModelState.AddModelError("", $"Unable to create the mud product: {ex.Message}");
                    throw ex;
                }
            }
            else
            {
                return View(model);
            }

            return View(model);
        }

        [HttpPost]
        public JsonResult AddProducts(int[] productIds)
        {
            List<ComponentItemViewModel> selectedProducts = productRepository.GetProductsByIds(productIds, SessionHelper.LocationId)
                .Select(c => new ComponentItemViewModel
                {
                    ProductId = c.ProductId,
                    ProductNumber = c.ProductNumber,
                    ProductName = c.ProductName,
                    Quantity = 1,
                    QuantityAvailable = c.Quantity
                }).ToList();

            ComponentHelper.AddToExistingItems(selectedProducts);

            return this.Json(new
            {
                result = this.RenderPartialViewToString("_Components", new ComponentViewModel
                {
                    Components = ComponentHelper.Items
                })
            });
        }

        public ActionResult GetProducts(ProductSearchRequest request)
        {
            int pageNumber = request.Page;

            request.LocationId = SessionHelper.LocationId;

            var model = productRepository
                .GetProductsWithLocations(request)
                .Where(p => p.IsMudProduct == false)
                .Select(p => new ComponentItemViewModel
                {
                    ProductId = p.ProductId,
                    ProductNumber = p.ProductNumber,
                    ProductName = p.ProductName,
                    Quantity = p.Quantity
                }).ToPagedList(pageNumber, DeepwellConstants.PAGESIZE);

            return this.PartialView("_ComponentsSelctionPopup", model);
        }

        [HttpPost]
        public JsonResult RemoveItem(int productId)
        {
            try
            {
                ComponentHelper.RemoveItem(productId);
            }
            catch (Exception ex)
            {
                Logger.Log.Error(ex.Message, ex);
            }

            return this.Json(new
            {
                result = this.RenderPartialViewToString("_Components", new ComponentViewModel
                {
                    Components = ComponentHelper.Items
                })
            });
        }

        [HttpPost]
        public JsonResult UpdateQuantity(int productId, int quantity)
        {
            try
            {
                ComponentHelper.UpdateQuantity(productId, quantity);
            }
            catch (Exception ex)
            {
                Logger.Log.Error(ex.Message, ex);
            }

            return this.Json(new
            {
                result = this.RenderPartialViewToString("_Components", new ComponentViewModel
                {
                    Components = ComponentHelper.Items
                })
            });
        }

        private void FillMudProductData(AddMudProductViewModel model)
        {
            model.ViewTitle = "Edit Mud Product";
            var product = productRepository.GetById(model.ProductId);
            if (product.IsNotNull())
            {
                bool isAdmin = SessionHelper.IsUserAnAdministrator();
                model.IsActive = product.IsActive;
                model.ProductName = product.ProductName;
                model.ProductNumber = product.ProductNumber;
                model.Price = product.Price;
                model.UnitOfMeasure = product.UOM.ToEnum(UnitOfMeasure.None);
                model.UnitOfMeasureOptions = this.GetUomOptions();
                model.InventoryInformation = product.ProductInventories.Select(pi => new ProductInventoryViewModel
                {
                    LocationId = (InventoryLocation)pi.LocationId,
                    LocationName = Common.Helpers.Utility.GetLocationName((InventoryLocation)pi.LocationId),
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
                model.ComponentInfo.Components = product.ProductComponents.Select(pc => new ComponentItemViewModel
                {
                    ProductId = pc.ComponentProductId,
                    ProductName = pc.Product1.ProductName,
                    ProductNumber = pc.Product1.ProductNumber,
                    Quantity = pc.Quantity
                }).ToList();
                model.ComponentInfo.IsReadOnly = true;
            }
        }
    }
}