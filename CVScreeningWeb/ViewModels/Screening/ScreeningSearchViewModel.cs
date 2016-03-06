using System;
using System.Collections;
using System.Collections.Generic;
using CVScreeningWeb.Filters;
using CVScreeningWeb.ViewModels.Shared;

namespace CVScreeningWeb.ViewModels.Screening
{
    public class ScreeningSearchViewModel
    {
        [LocalizedDisplayName("Name", NameResourceType = typeof(Resources.Common))]
        public string Name { get; set; }
        [LocalizedDisplayName("Client", NameResourceType = typeof(Resources.Common))]
        public DropDownListViewModel Client { get; set; }
        [LocalizedDisplayName("StartingDate", NameResourceType = typeof(Resources.Screening))]
        public string StartingDate { get; set; }
        [LocalizedDisplayName("EndingDate", NameResourceType = typeof(Resources.Screening))]
        public string EndingDate { get; set; }
        [LocalizedDisplayName("Status", NameResourceType = typeof(Resources.Common))]
        public DropDownListViewModel Status { get; set; }

        public IEnumerable<ScreeningManageViewModel> ScreeningManageList { get; set; }
    }
}