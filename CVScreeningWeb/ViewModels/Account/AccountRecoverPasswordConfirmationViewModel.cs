using System.ComponentModel.DataAnnotations;
using CVScreeningWeb.Filters;

namespace CVScreeningWeb.ViewModels.Account
{
    public class AccountRecoverPasswordConfirmationViewModel
    {

        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [LocalizedDisplayName("NewPassword", NameResourceType = typeof(Resources.Account))]
        public string NewPassword { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [LocalizedDisplayName("ConfirmPassword", NameResourceType = typeof(Resources.Account))]
        [Compare("NewPassword", ErrorMessageResourceType = typeof(Resources.Account), ErrorMessageResourceName = "PasswordDoesNotMatch")]
        public string ConfirmPassword { get; set; }

        [Required]
        public string TokenGenerated { get; set; }

    }
}