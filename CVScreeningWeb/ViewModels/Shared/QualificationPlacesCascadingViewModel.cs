using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using CVScreeningWeb.Filters;
using CVScreeningWeb.ViewModels.Shared;

namespace CVScreeningWeb.ViewModels.Shared
{
    public class QualificationPlacesCascadingViewModel : QualificationPlacesMultiSelectViewModel
    {
        [LocalizedDisplayName("Filter", NameResourceType = typeof(Resources.Common))]
        public DropDownListKendoUiViewModel DropDownListKendoUiViewModel { get; set; }
    }
}