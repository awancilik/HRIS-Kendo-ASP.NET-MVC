using System;
using CVScreeningService.Filters;

namespace CVScreeningService.DTO.Screening
{
    public class ScreeningReportDTO
    {
        [ObjectId]
        public int ScreeningReportId { get; set; }
        public int ScreeningReportVersion { get; set; }
        public byte[] ScreeningReportContent { get; set; }
        public string ScreeningReportUrl { get; set; }
        public DateTime ScreeningReportSubmittedDate { get; set; }
        public string ScreeningReportFilePath { get; set; }
        public string ScreeningReportGenerationType { get; set; }
        public ScreeningBaseDTO Screening { get; set; }

    }
}