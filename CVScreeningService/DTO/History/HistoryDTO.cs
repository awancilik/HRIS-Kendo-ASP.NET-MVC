using System;
using CVScreeningService.DTO.Screening;
using CVScreeningService.DTO.UserManagement;

namespace CVScreeningService.DTO.History
{
    public class HistoryDTO
    {
        public int HistoryId { get; set; }
        public string HistoryAction { get; set; }
        public DateTime HistoryCreatedDate { get; set; }
        public string HistoryDescription { get; set; }
        public byte HistoryScreeningOldStatus { get; set; }
        public byte HistoryScreeningNewStatus { get; set; }
        public byte? HistoryAtomicCheckOldStatus { get; set; }
        public byte? HistoryAtomicCheckNewStatus { get; set; }
        public byte? HistoryAtomicCheckOldValidationStatus { get; set; }
        public byte? HistoryAtomicCheckNewValidationStatus { get; set; }
        public byte HistoryTenantId { get; set; }

        public AtomicCheckDTO AtomicCheck { get; set; }
        public ScreeningDTO Screening { get; set; }
        public UserProfileDTO webpages_UserProfile { get; set; }
    }
}