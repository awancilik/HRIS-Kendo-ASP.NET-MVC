using CVScreeningWeb.Filters;
using Foolproof;
using System;
using System.ComponentModel.DataAnnotations;

namespace CVScreeningWeb.ViewModels.PublicHoliday
{
    public class PublicHolidayFormViewModel
    {
        public int Id { get; set; }

        [Required]
        [LocalizedDisplayName("Name", NameResourceType = typeof(Resources.Common))]
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
