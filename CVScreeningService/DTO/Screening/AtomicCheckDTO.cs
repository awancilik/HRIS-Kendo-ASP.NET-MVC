using System.Collections.Generic;
using CVScreeningService.DTO.Discussion;
using CVScreeningService.DTO.LookUpDatabase;
using CVScreeningService.DTO.UserManagement;
using CVScreeningService.Filters;

namespace CVScreeningService.DTO.Screening
{
    public class AtomicCheckDTO : AtomicCheckBaseDTO
    {
        public TypeOfCheckDTO TypeOfCheck { get; set; }
        public ICollection<AttachmentDTO> Attachment { get; set; }
        public ScreeningDTO Screening { get; set; }
        public UserProfileDTO Screener { get; set; }
        public BaseQualificationPlaceDTO QualificationPlace { get; set; }
        public ICollection<DiscussionDTO> Discussion { get; set; }

    }
}