using System.Web.Mvc;
using System.Web.Routing;

namespace CVScreeningWeb
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");
            routes.IgnoreRoute("elmah.axd");
            routes.MapRoute("Default", "{controller}/{action}/{id}/{secondaryId}",
                new { controller = "Home", action = "Index", id = UrlParameter.Optional, secondaryId = UrlParameter.Optional }
                );
        }
    }
}