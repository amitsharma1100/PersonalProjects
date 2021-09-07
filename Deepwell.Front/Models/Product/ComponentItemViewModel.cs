using System.ComponentModel.DataAnnotations;
using Deepwell.Common.Enum;

namespace Deepwell.Front.Models.Product
{
    public class ComponentItemViewModel
    {
        [Display(Name = "Product Number")]
        public string ProductNumber { get; set; }
        [Display(Name = "Product Name")]
        public string ProductName { get; set; }

        public int ProductId { get; set; }

        [Required(ErrorMessage = "Please provide Quantity")]
        [Display(Name = "Quantity")]
        [Range(0, int.MaxValue, ErrorMessage = "Quantity must be non negative")]       
        public int Quantity { get; set; }

        public int QuantityAvailable { get; set; }
    }
}