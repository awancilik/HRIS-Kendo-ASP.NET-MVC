using System.Collections;
using System.Collections.Generic;
using CVScreeningWeb.Filters;
using CVScreeningWeb.ViewModels.Screening;

namespace CVScreeningWeb.ViewModels.Home
{
    public class DashboardQualityControlViewModel
    {
        public AtomicCheckGridViewModel AtomicChecksPendingValidation;
        public ScreeningGridViewModel ScreeningOnGoing;
        public ScreeningGridViewModel ScreeningToSubmit;
    }
}