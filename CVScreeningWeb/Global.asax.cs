using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using CVScreeningWeb.App_Start;
using CVScreeningWeb.Job;
using Nalysa.Common.Log;
using Newtonsoft.Json;
using Ninject.Web.Mvc;

namespace CVScreeningWeb
{ 
    // visit http://go.microsoft.com/?LinkId=9394801

    public class MvcApplication : HttpApplication
    {
        protected void Application_Start()
        {
            LogManager.Instance.Info("Application CV Screening is starting ...");
            LogManager.Instance.Info("Web root folder: " + Server.MapPath("~"));
            LogManager.Instance.Info("Web data folder: " + Server.MapPath("~/Files"));


            AreaRegistration.RegisterAllAreas();
            WebApiConfig.Register(GlobalConfiguration.Configuration);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
            AuthConfig.RegisterAuth();
            StaticCache.LoadStaticCache();

            if (!WebMatrix.WebData.WebSecurity.Initialized)
                WebMatrix.WebData.WebSecurity.InitializeDatabaseConnection("DefaultConnection",
                    "webpages_UserProfile", "UserId", "UserName", true);

            var jsonSerializerSettings = new JsonSerializerSettings
            {
                PreserveReferencesHandling = PreserveReferencesHandling.Objects
            };


        }
    }
}