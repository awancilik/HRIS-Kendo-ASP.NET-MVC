using System.Linq;
using System.Web.Mvc;
using CVScreeningCore.Error;
using CVScreeningService.DTO.LookUpDatabase;
using CVScreeningService.Services.Common;
using CVScreeningService.Services.ErrorHandling;
using CVScreeningService.Services.LookUpDatabase;
using CVScreeningWeb.Helpers;
using CVScreeningWeb.ViewModels.BaseQualificationPlace;
using CVScreeningWeb.ViewModels.HighSchool;

namespace CVScreeningWeb.Controllers
{
    [Authorize(Roles = "Administrator, Screener, Qualifier, Production manager")]
    [Filters.HandleError]
    public class HighSchoolController : Controller
    {
        private readonly ICommonService _commonService;
        private readonly IErrorMessageFactoryService _errorMessageFactoryService;
        private readonly ILookUpDatabaseService<HighSchoolDTO> _highSchoolLookUpDatabaseService;

        public HighSchoolController(ILookUpDatabaseService<HighSchoolDTO> highSchoolLookUpDatabaseService,
            ICommonService commonService, IErrorMessageFactoryService errorMessageFactoryService)
        {
            _highSchoolLookUpDatabaseService = highSchoolLookUpDatabaseService;
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
        public JsonResult GetHighSchools()
        {
            var polices = _highSchoolLookUpDatabaseService.GetAllQualificationPlaces();

            return Json(polices.Select(c => new { HighSchoolId = c.QualificationPlaceId, HighSchoolName = string.Format("{0} - {1}", c.QualificationPlaceName, AddressHelper.GetShortAddressAsString(c.Address)) }),
                JsonRequestBehavior.AllowGet);
        }

        public ActionResult Index()
        {
            var highSchools = _highSchoolLookUpDatabaseService.GetAllQualificationPlaces();
            var highSchoolVMs = (from highSchool in highSchools
                let subDistrict = _commonService.GetLocation(highSchool.Address.Location.LocationId)
                let district = _commonService.GetLocation(subDistrict.LocationParentLocationId)
                select new HighSchoolManageViewModel
                {
                    Id = highSchool.QualificationPlaceId,
                    Name = highSchool.QualificationPlaceName,
                    Address = highSchool.Address.Location.LocationParentLocationId == null ?
                        string.Format("{0}, {1}", highSchool.Address.Street, highSchool.Address.Location.LocationName) :
                        string.Format(AddressHelper.GetAddressAsLabel(highSchool.Address))
                    }).ToList();
            return View(highSchoolVMs);
        }

        // GET: /HighSchool/Detail
        /// <summary>
        ///     To view in detail specific highSchool
        /// </summary>
        /// <param name="id">HighSchool ID</param>
        /// <returns></returns>
        public ActionResult Detail(int id)
        {
            var highSchoolDTO = _highSchoolLookUpDatabaseService.GetQualificationPlace(id);
            if (highSchoolDTO == null)
            {
                ModelState.AddModelError("",
                    _errorMessageFactoryService.Create(ErrorCode.DBLOOKUP_QUALIFICATION_PLACE_NOT_FOUND));
                return RedirectToAction("Index", "Error", ErrorCode.DBLOOKUP_QUALIFICATION_PLACE_NOT_FOUND);
            }

            var highSchoolVm = new HighSchoolFormViewModel
                {
                    Name = highSchoolDTO.QualificationPlaceName,
                    Description = highSchoolDTO.QualificationPlaceDescription,
                    Website = highSchoolDTO.QualificationPlaceWebSite ?? "",
                    AddressViewModel = AddressHelper.BuildAddressViewModel(highSchoolDTO.Address)
                };
            return View(highSchoolVm);
        }

        // GET: /HighSchool/Edit/id
        /// <summary>
        ///     Edit a highSchool
        /// </summary>
        /// <param name="id">HighSchool ID</param>
        /// <returns></returns>
        public ActionResult Edit(int id)
        {
            var highSchoolDTO = _highSchoolLookUpDatabaseService.GetQualificationPlace(id);
            if (highSchoolDTO == null)
            {
                ModelState.AddModelError("",
                    _errorMessageFactoryService.Create(ErrorCode.DBLOOKUP_QUALIFICATION_PLACE_NOT_FOUND));
                return RedirectToAction("Index", "Error", ErrorCode.DBLOOKUP_QUALIFICATION_PLACE_NOT_FOUND);
            }
            var highSchoolVm = new HighSchoolFormViewModel
            {
                Id = id,
                Name = highSchoolDTO.QualificationPlaceName,
                Description = highSchoolDTO.QualificationPlaceDescription,
                AddressViewModel = AddressHelper.BuildAddressViewModel(highSchoolDTO.Address),
                Website = highSchoolDTO.QualificationPlaceWebSite
            };
            return View("Create", highSchoolVm);
        }

        // GET: /HighSchool/Delete/id
        public ActionResult Delete(int id)
        {
            var errorCode = _highSchoolLookUpDatabaseService.DeleteQualificationPlace(new HighSchoolDTO
            {
                QualificationPlaceId = id
            });
            return errorCode == ErrorCode.NO_ERROR
                ? RedirectToAction("Index", "HighSchool")
                : RedirectToAction("Index", "Error", new {errorCodeParameter = errorCode});
        }

        // GET: /HighSchool/Create
        public ActionResult Create()
        {
            var highSchoolVm = (HighSchoolFormViewModel) InstatiateFormViewModel(new HighSchoolFormViewModel());
            return View(highSchoolVm);
        }

        //
        // POST: /HighSchool/Create/
        /// <summary>
        ///     Post method to create a highSchool
        /// </summary>
        /// <param name="iModel"></param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(HighSchoolFormViewModel iModel)
        {
            if (!ModelState.IsValid)
            {
                ModelState.AddModelError("", _errorMessageFactoryService.
                    Create(ErrorCode.COMMON_FORM_VALIDATION_ERROR));
                iModel = (HighSchoolFormViewModel)InstatiateFormViewModel(iModel);
                return View(iModel);
            }

            var highSchoolDTO = new HighSchoolDTO
            {
                QualificationPlaceId = iModel.Id,
                QualificationPlaceName = iModel.Name,
                QualificationPlaceDescription = iModel.Description,
                QualificationPlaceWebSite = iModel.Website,
                Address = AddressHelper.ExtractAddressViewModel(iModel.AddressViewModel)
            };

            var errorCode = _highSchoolLookUpDatabaseService.CreateOrEditQualificationPlace(ref highSchoolDTO);
            if (errorCode == ErrorCode.NO_ERROR)
                return RedirectToAction("Index", "HighSchool");
            iModel = (HighSchoolFormViewModel) InstatiateFormViewModel(iModel);
            ModelState.AddModelError("", _errorMessageFactoryService.Create(errorCode));
            return View(iModel);
        }
    }
}