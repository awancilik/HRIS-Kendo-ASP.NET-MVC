using System.Web.Mvc;
using CVScreeningWeb.Filters;
using HandleErrorAttribute = System.Web.Mvc.HandleErrorAttribute;

namespace CVScreeningWeb
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
            filters.Add(new NotificationAttribute());
        }
    }
}