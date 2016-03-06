using System;
using System.Linq;
using System.Web.Mvc;
using CVScreeningCore.Models;
using CVScreeningService.DTO.LookUpDatabase;
using CVScreeningService.Services.LookUpDatabase;
using Microsoft.AspNet.SignalR.Hubs;

namespace CVScreeningWeb.Controllers
{
    public class QualificationPlaceController : Controller
    {
        private readonly ILookUpDatabaseService<QualificationPlaceDTO> _qualifiationPlaceService;

        public QualificationPlaceController(ILookUpDatabaseService<QualificationPlaceDTO> qualifiationPlaceService)
        {
            _qualifiationPlaceService = qualifiationPlaceService;
        }



        public JsonResult GetTypeOfQualificationPlace()
        {
            var qualificationCodeEnums = (QualificationCode[]) Enum.GetValues(typeof (QualificationCode));

            return Json(qualificationCodeEnums.Select(e => new
            {
                QualificationPlaceTypeId = e,
                QualificationPlaceTypeName = Convert.ToString(e)
            }), JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetQualificationPlaceJSON()
        {
            var qualificationPlaces = _qualifiationPlaceService.GetAllQualificationPlaces();
            return Json(qualificationPlaces.Select(e => new
            {
                QualificationPlaceId = e.QualificationPlaceId,
                QualificationPlaceName = e.QualificationPlaceName
            }), JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetOfficeQualificationPlacesJSON()
        {
            var qualificationPlaces =
                _qualifiationPlaceService.GetQualificationPlaceByScreenerCategory(TypeOfCheckMeta.kOfficeCategory);

            return Json(qualificationPlaces.Select(e => new
            {
                QualificationPlaceId = e.QualificationPlaceId,
                QualificationPlaceName = e.QualificationPlaceName
            }), JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetOnFieldQualificationPlacesJSON()
        {
            var qualificationPlaces =
                _qualifiationPlaceService.GetQualificationPlaceByScreenerCategory(TypeOfCheckMeta.kOnFieldCategory);

            return Json(qualificationPlaces.Select(e => new
            {
                QualificationPlaceId = e.QualificationPlaceId,
                QualificationPlaceName = e.QualificationPlaceName
            }), JsonRequestBehavior.AllowGet);
        }
    }
}