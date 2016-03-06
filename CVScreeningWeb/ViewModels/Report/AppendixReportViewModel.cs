using System;
using System.Collections.Generic;

namespace CVScreeningWeb.ViewModels.Report
{
    [Serializable]
    public class AppendixReportViewModel
    {
        public string AppendixTypeOfCheck { get; set; }
        public string AppendixTitle { get; set; }
        public IEnumerable<AppendixAttachmentViewModel> Attachments { get; set; }
    }
}