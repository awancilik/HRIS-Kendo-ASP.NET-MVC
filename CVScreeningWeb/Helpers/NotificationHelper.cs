using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Security;
using CVScreeningWeb.ViewModels.Notivication;
using CVScreeningService.DTO.UserManagement;
using CVScreeningService.DTO.Notification;
using Kendo.Mvc.UI.Fluent;
using Ninject.Infrastructure.Language;

namespace CVScreeningWeb.Helpers
{
    public class NotificationHelper
    {
        public static IEnumerable<NotificationViewModel> GenerateNotificationViewModel(UserProfileDTO currentUser,
            IEnumerable<NotificationDTO> notificationDTOs)
        {
            IList<NotificationViewModel> notificationViewModels = new List<NotificationViewModel>();
            foreach (NotificationDTO notificationDTO in notificationDTOs)
            {
                var notificationOfUserDTO = notificationDTO.NotificationOfUser.FirstOrDefault();
                var notivicationViewModel = new NotificationViewModel()
                {
                    NotificationId = notificationDTO.NotificationId,
                    NotificationMessage = notificationDTO.NotificationMessage,
                    NotificationDate = notificationDTO.NotificationCreatedDate,
                    UserName = currentUser.FullName,
                    IsNotificationShown = notificationOfUserDTO != null? notificationOfUserDTO.IsNotificationShown : false
                };
                notificationViewModels.Add(notivicationViewModel);
            }
            var notificationVMResult = notificationViewModels.ToEnumerable().OrderByDescending(n => n.NotificationDate).Take(7);
            return notificationVMResult;
        }

    }
}