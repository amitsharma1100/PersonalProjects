using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Deepwell.Front.Helpers
{
    public static class Utility
    {
        public static List<SelectListItem> ActiveOptions =>
            new List<SelectListItem>
                {
                    new SelectListItem{Text = "All"},
                    new SelectListItem{Text = "Yes"},
                    new SelectListItem{Text = "No"},
                };
    }
}