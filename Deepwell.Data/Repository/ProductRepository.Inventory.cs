using Deepwell.Common.CommonModel.Product;
using Deepwell.Common.Enum;
using Deepwell.Common.Extensions;
using Deepwell.Common.Helpers;
using Deepwell.Common.Models;
using Deepwell.CommonModels;
using Deepwell.Data.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Deepwell.Data.Repository
{
    public partial class ProductRepository
    {
        public IEnumerable<ProductInventory> GetInventoryByProductId(int productId)
        {
            return _deepwellContext
                .ProductInventories
                .Where(pi => pi.ProductId == productId);
        }

        public IEnumerable<ProductInventory> GetInventoryByProductIdsAndLocation(int[] productIds, int locationId)
        {
            return _deepwellContext
                .ProductInventories
                .Where(pi =>productIds.Contains(pi.ProductId) && pi.LocationId == locationId);
        }

        public IEnumerable<InventoryLogResponse> GetInventoryLogsByProduct(InventoryLogRequest request)
        {           
            IEnumerable<InventoryLogResponse> response = _deepwellContext.InventoryLogs
                .Where(il => il.ProductId == request.ProductId)
                .Join(_deepwellContext.Staffs, il => il.UserId, s => s.UserId, (il, s) => new InventoryLogResponse
                {
                    ChangeType = il.ChangeType,
                    DateCreated = il.DateCreated.Value,
                    LocationId = il.LocationId,
                    QuantityAffected = il.QuantityAffected,
                    Remarks = il.Remarks,
                    UserId = il.UserId.Value,
                    UserName = s.AspNetUser.UserName
                });

            if (request.ChangeType != InventoryChangeType.NotSet)
            {
                response = response.Where(il => il.ChangeType == request.ChangeType.ToString());
            }

            if (request.Location != InventoryLocation.NotSet)
            {
                response = response.Where(il => il.LocationId == (int)request.Location);
            }

            if (request.FromDate != DateTime.MinValue)
            {
                response = response.Where(il => il.DateCreated.Date >= request.FromDate);
            }

            if (request.ToDate != DateTime.MinValue)
            {
                response = response.Where(il => il.DateCreated.Date <= request.ToDate);
            }

            response = response.OrderByDescending(il => il.DateCreated);

            return response;
        }

        public IEnumerable<ProductWithLocations> GetProductsByInventoryIds(int[] inventoryIds)
        {
            return _deepwellContext.ProductInventories
                .Where(pi => inventoryIds.Contains(pi.InventoryId))
                .Select(pi => new ProductWithLocations
                {
                    InventoryId = pi.InventoryId,
                    ProductId = pi.Product.ProductId,
                    ProductName = pi.Product.ProductName,
                    ProductNumber = pi.Product.ProductNumber,
                    Price = pi.Product.Price,
                    LocationId = pi.LocationId,
                    Quantity = pi.Quantity.Value,
                    UnitOfMeasure = pi.Product.UOM
                });
        }

        public void SaveInventoryLogs(IEnumerable<InventoryLog> logs)
        {
            _deepwellContext.InventoryLogs.AddRange(logs);          
        }
    }
}
