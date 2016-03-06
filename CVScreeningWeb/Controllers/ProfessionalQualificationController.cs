using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using CVScreeningCore.Error;
using CVScreeningService.DTO.LookUpDatabase;
using CVScreeningService.Services.ErrorHandling;
using CVScreeningService.Services.LookUpDatabase;
using CVScreeningWeb.Helpers;
using CVScreeningWeb.ViewModels.CertificationPlace;
using CVScreeningWeb.ViewModels.ProfessionalQualification;

namespace CVScreeningWeb.Controllers
{
    [Authorize(Roles = "Administrator, Screener, Qualifier, Production manager")]
    [Filters.HandleError]
    public class ProfessionalQualificationController : Controller
    {
        private readonly ILookUpDatabaseService<CertificationPlaceDTO> _certificationPlaceService;
        private readonly IProfessionalQualificationService _professionalQualificationService;
        private readonly IErrorMessageFactoryService _errorMessageFactoryService;

        public ProfessionalQualificationController(IProfessionalQualificationService professionalQualificationService,
            ILookUpDatabaseService<CertificationPlaceDTO> certificationPlaceService, IErrorMessageFactoryService errorMessageFactoryService)
        {
            _professionalQualificationService = professionalQualificationService;
            _errorMessageFactoryService = errorMessageFactoryService;
            _certificationPlaceService = certificationPlaceService;
        }

        /// <summary>
        /// Json action used by kendo ui to retrieve professionnal qualification list
        /// </summary>
        /// <returns></returns>
        public JsonResult GetProfessionnalQualifications()
        {
            var qualifications = _professionalQualificationService.GetAllProfessionalQualifications();

            return Json(qualifications.Select(c => new { ProfessionnalQualificationId = c.ProfessionalQualificationId, ProfessionnalQualificationName = c.ProfessionalQualificationName }),
                JsonRequestBehavior.AllowGet);
        }

        public ActionResult Index()
        {
            var professionalQualifications =
                _professionalQualificationService.GetAllProfessionalQualifications();

            var professionalQualificationVms =
                professionalQualifications.Select(e => new ProfessionalQualificationManageViewModel
                {
                    Id = e.ProfessionalQualificationId,
                    Code = e.ProfessionalQualificationCode,
                    Name = e.ProfessionalQualificationName
                });
            return View(professionalQualificationVms);
        }

        public ActionResult Detail(int id)
        {
            var professionalQualification =
                _professionalQualificationService.GetProfessionalQualification(id);
            if (professionalQualification == null)
                return RedirectToAction("Index", "Error",
                    new {errorCodeParameter = ErrorCode.DBLOOKUP_PROFESSIONAL_QUALIFICATION_NOT_FOUND});

            IEnumerable<int> selectedCenterIds =
                _professionalQualificationService.GetCertificationPlaces(id).Select(
                 center => center.QualificationPlaceId);
            IEnumerable<CertificationPlaceDTO> certificationPlaceDtos =
                _certificationPlaceService.GetAllQualificationPlaces();

            var professionalQualificationVm = new ProfessionalQualificationFormViewModel
            {
                Id = professionalQualification.ProfessionalQualificationId,
                Code = professionalQualification.ProfessionalQualificationCode,
                Name = professionalQualification.ProfessionalQualificationName,
                Description = professionalQualification.ProfessionalQualificationDescription,
                Centers = FormHelper.BuildSelectListViewModel(certificationPlaceDtos.ToDictionary(
                    e => e.QualificationPlaceId, e => e.QualificationPlaceName),selectedCenterIds)
            };
            return View(professionalQualificationVm);
        }

        public ActionResult Create()
        {
            var professionalQualificationVm = InstatiateViewModel(new ProfessionalQualificationFormViewModel());
            return View(professionalQualificationVm);
        }

        private ProfessionalQualificationFormViewModel InstatiateViewModel(ProfessionalQualificationFormViewModel iModel)
        {
            var viewModel = iModel;
            viewModel.Centers =
                FormHelper.BuildSelectListViewModel(_certificationPlaceService.GetAllQualificationPlaces().ToDictionary(
                    e => e.QualificationPlaceId, e => e.QualificationPlaceName));
            return viewModel;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(ProfessionalQualificationFormViewModel iModel)
        {
            if (!ModelState.IsValid)
            {
                ModelState.AddModelError("", _errorMessageFactoryService.
                    Create(ErrorCode.COMMON_FORM_VALIDATION_ERROR));
                iModel = InstatiateViewModel(iModel);
                return View(iModel);
            }

            var professionalQualificationDTO = new ProfessionalQualificationDTO
            {
                ProfessionalQualificationCode = iModel.Code,
                ProfessionalQualificationId = iModel.Id,
                ProfessionalQualificationName = iModel.Name,
                ProfessionalQualificationDescription = iModel.Description
            };

            if (iModel.Centers != null)
            {
                var certificationPlaces = ( from center in FormHelper.ExtractSelectListViewModel(iModel.Centers)
                                            select _certificationPlaceService.GetQualificationPlace(Convert.ToInt32(center))).ToList();
                professionalQualificationDTO.QualificationPlace = certificationPlaces;
            }
         
            var errorCode =
                _professionalQualificationService.CreateOrUpdateProfessionalQualification(
                    ref professionalQualificationDTO);
            if (errorCode == ErrorCode.NO_ERROR)
                return RedirectToAction("Index");
            iModel = InstatiateViewModel(iModel);
            ModelState.AddModelError("", _errorMessageFactoryService.Create(errorCode));
            return View(iModel);
        }

        public ActionResult Edit(int id)
        {
            var professionalQualification = _professionalQualificationService.GetProfessionalQualification(id);
            if (professionalQualification == null)
                return RedirectToAction("Index", "Error",
                    new {errorCodeParameter = ErrorCode.DBLOOKUP_PROFESSIONAL_QUALIFICATION_NOT_FOUND});
            var allCenters = _certificationPlaceService.GetAllQualificationPlaces();
            var selectedCenter = _professionalQualificationService.GetCertificationPlaces(id)
                    .Select(c => c.QualificationPlaceId);
            var professionalQualificationVm = new ProfessionalQualificationFormViewModel
            {   Id = id,
                Name = professionalQualification.ProfessionalQualificationName,
                Code = professionalQualification.ProfessionalQualificationCode,
                Description = professionalQualification.ProfessionalQualificationDescription,
                Centers = FormHelper.BuildSelectListViewModel(allCenters.ToDictionary
                    (e => e.QualificationPlaceId, e => e.QualificationPlaceName), selectedCenter.ToList())
            };
            return View("Create", professionalQualificationVm);
        }

        public ActionResult ManageCenters(int id)
        {
            return RedirectToAction("Index", "CertificationPlace",
                new {professionalQualificationId = id});
        }

        public ActionResult Delete(int id)
        {
            var errorCode = _professionalQualificationService.DeleteProfessionalQualification(id);
            if (errorCode == ErrorCode.NO_ERROR)
                return RedirectToAction("Index");
            return RedirectToAction("Index",
                "Error", new {errorCodeParameter = errorCode});
        }
    }
}