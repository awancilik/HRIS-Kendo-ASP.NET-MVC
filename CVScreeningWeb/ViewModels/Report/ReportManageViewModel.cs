using System.Collections.Generic;
using System.Web;
using CVScreeningWeb.Filters;

namespace CVScreeningWeb.ViewModels.Report
{
    public class ReportManageViewModel
    {
        [LocalizedDisplayName("Id", NameResourceType = typeof (Resources.Common))]
        public int Id { get; set; }

        [LocalizedDisplayName("Status", NameResourceType = typeof (Resources.Common))]
        public string Status { get; set; }

        [LocalizedDisplayName("Type", NameResourceType = typeof (Resources.Common))]
        public string Type { get; set; }

        [LocalizedDisplayName("Version", NameResourceType = typeof (Resources.Common))]
        public string Version { get; set; }

        [LocalizedDisplayName("SubmittedDate", NameResourceType = typeof (Resources.Screening))]
        public string SubmittedDate { get; set; }

        public string ScreeningStatus { get; set; }
    }

    public class ReportByScreeningViewModel
    {
        public int ScreeningId { get; set; }
        public IEnumerable<ReportManageViewModel> ReportManageViewModels { get; set; }
        public IEnumerable<string> ErrorMessages { get; set; }
    }
}