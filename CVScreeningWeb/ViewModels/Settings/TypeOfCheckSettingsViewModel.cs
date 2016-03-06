using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using CVScreeningWeb.Filters;
using CVScreeningWeb.ViewModels.Screening;

namespace CVScreeningWeb.ViewModels.Settings
{
    public class TypeOfCheckSettingsViewModel
    {
        public int TypeOfCheckMetaId { get; set; }
        public int TypeOfCheckId { get; set; }
        public string TypeOfCheckName { get; set; }
        public string TypeOfCheckCategory { get; set; }

        [LocalizedDisplayName("AverageCompletionRate", NameResourceType = typeof(Resources.Settings))]
        public int AverageCompletionRate { get; set; }

        [LocalizedDisplayName("CompletionMinimunWorkingDays", NameResourceType = typeof(Resources.Settings))]
        public int CompletionMinimunWorkingDays { get; set; }
    }
}