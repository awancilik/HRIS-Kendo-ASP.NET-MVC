using System;
using System.Collections.Generic;

namespace CVScreeningService.DTO.Notification
{
    public class NotificationDTO
    {
        public int NotificationId { get; set; }
        public string NotificationMessage { get; set; }
        public DateTime NotificationCreatedDate { get; set; }

        public ICollection<NotificationOfUserDTO> NotificationOfUser { get; set; }

    }
}
