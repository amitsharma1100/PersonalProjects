using Deepwell.Common;
using Deepwell.Common.CommonModel;
using Deepwell.Common.Enum;
using Deepwell.Common.Extensions;
using Deepwell.Front.Models;
using Deepwell.Front.Models.Customer;
using Deepwell.Front.Models.User;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Deepwell.Front.Controllers
{
    public partial class CustomerController
    {
        private ResponseResult CreateCustomer(CustomerViewModel customer, string identityId)
        {
            var response = new ResponseResult();
            string role = TypeOfUser.Customer.ToString();

            try
            {
                IdentityResult result = UserManager.AddToRole(identityId, role);

                if (result.Succeeded == false)
                {
                    AddErrors(result);
                    string errorMessage = $"Could not create role {role} for User: {customer.Email}";
                    logger.Error(errorMessage);
                    response.Success = false;
                    response.ErrorMessage = errorMessage;

                    this.RollBackUserCreation(identityId);
                    return response;
                }

                CustomerAddress customerAddress = customer.Address;
                var customerRequest = new CustomerInformation
                {
                    CustomerNumber = customer.CustomerNumber,
                    CustomerType = customer.CustomerType,
                    DateCreated = DateTime.Now,
                    DateModified = DateTime.Now,
                    IdentityId = identityId,
                    PhoneNumber = customer.Phone,
                    IsTaxable = customer.IsTaxable,
                    PricingSetting = customer.CustomerPricingSettings.ToEnum(CustomerPricingSetting.ListPrice),
                    Name = customer.Name,
                    IsActive = customer.CustomerStatus.Equals(CustomerStatus.Active.ToString())
                        ? true
                        : false,
                    WellName = customerAddress.WellName,
                    StateId = Convert.ToInt32(customerAddress.StateId),
                    City = customerAddress.City,
                    County = customerAddress.County,
                    PostalCode = customerAddress.Zipcode,
                    BillingType = customer.BillingType,
                };

                response = customerRepository.AddCustomer(customerRequest);

                if (response.Success == false)
                {
                    this.RollBackUserCreation(identityId);
                }

                TempData["Message"] = DisplayText.User.CustomerSuccessfullyCreatedMessage;
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.ErrorMessage = ex.Message;
                this.RollBackUserCreation(identityId);
                logger.Error(ex);                          
            }

            return response;
        }

        private void RollBackUserCreation(string identityId)
        {
            UserManager.RemoveFromRole(identityId, TypeOfUser.Customer.ToString());
            var user = UserManager.FindById(identityId);
            UserManager.Delete(user);
        }

        /// <summary>
        ///     Creates Identity User.
        /// </summary>
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
                    ModelState.AddModelError("", DisplayText.User.CustomerEmailAlreadyExistsErrorMessage);
                    errorList.Remove(emailErrorMessage);
                    errorList.Remove(nameErrorMessage);
                }
            }

            return new IdnetityUserCreateResponse
            {
                IsSucceeded = result.Succeeded,
                Errors = errorList,
                IdentityId = user.Id
            };
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
    }
}