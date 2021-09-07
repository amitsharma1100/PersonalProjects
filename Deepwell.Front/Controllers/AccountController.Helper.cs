using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Deepwell.Common;
using Deepwell.Common.CommonModel;
using Deepwell.Common.Enum;
using Deepwell.Common.Extensions;
using Deepwell.Common.Helpers;
using Deepwell.Data;
using Deepwell.Data.Repository;
using Deepwell.Front.Models;
using Deepwell.Front.Models.User;
using Microsoft.AspNet.Identity;

namespace Deepwell.Front.Controllers
{
    public partial class AccountController
    {
        /// <summary>
        ///     Creates Identity User.
        /// </summary>
        /// <param name="email"></param>
        /// <returns></returns>
        private IdnetityUserCreateResponse CreateIdentityUser(string email)
        {
            email = email.Trim();
            var user = new ApplicationUser { UserName = email, Email = email, EmailConfirmed = true };
            IdentityResult result = UserManager.Create(user);
            var errorList = result.Errors.ToList();

            if (result.Succeeded == false)
            {
                var emailErrorMessage = $"Email '{email}' is already taken.";
                var nameErrorMessage = $"Name {email} is already taken.";
                bool isEmailAlreadyTaken = errorList.Where(err => err.Equals(emailErrorMessage)).Count() > 0;

                if (isEmailAlreadyTaken)
                {
                    ModelState.AddModelError("", DisplayText.User.EmailAlreadyExistsErrorMessage);
                    errorList.Remove(emailErrorMessage);
                    errorList.Remove(nameErrorMessage);
                }
            }
            else
            {
                //UserManager.SendEmail(user.Id, "Registration successful", "Reset your password buddy")
               
            }

            return new IdnetityUserCreateResponse
            {
                IsSucceeded = result.Succeeded,
                Errors = errorList,
                IdentityId = user.Id
            };
        }

        private void SetUpUserInformation(string email, InventoryLocation location)
        {
            var user = UserManager.FindByEmail(email);
           
            var identityId = user.Id;
            var role = UserManager.GetRoles(identityId).FirstOrDefault();
            var userType = TypeOfUser.NotSet;

            switch (role.ToLower())
            {
                case "administrator":
                    userType = TypeOfUser.Administrator;
                    break;
                case "customer":
                    userType = TypeOfUser.Customer;
                    break;
                case "staff":
                    userType = TypeOfUser.Staff;
                    break;
                case "vendor":
                    userType = TypeOfUser.Vendor;
                    break;
            }

            UserInformation userInfo = _staffRepository.GetUserInfoByIndentiyAndUserType(identityId, userType);

            SessionHelper.CreateSession(identityId, userInfo.UserId, userInfo.FullName, userType, location);
        }

        private bool CreateStaffUser(StaffUserViewModel staffUserViewModel, string identityId, string email)
        {
            bool isSuccess = false;
            string role = staffUserViewModel.IsAdministrator
                ? TypeOfUser.Administrator.ToString()
                : TypeOfUser.Staff.ToString();

            IdentityResult result = UserManager.AddToRole(identityId, role);
            var logger = Logger.Log;

            if (result.Succeeded == false)
            {
                AddErrors(result);
                logger.Error($"Could not create role {role} for User: {staffUserViewModel.Email}");
                return false;
            }

            var staff = new Staff
            {
                FirstName = staffUserViewModel.FirstName,
                LastName = staffUserViewModel.LastName,
                IdentityId = identityId,
                IsActive = staffUserViewModel.IsActive,
                DateCreated = DateTime.Now,
                DateModified = DateTime.Now
            };

            try
            {
                _staffRepository.AddStaffUser(staff);

                string code = UserManager.GeneratePasswordResetToken(identityId);
                var callbackUrl = Url.Action("ResetPassword", "Account", new { userId = identityId, code = code , email=email}, protocol: Request.Url.Scheme);
                string subject = string.Concat("Deepwell New Registration - ", staff.UserId);
                Mailer.Send(email, subject, DisplayText.User.GetRegsiterStaffBody(callbackUrl, email, staffUserViewModel.FirstName));
           
                isSuccess = true;
            }
            catch (Exception ex)
            {
                if (ex.InnerException?.InnerException?.Message.Contains("Violation of UNIQUE KEY constraint 'UC_EmployeeId'.") ?? false)
                {
                    ModelState.AddModelError("", DisplayText.User.EmployeeIdAlreadyExistsErrorMessage);
                }
                else
                {
                    ModelState.AddModelError("", ex.Message);
                }

                _staffRepository.Remove(identityId);
                UserManager.RemoveFromRole(identityId, TypeOfUser.Staff.ToString());
                var user = UserManager.FindById(identityId);
                UserManager.Delete(user);
                logger.Error(ex);
            }

            return isSuccess;
        }

        private bool UpdateUserRole(UpdateStaffUserViewModel user)
        {
            bool response = false;

            TypeOfUser currentRole = user.CurrentRole.ToEnum<TypeOfUser>(TypeOfUser.NotSet);
            TypeOfUser newRole = user.IsAdministrator
                ? TypeOfUser.Administrator
                : TypeOfUser.Staff;

            try
            {
                if (currentRole.Equals(newRole) == false && newRole.Equals(TypeOfUser.NotSet) == false)
                {
                    UserManager.RemoveFromRole(user.IdentityId, currentRole.ToString());
                    UserManager.AddToRole(user.IdentityId, newRole.ToString());
                    response = true;
                }
            }
            catch(Exception ex)
            {
                Logger.Log.Error(ex);
                response = false;
            }

            return response;
        }        
    }
}