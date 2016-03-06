using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using CVScreeningCore.Models;
using CVScreeningService.DTO.LookUpDatabase;
using CVScreeningService.Services.DispatchingManagement;
using CVScreeningService.Services.ErrorHandling;
using CVScreeningService.Services.LookUpDatabase;
using CVScreeningService.Services.Screening;
using CVScreeningService.Services.UserManagement;
using CVScreeningWeb.ViewModels.Dispatching;
using CVScreeningWeb.ViewModels.Shared;
using Kendo.Mvc.UI;

namespace CVScreeningWeb.Controllers
{
    public class DispatchingController : Controller
    {
        private readonly IDispatchingManagementService _dispatchingManagementService;
        private readonly IErrorMessageFactoryService _errorMessageFactoryService;
        private readonly IUserManagementService _userManagementService;
        private readonly ILookUpDatabaseService<QualificationPlaceDTO> _qualificationService;
        private readonly IScreeningService _screeningService;
        private readonly IQualificationPlaceFactory _qualificationPlaceFactory;
        private static int _currentScreenerCategory;

        public DispatchingController(IDispatchingManagementService dispatchingManagementService,
            IErrorMessageFactoryService errorMessageFactoryService,
            IUserManagementService userManagementService,
            ILookUpDatabaseService<QualificationPlaceDTO> qualificationService,
            IScreeningService screeningService,
            IQualificationPlaceFactory qualificationPlaceFactory)
        {
            _dispatchingManagementService = dispatchingManagementService;
            _errorMessageFactoryService = errorMessageFactoryService;
            _userManagementService = userManagementService;
            _qualificationService = qualificationService;
            _screeningService = screeningService;
            _qualificationPlaceFactory = qualificationPlaceFactory;
        }


        /// <summary>
        /// Get screener indices which will be hiden
        /// </summary>
        /// <param name="colIds"></param>
        /// <returns></returns>
        private int[] GetColumnsToHide(int[] colIds)
        {
            if (colIds == null)
                return new[] {-1};

            var allScreeners = _userManagementService.GetAllUserScreeners();
            var ids = new List<int>();

            // Screener index starts from index 1
            var index = 1;
            foreach (var screener in allScreeners)
            {
                if (!colIds.Contains(screener.UserId))
                {
                    ids.Add(index);
                }
                index++;
            }
            return ids.ToArray();
        }

        /// <summary>
        /// Filter data source asyncronously for Skill Matrix
        /// </summary>
        /// <param name="colIds">Category is also considered as filtering parameter</param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult FilterColumns(int[] colIds)
        {
            return Json(GetColumnsToHide(colIds), JsonRequestBehavior.AllowGet);
        }

        #region DefaultMatrix

        public ActionResult ManageDefaultValue()
        {
            var defaultMatrix = new MatrixViewModel
            {
                // populated with list of screeners
                ColumnFilter = new MultiSelectViewModel
                {
                    SelectedItems = new List<string>(),
                    Controller = "Account",
                    Action = "GetUserScreenerJSON",
                    DataValueField = "ScreenerId",
                    DataTextField = "ScreenerName",
                    Placeholder = Resources.Dispatching.SelectScreeners,
                },
                // populated with list of type of checks
                RowFilter = new MultiSelectViewModel
                {
                    SelectedItems = new List<string>(),
                    Controller = "Screening",
                    Action = "GetTypeOfCheckJSON",
                    DataValueField = "TypeOfCheckId",
                    DataTextField = "TypeOfCheckName",
                    Placeholder = Resources.Dispatching.SelectTypeOfChecks
                },
                Matrix = BuildDefaultMatrixViewModel()
            };
            return View(defaultMatrix);
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public ActionResult DefaultMatrix_Read([DataSourceRequest] DataSourceRequest request, int[] rowIds)
        {
            var matrix = new MatrixFilterViewModel
            {
                RowIds = rowIds
            };

            return Json(BuildDefaultMatrixViewModel(matrix), JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="request"></param>
        /// <param name="models"></param>
        /// <returns></returns>
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult DefaultMatrix_Update([DataSourceRequest] DataSourceRequest request,
            [Bind(Prefix = "models")] List<Dictionary<string, string>> models)
        {
            if (models == null || !ModelState.IsValid)
                return Json(models);
            _dispatchingManagementService.UpdateDefaultMatrix(models);
            return Json(models);
        }

        /// <summary>
        /// A helper to build default matrix view model
        /// </summary>
        /// <param name="filter"></param>
        /// <returns></returns>
        private List<Dictionary<string, string>> BuildDefaultMatrixViewModel(MatrixFilterViewModel filter = null)
        {
            var allTypeOfChecks = _screeningService.GetAllTypeOfChecks();
            var allScreeners = _userManagementService.GetAllUserScreeners();

            if (filter != null)
            {
                if (filter.RowIds != null)
                    allTypeOfChecks = allTypeOfChecks.Where(e => filter.RowIds.Contains(e.TypeOfCheckId)).ToList();

                if (filter.ColIds != null)
                    allScreeners = allScreeners
                        .Where(e => filter.ColIds.Contains(e.UserId));
            }

            var result = new List<Dictionary<string, string>>();
            foreach (var check in allTypeOfChecks)
            {
                var row = new Dictionary<string, string>
                {
                    //RowId for the identifier
                    {"RowId", Convert.ToString(check.TypeOfCheckId)},
                    //RowName for displaying type of check name
                    {"RowName", check.CheckName}
                };
                var columnNames = "";
                var attributes = "";
                const string delimiter = "|";
                var indexCol = 0;

                //dynamic atributes based on name of screener
                foreach (var screener in allScreeners)
                {
                    //Remove space to meet JavaScipt convention for attribute creation
                    row.Add(screener.FullName.Replace(" ", string.Empty),
                        Convert.ToString(_dispatchingManagementService.GetDefaultMatrixValue(
                            screener.UserId, check.TypeOfCheckId)));

                    columnNames += screener.FullName + delimiter;
                    attributes += screener.FullName.Replace(" ", string.Empty) + delimiter;
                }
                row.Add("ColumnNames", columnNames);
                row.Add("Attributes", attributes);

                result.Add(row);
            }
            return result;
        }

        #endregion

        #region SkillMatrix

        /// <summary>
        /// To Display matrix of qualification places and screeners
        /// </summary>
        /// <param name="id">Null (default) is Office, 2 is On Field</param>
        /// <returns></returns>
        public ActionResult ManageSkillMatrix(int? id)
        {
            _dispatchingManagementService.DispatchAtomicChecks();
            _currentScreenerCategory = id == null ? 1 : 2;

            var skillMatrix = new MatrixViewModel
            {
                // populated with list of screeners
                ColumnFilter = new MultiSelectViewModel
                {
                    SelectedItems = new List<string>(),
                    Controller = "Account",
                    Action = _currentScreenerCategory == 1
                        ? "GetOfficeScreenerJSON"
                        : "GetOnFieldScreenerJSON",
                    DataValueField = "ScreenerId",
                    DataTextField = "ScreenerName",
                    Placeholder = Resources.Dispatching.SelectScreeners,
                },
                // populated with list of type of checks
                RowFilter = new MultiSelectViewModel
                {
                    SelectedItems = new List<string>(),
                    Controller = "QualificationPlace",
                    Action = _currentScreenerCategory == 1
                        ? "GetOfficeQualificationPlacesJSON"
                        : "GetOnFieldQualificationPlacesJSON",
                    DataValueField = "QualificationPlaceId",
                    DataTextField = "QualificationPlaceName",
                    Placeholder = Resources.Dispatching.SelectPlace
                },
                CategoryFilter = new MultiSelectViewModel
                {
                    SelectedItems = new List<string>(),
                    Controller = "QualificationPlace",
                    Action = "GetTypeOfQualificationPlace",
                    DataValueField = "QualificationPlaceTypeId",
                    DataTextField = "QualificationPlaceTypeName",
                    Placeholder = Resources.Dispatching.SelectPlaceType
                },
                ScreenerCategoryFilter = new DropDownListViewModel
                {
                    Sources = SkillMatrix.kCategories.Select(e => new SelectListItem
                    {
                        Text = e.Value,
                        Value = Convert.ToString(e.Key),
                        Selected = e.Key == _currentScreenerCategory
                    })
                },
                Matrix = BuildSkillMatrixViewModel()
            };
            return View(skillMatrix);
        }

        public ActionResult SkillMatrix_Read([DataSourceRequest] DataSourceRequest request, int[] rowIds, int[] catIds)
        {
            var filter = new MatrixFilterViewModel
            {
                RowIds = rowIds,
                CatIds = catIds
            };
            return Json(BuildSkillMatrixViewModel(filter), JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="request"></param>
        /// <param name="models"></param>
        /// <returns></returns>
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult SkillMatrix_Update([DataSourceRequest] DataSourceRequest request,
            [Bind(Prefix = "models")] List<Dictionary<string, string>> models)
        {
            if (models == null || !ModelState.IsValid)
                return Json(models);

            _dispatchingManagementService.UpdateSkillMatrix(models);

            return Json(models);
        }

        private List<Dictionary<string, string>> BuildSkillMatrixViewModel(MatrixFilterViewModel filter = null)
        {
            var allQualificationPlaces = _qualificationService.GetQualificationPlaceByScreenerCategory(
                SkillMatrix.kCategories[_currentScreenerCategory]);

            var allScreeners = _userManagementService.GetAllUserScreeners()
                .Where(e => e.ScreenerCategory == SkillMatrix.kCategories[_currentScreenerCategory]);

            if (filter != null)
            {
                if (filter.ColIds != null)
                    allScreeners = allScreeners
                        .Where(e => filter.ColIds.Contains(e.UserId));

                if (filter.RowIds != null)
                    allQualificationPlaces =
                        allQualificationPlaces.Where(e => filter.RowIds.Contains(e.QualificationPlaceId)).ToList();

                if (filter.CatIds != null)
                    allQualificationPlaces =
                        allQualificationPlaces.Where(e => IsAppropiateType(e, filter.CatIds)).ToList();
            }

            var result = new List<Dictionary<string, string>>();

            foreach (var qualificationPlace in allQualificationPlaces)
            {
                var categories = _qualificationService.GetTypeOfCheckCategory(qualificationPlace.QualificationPlaceId);
                foreach (var category in categories)
                {
                    if ((_currentScreenerCategory == 1 && category != TypeOfCheckMeta.kOfficeCategory)
                        || (_currentScreenerCategory == 2 && category != TypeOfCheckMeta.kOnFieldCategory))
                        continue;

                    var columnNames = "";
                    var attributes = "";
                    const string delimiter = "|";
                    var indexCol = 0;
                    var row = new Dictionary<string, string>
                    {
                        //RowId for the identifier
                        {
                            "RowId",
                            string.Format("{0}{1}{2}", Convert.ToString(qualificationPlace.QualificationPlaceId)
                                , delimiter, category)
                        },
                        //RowName for displaying type of check name
                        {"RowName", qualificationPlace.QualificationPlaceName}, 
                        //Category to display onfield or office
                        {"Category", category}
                    };

                    //dynamic atributes based on name of screener
                    foreach (var screener in allScreeners)
                    {
                        //Remove space to meet JavaScipt convention for attribute creation
                        var value = _dispatchingManagementService.GetSkillMatrixValue(
                            screener.UserId, qualificationPlace.QualificationPlaceId, category);

                        row.Add(screener.FullName.Replace(" ", string.Empty),
                            value >= 0
                                ? Convert.ToString(value)
                                : value == SkillMatrix.kDefaultValue
                                    ? SkillMatrix.kDefaultValueString
                                    : SkillMatrix.kImpossibleValueString);
                        columnNames += screener.FullName + delimiter;
                        attributes += screener.FullName.Replace(" ", string.Empty) + delimiter;
                    }

                    row.Add("ColumnNames", columnNames);
                    row.Add("Attributes", attributes);

                    result.Add(row);
                }
            }
            return result;
        }

        private bool IsAppropiateType(BaseQualificationPlaceDTO qualificationPlaceDTO, IEnumerable<int> catIds)
        {
            var type = (int) _qualificationPlaceFactory.GetType(qualificationPlaceDTO);
            return catIds.Contains(type);
        }

        #endregion
    }
}