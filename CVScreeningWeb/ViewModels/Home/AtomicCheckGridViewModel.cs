using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using CVScreeningWeb.ViewModels.AtomicCheck;
using CVScreeningWeb.ViewModels.Screening;

namespace CVScreeningWeb.ViewModels.Home
{
    public class AtomicCheckGridViewModel
    {
        public string Type;
        public IEnumerable<AtomicCheckManageViewModel> AtomicChecks;

    }
}