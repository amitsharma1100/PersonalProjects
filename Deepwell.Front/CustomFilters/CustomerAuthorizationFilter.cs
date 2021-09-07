using Deepwell.Common.Helpers;
using System.Web.Mvc;

namespace Deepwell.Front.CustomFilters
{
    public class CustomerAuthorizationFilter : ActionFilterAttribute, IActionFilter
    {
        void IActionFilter.OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (SessionHelper.UserId == 0)
            {
                filterContext.Result = new RedirectResult(string.Format("~/Account/Login?ReturnUrl={0}", filterContext.HttpContext.Request.Url.AbsolutePath));
            }
        }
    }
}