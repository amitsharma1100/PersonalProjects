using Deepwell.Common;
using Deepwell.Common.CommonModel;
using Deepwell.Common.Enum;
using Deepwell.Common.Extensions;
using Deepwell.Common.Models;
using Deepwell.CommonModels;
using Deepwell.Data.ExtendedEntities;
using Deepwell.Data.Interfaces;
using System.Collections.Generic;
using System.Linq;

namespace Deepwell.Data.Repository
{
    public partial class ProductRepository : IProductRepository
    {
        private DeepwellContext _deepwellContext;

        public ProductRepository()
        {
            _deepwellContext = new DeepwellContext();
        }

        public void Add(Product p)
        {
            _deepwellContext.Products.Add(p);
            _deepwellContext.SaveChanges();
        }

        public ResponseResult AddProductAndInventory(Product p, List<ProductInventory> productInventories, List<InventoryLog> inventoryLogs, bool isEdit)
        {
            var response = new ResponseResult();

            var isNumberExist = this.IsProductWithNumberExist(p.ProductNumber, p.ProductId);

            if (isNumberExist)
            {
                response.Success = false;
                response.ErrorMessage = DisplayText.Product.ProductNumberProductAlreadyExistsErrorMessage;
                return response;
            }

            if (isEdit)
            {
                this.Edit(p);
            }
            else
            {
                //var product = this.GetByProductNumber(p.ProductNumber);
                //if (product.IsNotNull() && product.ProductNumber.HasValue())
                //{
                //    response.Success = false;
                //    response.ErrorMessage = DisplayText.Product.ProductNumberProductAlreadyExistsErrorMessage;
                //    return response;
                //}

                _deepwellContext.Products.Add(p);
                productInventories.ForEach(pi =>
                {
                    pi.ProductId = p.ProductId;
                    _deepwellContext.ProductInventories.Add(pi);
                });
            }

            _deepwellContext.InventoryLogs.AddRange(inventoryLogs);
            _deepwellContext.SaveChanges();
            return response;
        }

        public void Edit(Product p)
        {
            _deepwellContext.Entry(p).State = System.Data.Entity.EntityState.Modified;
        }

        public void EditInventory(List<ProductInventory> pi)
        {
            _deepwellContext.Entry(pi).State = System.Data.Entity.EntityState.Modified;
        }

        public bool SaveChanges()
        {
            return _deepwellContext.SaveChanges() > 0;
        }

        public Product GetByProductNumber(string productNumber)
        {
            return _deepwellContext
                .Products
                .FirstOrDefault(p => p.ProductNumber == productNumber);
        }

        public bool IsProductWithNumberExist(string productNumber, int currentProductId = 0)
        {
            return _deepwellContext
                .Products
                .Any(p => p.ProductNumber == productNumber && p.ProductId != currentProductId);
        }

        public Product GetById(int Id)
        {
            return _deepwellContext
                .Products
                .FirstOrDefault(p => p.ProductId == Id);
        }

        public IEnumerable<Product> GetProducts()
        {
            return _deepwellContext
                 .Products
                 .OrderBy(p => p.ProductName);
        }

        public IEnumerable<Product> GetActiveProducts()
        {
            return this.GetProducts().Where(p => p.IsActive == true);
        }

        public IEnumerable<Product> GetActiveIndividualProducts()
        {
            return this.GetActiveProducts().Where(p => p.IsMudProduct == false);
        }

        public bool Remove(int Id)
        {
            Product p = _deepwellContext.Products.Find(Id);
            IEnumerable<ProductInventory> productInventory = _deepwellContext.ProductInventories
                .Where(pi => pi.ProductId == Id);
            IEnumerable<InventoryLog> inventoryLogs = _deepwellContext.InventoryLogs
                .Where(il => il.ProductId == Id);

            _deepwellContext.ProductInventories.RemoveRange(productInventory);
            _deepwellContext.InventoryLogs.RemoveRange(inventoryLogs);
            _deepwellContext.Products.Remove(p);

            return _deepwellContext.SaveChanges() > 0;
        }

        public IEnumerable<ProductAndMudProduct> SearchProducts(ProductSearchRequest searchRequest)
        {
            IEnumerable<ProductAndMudProduct> response = this.GetProducts().Select(pr => new ProductAndMudProduct
            {
                IsActive = pr.IsActive,
                IsAssociatedWithAnyOrder = pr.OrderDetails.Any(),
                ProductId = pr.ProductId,
                ProductNumber = pr.ProductNumber,
                ProductName = pr.ProductName,
                ProductType = pr.IsMudProduct ? TypeOfProduct.Mud : TypeOfProduct.Individual,
                Price = pr.Price,
            });

            if (searchRequest.ProductNumber.HasValue())
            {
                response = response.Where(p => p.ProductNumber.ToLower().Contains(searchRequest.ProductNumber.ToLower()));
            }

            if (searchRequest.ProductName.HasValue())
            {
                response = response.Where(p => p.ProductName.ToLower().Contains(searchRequest.ProductName.ToLower()));
            }

            switch (searchRequest.ProductType)
            {
                case TypeOfProduct.Individual:
                    response = response.Where(p => p.ProductType == TypeOfProduct.Individual);
                    break;
                case TypeOfProduct.Mud:
                    response = response.Where(p => p.ProductType == TypeOfProduct.Mud);
                    break;
            }

            switch (searchRequest.Active)
            {
                case IsActiveOptions.Yes:
                    response = response.Where(p => p.IsActive == true);
                    break;
                case IsActiveOptions.No:
                    response = response.Where(p => p.IsActive == false);
                    break;
            }

            return response.OrderBy(p => p.ProductName);
        }


        public IEnumerable<Product> Search(ProductSearchRequest product)
        {
            var response = this.GetProducts();

            if (product.ProductNumber.HasValue())
            {
                response = response.Where(p => p.ProductNumber.ToLower().Contains((product.ProductNumber.ToLower())));
            }

            if (product.ProductName.HasValue())
            {
                response = response.Where(p => p.ProductName.ToLower().Contains(product.ProductName.ToLower()));
            }

            TypeOfProduct productType = product.ProductType;
            switch (productType)
            {
                case TypeOfProduct.NotSet:
                    {
                        break;
                    }

                case TypeOfProduct.Individual:
                    {
                        response = response.Where(p => p.IsMudProduct == false);
                        break;
                    }

                case TypeOfProduct.Mud:
                    {
                        response = response.Where(p => p.IsMudProduct == true);
                        break;
                    }

            }

            return response.OrderBy(p => p.ProductName);
        }

        public void RemoveInventoryByProductId(int productId)
        {
            _deepwellContext.ProductInventories.RemoveRange(_deepwellContext.ProductInventories.Where(pi => pi.ProductId == productId));
        }


        public IEnumerable<ProductWithLocations> GetProductsWithLocations(ProductSearchRequest request)
        {
            IEnumerable<Product> products = _deepwellContext.Products;
            if (request.ProductNumber.HasValue())
            {
                products = products.Where(p => p.ProductNumber.ToLower().Contains((request.ProductNumber.ToLower())));
            }

            if (request.ProductName.HasValue())
            {
                products = products.Where(p => p.ProductName.ToLower().Contains(request.ProductName.ToLower()));
            }

            IEnumerable<ProductInventory> productInventories = _deepwellContext.ProductInventories;
            if (request.LocationId > 0)
            {
                productInventories = _deepwellContext.ProductInventories.Where(pi => pi.LocationId == request.LocationId);
            }

            return products
                .Where(p => p.IsActive == true)
                .Join(productInventories, p => p.ProductId, pi => pi.ProductId, (p, pi) => new ProductWithLocations
                {
                    InventoryId = pi.InventoryId,
                    ProductId = p.ProductId,
                    ProductName = p.ProductName,
                    ProductNumber = p.ProductNumber,
                    Price = p.Price,
                    IsMudProduct = p.IsMudProduct,
                    LocationId = pi.LocationId,
                    Quantity = pi.Quantity.Value,
                });
        }
    }
}
