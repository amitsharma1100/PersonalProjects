namespace Deepwell.Data.Repository
{
    using System.Collections.Generic;
    using System.Linq;
    using Deepwell.Common.Extensions;
    using Deepwell.CommonModels;

    public partial class ProductRepository
    {
        public IEnumerable<ProductWithLocations> GetProductsByIds(int[] productIds, int locationId)
        {
            return _deepwellContext.Products
                .Where(p => productIds.Contains(p.ProductId) && p.IsActive == true && p.IsMudProduct == false)
                .Select(p => new ProductWithLocations
                {
                    ProductId = p.ProductId,
                    ProductName = p.ProductName,
                    ProductNumber = p.ProductNumber,
                    Price = p.Price,
                    LocationId = locationId,
                    Quantity = p.ProductInventories.FirstOrDefault(l => l.LocationId == locationId) != null
                        ? p.ProductInventories.FirstOrDefault(l => l.LocationId == locationId).Quantity.Value
                        : 0
                });
        }
    }
}
