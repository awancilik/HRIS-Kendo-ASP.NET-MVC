using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Security;
using CVScreeningWeb.Resources;
using CVScreeningWeb.ViewModels.Navigation;
using Resources;
using WebMatrix.WebData;

namespace CVScreeningWeb.Helpers
{
    public class NavigationHelper
    {
        /// <summary>
        /// Returns true whether the client menu should be displayed for this user
        /// </summary>
        /// <returns></returns>
        private static bool IsClientMenuDisplayed()
        {
            var allowedRoles = new List<string> { CVScreeningCore.Models.webpages_Roles.kAdministratorRole};
            var roles = Roles.GetRolesForUser(WebSecurity.CurrentUserName);
            return roles.Any(allowedRoles.Contains);
        }

        /// <summary>
        /// Returns true whether the lookup menu should be displayed for this user
        /// </summary>
        /// <returns></returns>
        private static bool IsLookupMenuDisplayed()
        {
            var allowedRoles = new List<string>
            {
                CVScreeningCore.Models.webpages_Roles.kAdministratorRole,
                CVScreeningCore.Models.webpages_Roles.kProductionManagerRole,
                CVScreeningCore.Models.webpages_Roles.kQualifierRole,
                CVScreeningCore.Models.webpages_Roles.kScreenerRole,
            };
            var roles = Roles.GetRolesForUser(WebSecurity.CurrentUserName);
            return roles.Any(allowedRoles.Contains);
        }

        /// <summary>
        /// Returns true whether the production menu should be displayed for this user
        /// </summary>
        /// <returns></returns>
        private static bool IsProductionMenuDisplayed()
        {
            var allowedRoles = new List<string>
            {
                CVScreeningCore.Models.webpages_Roles.kAdministratorRole,
                CVScreeningCore.Models.webpages_Roles.kProductionManagerRole,
                CVScreeningCore.Models.webpages_Roles.kQualifierRole,
                CVScreeningCore.Models.webpages_Roles.kAccountManagerRole,
                CVScreeningCore.Models.webpages_Roles.kQualityControlRole,
                CVScreeningCore.Models.webpages_Roles.kScreenerRole
            };
            var roles = Roles.GetRolesForUser(WebSecurity.CurrentUserName);
            return roles.Any(allowedRoles.Contains);
        }

        /// <summary>
        /// Returns true whether the search menu should be displayed for this user
        /// </summary>
        /// <returns></returns>
        private static bool IsSearchMenuDisplayed()
        {
            var allowedRoles = new List<string>
            {
                CVScreeningCore.Models.webpages_Roles.kAdministratorRole,
                CVScreeningCore.Models.webpages_Roles.kAccountManagerRole,
            };
            var roles = Roles.GetRolesForUser(WebSecurity.CurrentUserName);
            return roles.Any(allowedRoles.Contains);
        }


        /// <summary>
        /// Returns true whether the user account should be displayed for this user
        /// </summary>
        /// <returns></returns>
        private static bool IsUserAccountMenuDisplayed()
        {
            var allowedRoles = new List<string>
            {
                CVScreeningCore.Models.webpages_Roles.kAdministratorRole,
                CVScreeningCore.Models.webpages_Roles.kHrRole,
            };
            var roles = Roles.GetRolesForUser(WebSecurity.CurrentUserName);
            return roles.Any(allowedRoles.Contains);
        }

        /// <summary>
        /// Returns true whether the user account should be displayed for this user
        /// </summary>
        /// <returns></returns>
        private static bool IsPublicHolidayMenuDisplayed()
        {
            var allowedRoles = new List<string>
            {
                CVScreeningCore.Models.webpages_Roles.kAdministratorRole,
                CVScreeningCore.Models.webpages_Roles.kHrRole,
            };
            var roles = Roles.GetRolesForUser(WebSecurity.CurrentUserName);
            return roles.Any(allowedRoles.Contains);
        }

        /// <summary>
        /// Returns true whether the user account should be displayed for this user
        /// </summary>
        /// <returns></returns>
        private static bool IsScreenerSkillsMenuDisplayed()
        {
            var allowedRoles = new List<string>
            {
                CVScreeningCore.Models.webpages_Roles.kAdministratorRole,
                CVScreeningCore.Models.webpages_Roles.kProductionManagerRole,
            };
            var roles = Roles.GetRolesForUser(WebSecurity.CurrentUserName);
            return roles.Any(allowedRoles.Contains);
        }

        public static NavigationViewModel BuildNavigationViewModel()
        {
            var viewModel= new NavigationViewModel
            {
                ClientMenu = new BaseMenuViewModel
                {
                    IsDisplayed = IsClientMenuDisplayed()
                },
                LookupMenu = new BaseMenuViewModel
                {
                    IsDisplayed = IsLookupMenuDisplayed()
                },
                ProductionMenu = new BaseMenuViewModel
                {
                    IsDisplayed = IsProductionMenuDisplayed()
                },
                SearchMenu = new BaseMenuViewModel
                {
                    IsDisplayed = IsSearchMenuDisplayed()
                },
                SettingsMenu = new List<SpecificMenuViewModel>
                {
                    new SpecificMenuViewModel
                    {
                        ActionName = "Manage",
                        ControllerName = "Account",
                        LinkText = Menu.UserManagement,
                        IsDisplayed = IsUserAccountMenuDisplayed()
                    },
                    new SpecificMenuViewModel
                    {
                        ActionName = "ManageSkillMatrix",
                        ControllerName = "Dispatching",
                        LinkText = Menu.ScreenerSkills,
                        IsDisplayed = IsScreenerSkillsMenuDisplayed()
                    },
                    new SpecificMenuViewModel
                    {
                        ActionName = "Index",
                        ControllerName = "PublicHoliday",
                        LinkText = Menu.PublicHoliday,
                        IsDisplayed = IsPublicHolidayMenuDisplayed()
                    }
                }
            };
            return viewModel;
        }
    }
}