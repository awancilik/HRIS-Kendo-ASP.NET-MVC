using System.Web;

namespace CVScreeningWeb.ViewModels.Report
{
    public class UploadManualReportViewModel
    {
        public int ScreeningId { get; set; }
        public string Name { get; set; }
        public HttpPostedFileBase ManualReport { get; set; }
    }
}