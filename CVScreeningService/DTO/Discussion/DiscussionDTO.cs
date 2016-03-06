using System;
using System.Collections.Generic;
using CVScreeningService.DTO.Screening;
using CVScreeningService.Filters;

namespace CVScreeningService.DTO.Discussion
{
    public class DiscussionDTO
    {
        [ObjectId]
        public int DiscussionId { get; set; }
        public string DiscussionTitle { get; set; }
        public string DiscussionType { get; set; }
        public DateTime DiscussionCreatedDate { get; set; }
        public byte DiscussionTenantId { get; set; }

        public virtual AtomicCheckDTO AtomicCheck { get; set; }
        public virtual ScreeningDTO Screening { get; set; }
        public virtual ICollection<MessageDTO> Message { get; set; }
    }
}