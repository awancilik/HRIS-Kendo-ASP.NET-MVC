using System;
using CVScreeningService.DTO.UserManagement;

namespace CVScreeningService.DTO.Discussion
{
    public class MessageDTO
    {
        public int MessageId { get; set; }
        public string MessageContent { get; set; }
        public DateTime MessageCreatedDate { get; set; }
        public byte MessageTenantId { get; set; }

        public virtual DiscussionDTO Discussion { get; set; }
        public virtual UserProfileDTO MessageCreatedBy { get; set; }


    }
}