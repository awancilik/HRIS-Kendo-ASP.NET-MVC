using System.Collections.Generic;
using System.Web.Mvc;

namespace CVScreeningWeb.ViewModels.Shared
{
    public class DropDownListViewModel
    {
        public string PostData { get; set; }
        public IEnumerable<SelectListItem> Sources { get; set; }
    }
}