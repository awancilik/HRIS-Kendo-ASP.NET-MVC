using System.ComponentModel.DataAnnotations;
using CVScreeningWeb.Filters;

namespace CVScreeningWeb.ViewModels.Account
{
    public class AccountLoginViewModel
    {
        [Required]
        [LocalizedDisplayName("Email", NameResourceType = typeof(Resources.Common))]
        public string UserName { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [LocalizedDisplayName("ConfirmPassword", NameResourceType = typeof(Resources.Account))]
        public string Password { get; set; }
    }
}