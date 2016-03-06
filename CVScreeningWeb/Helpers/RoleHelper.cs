using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using CVScreeningService.DTO.UserManagement;
using CVScreeningWeb.ViewModels.Contact;

namespace CVScreeningWeb.Helpers
{
    public class RoleHelper
    {
        public static List<SelectListItem> BuildRoleViewModel(List<RolesDTO> rolesDTO)
        {
            return rolesDTO.Select(role => new SelectListItem()
            {
                Text = role.RoleName,
                Value = role.RoleId.ToString(),
                Selected = role.RoleName == "Account manager" ? true : false
            }).ToList(); 
        }
    }
}