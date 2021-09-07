using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Deepwell.Common.Helpers;

namespace Deepwell.Front.CustomFilters
{
    public class AdminAccessFilter : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (SessionHelper.IsUserAnAdministrator() == false)
            {
                filterContext.Result = new RedirectToRouteResult(new RouteValueDictionary{{ "controller", "Home" },
                                          { "action", "Index" }

                                         });
            }

            base.OnActionExecuting(filterContext);
        }
    }
}