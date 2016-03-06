using System.Linq;
using System.Web.Mvc;
using CVScreeningCore.Error;
using CVScreeningService.DTO.LookUpDatabase;
using CVScreeningService.Services.Common;
using CVScreeningService.Services.ErrorHandling;
using CVScreeningService.Services.LookUpDatabase;
using CVScreeningWeb.Helpers;
using CVScreeningWeb.ViewModels.BaseQualificationPlace;
using CVScreeningWeb.ViewModels.ImmigrationOffice;

namespace CVScreeningWeb.Controllers
{
    [Authorize(Roles = "Administrator, Screener, Qualifier, Production manager")]
    [Filters.HandleError]
    public class ImmigrationOfficeController : Controller
    {
        private readonly ICommonService _commonService;
        private readonly IErrorMessageFactoryService _errorMessageFactoryService;
        private readonly ILookUpDatabaseService<ImmigrationOfficeDTO> _immigrationOfficeLookUpDatabaseService;

        public ImmigrationOfficeController(ILookUpDatabaseService<ImmigrationOfficeDTO> immigrationOfficeLookUpDatabaseService,
            ICommonService commonService, IErrorMessageFactoryService errorMessageFactoryService)
        {
            _immigrationOfficeLookUpDatabaseService = immigrationOfficeLookUpDatabaseService;
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
        /// Json action used by kendo ui to retrieve immigration office list
        /// </summary>
        /// <returns></returns>
        public JsonResult GetImmigrationOffices()
        {
            var offices = _immigrationOfficeLookUpDatabaseService.GetAllQualificationPlaces();

            return Json(offices.Select(c => new { ImmigrationOfficeId = c.QualificationPlaceId, ImmigrationOfficeName = string.Format("{0} - {1}", c.QualificationPlaceName, AddressHelper.GetShortAddressAsString(c.Address)) }),
                JsonRequestBehavior.AllowGet);
        }

        public ActionResult Index()
        {
            var immigrationOffices = _immigrationOfficeLookUpDatabaseService.GetAllQualificationPlaces();
            var immigrationOfficeVMs = (from immigrationOffice in immigrationOffices
                                                       let subDistrict = _commonService.GetLocation(immigrationOffice.Address.Location.LocationId)
                                                       let district = subDistrict.LocationParentLocationId != null ? _commonService.GetLocation(subDistrict.LocationParentLocationId) : null
                                                       select new ImmigrationOfficeManageViewModel
                                                       {
                                                           Id = immigrationOffice.QualificationPlaceId,
                                                           Name = immigrationOffice.QualificationPlaceName,
                                                           Address = immigrationOffice.Address.Location.LocationParentLocationId == null ?
                                                            string.Format("{0}, {1}", immigrationOffice.Address.Street, immigrationOffice.Address.Location.LocationName) :
                                                            string.Format(AddressHelper.GetAddressAsLabel(immigrationOffice.Address))
                                                       }).ToList();
            return View(immigrationOfficeVMs);
        }

        // GET: /ImmigrationOffice/Detail

        /// <summary>
        ///     To view in detail specific immigrationOffice
        /// </summary>
        /// <param name="id">ImmigrationOffice ID</param>
        /// <returns></returns>
        public ActionResult Detail(int id)
        {
            var immigrationOfficeDTO = _immigrationOfficeLookUpDatabaseService.GetQualificationPlace(id);
            if (immigrationOfficeDTO == null)
            {
                ModelState.AddModelError("",
                    _errorMessageFactoryService.Create(ErrorCode.DBLOOKUP_QUALIFICATION_PLACE_NOT_FOUND));
                return RedirectToAction("Index", "Error", ErrorCode.DBLOOKUP_QUALIFICATION_PLACE_NOT_FOUND);
            }  
            var immigrationOfficeVm =
                new ImmigrationOfficeFormViewModel
                {
                    Name = immigrationOfficeDTO.QualificationPlaceName,
                    Description = immigrationOfficeDTO.QualificationPlaceDescription,
                    Website = immigrationOfficeDTO.QualificationPlaceWebSite ?? "",
                    AddressViewModel = AddressHelper.BuildAddressViewModel(immigrationOfficeDTO.Address)
                                    };
            return View(immigrationOfficeVm);
        }

        // GET: /ImmigrationOffice/Edit/id
        /// <summary>
        ///     Edit a immigrationOffice
        /// </summary>
        /// <param name="id">ImmigrationOffice ID</param>
        /// <returns></returns>
        public ActionResult Edit(int id)
        {
            var immigrationOfficeDTO = _immigrationOfficeLookUpDatabaseService.GetQualificationPlace(id);
            if (immigrationOfficeDTO == null)
            {
                ModelState.AddModelError("",
                    _errorMessageFactoryService.Create(ErrorCode.DBLOOKUP_QUALIFICATION_PLACE_NOT_FOUND));
                return RedirectToAction("Index", "Error", ErrorCode.DBLOOKUP_QUALIFICATION_PLACE_NOT_FOUND);
            }
            var immigrationOfficeVm = new ImmigrationOfficeFormViewModel
            {
                Id = id,
                Name = immigrationOfficeDTO.QualificationPlaceName,
                Description = immigrationOfficeDTO.QualificationPlaceDescription,
                AddressViewModel = AddressHelper.BuildAddressViewModel(immigrationOfficeDTO.Address),
                Website = immigrationOfficeDTO.QualificationPlaceWebSite
            };

            return View("Create", immigrationOfficeVm);
        }

        // GET: /ImmigrationOffice/Delete/id
        public ActionResult Delete(int id)
        {
            var errorCode = _immigrationOfficeLookUpDatabaseService.DeleteQualificationPlace(new ImmigrationOfficeDTO
            {
                QualificationPlaceId = id
            });
            return errorCode == ErrorCode.NO_ERROR
                ? RedirectToAction("Index", "ImmigrationOffice")
                : RedirectToAction("Index", "Error", new {errorCodeParameter = errorCode});
        }

        // GET: /ImmigrationOffice/Create
        public ActionResult Create()
        {
            var immigrationOfficeVm =
                (ImmigrationOfficeFormViewModel) InstatiateFormViewModel(new ImmigrationOfficeFormViewModel());
            return View(immigrationOfficeVm);
        }

        //
        // POST: /ImmigrationOffice/Create/
        /// <summary>
        ///     Post method to create a immigrationOffice
        /// </summary>
        /// <param name="iModel"></param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(ImmigrationOfficeFormViewModel iModel)
        {
            if (!ModelState.IsValid)
            {
                ModelState.AddModelError("", _errorMessageFactoryService.
                    Create(ErrorCode.COMMON_FORM_VALIDATION_ERROR));
                iModel = (ImmigrationOfficeFormViewModel)InstatiateFormViewModel(new ImmigrationOfficeFormViewModel());
                return View(iModel);
            }

            var immigrationOfficeDTO = new ImmigrationOfficeDTO
            {
                QualificationPlaceId = iModel.Id,
                QualificationPlaceName = iModel.Name,
                QualificationPlaceDescription = iModel.Description,
                QualificationPlaceWebSite = iModel.Website,
                Address = AddressHelper.ExtractAddressViewModel(iModel.AddressViewModel)
            };

            var errorCode = _immigrationOfficeLookUpDatabaseService.CreateOrEditQualificationPlace(ref immigrationOfficeDTO);
            if (errorCode == ErrorCode.NO_ERROR)
                return RedirectToAction("Index", "ImmigrationOffice");
            iModel = (ImmigrationOfficeFormViewModel)InstatiateFormViewModel(new ImmigrationOfficeFormViewModel());
            ModelState.AddModelError("", _errorMessageFactoryService.Create(errorCode));
            return View(iModel);
        }
    }
}