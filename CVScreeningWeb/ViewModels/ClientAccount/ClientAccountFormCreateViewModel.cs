using System.ComponentModel.DataAnnotations;
using CVScreeningWeb.Filters;
using CVScreeningWeb.Resources;

namespace CVScreeningWeb.ViewModels.ClientAccount
{
    public class ClientAccountFormCreateViewModel : ClientAccountFormViewModel
    {
        [Required(ErrorMessageResourceName = "Required", ErrorMessageResourceType = typeof(Validation))]
        [LocalizedDisplayName("Password", NameResourceType = typeof(Resources.Account))]
        public string Password { get; set; }

        [LocalizedDisplayName("ConfirmPassword", NameResourceType = typeof(Resources.Account))]
        public string ConfirmPassword { get; set; }
    }
}