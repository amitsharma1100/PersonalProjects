using System.Web;
using System.Web.Optimization;

namespace Deepwell.Front
{
    public class BundleConfig
    {
        // For more information on bundling, visit https://go.microsoft.com/fwlink/?LinkId=301862
        public static void RegisterBundles(BundleCollection bundles)
        {
            bundles.Add(new ScriptBundle("~/bundles/jquery")
                .Include("~/Scripts/jquery-{version}.js"));

            bundles.Add(new ScriptBundle("~/bundles/jqueryui")
                            .Include("~/Scripts/jquery-ui-1.12.1.js"));

            bundles.Add(new ScriptBundle("~/bundles/jqueryval").Include(
                        "~/Scripts/jquery.validate*"));

            // Use the development version of Modernizr to develop with and learn from. Then, when you're
            // ready for production, use the build tool at https://modernizr.com to pick only the tests you need.
            bundles.Add(new ScriptBundle("~/bundles/modernizr").Include(
                        "~/Scripts/modernizr-*"));

            bundles.Add(new ScriptBundle("~/bundles/bootstrap").Include(
                      "~/Scripts/bootstrap.js"));

            bundles.Add(new StyleBundle("~/Content/css").Include(
                      "~/Content/bootstrap.css",
                      "~/Content/site.css"));

            // Common/Header
            bundles.Add(new ScriptBundle("~/bundles/common")
               .Include("~/Scripts/Common/Header.js")
               .Include("~/Scripts/Common/Effects.js")
               .Include("~/Content/js/main.js"));

            // User Management
            bundles.Add(new ScriptBundle("~/bundles/user")
                .Include("~/Scripts/User/ManageUser.js")
                .Include("~/Scripts/jquery.validate.unobtrusive.min.js"));

            // Product Management
            bundles.Add(new ScriptBundle("~/bundles/product")
                .Include("~/Scripts/Product/ManageProducts.js")
                .Include("~/Scripts/jquery.validate.unobtrusive.min.js"));

            // Product Add/Edit
            bundles.Add(new ScriptBundle("~/bundles/productAddEdit")
               .Include("~/Scripts/Product/ProductInventory.js")
               .Include("~/Scripts/Product/InventoryLogs.js"));

            // Product Add/Edit
            bundles.Add(new ScriptBundle("~/bundles/mudProductAddEdit")
                .Include("~/Scripts/Product/ProductInventory.js")
               .Include("~/Scripts/Product/InventoryLogs.js")
               .Include("~/Scripts/Product/MudProduct.js")
               .Include("~/Scripts/Common/CheckboxSelection.js"));

            // Customer Management
            bundles.Add(new ScriptBundle("~/bundles/customer")
               .Include("~/Scripts/Customer/manageCustomer.js"));

            bundles.Add(new ScriptBundle("~/bundles/OrderDetail")
                .Include("~/Scripts/Order/OrderDetail.js")
                .Include("~/Scripts/Order/OrderDetailItems.js")
                .Include("~/Scripts/Common/CheckboxSelection.js"));

            bundles.Add(new ScriptBundle("~/bundles/ManageOrders")
                .Include("~/Scripts/Order/ManageOrders.js"));

            bundles.Add(new ScriptBundle("~/bundles/PriceTier")
                .Include("~/Scripts/Libraries/sol.js")
                .Include("~/Scripts/PriceTier/PriceTier.js")
                .Include("~/Scripts/Common/CheckboxSelection.js"));
        }
    }
}
