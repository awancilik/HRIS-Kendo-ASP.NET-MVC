using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using CVScreeningWeb.Filters;

namespace CVScreeningWeb.ViewModels.Leave
{
    public class LeaveManageViewModel
    {
        [LocalizedDisplayName("Id", NameResourceType = typeof(Resources.Common))]
        public int UserId { get; set; }

        [LocalizedDisplayName("Email", NameResourceType = typeof(Resources.Common))]
        public string UserName { get; set; }

        [LocalizedDisplayName("FullName", NameResourceType = typeof(Resources.Common))]
        public string FullName { get; set; }
        public IEnumerable<LeaveUniqueViewModel> UserLeaves { get; set; }
    }

    public class LeaveUniqueViewModel
    {
        public int No { get; set; }
        [LocalizedDisplayName("Description", NameResourceType = typeof(Resources.Leave))]
        public string Name { get; set; }
        public string StartDate { get; set; }
        public string EndDate { get; set; }
        public string Remarks { get; set; }
    }
}