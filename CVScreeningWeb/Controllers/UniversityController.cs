using System.Linq;
using System.Web.Mvc;
using CVScreeningCore.Error;
using CVScreeningService.DTO.LookUpDatabase;
using CVScreeningService.Services.Common;
using CVScreeningService.Services.ErrorHandling;
using CVScreeningService.Services.LookUpDatabase;
using CVScreeningWeb.Helpers;
using CVScreeningWeb.ViewModels.University;

namespace CVScreeningWeb.Controllers
{
    [Authorize(Roles = "Administrator, Screener, Qualifier, Production manager")]
    [Filters.HandleError]
    public class UniversityController : Controller
    {
        private readonly ICommonService _commonService;
        private readonly IErrorMessageFactoryService _errorMessageFactoryService;
        private readonly ILookUpDatabaseService<FacultyDTO> _facultyLookUpDatabaseService;
        private readonly IUniversityLookUpDatabaseService _universityLookUpDatabaseService;

        public UniversityController(IUniversityLookUpDatabaseService universityLookUpDatabaseService,
            ILookUpDatabaseService<FacultyDTO> facultyLookUpDatabaseService, ICommonService commonService,
            IErrorMessageFactoryService errorMessageFactoryService)
        {
            _universityLookUpDatabaseService = universityLookUpDatabaseService;
            _facultyLookUpDatabaseService = facultyLookUpDatabaseService;
            _commonService = commonService;
            _errorMessageFactoryService = errorMessageFactoryService;
            ViewBag.IsKendoEnabled = true;
        }

        private FacultyFormViewModel InstatiateFormViewModel(FacultyFormViewModel iModel)
        {
            var viewModel = iModel;
            viewModel.AddressViewModel = AddressHelper.BuildAddressViewModel();
            return viewModel;
        }

        /// <summary>
        /// Json action used by kendo ui to retrieve faculty list
        /// </summary>
        /// <returns></returns>
        public JsonResult GetFaculties(int? id)
        {
            var faculties = _facultyLookUpDatabaseService.GetAllQualificationPlaces();
            if (id != null)
                faculties = faculties.Where(u => u.University.UniversityId == id).ToList();

            return Json(faculties.Select(c => new { FacultyId = c.QualificationPlaceId, FacultyName = string.Format("{0} - {1}", c.QualificationPlaceName, AddressHelper.GetShortAddressAsString(c.Address)) }),
                JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Json action used by kendo ui to retrieve university list
        /// </summary>
        /// <returns></returns>
        public JsonResult GetUniversities()
        {
            var universities = _universityLookUpDatabaseService.GetAllUniversities();

            return Json(universities.Select(c => new { UniversityId = c.UniversityId, UniversityName = c.UniversityName }),
                JsonRequestBehavior.AllowGet);
        }


        public ActionResult Index()
        {
            var universities = _universityLookUpDatabaseService.GetAllUniversities();
            var universityVms =
                universities.Select(universityDTO => new UniversityManageViewModel
                {
                    Id = universityDTO.UniversityId,
                    Name = universityDTO.UniversityName,
                    WebSite = universityDTO.UniversityWebSite
                }).ToList();

            return View(universityVms);
        }

        public ActionResult Create()
        {
            var universityVm = new UniversityFormViewModel();
            return View(universityVm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(UniversityFormViewModel iModel)
        {
            if (!ModelState.IsValid)
            {
                ModelState.AddModelError("", _errorMessageFactoryService.
                    Create(ErrorCode.COMMON_FORM_VALIDATION_ERROR));
                return View(iModel);
            }

            var universityDTO = new UniversityDTO
            {
                UniversityId = iModel.Id,
                UniversityName = iModel.Name,
                UniversityWebSite = iModel.WebSite
            };

            var errorCode = _universityLookUpDatabaseService.CreateOrUpdateUniversity(ref universityDTO);
            if (errorCode == ErrorCode.NO_ERROR)
                return RedirectToAction("Index");
            ModelState.AddModelError("", _errorMessageFactoryService.Create(errorCode));
            return View(iModel);
        }

        public ActionResult Detail(int id)
        {
            var universityBo = _universityLookUpDatabaseService.GetUniversity(id);
            var faculties = _facultyLookUpDatabaseService.GetAllQualificationPlaces().Where(
                fac => fac.University.UniversityId == id).ToList();
            var universityVm = new UniversityFormViewModel
            {
                Name = universityBo.UniversityName,
                WebSite = universityBo.UniversityWebSite,
                Id = id,
                Faculties = faculties.Select(fac => new FacultyManageViewModel
                {
                    Name = fac.QualificationPlaceName
                })
            };

            return View(universityVm);
        }

        public ActionResult Edit(int id)
        {
            var universityDTO = _universityLookUpDatabaseService.GetUniversity(id);
            var universityVm = new UniversityFormViewModel
            {
                Id = universityDTO.UniversityId,
                Name = universityDTO.UniversityName,
                WebSite = universityDTO.UniversityWebSite
            };
            return View("Create", universityVm);
        }

        public ActionResult Delete(int id)
        {
            var errorCode = _universityLookUpDatabaseService.DeleteUniversity(id);
            return errorCode == ErrorCode.NO_ERROR ? RedirectToAction("Index")
                : RedirectToAction("Index", "Error", new { errorCodeParameter = errorCode });
        }

        public ActionResult ManageFaculty(int id)
        {
            var faculties = _facultyLookUpDatabaseService.GetAllQualificationPlaces();
            var facultyVMs = (from faculty in faculties
                where (faculty.University != null && faculty.University.UniversityId == id)
                let subDistrict = _commonService.GetLocation(faculty.Address.Location.LocationId)
                let district = subDistrict.LocationParentLocationId != null ? _commonService.GetLocation(subDistrict.LocationParentLocationId) : null
                select new FacultyManageViewModel
                {
                    Id = faculty.QualificationPlaceId,
                    Name = faculty.QualificationPlaceName,
                    Address = faculty.Address.Location.LocationParentLocationId == null ?
                        string.Format("{0}, {1}", faculty.Address.Street, faculty.Address.Location.LocationName) :
                        string.Format(AddressHelper.GetAddressAsLabel(faculty.Address))
                }).ToList();

            var facultyVm = new FacultyManageUniversityViewModel
            {
                Faculties = facultyVMs,
                UniversityId = id,
                UniversityName = _universityLookUpDatabaseService.GetUniversity(id).UniversityName
            };
            return View(facultyVm);
        }

        public ActionResult DetailFaculty(int id)
        {
            var facultyDTO = _facultyLookUpDatabaseService.GetQualificationPlace(id);
            var facultyVm = new FacultyFormViewModel
            {
                Id = facultyDTO.QualificationPlaceId,
                Name = facultyDTO.QualificationPlaceName,
                Website = facultyDTO.QualificationPlaceWebSite,
                Description = facultyDTO.QualificationPlaceDescription,
                AlumniWebsite = facultyDTO.QualificationPlaceAlumniWebSite,
                UniversityId = facultyDTO.University.UniversityId,
                AddressViewModel = AddressHelper.BuildAddressViewModel(facultyDTO.Address)
            };
            return View(facultyVm);
        }

        public ActionResult CreateFaculty(int universityId)
        {
            var facultyVm = InstatiateFormViewModel(new FacultyFormViewModel
            {
                UniversityId = universityId
            });
            return View(facultyVm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateFaculty(FacultyFormViewModel iModel)
        {
            if (!ModelState.IsValid)
            {
                ModelState.AddModelError("", _errorMessageFactoryService.
                    Create(ErrorCode.COMMON_FORM_VALIDATION_ERROR));
                iModel = InstatiateFormViewModel(iModel);
                return View(iModel);
            }

            var facultyDTO = new FacultyDTO
            {
                QualificationPlaceId = iModel.Id,
                QualificationPlaceName = iModel.Name,
                QualificationPlaceDescription = iModel.Description,
                QualificationPlaceWebSite = iModel.Website,
                QualificationPlaceAlumniWebSite = iModel.AlumniWebsite,
                University = new UniversityDTO{ UniversityId = iModel.UniversityId},
                Address = AddressHelper.ExtractAddressViewModel(iModel.AddressViewModel)
            };
            
            var errorCode = _facultyLookUpDatabaseService.CreateOrEditQualificationPlace(ref facultyDTO);
            if (errorCode == ErrorCode.NO_ERROR)
                return RedirectToAction("ManageFaculty", "University", new {id = iModel.UniversityId});
            iModel = InstatiateFormViewModel(iModel);
            ModelState.AddModelError("", _errorMessageFactoryService.Create(errorCode));
            return View(iModel);
        }

        public ActionResult EditFaculty(int id)
        {
            var facultyDTO = _facultyLookUpDatabaseService.GetQualificationPlace(id);
            var facultyVm = new FacultyFormViewModel
            {
                Id = facultyDTO.QualificationPlaceId,
                Name = facultyDTO.QualificationPlaceName,
                Description = facultyDTO.QualificationPlaceDescription,
                Website = facultyDTO.QualificationPlaceWebSite,
                AlumniWebsite = facultyDTO.QualificationPlaceAlumniWebSite,
                UniversityId = facultyDTO.University.UniversityId,
                AddressViewModel = AddressHelper.BuildAddressViewModel(facultyDTO.Address)
            };
            return View("CreateFaculty", facultyVm);
        }

        public ActionResult DeleteFaculty(int id)
        {
            var facultyDTO = _facultyLookUpDatabaseService.GetQualificationPlace(id);

            var errorCode = _facultyLookUpDatabaseService.DeleteQualificationPlace(new FacultyDTO
            {
                QualificationPlaceId = id
            });
            return errorCode == ErrorCode.NO_ERROR
                ? RedirectToAction("ManageFaculty", "University", new { id = facultyDTO.University.UniversityId })
                : RedirectToAction("Index", "Error", new {errorCodeParameter = errorCode});
        }
    }
}