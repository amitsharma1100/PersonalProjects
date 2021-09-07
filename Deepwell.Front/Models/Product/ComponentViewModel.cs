using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Deepwell.Front.Models.Product;

namespace Deepwell.Front.Models.Product
{
    public class ComponentViewModel
    {
        public List<ComponentItemViewModel> Components { get; set; }

        public bool IsReadOnly { get; set; }
    }
}