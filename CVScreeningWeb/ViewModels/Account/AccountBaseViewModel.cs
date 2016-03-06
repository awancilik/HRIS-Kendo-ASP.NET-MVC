using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web;
using System.Web.Mvc;
using CVScreeningWeb.Filters;
using CVScreeningWeb.ViewModels.Common;
using CVScreeningWeb.ViewModels.Contact;
using CVScreeningWeb.ViewModels.Shared;

namespace CVScreeningWeb.ViewModels.Account
{
    public class AccountBaseViewModel
    {

        [Required]
        [RegularExpression(@"[a-z0-9._%+-]+@[a-z0-9.-]+\.[a-z]{2,4}", ErrorMessageResourceType = typeof (Resources.Common), ErrorMessageResourceName = "IncorrectEmail")]
        [LocalizedDisplayName("Email", NameResourceType = typeof(Resources.Common))]
        public string UserName { get; set; }

        [Required]
        [LocalizedDisplayName("FullName", NameResourceType = typeof(Resources.Common))]
        public string FullName { get; set; }

        [LocalizedDisplayName("HomePhoneNumber", NameResourceType = typeof(Resources.Contact))]
        public PhoneViewModel HomePhoneNumber { get; set; }

        [LocalizedDisplayName("WorkPhoneNumber", NameResourceType = typeof(Resources.Contact))]
        public PhoneViewModel WorkPhoneNumber { get; set; }

        [AddressMandatory]
        [UIHint("AddressViewModel")]
        public AddressViewModel AddressViewModel { get; set; }

        [LocalizedDisplayName("EmergencyFullName", NameResourceType = typeof(Resources.Contact))]
        public string EmergencyContactName { get; set; }

        [LocalizedDisplayName("EmergencyMobilePhoneNumber", NameResourceType = typeof(Resources.Contact))]
        public PhoneViewModel EmergencyMobilePhoneNumber { get; set; }

        [LocalizedDisplayName("EmergencyWorkPhoneNumber", NameResourceType = typeof(Resources.Contact))]
        public PhoneViewModel EmergencyWorkPhoneNumber { get; set; }

        [LocalizedDisplayName("Remarks", NameResourceType = typeof(Resources.Common))]
        public string Remarks { get; set; }

        [LocalizedDisplayName("ScreenerCategory", NameResourceType = typeof(Resources.Account))]
        public RadioButtonViewModel ScreenerCategory { get; set; }

        // List box for
        [Required]
        [LocalizedDisplayName("Roles", NameResourceType = typeof(Resources.Common))]
        public IEnumerable<string> SelectedRoles { get; set; }

        [LocalizedDisplayName("Roles", NameResourceType = typeof(Resources.Common))]
        public List<SelectListItem> Roles { get; set; }

    }
}