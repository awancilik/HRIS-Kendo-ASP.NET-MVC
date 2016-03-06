using System;
using System.Collections.Generic;
using System.Web.Mvc;

namespace CVScreeningWeb.ViewModels.Report
{
    [Serializable]
    public class VerificationReportViewModel
    {
        public string Type { get; set; }
        public IEnumerable<string> Lines { get; set; }
        public string Passage { get; set; }
    }

    [Serializable]
    public class TypeOfCheckReportViewModel
    {
        public string TypeOfCheckName { get; set; }
        public IEnumerable<VerificationReportViewModel> VerificationReports { get; set; }  
    }
}