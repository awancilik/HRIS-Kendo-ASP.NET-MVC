using CVScreeningService.DTO.UserManagement;

namespace CVScreeningService.DTO.Notification
{
    public class NotificationOfUserDTO
    {
        public int NotificationId { get; set; }
        public int UserId { get; set; }
        public bool IsNotificationShown { get; set; }

        public NotificationDTO Notification { get; set; }
    }
}
