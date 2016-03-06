using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using CVScreeningWeb.ViewModels.Screening;

namespace CVScreeningWeb.ViewModels.Home
{
    public class ScreeningGridViewModel
    {
        public string Type;
        public IEnumerable<ScreeningManageViewModel> Screenings;

    }
}