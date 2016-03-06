using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CVScreeningWeb.ViewModels.Notivication
{
    public class NotificationViewModel
    {
        public string UserName { get; set; }
        public int NotificationId { get; set; }
        public string NotificationMessage { get; set; }
        public DateTime NotificationDate { get; set; }
        public bool IsNotificationShown { get; set; }
    }
}