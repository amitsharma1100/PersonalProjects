namespace System.Web.Mvc
{
    using System;
    using System.IO;
    using Deepwell.Common.Extensions;

    /// <summary>
    /// Provides extension methods for MVC objects.
    /// </summary>
    public static class MvcExtensions
    {
        /// <summary>
        /// Executes the specified partial view, passing it the specified Model, and returns the HTML result.
        /// </summary>
        /// <param name="ctrl">
        /// The controller to which the extension applies.
        /// </param>
        /// <param name="viewName">
        /// The name of the view to execute.
        /// </param>
        /// <param name="model">
        /// The model to be passed into the view.
        /// </param>
        /// <returns>
        /// The HTML result of executing the specified partial view.
        /// </returns>
        public static string RenderPartialViewToString(this Controller ctrl, string viewName, object model)
        {
            if (viewName.HasNoValue())
            {
                viewName = ctrl.ControllerContext.RouteData.GetRequiredString("action");
            }

            ctrl.ViewData.Model = model;

            using (var sw = new StringWriter())
            {
                try
                {
                    ViewEngineResult viewResult = ViewEngines.Engines.FindPartialView(ctrl.ControllerContext, viewName);
                    ViewContext viewContext = new ViewContext(ctrl.ControllerContext, viewResult.View, ctrl.ViewData, ctrl.TempData, sw);
                    viewResult.View.Render(viewContext, sw);
                    viewResult.ViewEngine.ReleaseView(ctrl.ControllerContext, viewResult.View);

                    string s = sw.GetStringBuilder().ToString();
                    return s;
                }
                catch (Exception e)
                {
                    bool ctrlIsNull = ctrl == null;
                    bool viewNameIsEmpty = viewName.HasNoValue();
                    bool modelIsNull = model == null;

                    string traceMessage =
                        $"Render partial view to string failed.  Arguments: ctrl == null = {ctrlIsNull}; viewName.HasNoValue() = {viewNameIsEmpty}; model == null = {modelIsNull}; exception = {e.Message}";

              
                    return e.ToString();
                }
            }
        }

        /// <summary>
        /// Executes the specified partial view, passing it the view data dictionary, and returns the HTML result.
        /// </summary>
        /// <param name="ctrl">
        /// The controller to which the extension applies.
        /// </param>
        /// <param name="viewName">
        /// The name of the view to execute.
        /// </param>
        /// <param name="dictionary">
        /// The dictionary to be passed into the view.
        /// </param>
        /// <returns></returns>
        public static string RenderPartialViewToString(this Controller ctrl, string viewName, ViewDataDictionary dictionary)
        {
            if (viewName.HasNoValue())
            {
                viewName = ctrl.ControllerContext.RouteData.GetRequiredString("action");
            }

            ctrl.ViewData = dictionary;

            using (var sw = new StringWriter())
            {
                try
                {
                    ViewEngineResult viewResult = ViewEngines.Engines.FindPartialView(ctrl.ControllerContext, viewName);
                    ViewContext viewContext = new ViewContext(ctrl.ControllerContext, viewResult.View, ctrl.ViewData, ctrl.TempData, sw);
                    viewResult.View.Render(viewContext, sw);
                    viewResult.ViewEngine.ReleaseView(ctrl.ControllerContext, viewResult.View);

                    string s = sw.GetStringBuilder().ToString();
                    return s;
                }
                catch (Exception e)
                {
                    bool ctrlIsNull = ctrl == null;
                    bool viewNameIsEmpty = viewName.HasNoValue();
                    bool modelIsNull = dictionary == null;

                    string traceMessage =
                        $"Render partial view to string failed.  Arguments: ctrl == null = {ctrlIsNull}; viewName.HasNoValue() = {viewNameIsEmpty}; dictionary == null = {modelIsNull}; exception = {e.Message}";

                  
                    return e.ToString();
                }
            }
        }
    }
}