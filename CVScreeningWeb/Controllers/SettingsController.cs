using System.Collections.Generic;
using System.Web.Mvc;
using CVScreeningCore.Error;
using CVScreeningCore.Models;
using CVScreeningService.Services.Common;
using CVScreeningService.Services.ErrorHandling;
using CVScreeningService.Services.Screening;
using CVScreeningService.Services.Settings;
using CVScreeningWeb.Helpers;
using CVScreeningWeb.ViewModels.Settings;
using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;


namespace CVScreeningWeb.Controllers
{
    [Filters.HandleError]
    [Authorize(Roles = "Administrator")]
    public class SettingsController : Controller
    {

        private readonly ISettingsService _settingsService;
        private readonly IScreeningService _screeningService;
        private readonly IErrorMessageFactoryService _errorMessageFactoryService;

        /// <summary>
        ///     Constructor to pass service
        /// </summary>
        /// <param name="settingsService"></param>
        public SettingsController(
            ISettingsService settingsService, 
            IErrorMessageFactoryService errorMessageFactoryService,
            IScreeningService screeningService)
        {
            _settingsService = settingsService;
            _errorMessageFactoryService = errorMessageFactoryService;
            _screeningService = screeningService;
        }

        //
        // GET: /Settings/TypeOfChecks
        public ActionResult TypeOfChecks()
        {
            return View();
        }


        public ActionResult TypeOfCheckMeta_Editing_Read([DataSourceRequest] DataSourceRequest request)
        {
            var averageCompletionRateMeta = _settingsService.GetTypeOfCheckMeta(TypeOfCheckMeta.kAverageCompletionRateKey);
            var completionMinimumWorkingDaysMeta = _settingsService.GetTypeOfCheckMeta(TypeOfCheckMeta.kCompletionMinimumWorkingDays);

            var model = SettingsHelper.BuildTypeOfChecksMetaViewModels(
                averageCompletionRateMeta, completionMinimumWorkingDaysMeta);
            return Json(model.ToDataSourceResult(request));
        }

        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult TypeOfCheckMeta_Editing_Update(
            [DataSourceRequest] DataSourceRequest request,
            [Bind(Prefix = "models")] IEnumerable<TypeOfCheckSettingsViewModel> models)
        {
            if (!ModelState.IsValid)
            {
                return Json(models.ToDataSourceResult(request, ModelState));
            }
            ErrorCode error = _settingsService.SetTypeOfCheckMeta(TypeOfCheckMeta.kAverageCompletionRateKey,
                SettingsHelper.ExtractTypeOfChecksMetaAverageCompletionRate(models));

            error = _settingsService.SetTypeOfCheckMeta(TypeOfCheckMeta.kCompletionMinimumWorkingDays,
                SettingsHelper.ExtractTypeOfChecksMetaCompletionMinimumWorkingDays(models));

            return Json(models.ToDataSourceResult(request, ModelState));
        }

    }
}
