using CVScreeningWeb.Filters;
using Foolproof;
using System;
using System.ComponentModel.DataAnnotations;

namespace CVScreeningWeb.ViewModels.Leave
{
    public class LeaveFormViewModel
    {

        public int LeaveId { get; set; }

        public int UserId { get; set; }

        [Required]
        [LocalizedDisplayName("Email", NameResourceType = typeof(Resources.Common))]
        public string UserName { get; set; }

        [Required]
        [LocalizedDisplayName("FullName", NameResourceType = typeof(Resources.Common))]
        public string FullName { get; set; }

        [Required]
        [LocalizedDisplayName("Description", NameResourceType = typeof(Resources.Leave))]
        public string Name { get; set; }

        [Required]
        [LocalizedDisplayName("StartDate", NameResourceType = typeof(Resources.Common))]
        [LessThanOrEqualTo("EndDate")]
        public DateTime? StartDate { get; set; }

        [Required]
        [LocalizedDisplayName("EndDate", NameResourceType = typeof(Resources.Common))]
        [GreaterThanOrEqualTo("StartDate")]
        public DateTime? EndDate { get; set; }

        [LocalizedDisplayName("Remarks", NameResourceType = typeof(Resources.Common))]
        public string Remarks { get; set; }

    }
}