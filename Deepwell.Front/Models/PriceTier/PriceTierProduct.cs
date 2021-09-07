using System.ComponentModel.DataAnnotations;
using Deepwell.Common.Enum;

namespace Deepwell.Front.Models.PriceTier
{
    public class PriceTierProduct
    {
        [Display(Name = "Product Number")]
        public string ProductNumber { get; set; }

        [Display(Name = "Product Name")]
        public string ProductTitle { get; set; }

        public int ProductId { get; set; }

        [Required(ErrorMessage = "Please provide Product Price")]
        [DataType(DataType.Currency, ErrorMessage = "Only numeric value can be entered for Standard Price")]
        [Display(Name = "Price (USD)")]
        [Range(0, int.MaxValue, ErrorMessage = "Price must be non negative")]
        [DisplayFormat(DataFormatString = " {0:n3}")]
        public decimal Price { get; set; }

        public PriceTierProductStatus ProductStatus { get; set; }
    }
}