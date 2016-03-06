using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using CVScreeningWeb.Filters;
using CVScreeningWeb.ViewModels.Common;
using CVScreeningWeb.ViewModels.Contact;

namespace CVScreeningWeb.ViewModels.Account
{
    public class AccountEditViewModel : AccountBaseViewModel
    {
        [LocalizedDisplayName("Id", NameResourceType = typeof(Resources.Common))]
        public int UserId { get; set; }
    }
}