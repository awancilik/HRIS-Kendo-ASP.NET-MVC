using System.Diagnostics;
using System.Linq;
using System.Web.Mvc;
using System.Web.Security;
using CVScreeningService.Services.Client;
using CVScreeningService.Services.Common;
using CVScreeningService.Services.ErrorHandling;
using CVScreeningService.Services.Notification;
using CVScreeningService.Services.Screening;
using CVScreeningService.Services.Settings;
using CVScreeningService.Services.UserManagement;
using CVScreeningWeb.Helpers;
using CVScreeningWeb.ViewModels.Common;
using System.Collections.Generic;
using CVScreeningService.DTO.Notification;
using Nalysa.Common.Log;

namespace CVScreeningWeb.Controllers
{
    [Authorize]
    [Filters.HandleError]
    public class HomeController : Controller
    {
        private readonly ICommonService _commonService;
        private readonly IScreeningService _screeningService;
        private readonly IClientService _clientService;
        private readonly IErrorMessageFactoryService _errorMessageFactoryService;
        private readonly ISettingsService _settingsService;
        private readonly INotificationService _notificationService;
        private readonly IUserManagementService _userManagementService;

        public HomeController(
            ICommonService commonService, 
            IErrorMessageFactoryService errorMessageFactoryService,
            IScreeningService screeningService, 
            IClientService clientService, 
            ISettingsService settingsService,
            INotificationService notificatioNService,
            IUserManagementService userManagementService)
        {
            _commonService = commonService;
            _errorMessageFactoryService = errorMessageFactoryService;
            _screeningService = screeningService;
            _clientService = clientService;
            _settingsService = settingsService;
            _notificationService = notificatioNService;
            _userManagementService = userManagementService;

        }

        public ActionResult Index()
        {
            //If user belongs only to HR roles, redirect him to account management
            if (Roles.IsUserInRole("Hr") && Roles.GetRolesForUser().Count() == 1)
                return RedirectToAction("Manage", "Account");


            return View("Index");
        }


        [ChildActionOnly]
        public ActionResult DashboardAdmin()
        {
            LogManager.Instance.Debug(string.Format("DashboardAdmin begin ..."));
            var sw = new Stopwatch();
            sw.Start();

            var screeningsDTO = _screeningService.GetAllScreenings();
            var screeningToQualifyDTO = screeningsDTO.Where(s => s.ScreeningToQualify);
            var screeningsToRequalifyDTO = screeningsDTO.Where(s => s.ScreeningToReQualify);
            var atomicChecksToAssignDTO = _screeningService.GetAllAtomicChecksToAssign();
            var atomicChecksOnGoingDTO = _screeningService.GetAllAtomicChecksOnGoing();
            var atomicChecksPendingValidationDTO = _screeningService.GetAllAtomicChecksPendingValidation();
            var viewModel = DashboardHelper.BuildDashboardAdministratorViewModel(screeningsDTO, screeningToQualifyDTO, 
                screeningsToRequalifyDTO, atomicChecksToAssignDTO, atomicChecksOnGoingDTO, atomicChecksPendingValidationDTO,
                _settingsService.GetAllPublicHolidays());

            sw.Stop();
            LogManager.Instance.Debug(string.Format("DashboardAdmin end: duration in ms: {0}", sw.ElapsedMilliseconds));

            return PartialView("_DashboardAdmin", viewModel);
        }

        [ChildActionOnly]
        public ActionResult DashboardClient()
        {
            var screeningsDTO = _screeningService.GetAllScreenings();
            var viewModel = DashboardHelper.BuildDashboardClientViewModel(screeningsDTO, _settingsService.GetAllPublicHolidays());
            return PartialView("_DashboardClient", viewModel);
        }

        [ChildActionOnly]
        public ActionResult DashboardAccountManager()
        {
            var screeningsDTO = _screeningService.GetAllScreenings();
            var viewModel = DashboardHelper.BuildDashboardAccountManagerViewModel(screeningsDTO, _settingsService.GetAllPublicHolidays());
            return PartialView("_DashboardAccountManager", viewModel);
        }

        [ChildActionOnly]
        public ActionResult DashboardQualifier()
        {
            var screeningsDTO = _screeningService.GetAllScreenings();
            var screeningToQualifyDTO = screeningsDTO.Where(s => s.ScreeningToQualify);
            var screeningsToRequalifyDTO = screeningsDTO.Where(s => s.ScreeningToReQualify);
            var viewModel = DashboardHelper.BuildDashboardQualifierViewModel(screeningToQualifyDTO, screeningsToRequalifyDTO, _settingsService.GetAllPublicHolidays());
            return PartialView("_DashboardQualifier", viewModel);
        }

        public ActionResult DashboardProductionManager()
        {
            var atomicChecksToAssignDTO = _screeningService.GetAllAtomicChecksToAssign();
            var atomicChecksOnGoingDTO = _screeningService.GetAllAtomicChecksOnGoing();
            var atomicChecksPendingValidationDTO = _screeningService.GetAllAtomicChecksPendingValidation();
            var viewModel = DashboardHelper.BuildDashboardProductionManagerViewModel(
                atomicChecksToAssignDTO, atomicChecksOnGoingDTO, atomicChecksPendingValidationDTO,
                _settingsService.GetAllPublicHolidays());
            return PartialView("_DashboardProductionManager", viewModel);
        }

        public ActionResult DashboardScreener()
        {
            var atomicChecksOnGoingDTO = _screeningService.GetAllAtomicChecksOnGoingAssignedAsScreener();
            var atomicChecksPendingValidationDTO = _screeningService.GetAllAtomicChecksBasePendingValidationAssignedAsScreener();

            var viewModel = DashboardHelper.BuildDashboardScreenerViewModel(atomicChecksOnGoingDTO, 
                    atomicChecksPendingValidationDTO, _settingsService.GetAllPublicHolidays());
            return PartialView("_DashboardScreener", viewModel);
        }

        public ActionResult DashboardQualityControl()
        {
            var atomicChecksOnGoingDTO = _screeningService.GetAllAtomicChecksBasePendingValidationAssignedAsQualityControl();
            var screeningsDTO = _screeningService.GetAllScreeningsAssignedAsQualityControl();

            var viewModel = DashboardHelper.BuildDashboardQualityControlViewModel(
                atomicChecksOnGoingDTO, screeningsDTO, _settingsService.GetAllPublicHolidays());
            return PartialView("_DashboardQualityControl", viewModel);
        }

        public ActionResult About()
        {
            ViewBag.Title = "About";
            var content = _commonService.GetPostByName("about");
            var model = new PostViewModel
            {
                PostTitle = content.PostTitle ?? "",
                PostContent = content.PostContent ?? ""
            };
            return View(model);
        }

        public ActionResult Contact()
        {
            ViewBag.Title = "Contact Page";
            var content = _commonService.GetPostByName("Contact Us");
            var model = new PostViewModel
            {
                PostTitle = content.PostTitle,
                PostContent = content.PostContent
            };
            return View("_Post", model);
        }

        public ActionResult Notification()
        {
            LogManager.Instance.Debug(string.Format("Notification begin ..."));
            var sw = new Stopwatch();
            sw.Start();

            var currentUserDTO = _userManagementService.GetCurrentUser();
            var notificationDTO = _notificationService.GetNotificationsByUser(currentUserDTO);
            var notificationViewModel = NotificationHelper.GenerateNotificationViewModel(currentUserDTO, notificationDTO);
            UpdateNotification(notificationDTO, currentUserDTO.UserId);

            sw.Stop();
            LogManager.Instance.Debug(string.Format("Notification end: duration in ms: {0}", sw.ElapsedMilliseconds));

            return PartialView(notificationViewModel);
        }

        private void UpdateNotification(IEnumerable<NotificationDTO> notification, int userId)
        {
            foreach (NotificationDTO notificationDTO in notification)
            {
                if (!notificationDTO.NotificationOfUser.Any(
                    n => n.UserId == userId && n.IsNotificationShown == false))
                    continue;

                var notificationOfUser = notificationDTO.NotificationOfUser.First(
                    n => n.UserId == userId && n.IsNotificationShown == false);
                _notificationService.EditNotification(ref notificationOfUser, true);
            }
        }

    }
}