using System.Collections;
using System.Collections.Generic;
using CVScreeningWeb.Filters;
using CVScreeningWeb.ViewModels.Screening;

namespace CVScreeningWeb.ViewModels.Home
{
    public class DashboardAdministratorViewModel
    {
        public AtomicCheckGridViewModel AtomicChecksToAssign;
        public AtomicCheckGridViewModel AtomicChecksOnGoing;
        public AtomicCheckGridViewModel AtomicChecksPendingValidation;
        public ScreeningGridViewModel ScreeningToQualify;
        public ScreeningGridViewModel ScreeningOnGoing;
        public ScreeningGridViewModel ScreeningToSubmit;
    }

}