using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net.Mail;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using Deepwell.Common;
using Deepwell.Common.Extensions;
using Deepwell.Data.Repository;
using Deepwell.Front.Models;

namespace Deepwell.Front.Controllers
{
    public class HomeController : Controller
    {
        private OrderRepository _orderRepository;

        public HomeController()
        {
            if (_orderRepository.IsNull())
            {
                _orderRepository = new OrderRepository();
            }
        }

        public ActionResult Index()
        {
            var response = _orderRepository.GetStats();
            var model = new InformationViewModel
            {
                OrderCount = response.OrderCount,
                OrderRevenue = 0,
                OrderTotal = response.OrderTotal
            };

            return View(model);
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }


        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
    }
}