using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using Deepwell.Common.Enum;

namespace Deepwell.Front.Models.Product
{
    public class AddProductViewModel
    {
        public int ProductId { get; set; }

        public bool IsEditMode => this.ProductId > 0;

        [Required(ErrorMessage = "Please provide Product Number")]
        [Display(Name = "Product Number:")]
        public string ProductNumber { get; set; }

        public string ViewTitle { get; set; }

        [Display(Name = "Product Type:")]
        public TypeOfProduct ProductType { get; set; }

        [Required(ErrorMessage = "Please provide Product Name")]
        [Display(Name = "Product Name:")]
        public string ProductName { get; set; }

        [Required(ErrorMessage = "Please provide Product Price")]
        [DataType(DataType.Currency, ErrorMessage = "Only numeric value can be entered for Standard Price")]
        [Display(Name = "Standard Price (USD):")]
        [Range(0, int.MaxValue, ErrorMessage = "Price must be non negative")]
        [DisplayFormat(DataFormatString = " {0:n3}")]
        public decimal Price { get; set; }

        [Display(Name = "UOM")]
        public UnitOfMeasure UnitOfMeasure { get; set; }

        public IEnumerable<SelectListItem> UnitOfMeasureOptions { get; set; }

        [Required]
        [Display(Name = "Active:")]
        public bool IsActive { get; set; }

        public List<ProductInventoryViewModel> InventoryInformation { get; set; }

        public InventoryChangeType ChangeType { get; set; }

    }
}
