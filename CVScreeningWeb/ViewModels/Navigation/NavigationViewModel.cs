using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CVScreeningWeb.ViewModels.Navigation
{
    public class SpecificMenuViewModel
    {
        public string LinkText;
        public string ActionName;
        public string ControllerName;
        public bool IsDisplayed;
    }

    public class BaseMenuViewModel
    {
        public bool IsDisplayed;
    }

    public class NavigationViewModel
    {
        public BaseMenuViewModel ClientMenu;
        public BaseMenuViewModel LookupMenu;
        public BaseMenuViewModel ProductionMenu;
        public BaseMenuViewModel SearchMenu;
        public ICollection<SpecificMenuViewModel> SettingsMenu;
        public int ClientCompanyId;
    }
}