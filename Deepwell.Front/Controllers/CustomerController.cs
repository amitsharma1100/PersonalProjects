using Deepwell.Common;
using Deepwell.Common.CommonModel;
using Deepwell.Common.Enum;
using Deepwell.Common.Extensions;
using Deepwell.Common.Helpers;
using Deepwell.Data;
using Deepwell.Data.Repository;
using Deepwell.Front.CustomFilters;
using Deepwell.Front.Models.Constants;
using Deepwell.Front.Models.Customer;
using Deepwell.Front.Models.User;
using log4net;
using Microsoft.AspNet.Identity.Owin;
using PagedList;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Deepwell.Front.Controllers
{
    [Authorize, CustomerAuthorizationFilter]
    public partial class CustomerController : Controller
    {
        private ApplicationUserManager _userManager;
        private CustomerRepository customerRepository;
        ILog logger;

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

        public CustomerController()
        {
            customerRepository = new CustomerRepository();
            logger = Logger.Log;
        }

        // GET: Customer
        public ActionResult Index()
        {
            var model = new CustomerSearchModel
            {
                IsAdministrator = SessionHelper.IsUserAnAdministrator(),
                Taxable = new List<SelectListItem>
                {
                    new SelectListItem{Text = "All"},
                    new SelectListItem{Text = "Yes"},
                    new SelectListItem{Text = "No"},
                },
            };

            if (TempData["Message"].IsNotNull())
            {
                ViewBag.Message = TempData["Message"].ToString();
            }

            return View(model);
        }

        [AdminAccessFilter]
        public ViewResult Add()
        {
            return View(this.GetModelWithStates());
        }

        [HttpPost, AdminAccessFilter]
        public ActionResult Add(CustomerViewModel customer)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    IdnetityUserCreateResponse response = CreateIdentityUser(customer.Email);
                    List<string> errorList = response.Errors.ToList();

                    if (response.IsSucceeded)
                    {
                        ResponseResult result = this.CreateCustomer(customer, response.IdentityId);
                        if (result.Success)
                        {
                            TempData["Message"] = result.Message;
                            return RedirectToAction("Index");
                        }
                        else
                        {
                            errorList.Add(result.ErrorMessage);
                        }
                    }

                    AddErrors(errorList);
                }
                catch (Exception ex)
                {
                    logger.Error(ex);
                    ModelState.AddModelError("", DisplayText.User.CustomerCreationErrorMessage);
                }
            }

            return View(this.GetModelWithStates());
        }

        [HttpGet]
        public PartialViewResult Search(string CustomerNumber, string Name, string Status, string Type, int Page, string Taxable)
        {
            try
            {
                int pageNumber = Page == 0
                    ? 1
                    : Page;

                var searchRequest = new CustomerSearchRequest
                {
                    CustomerNumber = CustomerNumber,
                    Name = Name,
                    Status = Status.ToEnum(CustomerStatus.All),
                    Type = Type.ToEnum(CustomerType.All),
                    Taxable = Taxable,
                };

                var searchResponse = customerRepository.Search(searchRequest);

                var IsAdmin = SessionHelper.IsUserAnAdministrator();
                var customers = searchResponse.Select(customer =>
                    new CustomerSearchModel
                    {
                        CustomerID = customer.UserId,
                        CustomerNumber = customer.CustomerNumber,
                        IsAdministrator = IsAdmin,
                        Name = customer.Name,
                        Type = customer.CustomerType.ToEnum(CustomerType.All),
                        Status = customer?.IsActive ?? false
                            ? CustomerStatus.Active
                            : CustomerStatus.Inactive,

                    }
               ).ToPagedList(pageNumber, DeepwellConstants.PAGESIZE);

                return PartialView("_Search", customers);
            }
            catch (Exception ex)
            {
                var logger = Logger.Log;
                logger.Error(ex);
                AddErrors(new List<string> { "Could not fetch the customer list" });
                return PartialView("_Search");
            }
        }

        [HttpGet, AdminAccessFilter]
        public ActionResult Edit(int id)
        {
            var model = this.GetModelWithStates();

            var customer = customerRepository.GetById(id);
            if (customer.IsNotNull())
            {
                model.CustomerId = customer.UserId;
                model.CustomerNumber = customer.CustomerNumber;
                model.CustomerStatus = customer?.IsActive ?? false
                    ? "Active"
                    : "Inactive";
                model.CustomerType = customer.CustomerType;
                model.Email = customer.AspNetUser.Email;
                model.Name = customer.Name;
                model.Phone = customer.PhoneNumber;
                model.BillingType = customer.BillingType;
                model.IsTaxable = customer.IsTaxable;
                model.CustomerPricingSettings = customer.PricingSetting;

                var customerAddress = customer.Address;
                model.Address = new CustomerAddress
                {
                    WellName = customerAddress.WellName,
                    City = customerAddress.City,
                    StateId = customerAddress.StateId.ToString(),
                    States = this.GetStates(),
                    Zipcode = customerAddress.PostalCode,
                    County = customerAddress.County,
                };
            }
            else
            {
                return RedirectToAction("Index");
            }

            return View("Edit", model);
        }

        [HttpPost, AdminAccessFilter]
        public ActionResult Edit(CustomerViewModel customer)
        {
            string response = string.Empty;
            var customerAddress = customer.Address;

            var customerRequest = new CustomerInformation
            {
                UserId = customer.CustomerId,
                CustomerNumber = customer.CustomerNumber,
                Name = customer.Name,
                Email = customer.Email,
                IsActive = customer.CustomerStatus.Equals(CustomerStatus.Active.ToString())
                    ? true
                    : false,
                CustomerType = customer.CustomerType,
                DateModified = DateTime.Now,
                PhoneNumber = customer.Phone,
                WellName = customerAddress.WellName,
                PostalCode = customerAddress.Zipcode,
                City = customerAddress.City,
                StateId = Convert.ToInt32(customerAddress.StateId),
                County = customerAddress.County,
                BillingType = customer.BillingType,
                IsTaxable = customer.IsTaxable,
                PricingSetting = customer.CustomerPricingSettings.ToEnum(CustomerPricingSetting.ListPrice),
            };

            try
            {
                response = customerRepository.Edit(customerRequest);
                if (response.HasValue())
                {
                    AddErrors(new List<string> { response });
                    customer.Address.States = this.GetStates();
                    customer.BillingTypeOptions = new List<SelectListItem>
                     {
                            new SelectListItem{Text = "Detail"},
                            new SelectListItem{Text = "Summary"},
                     };

                    customer.CustomerStatusOptions = new List<SelectListItem>
                    {
                        new SelectListItem{Text = "Active"},
                        new SelectListItem{Text = "Inactive"},
                    };

                    customer.CustomerTypeOptions = new List<SelectListItem>
                    {
                        new SelectListItem{Text = "Retail"},
                        new SelectListItem{Text = "Vendor"},
                    };

                    customer.CustomerPricingSettingsOptions = new List<SelectListItem>
                    {
                        new SelectListItem
                        {
                            Text = CustomerPricingSetting.ListPrice.ToDescription(),
                            Value = CustomerPricingSetting.ListPrice.ToString(),
                        },
                        new SelectListItem
                        {
                            Text = CustomerPricingSetting.CustomerPrice.ToDescription(),
                            Value = CustomerPricingSetting.CustomerPrice.ToString(),
                        },
                        new SelectListItem
                        {
                            Text = CustomerPricingSetting.NoPrice.ToDescription(),
                            Value = CustomerPricingSetting.NoPrice.ToString(),
                        }
                    };

                    return View(customer);
                }
                else
                {
                    TempData["Message"] = DisplayText.User.CustomerSuccessfullyUpdatedMessage;
                    return RedirectToAction("Index");
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                AddErrors(new List<string> { response });
                return View(customer);
            }
        }

        public ActionResult LoginPage()
        {
            return View();
        }

        private CustomerViewModel GetModelWithStates()
        {
            return new CustomerViewModel
            {
                Address = new CustomerAddress
                {
                    States = this.GetStates(),
                },
                BillingTypeOptions = new List<SelectListItem>
                {
                    new SelectListItem{Text = "Detail"},
                    new SelectListItem{Text = "Summary"},
                },
                CustomerStatusOptions = new List<SelectListItem>
                {
                    new SelectListItem{Text = "Active"},
                    new SelectListItem{Text = "Inactive"},
                },
                CustomerTypeOptions = new List<SelectListItem>
                {
                    new SelectListItem{Text = "Retail"},
                    new SelectListItem{Text = "Vendor"},
                },
                CustomerPricingSettingsOptions = new List<SelectListItem>
                {
                        new SelectListItem
                        {
                            Text = CustomerPricingSetting.ListPrice.ToDescription(),
                            Value = CustomerPricingSetting.ListPrice.ToString(),
                        },
                        new SelectListItem
                        {
                            Text = CustomerPricingSetting.CustomerPrice.ToDescription(),
                            Value = CustomerPricingSetting.CustomerPrice.ToString(),
                        },
                        new SelectListItem
                        {
                            Text = CustomerPricingSetting.NoPrice.ToDescription(),
                            Value = CustomerPricingSetting.NoPrice.ToString(),
                        }
                },
            };
        }

        private List<SelectListItem> GetStates()
        {
            var allStates = customerRepository.GetStates();
            var stateList = new List<SelectListItem>();
            foreach (State state in allStates)
            {
                stateList.Add(
                    new SelectListItem
                    {
                        Text = state.StateName,
                        Value = state.StateID.ToString(),
                    });
            }

            return stateList;
        }
    }
}