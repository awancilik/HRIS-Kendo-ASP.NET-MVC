using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using CVScreeningWeb.Filters;

namespace CVScreeningWeb.ViewModels.PublicHoliday
{
    public class PublicHolidayManageViewModel
    {
        public IEnumerable<PublicHolidayUniqueViewModel> PublicHolidays { get; set; }
    }

    public class PublicHolidayUniqueViewModel
    {
        [LocalizedDisplayName("Id", NameResourceType = typeof(Resources.Common))]
        public int Id { get; set; }

        [LocalizedDisplayName("Name", NameResourceType = typeof(Resources.Common))]
        public string Name { get; set; }

        [LocalizedDisplayName("StartDate", NameResourceType = typeof(Resources.Common))]
        public string StartDate { get; set; }

        [LocalizedDisplayName("EndDate", NameResourceType = typeof(Resources.Common))]
        public string EndDate { get; set; }

        [LocalizedDisplayName("Remarks", NameResourceType = typeof(Resources.Common))]
        public string Remarks { get; set; }
    }
}