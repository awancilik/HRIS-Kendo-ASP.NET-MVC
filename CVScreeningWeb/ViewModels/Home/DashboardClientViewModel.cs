using System.Collections;
using System.Collections.Generic;
using CVScreeningWeb.Filters;
using CVScreeningWeb.ViewModels.Screening;

namespace CVScreeningWeb.ViewModels.Home
{
    public class DashboardClientViewModel
    {
        public ScreeningGridViewModel ScreeningOnGoing;

        public ScreeningGridViewModel ScreeningCompleted;
    }

}