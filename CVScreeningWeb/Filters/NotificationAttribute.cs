using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CVScreeningService.Services.Notification;
using CVScreeningWeb.SignalR;
using Nalysa.Common.Log;

namespace CVScreeningWeb.Filters
{
    public class NotificationAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuted(ActionExecutedContext filterContext)
        {
            var notificationService = (INotificationService)filterContext.RouteData.Values["notificationService"];
            if (notificationService != null)
            {
                var hubHelper = new HubHelper(notificationService);
                hubHelper.GetNotification();
                LogManager.Instance.Debug("Send Notification to user");
            }
        }
    }
}