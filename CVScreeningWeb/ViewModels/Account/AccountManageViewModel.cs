using System.ComponentModel.DataAnnotations;
using CVScreeningCore.Models;
using CVScreeningWeb.Filters;
using PagedList;

namespace CVScreeningWeb.ViewModels.Account
{


    public class AccountManageViewModel
    {
        [LocalizedDisplayName("Id", NameResourceType = typeof(Resources.Common))]
        public int Id { get; set; }

        [LocalizedDisplayName("Email", NameResourceType = typeof(Resources.Common))]
        public string UserName { get; set; }

        [LocalizedDisplayName("FullName", NameResourceType = typeof(Resources.Common))]
        public string FullName { get; set; }

        [LocalizedDisplayName("Roles", NameResourceType = typeof(Resources.Common))]
        public string Roles { get; set; }

        [LocalizedDisplayName("Deactivated", NameResourceType = typeof(Resources.Common))]
        public bool IsDeactivated { get; set; }
    }
}