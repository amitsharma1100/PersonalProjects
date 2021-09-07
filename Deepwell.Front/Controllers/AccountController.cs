using System;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using Deepwell.Front.Models;
using Deepwell.Data;
using Deepwell.Data.Repository;
using Deepwell.Common;
using Deepwell.Front.Models.User;
using System.Linq;
using System.Collections.Generic;
using Deepwell.Common.Helpers;
using Deepwell.Common.Enum;
using Deepwell.Common.Models;
using Deepwell.Common.Extensions;
using PagedList;
using Deepwell.Front.Models.Constants;
using Deepwell.Front.CustomFilters;

namespace Deepwell.Front.Controllers
{
    public partial class AccountController : Controller
    {
        private ApplicationSignInManager _signInManager;
        private ApplicationUserManager _userManager;
        private StaffRepository _staffRepository;

        public AccountController()
        {
            _staffRepository = new StaffRepository();
        }

        public AccountController(ApplicationUserManager userManager, ApplicationSignInManager signInManager)
        {
            UserManager = userManager;
            SignInManager = signInManager;
        }

        public ApplicationSignInManager SignInManager
        {
            get
            {
                return _signInManager ?? HttpContext.GetOwinContext().Get<ApplicationSignInManager>();
            }
            private set
            {
                _signInManager = value;
            }
        }

        public ApplicationUserManager UserManager
        {
            get
            {
                return _userManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
            private set
            {
                _userManager = value;
            }
        }

        //
        // GET: /Account/Login
        [AllowAnonymous]
        public ActionResult Login(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View(new LoginViewModel { });
        }

        //
        // POST: /Account/Login
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult Login(LoginViewModel model, string returnUrl)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = _staffRepository.GetUserByEmail(model.Email);
            if (user.IsNotNull() && user.IsActive == false)
            {
                ModelState.AddModelError("", DisplayText.User.InactiveUserLoginErrorMessage);
                return View(model);
            }

            SignInStatus result = SignInManager.PasswordSignIn(model.Email, model.Password, model.RememberMe, shouldLockout: false);

            switch (result)
            {
                case SignInStatus.Success:
                    {
                        SetUpUserInformation(model.Email, model.LocationSelected);
                        if (Request.QueryString["ReturnUrl"].HasValue())
                        {
                            return Redirect(Request.QueryString["ReturnUrl"]);
                        }
                        else
                        {
                            return RedirectToAction("Index", "Home");
                        }
                    }
                case SignInStatus.LockedOut:
                    return View("Lockout");
                case SignInStatus.RequiresVerification:
                    return RedirectToAction("SendCode", new { ReturnUrl = returnUrl, RememberMe = model.RememberMe });
                case SignInStatus.Failure:
                default:
                    ModelState.AddModelError("", "Invalid login attempt.");
                    return View(model);
            }
        }

        //
        // GET: /Account/Register
        [AllowAnonymous]
        public ActionResult Register()
        {
            return View();
        }

        //
        // POST: /Account/Register
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = new ApplicationUser { UserName = model.Email, Email = model.Email };
                var result = await UserManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
                    return RedirectToAction("Index", "Home");
                }
                AddErrors(result);
            }

            return View(model);
        }

        [AdminAccessFilter]
        public ActionResult RegisterStaff()
        {
            var model = new StaffUserViewModel
            {
                IsActive = true,
                IsAdministrator = false
            };

            return View(model);
        }

        [HttpPost, AdminAccessFilter]
        public ActionResult RegisterStaff(StaffUserViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    IdnetityUserCreateResponse response = CreateIdentityUser(model.Email);
                    IEnumerable<string> errorList = response.Errors;

                    if (response.IsSucceeded)
                    {
                        bool result = this.CreateStaffUser(model, response.IdentityId, model.Email);
                        if (result)
                        {
                            return RedirectToAction("ManageUsers", new { msgId = 1 });
                        }
                    }

                    AddErrors(errorList);
                }
                catch (Exception ex)
                {
                    var logger = Logger.Log;
                    logger.Error(ex);
                    ModelState.AddModelError("", DisplayText.User.StaffUserCreationErrorMessage);
                }
            }

            return View(model);
        }

        [HttpGet]
        public ActionResult Search(string FirstName, string LastName, string EmployeeId, string IsActive, int page)
        {
            try
            {
                int pageNumber = page == 0
                    ? 1
                    : page;

                var searchRequest = new UserSearchRequest
                {
                    EmployeeId = EmployeeId,
                    FirstName = FirstName,
                    LastName = LastName,
                    ActiveOption = IsActive.ToEnum<IsActiveOptions>(IsActiveOptions.All),
                };

                bool isAdmin = SessionHelper.IsUserAnAdministrator();

                var allUsers = _staffRepository.Search(searchRequest);
                var users = allUsers.Select(user =>
                    new SearchViewModel
                    {
                        EmployeeId = user.UserId,
                        FirstName = user.FirstName,
                        IsActive = user.IsActive.Value
                            ? "Yes"
                            : "No",
                        IsAdministrator = isAdmin,
                        LastName = user.LastName,
                    }
               ).ToPagedList(pageNumber, DeepwellConstants.PAGESIZE);

                return PartialView("_Search", users);

            }
            catch (Exception ex)
            {
                var logger = Logger.Log;
                logger.Error(ex);
                AddErrors(new List<string> { "Could not fetch the user list" });
                return PartialView("_Search");
            }
        }

        [HttpGet]
        [Authorize, CustomerAuthorizationFilter]
        public ActionResult EditStaff(int id)
        {
            if (SessionHelper.IsUserAnAdministrator())
            {
                Staff user = _staffRepository.GetUserByEmployeeId(id);

                bool isAdmin = user.AspNetUser.AspNetRoles.FirstOrDefault().Name.Equals("Administrator");
                var result = new UpdateStaffUserViewModel
                {
                    EmployeeId = user.UserId,
                    Email = user.AspNetUser.Email,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    IsActive = (bool)user.IsActive,
                    IdentityId = user.IdentityId,
                    IsAdministrator = isAdmin,
                    CurrentRole = isAdmin
                    ? TypeOfUser.Administrator.ToString()
                    : TypeOfUser.Staff.ToString(),
                };

                return View(result);
            }
            else
            {
                return RedirectToActionPermanent("ManageUsers", "Account");
            }
        }

        [HttpPost]
        [Authorize, CustomerAuthorizationFilter]
        public ActionResult EditStaff(UpdateStaffUserViewModel user)
        {
            var staff = new Staff
            {
                IsActive = user.IsActive,
                FirstName = user.FirstName,
                LastName = user.LastName,
                AspNetUser = new AspNetUser
                {
                    Email = user.Email,
                }
            };

            try
            {
                var response = _staffRepository.Edit(staff, user.IdentityId);

                bool updateRole = this.UpdateUserRole(user);

                if (response.Any())
                {
                    AddErrors(response);
                }
                else
                {
                    return RedirectToAction("ManageUsers", new { msgId = 2 });
                }
            }
            catch (Exception ex)
            {
                var logger = Logger.Log;
                logger.Error(ex);
                AddErrors(new List<string> { $"Could not update user: {user.Email}" });
            }

            return View(user);
        }

        [Authorize, CustomerAuthorizationFilter]
        public ActionResult ManageUsers(int msgId = 0)
        {
            var msg = string.Empty;
            switch (msgId)
            {
                case 0:
                default:
                    {
                        break;
                    }
                case 1:
                    {
                        msg = DisplayText.User.UserSuccessfullyCreatedMessage;
                        break;
                    }
                case 2:
                    {
                        msg = DisplayText.User.UserSuccessfullyUpdated;
                        break;
                    }
            }

            var model = new SearchViewModel
            {
                IsActiveOptions = new List<SelectListItem>
                {
                    new SelectListItem{Text = "All", Value = "All"},
                    new SelectListItem{Text = "Yes", Value = "Yes"},
                    new SelectListItem{Text = "No", Value = "No"},
                },
                IsAdministrator = SessionHelper.IsUserAnAdministrator(),
                ValidationMessage = msg,
            };

            return View(model);
        }

        //
        // GET: /Account/ConfirmEmail
        [AllowAnonymous]
        public async Task<ActionResult> ConfirmEmail(string userId, string code)
        {
            if (userId == null || code == null)
            {
                return View("Error");
            }
            var result = await UserManager.ConfirmEmailAsync(userId, code);
            return View(result.Succeeded ? "ConfirmEmail" : "Error");
        }

        //
        // GET: /Account/ForgotPassword
        [AllowAnonymous]
        public ActionResult ForgotPassword()
        {
            return View();
        }

        //
        // POST: /Account/ForgotPassword
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult ForgotPassword(ForgotPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var user = UserManager.FindByName(model.Email);
                    if (user == null || !(UserManager.IsEmailConfirmed(user.Id)))
                    {
                        ModelState.AddModelError("UserNotExist", DisplayText.User.ResetPasswordUserNotFoundError);
                        return View("ForgotPasswordConfirmation");
                    }
                    else
                    {
                        // For more information on how to enable account confirmation and password reset please visit https://go.microsoft.com/fwlink/?LinkID=320771
                        // Send an email with this link
                        string code = UserManager.GeneratePasswordResetToken(user.Id);
                        var callbackUrl = Url.Action("ResetPassword", "Account", new { userId = user.Id, code = code, email = model.Email }, protocol: Request.Url.Scheme);
                        Mailer.Send(model.Email, "Password Reset Confirmation", DisplayText.User.GetResetPasswordBody(callbackUrl));
                        return RedirectToAction("ForgotPasswordConfirmation", "Account");
                    }
                }
                catch (Exception ex)
                {
                    Logger.Log.Error(ex.Message, ex);
                    ModelState.AddModelError("Exception", ex.Message);
                }
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        //
        // GET: /Account/ForgotPasswordConfirmation
        [AllowAnonymous]
        public ActionResult ForgotPasswordConfirmation()
        {
            return View();
        }

        //
        // GET: /Account/ResetPassword
        [AllowAnonymous]
        public ActionResult ResetPassword(string code, string email = "")
        {
            var model = new ResetPasswordViewModel
            {
                Email = email
            };

            return code == null ? View("Error") : View(model);
        }

        //
        // POST: /Account/ResetPassword
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            var user = await UserManager.FindByNameAsync(model.Email);
            if (user == null)
            {
                // Don't reveal that the user does not exist
                return RedirectToAction("ResetPasswordConfirmation", "Account");
            }
            var result = await UserManager.ResetPasswordAsync(user.Id, model.Code, model.Password);
            if (result.Succeeded)
            {
                return RedirectToAction("ResetPasswordConfirmation", "Account");
            }
            AddErrors(result);
            return View();
        }

        //
        // GET: /Account/ResetPasswordConfirmation
        [AllowAnonymous]
        public ActionResult ResetPasswordConfirmation()
        {
            return View();
        }

        public ActionResult GetUserInfo()
        {
            var userInfoModel = new UserInfoModel();
            if (SessionHelper.IsUserLoggedIn)
            {
                userInfoModel.IsLoggedIn = true;
                userInfoModel.FullName = SessionHelper.FullName;
                userInfoModel.Location = Utility.GetLocationName(SessionHelper.Location);
                userInfoModel.Roll = SessionHelper.UserType.ToString();
            }

            return this.PartialView("_UserInfo", userInfoModel);
        }

        //
        // POST: /Account/LogOff
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult LogOff()
        {
            AuthenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
            SessionHelper.ClearSession();
            return RedirectToAction("Login", "Account");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_userManager != null)
                {
                    _userManager.Dispose();
                    _userManager = null;
                }

                if (_signInManager != null)
                {
                    _signInManager.Dispose();
                    _signInManager = null;
                }
            }

            base.Dispose(disposing);
        }

        #region Helpers
        // Used for XSRF protection when adding external logins
        private const string XsrfKey = "XsrfId";

        private IAuthenticationManager AuthenticationManager
        {
            get
            {
                return HttpContext.GetOwinContext().Authentication;
            }
        }

        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error);
            }
        }

        private void AddErrors(IEnumerable<string> result)
        {
            foreach (var error in result)
            {
                ModelState.AddModelError("", error);
            }
        }

        private ActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            return RedirectToAction("Index", "Home");
        }

        internal class ChallengeResult : HttpUnauthorizedResult
        {
            public ChallengeResult(string provider, string redirectUri)
                : this(provider, redirectUri, null)
            {
            }

            public ChallengeResult(string provider, string redirectUri, string userId)
            {
                LoginProvider = provider;
                RedirectUri = redirectUri;
                UserId = userId;
            }

            public string LoginProvider { get; set; }
            public string RedirectUri { get; set; }
            public string UserId { get; set; }

            public override void ExecuteResult(ControllerContext context)
            {
                var properties = new AuthenticationProperties { RedirectUri = RedirectUri };
                if (UserId != null)
                {
                    properties.Dictionary[XsrfKey] = UserId;
                }
                context.HttpContext.GetOwinContext().Authentication.Challenge(properties, LoginProvider);
            }
        }
        #endregion
    }
}