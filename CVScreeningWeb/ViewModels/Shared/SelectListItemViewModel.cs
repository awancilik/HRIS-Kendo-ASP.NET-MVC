using System.Collections.Generic;
using System.Web.Mvc;

namespace CVScreeningWeb.ViewModels.Shared
{
    public class SelectListItemViewModel
    {
        public IEnumerable<string> ItemNames { get; set; }
        public IEnumerable<string> ItemIds { get; set; }
        public IEnumerable<SelectListItem> SelectListItems { get; set; }
    }
}