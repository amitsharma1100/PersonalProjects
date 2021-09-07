using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using Deepwell.Front.Models;
using Deepwell.Front.Models.User;

namespace Deepwell.Controllers
{
    public class UserController : Controller
    {
        // GET: User
        public ActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public ActionResult ResetPassword()
        {
            return View();
        }

        [HttpPost]
        public ActionResult ResetPassword(ResetPasswordViewModel model)
        {
            //if(ModelState.IsValid)
            //{
            //    MembershipUser user = Membership.GetUser(model)
            //}

            return View();
        }
    }
}