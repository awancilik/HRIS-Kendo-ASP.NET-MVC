using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using CVScreeningCore.Error;
using CVScreeningService.DTO.LookUpDatabase;
using CVScreeningService.Services.Common;
using CVScreeningService.Services.ErrorHandling;
using CVScreeningService.Services.LookUpDatabase;
using CVScreeningWeb.Helpers;
using CVScreeningWeb.ViewModels.BaseQualificationPlace;
using CVScreeningWeb.ViewModels.CertificationPlace;

namespace CVScreeningWeb.Controllers
{
    [Authorize(Roles = "Administrator, Screener, Qualifier, Production manager")]
    [Filters.HandleError]
    public class CertificationPlaceController : Controller
    {
        private readonly ILookUpDatabaseService<CertificationPlaceDTO> _certificationPlaceLookUpDatabaseService;
        private readonly ICommonService _commonService;
        private readonly IErrorMessageFactoryService _errorMessageFactoryService;
        private readonly IProfessionalQualificationService _professionalQualificationService;

        public CertificationPlaceController(
            ILookUpDatabaseService<CertificationPlaceDTO> certificationPlaceLookUpDatabaseService,
            ICommonService commonService, IErrorMessageFactoryService errorMessageFactoryService,
            IProfessionalQualificationService professionalQualificationService)
        {
            _certificationPlaceLookUpDatabaseService = certificationPlaceLookUpDatabaseService;
            _commonService = commonService;
            _errorMessageFactoryService = errorMessageFactoryService;
            _professionalQualificationService = professionalQualificationService;
            ViewBag.IsKendoEnabled = true;
        }

        /// <summary>
        /// Json action used by kendo ui to retrieve certification places
        /// </summary>
        /// <returns></returns>
        public JsonResult GetCertificationPlaces(int? id)
        {
            var places = _certificationPlaceLookUpDatabaseService.GetAllQualificationPlaces();
            if (id != null)
                places = places.Where(u => u.ProfessionalQualification.Any(p => p.ProfessionalQualificationId == id)).ToList();

            return Json(places.Select(c => new { CertificationPlaceId = c.QualificationPlaceId, CertificationPlaceName = string.Format("{0} - {1}", c.QualificationPlaceName, AddressHelper.GetShortAddressAsString(c.Address)) }),
                JsonRequestBehavior.AllowGet);
        }

        private IEnumerable<ProfessionalQualificationDTO> GetAllProfessionalQualification()
        {
            return _professionalQualificationService.GetAllProfessionalQualifications();
        }

        private CertificationPlaceFormViewModel InstatiateFormViewModel(CertificationPlaceFormViewModel iModel)
        {
            var viewModel = iModel;
            viewModel.AddressViewModel = AddressHelper.BuildAddressViewModel();
            viewModel.ProfessionalQualification = FormHelper.BuildSelectListViewModel(
                GetAllProfessionalQualification().ToDictionary(
                    e => e.ProfessionalQualificationId, e => e.ProfessionalQualificationName));
            return viewModel;
        }

        public ActionResult Index(int? professionalQualificationId = null)
        {
            var certificationPlaces =
                professionalQualificationId != null
                    ? _professionalQualificationService.GetCertificationPlaces(professionalQualificationId ?? 0)
                    : _certificationPlaceLookUpDatabaseService.GetAllQualificationPlaces();

            var certificationPlaceVMs =
                (from certificationPlace in certificationPlaces
                    let subDistrict = _commonService.GetLocation(certificationPlace.Address.Location.LocationId)
                    let district = subDistrict.LocationParentLocationId != null ? _commonService.GetLocation(subDistrict.LocationParentLocationId) : null
                    select new CertificationPlaceManageViewModel
                    {
                        Id = certificationPlace.QualificationPlaceId,
                        Name = certificationPlace.QualificationPlaceName,
                        Address = certificationPlace.Address.Location.LocationParentLocationId == null ?
                            string.Format("{0}, {1}", certificationPlace.Address.Street, certificationPlace.Address.Location.LocationName) :
                            string.Format(AddressHelper.GetAddressAsLabel(certificationPlace.Address))
                    }).ToList();
            return View(certificationPlaceVMs);
        }

        // GET: /CertificationPlace/Detail

        /// <summary>
        ///     To view in detail specific certificationPlace
        /// </summary>
        /// <param name="id">CertificationPlace ID</param>
        /// <returns></returns>
        public ActionResult Detail(int id)
        {
            var certificationPlaceDTO =
                _certificationPlaceLookUpDatabaseService.GetQualificationPlace(id);
            if (certificationPlaceDTO == null)
            {
                ModelState.AddModelError("",
                    _errorMessageFactoryService.Create(ErrorCode.DBLOOKUP_QUALIFICATION_PLACE_NOT_FOUND));
                return RedirectToAction("Index", "Error", ErrorCode.DBLOOKUP_QUALIFICATION_PLACE_NOT_FOUND);
            }

            var certificationPlaceVm = new CertificationPlaceFormViewModel
            {
                Name = certificationPlaceDTO.QualificationPlaceName,
                Description = certificationPlaceDTO.QualificationPlaceDescription,
                AddressViewModel = AddressHelper.BuildAddressViewModel(certificationPlaceDTO.Address),
                ProfessionalQualification = FormHelper.BuildSelectListViewModel(
                GetAllProfessionalQualification().ToDictionary(e => e.ProfessionalQualificationId,
                    e => e.ProfessionalQualificationName), GetSelectedProfessionalQualificationIds(id)),
                Website = certificationPlaceDTO.QualificationPlaceWebSite ?? ""
            };
            return View(certificationPlaceVm);
        }

        // GET: /CertificationPlace/Edit/id
        /// <summary>
        ///     Edit a certificationPlace
        /// </summary>
        /// <param name="id">CertificationPlace ID</param>
        /// <returns></returns>
        public ActionResult Edit(int id)
        {
            var certificationPlaceDTO =
                _certificationPlaceLookUpDatabaseService.GetQualificationPlace(id);
            if (certificationPlaceDTO == null)
            {
                ModelState.AddModelError("",
                    _errorMessageFactoryService.Create(ErrorCode.DBLOOKUP_QUALIFICATION_PLACE_NOT_FOUND));
                return RedirectToAction("Index", "Error", ErrorCode.DBLOOKUP_QUALIFICATION_PLACE_NOT_FOUND);
            }

            var certificationPlaceVm = new CertificationPlaceFormViewModel
            {
                Id = id,
                Name = certificationPlaceDTO.QualificationPlaceName,
                Description = certificationPlaceDTO.QualificationPlaceDescription,
                AddressViewModel = AddressHelper.BuildAddressViewModel(certificationPlaceDTO.Address),
                ProfessionalQualification = FormHelper.BuildSelectListViewModel(
                GetAllProfessionalQualification().ToDictionary(e => e.ProfessionalQualificationId,
                    e=> e.ProfessionalQualificationName), GetSelectedProfessionalQualificationIds(id)),
                Website = certificationPlaceDTO.QualificationPlaceWebSite,
            };

            return View("Create", certificationPlaceVm);
        }

        private IEnumerable<int> GetSelectedProfessionalQualificationIds(int certificationPlaceId)
        {
            var allProfesionalQualifications = _professionalQualificationService.GetAllProfessionalQualifications();
            var selectedProfessionalQualifications = new List<ProfessionalQualificationDTO>();
// ReSharper disable LoopCanBeConvertedToQuery
            foreach (var professionalQualificationDTO in allProfesionalQualifications)
            {
                foreach (var certificationPlaceDTO in professionalQualificationDTO.QualificationPlace)
// ReSharper restore LoopCanBeConvertedToQuery
                {
                    if(certificationPlaceDTO.QualificationPlaceId == certificationPlaceId)
                        selectedProfessionalQualifications.Add(professionalQualificationDTO);
                }
            }
            return selectedProfessionalQualifications.Select(e =>e.ProfessionalQualificationId);
        }

        // GET: /CertificationPlace/Delete/id
        public ActionResult Delete(int id)
        {
            var errorCode =
                _certificationPlaceLookUpDatabaseService.DeleteQualificationPlace(new CertificationPlaceDTO
                {
                    QualificationPlaceId = id
                });
            return errorCode == ErrorCode.NO_ERROR
                ? RedirectToAction("Index", "CertificationPlace")
                : RedirectToAction("Index", "Error", new {errorCodeParameter = errorCode});
        }

        // GET: /CertificationPlace/Create
        public ActionResult Create()
        {
            var certificationPlaceVm = InstatiateFormViewModel(new CertificationPlaceFormViewModel());
            return View(certificationPlaceVm);
        }

        // POST: /CertificationPlace/Create/
        /// <summary>
        ///     Post method to create a certificationPlace
        /// </summary>
        /// <param name="iModel"></param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(CertificationPlaceFormViewModel iModel)
        {
            if (!ModelState.IsValid)
            {
                ModelState.AddModelError("", _errorMessageFactoryService.
                    Create(ErrorCode.COMMON_FORM_VALIDATION_ERROR));
                iModel = InstatiateFormViewModel(iModel);
                return View(iModel);
            }

            var certificationPlaceDTO = new CertificationPlaceDTO
            {
                QualificationPlaceId = iModel.Id,
                QualificationPlaceName = iModel.Name,
                QualificationPlaceDescription = iModel.Description,
                QualificationPlaceWebSite = iModel.Website,
                ProfessionalQualification = GetAllProfessionalQualification().Where(
                   e => FormHelper.ExtractSelectListViewModel(iModel.ProfessionalQualification)
                       .Contains(e.ProfessionalQualificationId+ "")),
                Address = AddressHelper.ExtractAddressViewModel(iModel.AddressViewModel)
            };

            var errorCode =
                _certificationPlaceLookUpDatabaseService.CreateOrEditQualificationPlace(ref certificationPlaceDTO);
            if (errorCode == ErrorCode.NO_ERROR)
                return RedirectToAction("Index", "CertificationPlace");

            iModel = InstatiateFormViewModel(iModel);
            ModelState.AddModelError("", _errorMessageFactoryService.Create(errorCode));
            return View(iModel);
        }
    }
}