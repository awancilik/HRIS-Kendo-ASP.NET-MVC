using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using CVScreeningWeb.Helpers;


namespace CVScreeningWeb.Controllers
{
    public class NavigationController : Controller
    {
        
        [ChildActionOnly]
        public ActionResult Menu()
        {
            if (Roles.IsUserInRole("Client"))
            {
                return PartialView("Navigation/_NavigationClient");
            }
            var viewModel = NavigationHelper.BuildNavigationViewModel();
            return PartialView("Navigation/_Navigation", viewModel);
        }


    }
}
