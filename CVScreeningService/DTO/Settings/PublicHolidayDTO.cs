using System;

namespace CVScreeningService.DTO.Settings
{
    public class PublicHolidayDTO
    {
        public int PublicHolidayId { get; set; }
        public string PublicHolidayName { get; set; }
        public DateTime PublicHolidayStartDate { get; set; }
        public DateTime PublicHolidayEndDate { get; set; }
        public string PublicHolidayRemarks { get; set; }
    }
}