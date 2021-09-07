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
using Deepwell.Common.CommonModel.Product;
using Deepwell.Front.Models.Inventory;

namespace Deepwell.Controllers
{    
    public partial class ProductController : Controller
    {       
        public PartialViewResult InventoryLog()
        {
            try
            {
                var model = new InventoryLogViewModel();             
                
                return PartialView("_ProductInventoryLog", model);
            }
            catch (Exception ex)
            {
                var logger = Logger.Log;
                logger.Error(ex);
                AddErrors(new List<string> { "Could not fetch the product list" });
                return PartialView("_Search");
            }
        }

        [HttpGet]
        public PartialViewResult ShowLogs(InventoryLogRequest request)
        {
            try
            {  
                IEnumerable<InventoryLogResponse> inventoryLogs = productRepository.GetInventoryLogsByProduct(request);

                IEnumerable<InventoryLogListViewModel> listViewModel = inventoryLogs.Select(il => new InventoryLogListViewModel
                {
                    ChangeType = il.ChangeType,
                    DateCreated = il.DateCreated,
                    Location = Utility.GetLocationName(il.LocationId),
                    QuantityAffected = il.QuantityAffected,
                    Remarks = il.Remarks,
                    UserName = il.UserName
                });

                return PartialView("_InventoryLogList", listViewModel);
            }
            catch (Exception ex)
            {
                var logger = Logger.Log;
                logger.Error(ex);
                AddErrors(new List<string> { "Could not fetch the product list" });
                return PartialView("_Search");
            }
        }
    }
}