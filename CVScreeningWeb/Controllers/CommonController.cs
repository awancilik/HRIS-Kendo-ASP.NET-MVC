using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CVScreeningCore.Models;
using CVScreeningService.DTO.Common;
using CVScreeningService.Services.Common;
using CVScreeningWeb.Helpers;

namespace CVScreeningWeb.Controllers
{
    [Authorize]
    [Filters.HandleError]
    public class CommonController : Controller
    {
        // GET: /Common/
        private readonly ICommonService _commonService;

        public CommonController(ICommonService commonService)
        {
            _commonService = commonService;
        }


        public JsonResult GetCountry()
        {
            var locations = HttpRuntime.Cache["locations"] as List<Location>;
            var country = locations.Where(l => l.LocationLevel == 1 
                && l.LocationTenantId == TenantHelper.GetTenantId(Request.Url.Host));
            return Json(country.Select(c => new { CountryId = c.LocationId, CountryName = c.LocationName }),
                JsonRequestBehavior.AllowGet);
        }

        [OutputCache(Duration = int.MaxValue)]
        public JsonResult GetProvince()
        {
            var locations = HttpRuntime.Cache["locations"] as List<Location>;
            var province = locations.Where(l => l.LocationLevel == 2
                 && l.LocationTenantId == TenantHelper.GetTenantId(Request.Url.Host));
            return Json(province.Select(c => new { ProvinceId = c.LocationId, ProvinceName = c.LocationName }),
                JsonRequestBehavior.AllowGet);
        }

        [OutputCache(Duration = int.MaxValue, VaryByParam = "parentId;level")]
        public JsonResult GetChildrenLocationByParentId(int? parentId, int level)
        {
            var locations = HttpRuntime.Cache["locations"] as List<Location>;
            var children = locations.Where(l => l.LocationLevel == level
                 && l.LocationTenantId == TenantHelper.GetTenantId(Request.Url.Host)).OrderBy(u => u.LocationName);
            if (parentId != null)
            {
                children = children.Where(l => l.LocationParent.LocationId == parentId).OrderBy(u => u.LocationName);
            }
            return Json(children.Select(p => new { ChildrenId = p.LocationId, ChildrenName = p.LocationName }),
                JsonRequestBehavior.AllowGet);
        }

    }
}
