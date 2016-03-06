using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Web;
using CVScreeningDAL.UnitOfWork;
using CVScreeningService.Services.Permission;
using CVScreeningService.Services.DispatchingManagement;
using CVScreeningService.Services.Screening;
using CVScreeningService.Services.SystemTime;
using CVScreeningService.Services.UserManagement;
using Nalysa.Common.Log;
using Quartz;
using CVScreeningService.Services.Notification;

namespace CVScreeningWeb.Job
{
    public class DispatchingJob : IJob
    {
        private readonly IDispatchingManagementService _dispatchingManagementService;
        private readonly IPermissionService _permissionService;
        private readonly INotificationService _notificationService;

        private readonly IUserManagementService _userManagementService;
        private readonly IWebSecurity _webSecurity;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ISystemTimeService _systemTimeService;
        private readonly IScreeningService _screeningService;

        public DispatchingJob(
            IUnitOfWork unitOfWork, IUserManagementService userManagementService, ISystemTimeService systemTimeService)
        {
            _unitOfWork = unitOfWork;
            _webSecurity = new JobWebSecurity();
            _permissionService = new PermissionService(_unitOfWork, "job@admin.com");
            _userManagementService = userManagementService;
            _systemTimeService = systemTimeService;
            _notificationService = new NotificationService(_unitOfWork, _userManagementService, _systemTimeService, _webSecurity);
            _screeningService = new ScreeningService(_unitOfWork, _permissionService, _userManagementService, _systemTimeService, _webSecurity, _notificationService);
            _dispatchingManagementService = new DispatchingManagementService(_unitOfWork, _userManagementService, _systemTimeService, _screeningService);
        }

        public void Execute(IJobExecutionContext context)
         {
            LogManager.Instance.Info(string.Format("Dispatching job started at: {0}", DateTime.Now));
            _dispatchingManagementService.DispatchAtomicChecks();
            LogManager.Instance.Info(string.Format("Dispatching job ended at: {0}", DateTime.Now));
         }

    }
}