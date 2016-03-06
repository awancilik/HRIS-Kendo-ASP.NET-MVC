using CVScreeningService.DTO.Notification;
using CVScreeningService.DTO.UserManagement;
using CVScreeningService.Services.Notification;
using CVScreeningService.Services.UserManagement;
using Microsoft.AspNet.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Newtonsoft.Json;

namespace CVScreeningWeb.SignalR
{
    public class HubHelper
    {
        private readonly INotificationService _notificationService;

        public HubHelper(INotificationService notificationService)
        {
            _notificationService = notificationService;
        }

        public void GetNotification()
        {
            var hubContext = GlobalHost.ConnectionManager.GetHubContext<NotificationHub>();

            var notificationDictionary = _notificationService.GetNotifications();
            foreach (KeyValuePair<UserProfileDTO, IList<NotificationDTO>> notificationVP in notificationDictionary)
            {
                var connectionIds = SignalRUserMapper.GetConnections(notificationVP.Key.UserName);
                if (connectionIds.Count() != 0)
                {
                    foreach (var connectionId in connectionIds)
                    {
                        var notification =
                            notificationVP.Value.Select(
                                n =>
                                    new
                                    {
                                        id = n.NotificationId,
                                        message = n.NotificationMessage,
                                        createdDate = n.NotificationCreatedDate
                                    }).ToList();

                        hubContext.Clients.Client(connectionId).getNotifications(JsonConvert.SerializeObject(notification));
                    }
                    UpdateNotification(notificationVP.Value, notificationVP.Key.UserId);
                }
            }
        }

        private void UpdateNotification(IList<NotificationDTO> notification, int userId)
        {
            foreach (NotificationDTO notificationDTO in notification)
            {
                var notificationOfUser = notificationDTO.NotificationOfUser.Where(n=>n.UserId == userId).FirstOrDefault();
                _notificationService.EditNotification(ref notificationOfUser, true);
            }
        }
    }
}