using System;

namespace CVScreeningService.DTO.UserManagement
{
    public class UserLeaveDTO
    {
        public int UserLeaveId { get; set; }
        public string UserLeaveName { get; set; }
        public string UserLeaveRemarks { get; set; }
        public DateTime UserLeaveStartDate { get; set; }
        public DateTime UserLeaveEndDate { get; set; }
    }
}