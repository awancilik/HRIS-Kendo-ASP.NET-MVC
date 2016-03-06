using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Caching;
using CVScreeningDAL.EntityFramework;

namespace CVScreeningWeb.App_Start
{
    public class StaticCache
    {
        public static void LoadStaticCache()
        {
            using (var dbContext = new CVScreeningEFContext())
            {
                HttpRuntime.Cache["locations"] = dbContext.Location.Where(u => u.LocationTenantId == 1).ToList();
            }

        }

    }
}