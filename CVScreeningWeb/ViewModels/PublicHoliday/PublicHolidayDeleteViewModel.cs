using System;
using System.ComponentModel.DataAnnotations;
using CVScreeningWeb.Filters;

namespace CVScreeningWeb.ViewModels.PublicHoliday
{
    public class PublicHolidayDeleteViewModel
    {

        public int Id { get; set; }

        [Required]
        [LocalizedDisplayName("Name", NameResourceType = typeof(Resources.Common))]
        public string Name { get; set; }

        [Required]
        [LocalizedDisplayName("StartDate", NameResourceType = typeof(Resources.Common))]
        public DateTime? StartDate { get; set; }

        [Required]
        [LocalizedDisplayName("EndDate", NameResourceType = typeof(Resources.Common))]
        public DateTime? EndDate { get; set; }

        [LocalizedDisplayName("Remarks", NameResourceType = typeof(Resources.Common))]
        public string Remarks { get; set; }

    }
}