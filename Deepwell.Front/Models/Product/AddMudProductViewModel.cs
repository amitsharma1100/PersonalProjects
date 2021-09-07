using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web.Mvc;
using Deepwell.Common.Enum;

namespace Deepwell.Front.Models.Product
{
    public class AddMudProductViewModel : AddProductViewModel
    {

        //public IEnumerable<string> SelectedProductIds { get; set; }

        //public IEnumerable<ProductSearchResponse> SelectedProducts { get; set; }

        //public IEnumerable<SelectListItem> Products { get; set; }

        public ComponentViewModel ComponentInfo { get; set; }   
    }
}
