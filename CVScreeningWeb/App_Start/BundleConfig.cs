using System.Web.Optimization;

namespace CVScreeningWeb
{
    public class BundleConfig
    {
        // For more information on Bundling, visit http://go.microsoft.com/fwlink/?LinkId=254725
        public static void RegisterBundles(BundleCollection bundles)
        {
            #region Script Bundle

            // Bundle Javascript files

            #region Kendo Scripts
            bundles.Add(new ScriptBundle("~/bundles/jquery").Include(
                    "~/Scripts/kendo/jquery.min.js"));

            bundles.Add(new ScriptBundle("~/bundles/kendo-web").Include(
                "~/Scripts/kendo/kendo.web.min.js",
                "~/Scripts/kendo/kendo.aspnetmvc.min.js",
                "~/Scripts/kendo/kendo.timezones.min.js"));

            bundles.Add(new ScriptBundle("~/bundles/kendo-dataviz").Include(
                "~/Scripts/kendo/kendo.all.min.js",
                "~/Scripts/kendo/kendo.aspnetmvc.min.js"));

            bundles.Add(new ScriptBundle("~/bundles/kendo-mobile").Include(
                "~/Scripts/kendo/kendo.all.min.js",
                "~/Scripts/kendo/kendo.aspnetmvc.min.js"));

            #endregion

            bundles.Add(new ScriptBundle("~/bundles/bootstrap").Include(
                "~/Content/theme/bs3/js/bootstrap.min.js", 
                "~/Content/theme/bs3/js/bootstrap-inputmask.min.js" ));

            bundles.Add(new ScriptBundle("~/bundles/modernizr").Include(
                "~/Scripts/modernizr-*"));

            bundles.Add(new ScriptBundle("~/bundles/mytheme").Include(
                "~/Scripts/theme/scripts.js"));

            // Include when necessary if page requiring client side validation
            bundles.Add(new ScriptBundle("~/bundles/jqueryval").Include(
                "~/Scripts/jquery.unobtrusive-ajax.min.js",
                "~/Scripts/jquery.validate-vsdoc.js",
                "~/Scripts/jquery.validate.min.js",
                "~/Scripts/jquery.validate.unobtrusive.min.js",
                "~/Scripts/custom.jquery.validate.js",
                "~/Scripts/location.validate.js"));

            // Custom javascript provided by Nalysa
            bundles.Add(new ScriptBundle("~/bundles/screening").Include(
                "~/Scripts/screening.atomiccheck.js"));

            #endregion


            #region  Css style bundle

            #region Kendo Styles
            bundles.Add(new StyleBundle("~/Content/web/css").Include(
                    "~/Content/kendo/kendo.common.min.css",
                    "~/Content/kendo/kendo.flat.min.css", 
                    "~/Content/kendo/customkendo.css"));

            bundles.Add(new StyleBundle("~/Content/dataviz/css").Include(
                "~/Content/kendo/kendo.dataviz.min.css",
                "~/Content/kendo/kendo.common.min.css",
                "~/Content/kendo/kendo.dataviz.default.min.css"));

            bundles.Add(new StyleBundle("~/Content/mobile/css").Include(
                "~/Content/kendo/kendo.mobile.all.min.css"));
            #endregion

            // Add CVScreening Nalysa custom css with font awesome
            bundles.Add(new StyleBundle("~/Content/site").Include(
                  "~/Content/mysite.css"));

            bundles.Add(new StyleBundle("~/Content/bootstrap").Include(
                "~/Content/theme/bs3/css/bootstrap.min.css",
                "~/Content/theme/css/bootstrap-reset.css"));

            bundles.Add(new StyleBundle("~/Content/mytheme").Include(
                "~/Content/theme/css/style.css",
                "~/Content/theme/css/style-responsive.css")); 
            #endregion

            // Tell ASP.NET bundles to allow minified files in debug mode.
            bundles.IgnoreList.Clear();
            BundleTable.EnableOptimizations = false;
        }
    }
}