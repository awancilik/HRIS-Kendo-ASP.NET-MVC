using System;
using System.Collections.Generic;

namespace CVScreeningWeb.ViewModels.Report
{
    [Serializable]
    public class SubmitReportViewModel
    {
        public int ScreeningId { get; set; }
        public int ReportId{ get; set; }
    }


}