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
using CVScreeningWeb.ViewModels.Company;

namespace CVScreeningWeb.Controllers
{

    [Filters.HandleError]
    [Authorize(Roles = "Administrator, Screener, Qualifier, Production manager")]
    public class CompanyController : Controller
    {
        private readonly ICommonService _commonService;
        private readonly IErrorMessageFactoryService _errorMessageFactoryService;
        private readonly ILookUpDatabaseService<CompanyDTO> _companyLookUpDatabaseService;

        public CompanyController(ILookUpDatabaseService<CompanyDTO> companyLookUpDatabaseService,
            ICommonService commonService, IErrorMessageFactoryService errorMessageFactoryService)
        {
            _companyLookUpDatabaseService = companyLookUpDatabaseService;
            _commonService = commonService;
            _errorMessageFactoryService = errorMessageFactoryService;
        }

        /// <summary>
        /// Json action used by kendo ui to retrieve company list
        /// </summary>
        /// <returns></returns>
        public JsonResult GetCompanies()
        {
            var companies = _companyLookUpDatabaseService.GetAllQualificationPlaces();

            return Json(companies.Select(c => new { CompanyId = c.QualificationPlaceId, CompanyName = string.Format("{0} - {1}", c.QualificationPlaceName, AddressHelper.GetShortAddressAsString(c.Address)) }),
                JsonRequestBehavior.AllowGet);
        }

        public ActionResult Index()
        {
            var companys = _companyLookUpDatabaseService.GetAllQualificationPlaces();
            var companyVMs = (from company in companys
                let subDistrict = _commonService.GetLocation(company.Address.Location.LocationId)
                let district = subDistrict.LocationParentLocationId != null ? _commonService.GetLocation(subDistrict.LocationParentLocationId) : null
                select new CompanyManageViewModel
                {
                    Id = company.QualificationPlaceId,
                    Name = company.QualificationPlaceName,
                    Address = company.Address.Location.LocationParentLocationId == null? 
                        string.Format("{0}, {1}", company.Address.Street, company.Address.Location.LocationName) :
                        string.Format(AddressHelper.GetAddressAsLabel(company.Address))
                }).ToList();
            return View(companyVMs);
        }

        // GET: /Company/Detail

        /// <summary>
        ///     To view in detail specific company
        /// </summary>
        /// <param name="id">Company ID</param>
        /// <returns></returns>
        public ActionResult Detail(int id)
        {
            var companyDTO = _companyLookUpDatabaseService.GetQualificationPlace(id);
            if (companyDTO == null)
            {
                ModelState.AddModelError("",
                    _errorMessageFactoryService.Create(ErrorCode.DBLOOKUP_QUALIFICATION_PLACE_NOT_FOUND));
                return RedirectToAction("Index", "Error", ErrorCode.DBLOOKUP_QUALIFICATION_PLACE_NOT_FOUND);
            }
            var companyVm =
                new CompanyFormViewModel
                {
                    Name = companyDTO.QualificationPlaceName,
                    Description = companyDTO.QualificationPlaceDescription,
                    Category = FormHelper.BuildDropDownListViewModel(GetCategories(), 
                        selectedValue:companyDTO.QualificationPlaceCategory),
                    Website = companyDTO.QualificationPlaceWebSite ?? "",
                    AddressViewModel = AddressHelper.BuildAddressViewModel(companyDTO.Address)
                };
            return View(companyVm);
        }

        // GET: /Company/Edit/id
        /// <summary>
        ///     Edit a company
        /// </summary>
        /// <param name="id">Company ID</param>
        /// <returns></returns>
        public ActionResult Edit(int id)
        {
            var companyDTO = _companyLookUpDatabaseService.GetQualificationPlace(id);
            if (companyDTO == null)
            {
                ModelState.AddModelError("",
                    _errorMessageFactoryService.Create(ErrorCode.DBLOOKUP_QUALIFICATION_PLACE_NOT_FOUND));
                return RedirectToAction("Index", "Error", ErrorCode.DBLOOKUP_QUALIFICATION_PLACE_NOT_FOUND);
            }
            var companyVm = new CompanyFormViewModel
            {
                Id = id,
                Name = companyDTO.QualificationPlaceName,
                Category = FormHelper.BuildDropDownListViewModel(GetCategories(),
                       selectedValue:companyDTO.QualificationPlaceCategory),
                Description = companyDTO.QualificationPlaceDescription,
                Website = companyDTO.QualificationPlaceWebSite,
                AddressViewModel = AddressHelper.BuildAddressViewModel(companyDTO.Address)
            };
            return View("Create", companyVm);
        }

        // GET: /Company/Delete/id
        public ActionResult Delete(int id)
        {
            var errorCode = _companyLookUpDatabaseService.DeleteQualificationPlace(new CompanyDTO
            {
                QualificationPlaceId = id
            });

            return errorCode == ErrorCode.NO_ERROR
                ? RedirectToAction("Index", "Company")
                : RedirectToAction("Index", "Error", new {errorCodeParameter = errorCode});
        }

        // GET: /Company/Create
        public ActionResult Create()
        {
            var companyVm = (CompanyFormViewModel) InstatiateFormViewModel(new CompanyFormViewModel());
            return View(companyVm);
        }

        //
        // POST: /Company/Create/
        /// <summary>
        ///     Post method to create a company
        /// </summary>
        /// <param name="iModel"></param>

        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(CompanyFormViewModel iModel)
        {
            if (!ModelState.IsValid)
            {
                ModelState.AddModelError("", _errorMessageFactoryService.
                    Create(ErrorCode.COMMON_FORM_VALIDATION_ERROR));
                iModel = (CompanyFormViewModel)InstatiateFormViewModel(iModel);
                return View(iModel);
            }

            var companyDTO = new CompanyDTO
            {
                QualificationPlaceId = iModel.Id,
                QualificationPlaceName = iModel.Name,
                QualificationPlaceCategory = GetCategories()[FormHelper.ExtractDropDownListViewModel(iModel.Category)],
                QualificationPlaceDescription = iModel.Description,
                QualificationPlaceWebSite = iModel.Website,
                Address = AddressHelper.ExtractAddressViewModel(iModel.AddressViewModel)
            };
            var errorCode = _companyLookUpDatabaseService.CreateOrEditQualificationPlace(ref companyDTO);
            
            if (errorCode == ErrorCode.NO_ERROR)
                return RedirectToAction("Index", "Company");
            iModel = (CompanyFormViewModel)InstatiateFormViewModel(iModel);
            ModelState.AddModelError("", _errorMessageFactoryService.Create(errorCode));
            return View(iModel);
        }


        #region Helpers
        private static QualificationPlaceFormViewModel InstatiateFormViewModel(QualificationPlaceFormViewModel iModel)
        {
            var viewModel = iModel;
            viewModel.AddressViewModel = AddressHelper.BuildAddressViewModel();
            viewModel.Category = FormHelper.BuildDropDownListViewModel(GetCategories());
            return viewModel;
        }

        private static IDictionary<int, string> GetCategories()
        {
            return new Dictionary<int, string> { { 1, "Public" }, { 2, "Private" } };
        }
        #endregion
    }
}