using System.Linq;
using System.Web.Mvc;
using CVScreeningCore.Error;
using CVScreeningService.DTO.LookUpDatabase;
using CVScreeningService.Services.Common;
using CVScreeningService.Services.ErrorHandling;
using CVScreeningService.Services.LookUpDatabase;
using CVScreeningWeb.Helpers;
using CVScreeningWeb.ViewModels.BaseQualificationPlace;
using CVScreeningWeb.ViewModels.Police;

namespace CVScreeningWeb.Controllers
{
    [Authorize(Roles = "Administrator, Screener, Qualifier, Production manager")]
    [Filters.HandleError]
    public class PoliceController : Controller
    {
        private readonly ICommonService _commonService;
        private readonly IErrorMessageFactoryService _errorMessageFactoryService;
        private readonly ILookUpDatabaseService<PoliceDTO> _policeLookUpDatabaseService;

        public PoliceController(ILookUpDatabaseService<PoliceDTO> policeLookUpDatabaseService,
            ICommonService commonService, IErrorMessageFactoryService errorMessageFactoryService)
        {
            _policeLookUpDatabaseService = policeLookUpDatabaseService;
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
        /// Json action used by kendo ui to retrieve police list
        /// </summary>
        /// <returns></returns>
        public JsonResult GetPolices()
        {
            var polices = _policeLookUpDatabaseService.GetAllQualificationPlaces();
            return Json(polices.Select(c => new { PoliceId = c.QualificationPlaceId, PoliceName = string.Format("{0} - {1}", c.QualificationPlaceName, AddressHelper.GetShortAddressAsString(c.Address)) }),
                JsonRequestBehavior.AllowGet);
        }

        public ActionResult Index()
        {
            var polices = _policeLookUpDatabaseService.GetAllQualificationPlaces();
            var policeVMs = (from police in polices
                let subDistrict = _commonService.GetLocation(police.Address.Location.LocationId)
                let district = subDistrict.LocationParentLocationId != null ? _commonService.GetLocation(subDistrict.LocationParentLocationId) : null
                select new PoliceManageViewModel
                {
                    Id = police.QualificationPlaceId,
                    Name = police.QualificationPlaceName,
                    Address = police.Address.Location.LocationParentLocationId == null ?
                        string.Format("{0}, {1}", police.Address.Street, police.Address.Location.LocationName) :
                        string.Format(AddressHelper.GetAddressAsLabel(police.Address))
                }).ToList();
            return View(policeVMs);
        }

        // GET: /Police/Detail

        /// <summary>
        ///     To view in detail specific police
        /// </summary>
        /// <param name="id">Police ID</param>
        /// <returns></returns>
        public ActionResult Detail(int id)
        {
            var policeDTO = _policeLookUpDatabaseService.GetQualificationPlace(id);
            if (policeDTO == null)
            {
                ModelState.AddModelError("",
                    _errorMessageFactoryService.Create(ErrorCode.DBLOOKUP_QUALIFICATION_PLACE_NOT_FOUND));
                return RedirectToAction("Index", "Error", ErrorCode.DBLOOKUP_QUALIFICATION_PLACE_NOT_FOUND);
            }
            var policeVm =
                new PoliceFormViewModel
                {
                    Name = policeDTO.QualificationPlaceName,
                    Description = policeDTO.QualificationPlaceDescription,
                    Website = policeDTO.QualificationPlaceWebSite ?? "",
                    AddressViewModel = AddressHelper.BuildAddressViewModel(policeDTO.Address)
                };
            return View(policeVm);
        }

        // GET: /Police/Edit/id
        /// <summary>
        ///     Edit a police
        /// </summary>
        /// <param name="id">Police ID</param>
        /// <returns></returns>
        public ActionResult Edit(int id)
        {
            var policeDTO = _policeLookUpDatabaseService.GetQualificationPlace(id);
            if (policeDTO == null)
            {
                ModelState.AddModelError("",
                    _errorMessageFactoryService.Create(ErrorCode.DBLOOKUP_QUALIFICATION_PLACE_NOT_FOUND));
                return RedirectToAction("Index", "Error", ErrorCode.DBLOOKUP_QUALIFICATION_PLACE_NOT_FOUND);
            }
            var policeVm = new PoliceFormViewModel
            {
                Id = id,
                Name = policeDTO.QualificationPlaceName,
                Description = policeDTO.QualificationPlaceDescription,
                AddressViewModel = AddressHelper.BuildAddressViewModel(policeDTO.Address),
                Website = policeDTO.QualificationPlaceWebSite
            };

            return View("Create", policeVm);
        }

        // GET: /Police/Delete/id
        public ActionResult Delete(int id)
        {
            var errorCode = _policeLookUpDatabaseService.DeleteQualificationPlace(new PoliceDTO
            {
                QualificationPlaceId = id
            });
            return errorCode == ErrorCode.NO_ERROR
                ? RedirectToAction("Index", "Police")
                : RedirectToAction("Index", "Error", new {errorCodeParameter = errorCode});
        }

        // GET: /Police/Create
        public ActionResult Create()
        {
            var policeVm = (PoliceFormViewModel) InstatiateFormViewModel(new PoliceFormViewModel());
            return View(policeVm);
        }

        //
        // POST: /Police/Create/
        /// <summary>
        ///     Post method to create a police
        /// </summary>
        /// <param name="iModel"></param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(PoliceFormViewModel iModel)
        {
            if (!ModelState.IsValid)
            {
                ModelState.AddModelError("", _errorMessageFactoryService.
                    Create(ErrorCode.COMMON_FORM_VALIDATION_ERROR));
                iModel = (PoliceFormViewModel)InstatiateFormViewModel(new PoliceFormViewModel());
                return View(iModel);
            }

            var policeDTO = new PoliceDTO
            {
                QualificationPlaceId = iModel.Id,
                QualificationPlaceName = iModel.Name,
                QualificationPlaceDescription = iModel.Description,
                QualificationPlaceWebSite = iModel.Website,
                Address = AddressHelper.ExtractAddressViewModel(iModel.AddressViewModel)
            };

            var errorCode = _policeLookUpDatabaseService.CreateOrEditQualificationPlace(ref policeDTO);
            if (errorCode == ErrorCode.NO_ERROR)
                return RedirectToAction("Index", "Police");
            iModel = (PoliceFormViewModel)InstatiateFormViewModel(new PoliceFormViewModel());
            ModelState.AddModelError("", _errorMessageFactoryService.Create(errorCode));
            return View(iModel);
        }
    }
}