using System.Linq;
using System.Web.Mvc;
using CVScreeningCore.Error;
using CVScreeningService.DTO.LookUpDatabase;
using CVScreeningService.Services.Common;
using CVScreeningService.Services.ErrorHandling;
using CVScreeningService.Services.LookUpDatabase;
using CVScreeningWeb.Helpers;
using CVScreeningWeb.ViewModels.BaseQualificationPlace;
using CVScreeningWeb.ViewModels.DrivingLicenseOffice;

namespace CVScreeningWeb.Controllers
{
    [Authorize(Roles = "Administrator, Screener, Qualifier, Production manager")]
    [Filters.HandleError]
    public class DrivingLicenseOfficeController : Controller
    {
        private readonly ICommonService _commonService;
        private readonly ILookUpDatabaseService<DrivingLicenseOfficeDTO> _drivingLicenseOfficeLookUpDatabaseService;
        private readonly IErrorMessageFactoryService _errorMessageFactoryService;

        public DrivingLicenseOfficeController(
            ILookUpDatabaseService<DrivingLicenseOfficeDTO> drivingLicenseOfficeLookUpDatabaseService,
            ICommonService commonService, IErrorMessageFactoryService errorMessageFactoryService)
        {
            _drivingLicenseOfficeLookUpDatabaseService = drivingLicenseOfficeLookUpDatabaseService;
            _commonService = commonService;
            _errorMessageFactoryService = errorMessageFactoryService;
        }

        private QualificationPlaceFormViewModel InstatiateFormViewModel(QualificationPlaceFormViewModel iModel)
        {
            var viewModel = iModel;
            viewModel.AddressViewModel = AddressHelper.BuildAddressViewModel();
            return viewModel;
        }

        /// <summary>
        /// Json action used by kendo ui to retrieve driving license office list
        /// </summary>
        /// <returns></returns>
        public JsonResult GetDrivingLicenseOffices()
        {
            var offices = _drivingLicenseOfficeLookUpDatabaseService.GetAllQualificationPlaces();

            return Json(offices.Select(c => new { DrivingLicenseOfficeId = c.QualificationPlaceId, DrivingLicenseOfficeName = string.Format("{0} - {1}", c.QualificationPlaceName, AddressHelper.GetShortAddressAsString(c.Address)) }),
                JsonRequestBehavior.AllowGet);
        }

        public ActionResult Index()
        {
            var drivingLicenseOffices =
                _drivingLicenseOfficeLookUpDatabaseService.GetAllQualificationPlaces();
            var drivingLicenseOfficeVMs =
                (from drivingLicenseOffice in drivingLicenseOffices
                    let subDistrict = _commonService.GetLocation(drivingLicenseOffice.Address.Location.LocationId)
                    let district = subDistrict.LocationParentLocationId != null ? _commonService.GetLocation(subDistrict.LocationParentLocationId) : null
                    select new DrivingLicenseOfficeManageViewModel
                    {
                        Id = drivingLicenseOffice.QualificationPlaceId,
                        Name = drivingLicenseOffice.QualificationPlaceName,
                        Address = drivingLicenseOffice.Address.Location.LocationParentLocationId == null ?
                        string.Format("{0}, {1}", drivingLicenseOffice.Address.Street, drivingLicenseOffice.Address.Location.LocationName) :
                        string.Format(AddressHelper.GetAddressAsLabel(drivingLicenseOffice.Address))
                    }).ToList();
            return View(drivingLicenseOfficeVMs);
        }

        // GET: /DrivingLicenseOffice/Detail

        /// <summary>
        ///     To view in detail specific drivingLicenseOffice
        /// </summary>
        /// <param name="id">DrivingLicenseOffice ID</param>
        /// <returns></returns>
        public ActionResult Detail(int id)
        {
            var drivingLicenseOfficeDTO =
                _drivingLicenseOfficeLookUpDatabaseService.GetQualificationPlace(id);
            if (drivingLicenseOfficeDTO == null)
            {
                ModelState.AddModelError("",
                    _errorMessageFactoryService.Create(ErrorCode.DBLOOKUP_QUALIFICATION_PLACE_NOT_FOUND));
                return RedirectToAction("Index", "Error", ErrorCode.DBLOOKUP_QUALIFICATION_PLACE_NOT_FOUND);
            }
            var drivingLicenseOfficeVm =
                new DrivingLicenseOfficeFormViewModel
                {
                    Name = drivingLicenseOfficeDTO.QualificationPlaceName,
                    Description = drivingLicenseOfficeDTO.QualificationPlaceDescription,
                    Website = drivingLicenseOfficeDTO.QualificationPlaceWebSite ?? "",
                    AddressViewModel = AddressHelper.BuildAddressViewModel(drivingLicenseOfficeDTO.Address)
                };
            return View(drivingLicenseOfficeVm);
        }

        // GET: /DrivingLicenseOffice/Edit/id
        /// <summary>
        ///     Edit a drivingLicenseOffice
        /// </summary>
        /// <param name="id">DrivingLicenseOffice ID</param>
        /// <returns></returns>
        [Authorize(Roles = "Administrator")]
        public ActionResult Edit(int id)
        {
            var drivingLicenseOfficeDTO =
                _drivingLicenseOfficeLookUpDatabaseService.GetQualificationPlace(id);
            if (drivingLicenseOfficeDTO == null)
            {
                ModelState.AddModelError("",
                    _errorMessageFactoryService.Create(ErrorCode.DBLOOKUP_QUALIFICATION_PLACE_NOT_FOUND));
                return RedirectToAction("Index", "Error", ErrorCode.DBLOOKUP_QUALIFICATION_PLACE_NOT_FOUND);
            }
            var drivingLicenseOfficeVm = new DrivingLicenseOfficeFormViewModel
            {
                Id = id,
                Name = drivingLicenseOfficeDTO.QualificationPlaceName,
                Description = drivingLicenseOfficeDTO.QualificationPlaceDescription,
                Website = drivingLicenseOfficeDTO.QualificationPlaceWebSite,
                AddressViewModel = AddressHelper.BuildAddressViewModel(drivingLicenseOfficeDTO.Address)
            };

            return View("Create", drivingLicenseOfficeVm);
        }

        // GET: /DrivingLicenseOffice/Delete/id
        public ActionResult Delete(int id)
        {
            var errorCode = _drivingLicenseOfficeLookUpDatabaseService.DeleteQualificationPlace(new DrivingLicenseOfficeDTO
            {
                QualificationPlaceId = id
            });
            return errorCode == ErrorCode.NO_ERROR
              ? RedirectToAction("Index", "DrivingLicenseOffice")
              : RedirectToAction("Index", "Error", new { errorCodeParameter = errorCode });
        }

        // GET: /DrivingLicenseOffice/Create
        public ActionResult Create()
        {
            var drivingLicenseOfficeVm =
                (DrivingLicenseOfficeFormViewModel) InstatiateFormViewModel(new DrivingLicenseOfficeFormViewModel());
            return View(drivingLicenseOfficeVm);
        }

        //
        // POST: /DrivingLicenseOffice/Create/
        /// <summary>
        ///     Post method to create a drivingLicenseOffice
        /// </summary>
        /// <param name="iModel"></param>

        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(DrivingLicenseOfficeFormViewModel iModel)
        {
            if (!ModelState.IsValid)
            {
                ModelState.AddModelError("", _errorMessageFactoryService.
                    Create(ErrorCode.COMMON_FORM_VALIDATION_ERROR));
                iModel = (DrivingLicenseOfficeFormViewModel)InstatiateFormViewModel(iModel);
                return View(iModel);
            }

            var drivingLicenseOfficeDTO = new DrivingLicenseOfficeDTO
            {
                QualificationPlaceId = iModel.Id,
                QualificationPlaceName = iModel.Name,
                QualificationPlaceDescription = iModel.Description,
                QualificationPlaceWebSite = iModel.Website,
                Address = AddressHelper.ExtractAddressViewModel(iModel.AddressViewModel)
            };

            var errorCode =
                _drivingLicenseOfficeLookUpDatabaseService.CreateOrEditQualificationPlace(ref drivingLicenseOfficeDTO);
            if (errorCode == ErrorCode.NO_ERROR)
                return RedirectToAction("Index", "DrivingLicenseOffice");
            iModel = (DrivingLicenseOfficeFormViewModel)InstatiateFormViewModel(iModel);
            ModelState.AddModelError("", _errorMessageFactoryService.Create(errorCode));
            return View(iModel);
        }
    }
}