using Deepwell.Common;
using Deepwell.Common.CommonModel;
using Deepwell.Common.Enum;
using Deepwell.Common.Extensions;
using Deepwell.Data.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Deepwell.Data.Repository
{
    public class CustomerRepository : ICustomerRepository
    {
        private DeepwellContext _deepwellContext;

        public CustomerRepository()
        {
            _deepwellContext = new DeepwellContext();
        }

        public CustomerRepository(DeepwellContext context)
        {
            _deepwellContext = context;
        }

        public ResponseResult AddCustomer(CustomerInformation customer)
        {
            var response = new ResponseResult();

            try
            {
                Customer customerByNumber = this.GetByCustomerNumber(customer.CustomerNumber);

                if (customerByNumber.IsNotNull())
                {
                    response.ErrorMessage = DisplayText.User.CustomerNumberAlreadyExistsErrorMessage;
                    response.Success = false;
                    return response;
                }

                var address = new Address
                {
                    WellName = customer.WellName,
                    City = customer.City,
                    DateCreated = DateTime.Now,
                    DateModified = DateTime.Now,
                    StateId = customer.StateId,
                    PostalCode = customer.PostalCode,
                    County = customer.County,
                    AddressType = AddressType.Billing.ToString(),
                };

                _deepwellContext.Addresses.Add(address);

                var customerToAdd = new Customer
                {
                    CustomerNumber = customer.CustomerNumber,
                    BillingType = customer.BillingType,
                    BillingAddressId = address.AddressId,
                    CustomerType = customer.CustomerType.ToString(),
                    IdentityId = customer.IdentityId,
                    IsActive = customer.IsActive,
                    Name = customer.Name,
                    PhoneNumber = customer.PhoneNumber,
                    DateCreated = DateTime.Now,
                    DateModified = DateTime.Now,
                    IsTaxable = customer.IsTaxable,
                    PricingSetting = customer.PricingSetting.ToString(),
                };

                var result = _deepwellContext.Customers.Add(customerToAdd);
                if (_deepwellContext.SaveChanges() > 0)
                {
                    response.Message = DisplayText.User.CustomerSuccessfullyCreatedMessage;
                }
                else
                {
                    response.ErrorMessage = DisplayText.User.CustomerCreationErrorMessage;
                    response.Success = false;
                }
            }
            catch(Exception ex)
            {               
                response.ErrorMessage = DisplayText.User.CustomerCreationErrorMessage + ": " + ex.Message;
                response.Success = false;
            }

            return response;
        }

        public IEnumerable<State> GetStates()
        {
            return _deepwellContext.States
                .Where(s => s.IsActive == true)
                .OrderBy(s => s.StateName);
        }

        public IEnumerable<Customer> GetCustomers()
        {
            return _deepwellContext
                 .Customers
                 .OrderBy(c => c.Name);
        }

        public IEnumerable<Customer> GetCustomersActive()
        {
            return GetCustomers().Where(c => c.IsActive == true);
        }

        public IEnumerable<Customer> Search(CustomerSearchRequest request)
        {
            var response = this.GetCustomers();

            if (request.CustomerNumber.HasValue())
            {
                response = response.Where(c => c.CustomerNumber.Contains(request.CustomerNumber));
            }

            if (request.Name.HasValue())
            {
                response = response.Where(c => c.Name.ToLower().Contains(request.Name.ToLower()));
            }

            switch(request.Status)
            {
                case CustomerStatus.Active:
                    {
                        response = response.Where(c => c.IsActive == true);
                        break;
                    }

                case CustomerStatus.Inactive:
                    {
                        response = response.Where(c => c.IsActive == false);
                        break;
                    }
                case CustomerStatus.All:
                default:
                    {
                        break;
                    }
            }

            switch (request.Type)
            {
                case CustomerType.Retail:
                    {
                        response = response.Where(c => c.CustomerType.ToLower().Contains(CustomerType.Retail.ToString().ToLower()));
                        break;
                    }
                case CustomerType.Vendor:
                    {
                        response = response.Where(c => c.CustomerType.ToLower().Contains(CustomerType.Vendor.ToString().ToLower()));
                        break;
                    }
                case CustomerType.All:
                default:
                    {
                        break;
                    }
            }

            switch (request.Taxable)
            {
                case "All":
                default:
                    {
                        break;
                    }

                case "Yes":
                    {
                        response = response.Where(c => c.IsTaxable == true);
                        break;
                    }

                case "No":
                    {
                        response = response.Where(c => c.IsTaxable == false);
                        break;
                    }
            }

            return response;
        }

        public string Edit(CustomerInformation customerToUpdate)
        {
            try
            {
                var existingCustomer = this.GetById(customerToUpdate.UserId);

                if (existingCustomer.IsNotNull() && existingCustomer.UserId > 0)
                {
                    // Check if email is to be updated.
                    if (existingCustomer.AspNetUser.Email.IsNotEqualTo(customerToUpdate.Email))
                    {
                        var updateEmailResponse = this.UpdateEmail(ref existingCustomer, customerToUpdate.Email);
                        if (updateEmailResponse.HasValue())
                        {
                            return updateEmailResponse;
                        }
                    }

                    // Check if customer number is to be updated.
                    if (existingCustomer.CustomerNumber.IsNotEqualTo(customerToUpdate.CustomerNumber))
                    {
                        Customer customerByNumber = this.GetByCustomerNumber(customerToUpdate.CustomerNumber);
                        if (customerByNumber.IsNotNull() && customerByNumber.CustomerNumber.HasValue())
                        {
                            return DisplayText.User.CustomerNumberAlreadyExistsErrorMessage;
                        }

                        existingCustomer.CustomerNumber = customerToUpdate.CustomerNumber;
                    }

                    existingCustomer.Name = customerToUpdate.Name;
                    existingCustomer.PhoneNumber = customerToUpdate.PhoneNumber;
                    existingCustomer.CustomerType = customerToUpdate.CustomerType.ToString();
                    existingCustomer.BillingType = customerToUpdate.BillingType;
                    existingCustomer.IsActive = customerToUpdate.IsActive;
                    existingCustomer.IsTaxable = customerToUpdate.IsTaxable;
                    existingCustomer.PricingSetting = customerToUpdate.PricingSetting.ToString();

                    // Update address.
                    var currentAddress = existingCustomer.Address;
                    existingCustomer.Address.WellName = customerToUpdate.WellName;
                    existingCustomer.Address.City = customerToUpdate.City;
                    existingCustomer.Address.County = customerToUpdate.County;
                    existingCustomer.Address.StateId = customerToUpdate.StateId;
                    existingCustomer.Address.PostalCode = customerToUpdate.PostalCode;

                    existingCustomer.DateModified = DateTime.Now;
                    _deepwellContext.Entry(existingCustomer).State = System.Data.Entity.EntityState.Modified;
                    return _deepwellContext.SaveChanges() > 0
                        ? string.Empty
                        : DisplayText.User.CustomerUpdateErrorMessage;
                }
                else
                {
                    return DisplayText.User.CustomerNotFoundErrorMessage;
                }
            }
            catch(Exception ex)
            {
                return DisplayText.User.CustomerUpdateErrorMessage;
            }

        }

        public Customer GetById(int Id)
        {
            return _deepwellContext
                .Customers
                .FirstOrDefault(c => c.UserId == Id);
        }

        public Customer GetByCustomerNumber(string CustomerNumber)
        {
            return _deepwellContext
                .Customers
                .FirstOrDefault(c => c.CustomerNumber == CustomerNumber);
        }

        private Customer GetByEmail(string email)
        {
            return _deepwellContext
                .Customers
                .FirstOrDefault(c => c.AspNetUser.Email == email);
        }

        private string UpdateEmail(ref Customer existingCustomer, string newEmail)
        {
            var userByEmail = this.GetByEmail(newEmail);
            if (userByEmail.IsNotNull() && userByEmail.UserId > 0)
            {
                return DisplayText.User.CustomerEmailAlreadyExistsErrorMessage;
            }
            else
            {
                existingCustomer.AspNetUser.Email = newEmail;
                return string.Empty;
            }
        }
    }
}
