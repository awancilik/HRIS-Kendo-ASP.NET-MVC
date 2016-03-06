using System;
using System.Collections.Generic;
using CVScreeningService.DTO.Client;
using CVScreeningService.DTO.Discussion;
using CVScreeningService.DTO.LookUpDatabase;
using CVScreeningService.DTO.UserManagement;
using CVScreeningService.Filters;

namespace CVScreeningService.DTO.Screening
{
    public class ScreeningBaseDTO
    {
        [ObjectId]
        public int ScreeningId { get; set; }
        public int ClientCompanyId { get; set; }
        public string ClientCompanyName { get; set; }
        public string ScreeningLevelName { get; set; }
        public string ScreeningReference { get; set; }
        public string ScreeningComments { get; set; }
        public DateTime? ScreeningUploadedDate { get; set; }
        public DateTime? ScreeningStartingDate { get; set; }
        public DateTime? ScreeningDeadlineDate { get; set; }
        public DateTime? ScreeningDeliveryDate { get; set; }
        public DateTime? ScreeningLastUpdatedDate { get; set; }
        public int PendingDays { get; set; }
        public string ScreeningAdditionalRemarks { get; set; }
        public bool ScreeningIsDeactivated { get; set; }
        public byte ScreeningTenantId { get; set; }
        public byte ScreeningState { get; set; }
        public string State { get; set; }
        public string ScreeningFullName { get; set; }
        public string ScreeningPhysicalPath { get; set; }
        public string ScreeningVirtualPath { get; set; }
        public int ExternalDiscussionId { get; set; }
        public int InternalDiscussionId { get; set; }
        public bool ScreeningToQualify { get; set; }
        public bool ScreeningToReQualify { get; set; }
        public ICollection<ScreeningReportDTO> ScreeningReport { get; set; }


    }
}
