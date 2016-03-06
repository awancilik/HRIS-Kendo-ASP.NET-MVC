using CVScreeningCore.Error;
using CVScreeningService.DTO.LookUpDatabase;
using CVScreeningService.Services.Common;
using CVScreeningService.Services.ErrorHandling;
using CVScreeningService.Services.LookUpDatabase;
using CVScreeningWeb.Helpers;
using CVScreeningWeb.ViewModels.BaseQualificationPlace;
using CVScreeningWeb.ViewModels.PopulationOffice;
using System.Linq;
using System.Web.Mvc;

namespace CVScreeningWeb.Controllers
{
    [Authorize(Roles = "Administrator, Screener, Qualifier, Production manager")]
    [Filters.HandleError]
    public class PopulationOfficeController : Controller
    {
        private readonly ICommonService _commonService;
        private readonly IErrorMessageFactoryService _errorMessageFactoryService;
        private readonly ILookUpDatabaseService<PopulationOfficeDTO> _populationOfficeLookUpDatabaseService;

        public PopulationOfficeController(ILookUpDatabaseService<PopulationOfficeDTO> populationOfficeLookUpDatabaseService,
            ICommonService commonService, IErrorMessageFactoryService errorMessageFactoryService)
        {
            _populationOfficeLookUpDatabaseService = populationOfficeLookUpDatabaseService;
            _commonService = commonService;
            _errorMessageFactoryService = errorMessageFactoryService;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="iModel"></param>
        /// <returns></returns>
        private QualificationPlaceFormViewModel InstatiateFormViewModel(QualificationPlaceFormViewModel iModel)
        {
            var viewModel = iModel;
            viewModel.AddressViewModel = AddressHelper.BuildAddressViewModel();
            return viewModel;
        }

        /// <summary>
        /// Json action used by kendo ui to retrieve population office list
        /// </summary>
        /// <returns></returns>
        public JsonResult GetPopulationOffices()
        {
            var offices = _populationOfficeLookUpDatabaseService.GetAllQualificationPlaces();

            return Json(offices.Select(c => new { PopulationOfficeId = c.QualificationPlaceId, PopulationOfficeName = string.Format("{0} - {1}", c.QualificationPlaceName, AddressHelper.GetShortAddressAsString(c.Address)) }),
                JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public ActionResult Index()
        {
            var populationOffices = _populationOfficeLookUpDatabaseService.GetAllQualificationPlaces();
            var populationOfficeVMs = (from populationOffice in populationOffices
                                        let subDistrict = _commonService.GetLocation(populationOffice.Address.Location.LocationId)
                                        let district = subDistrict.LocationParentLocationId != null ? _commonService.GetLocation(subDistrict.LocationParentLocationId) : null
                                        select new PopulationOfficeManageViewModel
                                        {
                                            Id = populationOffice.QualificationPlaceId,
                                            Name = populationOffice.QualificationPlaceName,
                                            Address = populationOffice.Address.Location.LocationParentLocationId == null ?
                                             string.Format("{0}, {1}", populationOffice.Address.Street, populationOffice.Address.Location.LocationName) :
                                             string.Format(AddressHelper.GetAddressAsLabel(populationOffice.Address))
                                        }).ToList();
            return View(populationOfficeVMs);
        }

        // GET: /PopulationOffice/Detail

        /// <summary>
        ///     To view in detail specific populationOffice
        /// </summary>
        /// <param name="id">PopulationOffice ID</param>
        /// <returns></returns>
        public ActionResult Detail(int id)
        {
            var populationOfficeDTO = _populationOfficeLookUpDatabaseService.GetQualificationPlace(id);
            if (populationOfficeDTO == null)
            {
                ModelState.AddModelError("",
                    _errorMessageFactoryService.Create(ErrorCode.DBLOOKUP_QUALIFICATION_PLACE_NOT_FOUND));
                return RedirectToAction("Index", "Error", ErrorCode.DBLOOKUP_QUALIFICATION_PLACE_NOT_FOUND);
            }
            var populationOfficeVm =
                new PopulationOfficeFormViewModel
                {
                    Name = populationOfficeDTO.QualificationPlaceName,
                    Description = populationOfficeDTO.QualificationPlaceDescription,
                    Website = populationOfficeDTO.QualificationPlaceWebSite ?? "",
                    AddressViewModel = AddressHelper.BuildAddressViewModel(populationOfficeDTO.Address)
                };
            return View(populationOfficeVm);
        }

        // GET: /PopulationOffice/Edit/id
        /// <summary>
        ///     Edit a populationOffice
        /// </summary>
        /// <param name="id">PopulationOffice ID</param>
        /// <returns></returns>
        public ActionResult Edit(int id)
        {
            var populationOfficeDTO = _populationOfficeLookUpDatabaseService.GetQualificationPlace(id);
            if (populationOfficeDTO == null)
            {
                ModelState.AddModelError("",
                    _errorMessageFactoryService.Create(ErrorCode.DBLOOKUP_QUALIFICATION_PLACE_NOT_FOUND));
                return RedirectToAction("Index", "Error", ErrorCode.DBLOOKUP_QUALIFICATION_PLACE_NOT_FOUND);
            }
            var populationOfficeVm = new PopulationOfficeFormViewModel
            {
                Id = id,
                Name = populationOfficeDTO.QualificationPlaceName,
                Description = populationOfficeDTO.QualificationPlaceDescription,
                AddressViewModel = AddressHelper.BuildAddressViewModel(populationOfficeDTO.Address),
                Website = populationOfficeDTO.QualificationPlaceWebSite
            };

            return View("Create", populationOfficeVm);
        }

        // GET: /PopulationOffice/Delete/id
        public ActionResult Delete(int id)
        {
            var errorCode = _populationOfficeLookUpDatabaseService.DeleteQualificationPlace(new PopulationOfficeDTO
            {
                QualificationPlaceId = id
            });
            return errorCode == ErrorCode.NO_ERROR
                ? RedirectToAction("Index", "ImmigrationOffice")
                : RedirectToAction("Index", "Error", new { errorCodeParameter = errorCode });
        }

        // GET: /PopulationOffice/Create
        public ActionResult Create()
        {
            var populationOfficeVm =
                (PopulationOfficeFormViewModel)InstatiateFormViewModel(new PopulationOfficeFormViewModel());
            return View(populationOfficeVm);
        }

        //
        // POST: /PopulationOffice/Create/
        /// <summary>
        ///     Post method to create a populationOffice
        /// </summary>
        /// <param name="iModel"></param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(PopulationOfficeFormViewModel iModel)
        {
            if (!ModelState.IsValid)
            {
                ModelState.AddModelError("", _errorMessageFactoryService.
                    Create(ErrorCode.COMMON_FORM_VALIDATION_ERROR));
                iModel = (PopulationOfficeFormViewModel)InstatiateFormViewModel(new PopulationOfficeFormViewModel());
                return View(iModel);
            }

            var populationOfficeDTO = new PopulationOfficeDTO
            {
                QualificationPlaceId = iModel.Id,
                QualificationPlaceName = iModel.Name,
                QualificationPlaceDescription = iModel.Description,
                QualificationPlaceWebSite = iModel.Website,
                Address = AddressHelper.ExtractAddressViewModel(iModel.AddressViewModel)
            };

            var errorCode = _populationOfficeLookUpDatabaseService.CreateOrEditQualificationPlace(ref populationOfficeDTO);
            if (errorCode == ErrorCode.NO_ERROR)
                return RedirectToAction("Index", "PopulationOffice");
            iModel = (PopulationOfficeFormViewModel)InstatiateFormViewModel(new PopulationOfficeFormViewModel());
            ModelState.AddModelError("", _errorMessageFactoryService.Create(errorCode));
            return View(iModel);
        }
    }
}
