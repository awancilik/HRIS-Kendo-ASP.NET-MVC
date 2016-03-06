using System.Collections;
using System.Collections.Generic;
using CVScreeningWeb.Filters;
using CVScreeningWeb.ViewModels.Screening;

namespace CVScreeningWeb.ViewModels.Home
{
    public class DashboardProductionManagerViewModel
    {
        public AtomicCheckGridViewModel AtomicChecksToAssign;
        public AtomicCheckGridViewModel AtomicChecksOnGoing;
        public AtomicCheckGridViewModel AtomicChecksPendingValidation;
    }
}