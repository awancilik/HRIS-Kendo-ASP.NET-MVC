using System.ComponentModel.DataAnnotations;
using CVScreeningWeb.Filters;

namespace CVScreeningWeb.ViewModels.Account
{
    public class AccountRecoverPasswordViewModel
    {
        /// <summary>
        /// To indicate response regarding certain messages
        /// </summary>
        public enum AccountRecoverPasswordMessageId
        {
            RECOVER_PASSWORD_EMAIL_SENT_SUCCESS,
            RECOVER_PASSWORD_EMAIL_NOT_FOUND_FAILURE,
            RECOVER_PASSWORD_FAILURE,
            RECOVER_PASSWORD_SUCCESS
        }

        [Required]
        [LocalizedDisplayName("Email", NameResourceType = typeof(Resources.Common))]
        public string UserName { get; set; }
    }
}