using System.ComponentModel.DataAnnotations;
using CVScreeningWeb.Filters;

namespace CVScreeningWeb.ViewModels.Account
{
    public class AccountDeactivateViewModel
    {
        [Required]
        [LocalizedDisplayName("Id", NameResourceType = typeof(Resources.Common))]
        public int UserId { get; set; }

        [Required]
        [LocalizedDisplayName("Email", NameResourceType = typeof(Resources.Common))]
        public string UserName { get; set; }
    }
}