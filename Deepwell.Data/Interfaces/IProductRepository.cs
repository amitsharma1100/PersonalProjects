using Deepwell.Common.CommonModel;
using Deepwell.Common.CommonModel.Product;
using Deepwell.Common.Models;
using Deepwell.CommonModels;
using System.Collections.Generic;

namespace Deepwell.Data.Interfaces
{
    interface IProductRepository
    {
        void Add(Product p);
        void Edit(Product p);
        bool Remove(int Id);
        IEnumerable<Product> GetProducts();
        Product GetById(int Id);
        Product GetByProductNumber(string productNumber);
               
        ResponseResult AddProductAndInventory(Product p, List<ProductInventory> pi, List<InventoryLog> inventoryLogs, bool isEdit);

        void RemoveInventoryByProductId(int productId);
        IEnumerable<Product> Search(ProductSearchRequest product);

        IEnumerable<ProductInventory> GetInventoryByProductId(int productId);        

        IEnumerable<InventoryLogResponse> GetInventoryLogsByProduct(InventoryLogRequest request);
                
        IEnumerable<ProductWithLocations> GetProductsWithLocations(ProductSearchRequest request);

        IEnumerable<ProductWithLocations> GetProductsByInventoryIds(int[] inventoryIds);
        IEnumerable<ProductWithLocations> GetProductsByIds(int[] productIds, int locationId);
        void SaveInventoryLogs(IEnumerable<InventoryLog> logs);
    }
}
    

