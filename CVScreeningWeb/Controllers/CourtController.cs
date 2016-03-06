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
using CVScreeningWeb.ViewModels.Court;

namespace CVScreeningWeb.Controllers
{
    [Authorize(Roles = "Administrator, Screener, Qualifier, Production manager")]
    [Filters.HandleError]
    public class CourtController : Controller
    {
        private readonly ICommonService _commonService;
        private readonly IErrorMessageFactoryService _errorMessageFactoryService;
        private readonly ILookUpDatabaseService<CourtDTO> _courtLookUpDatabaseService;

        public CourtController(ILookUpDatabaseService<CourtDTO> courtLookUpDatabaseService,
            ICommonService commonService, IErrorMessageFactoryService errorMessageFactoryService)
        {
            _courtLookUpDatabaseService = courtLookUpDatabaseService;
            _commonService = commonService;
            _errorMessageFactoryService = errorMessageFactoryService;
        }

        private IDictionary<int, string> GetCategories()
        {
            return new Dictionary<int, string> {{1, "District"}, {2, "Commercial"}, {3, "Industrial"}};
        }

        /// <summary>
        /// Json action used by kendo ui to retrieve commercial court list
        /// </summary>
        /// <returns></returns>
        public JsonResult GetCommercialCourts()
        {
            var courts = _courtLookUpDatabaseService.GetAllQualificationPlaces();

            return Json(courts.Where(c => c.QualificationPlaceCategory == CourtDTO.kCommercialCategory).
                Select(c => new { CommercialCourtId = c.QualificationPlaceId, CommercialCourtName = string.Format("{0} - {1}", c.QualificationPlaceName, AddressHelper.GetShortAddressAsString(c.Address)) }),
                JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Json action used by kendo ui to retrieve industrial court list
        /// </summary>
        /// <returns></returns>
        public JsonResult GetIndustrialCourts()
        {
            var courts = _courtLookUpDatabaseService.GetAllQualificationPlaces();

            return Json(courts.Where(c => c.QualificationPlaceCategory == CourtDTO.kIndustrialCategory).
                Select(c => new { IndustrialCourtId = c.QualificationPlaceId, IndustrialCourtName = string.Format("{0} - {1}", c.QualificationPlaceName, AddressHelper.GetShortAddressAsString(c.Address)) }),
                JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Json action used by kendo ui to retrieve district court list
        /// </summary>
        /// <returns></returns>
        public JsonResult GetDistrictCourts()
        {
            var courts = _courtLookUpDatabaseService.GetAllQualificationPlaces();

            return Json(courts.Where(c => c.QualificationPlaceCategory == CourtDTO.kDistrictCategory).
                Select(c => new { DistrictCourtId = c.QualificationPlaceId, DistrictCourtName = string.Format("{0} - {1}", c.QualificationPlaceName, AddressHelper.GetShortAddressAsString(c.Address)) }),
                JsonRequestBehavior.AllowGet);
        }

        public ActionResult Index()
        {
            var courts = _courtLookUpDatabaseService.GetAllQualificationPlaces();
            var courtVMs = (from court in courts
                let subDistrict = _commonService.GetLocation(court.Address.Location.LocationId)
                let district = subDistrict.LocationParentLocationId != null ? _commonService.GetLocation(subDistrict.LocationParentLocationId) : null
                select new CourtManageViewModel
                {
                    Id = court.QualificationPlaceId,
                    Name = court.QualificationPlaceName,
                    Category = court.QualificationPlaceCategory,
                    Address = court.Address.Location.LocationParentLocationId == null ?
                            string.Format("{0}, {1}", court.Address.Street, court.Address.Location.LocationName) :
                            string.Format(AddressHelper.GetAddressAsLabel(court.Address))
                }).ToList();
            return View(courtVMs);
        }

        // GET: /Court/Detail

        /// <summary>
        ///     To view in detail specific court
        /// </summary>
        /// <param name="id">Court ID</param>
        /// <returns></returns>
        public ActionResult Detail(int id)
        {
            var courtDTO = _courtLookUpDatabaseService.GetQualificationPlace(id);
            if (courtDTO == null)
            {
                ModelState.AddModelError("",
                    _errorMessageFactoryService.Create(ErrorCode.DBLOOKUP_QUALIFICATION_PLACE_NOT_FOUND));
                return RedirectToAction("Index", "Error", ErrorCode.DBLOOKUP_QUALIFICATION_PLACE_NOT_FOUND);
            }
            var courtVm =
                new CourtFormViewModel
                {
                    Name = courtDTO.QualificationPlaceName,
                    Description = courtDTO.QualificationPlaceDescription,
                    Website = courtDTO.QualificationPlaceWebSite ?? "",
                    Category = FormHelper.BuildDropDownListViewModel(
                        GetCategories(), selectedValue: courtDTO.QualificationPlaceCategory),
                    AddressViewModel = AddressHelper.BuildAddressViewModel(courtDTO.Address)
                };
            return View(courtVm);
        }

        // GET: /Court/Edit/id
        /// <summary>
        ///     Edit a court
        /// </summary>
        /// <param name="id">Court ID</param>
        /// <returns></returns>
        public ActionResult Edit(int id)
        {
            var courtDTO = _courtLookUpDatabaseService.GetQualificationPlace(id);
            if (courtDTO == null)
            {
                ModelState.AddModelError("",
                    _errorMessageFactoryService.Create(ErrorCode.DBLOOKUP_QUALIFICATION_PLACE_NOT_FOUND));
                return RedirectToAction("Index", "Error", ErrorCode.DBLOOKUP_QUALIFICATION_PLACE_NOT_FOUND);
            }
            var courtVm = new CourtFormViewModel
            {
                Id = id,
                Name = courtDTO.QualificationPlaceName,
                Description = courtDTO.QualificationPlaceDescription,
                Website = courtDTO.QualificationPlaceWebSite,
                Category = FormHelper.BuildDropDownListViewModel(
                    GetCategories(), selectedValue: courtDTO.QualificationPlaceCategory),
                AddressViewModel = AddressHelper.BuildAddressViewModel(courtDTO.Address)
            };

            return View("Create", courtVm);
        }

        // GET: /Court/Delete/id
        public ActionResult Delete(int id)
        {
            var errorCode = _courtLookUpDatabaseService.DeleteQualificationPlace(new CourtDTO
            {
                QualificationPlaceId = id
            });
            return errorCode == ErrorCode.NO_ERROR
                ? RedirectToAction("Index", "Court")
                : RedirectToAction("Index", "Error", new {errorCodeParameter = errorCode});
        }

        // GET: /Court/Create
        public ActionResult Create()
        {
            var courtVm = (CourtFormViewModel) InstatiateFormViewModel(new CourtFormViewModel());
            return View(courtVm);
        }

        private QualificationPlaceFormViewModel InstatiateFormViewModel(QualificationPlaceFormViewModel iModel)
        {
            var viewModel = iModel;
            viewModel.AddressViewModel = AddressHelper.BuildAddressViewModel();
            viewModel.Category = FormHelper.BuildDropDownListViewModel(GetCategories());
            return viewModel;
        }

        //
        // POST: /Court/Create/
        /// <summary>
        ///     Post method to create a court
        /// </summary>
        /// <param name="iModel"></param>

        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(CourtFormViewModel iModel)
        {
            if (!ModelState.IsValid)
            {
                ModelState.AddModelError("", _errorMessageFactoryService.
                    Create(ErrorCode.COMMON_FORM_VALIDATION_ERROR));
                iModel = (CourtFormViewModel)InstatiateFormViewModel(iModel);
                return View(iModel);
            }

            var courtDTO = new CourtDTO
            {
                QualificationPlaceId = iModel.Id,
                QualificationPlaceName = iModel.Name,
                QualificationPlaceDescription = iModel.Description,
                QualificationPlaceWebSite = iModel.Website,
                QualificationPlaceCategory = GetCategories()[FormHelper.ExtractDropDownListViewModel(iModel.Category)],
                Address = AddressHelper.ExtractAddressViewModel(iModel.AddressViewModel)
            };

            var errorCode = _courtLookUpDatabaseService.CreateOrEditQualificationPlace(ref courtDTO);
            if (errorCode == ErrorCode.NO_ERROR)
                return RedirectToAction("Index", "Court");
            
            iModel = (CourtFormViewModel) InstatiateFormViewModel(iModel);
            ModelState.AddModelError("", _errorMessageFactoryService.Create(errorCode));
            return View(iModel);
        }
    }
}