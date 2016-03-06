using System;
using System.Collections.Generic;

namespace CVScreeningWeb.ViewModels.Report
{
    [Serializable]
    public class ReportViewModel
    {
        public int ScreeningId { get; set; }
        public string Title { get; set; }
        public SummaryReportViewModel Summary { get; set; }
        public IEnumerable<TypeOfCheckReportViewModel> TypeOfCheckReports { get; set; }
        public IEnumerable<AppendixReportViewModel> Appendices { get; set; }
    }
}