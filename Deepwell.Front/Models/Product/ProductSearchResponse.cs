using System.ComponentModel.DataAnnotations;

namespace Deepwell.Front.Models.Product
{
    public class ProductSearchResponse
    {
        public int ProductId { get; set; }

        [Display(Name = "Product Number")]
        public string ProductNumber { get; set; }

        [Display(Name = "Product Name")]
        public string ProductName { get; set; }

        [Display(Name = "Taxable")]
        public string Taxable { get; set; }

        [Display(Name = "Active")]
        public string Active { get; set; }

        [Display(Name = "Product Type")]
        public string ProductType { get; set; }

        [Display(Name = "Price")]
        public string Price { get; set; }

        public string EditUrl { get; set; }

        public bool IsAssociatedWithOrder { get; set; }

        public bool IsAdministrator { get; set; }
    }
}