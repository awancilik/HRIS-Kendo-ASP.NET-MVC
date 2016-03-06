using System;
using System.Collections.Generic;
using CVScreeningService.DTO.Client;
using CVScreeningService.DTO.Discussion;
using CVScreeningService.DTO.LookUpDatabase;
using CVScreeningService.DTO.UserManagement;
using CVScreeningService.Filters;

namespace CVScreeningService.DTO.Screening
{
    public class ScreeningDTO : ScreeningBaseDTO
    {
        public ScreeningLevelVersionDTO ScreeningLevelVersion { get; set; }
        public UserProfileDTO QualityControl { get; set; }
        public ICollection<AtomicCheckDTO> AtomicCheck { get; set; }
        public ScreeningQualificationDTO ScreeningQualification { get; set; }
        public ICollection<ScreeningReportDTO> ScreeningReport { get; set; }
        public ICollection<BaseQualificationPlaceDTO> QualificationPlace { get; set; }
        public ICollection<AttachmentDTO> Attachment { get; set; }
        public ICollection<DiscussionDTO> Discussion { get; set; }
        public ICollection<ScreeningQualificationPlaceMetaDTO> ScreeningQualificationPlaceMeta { get; set; }

    }
}
