using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using CVScreeningWeb.Filters;
using CVScreeningWeb.ViewModels.Common;
using CVScreeningWeb.ViewModels.Contact;

namespace CVScreeningWeb.ViewModels.Account
{
    public class AccountCreateViewModel : AccountBaseViewModel
    {
        [Required]
        [StringLength(100, 
            ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [LocalizedDisplayName("Password", NameResourceType = typeof(Resources.Account))]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [LocalizedDisplayName("ConfirmPassword", NameResourceType = typeof(Resources.Account))]
        [System.ComponentModel.DataAnnotations.Compare("Password", ErrorMessageResourceType = typeof(Resources.Account), ErrorMessageResourceName = "PasswordDoesNotMatch")]
        public string ConfirmPassword { get; set; }
    }

}