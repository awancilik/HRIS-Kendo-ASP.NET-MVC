using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using CVScreeningWeb.Filters;
using CVScreeningWeb.ViewModels.Common;
using CVScreeningWeb.ViewModels.Contact;

namespace CVScreeningWeb.ViewModels.Account
{
    public class AccountDetailsViewModel
    {
        public int UserId { get; set; }

        [LocalizedDisplayName("Email", NameResourceType = typeof(Resources.Common))]
        public string UserName { get; set; }

        [LocalizedDisplayName("FullName", NameResourceType = typeof(Resources.Common))]
        public string FullName { get; set; }

        [UIHint("AddressViewModel")]
        public AddressViewModel AddressViewModel { get; set; }

        [LocalizedDisplayName("HomePhoneNumber", NameResourceType = typeof(Resources.Contact))]
        public PhoneViewModel HomePhoneNumber { get; set; }

        [LocalizedDisplayName("WorkPhoneNumber", NameResourceType = typeof(Resources.Contact))]
        public PhoneViewModel WorkPhoneNumber { get; set; }

        [LocalizedDisplayName("EmergencyFullName", NameResourceType = typeof(Resources.Contact))]
        public string EmergencyContactName { get; set; }

        [LocalizedDisplayName("EmergencyMobilePhoneNumber", NameResourceType = typeof(Resources.Contact))]
        public PhoneViewModel EmergencyMobilePhoneNumber { get; set; }

        [LocalizedDisplayName("EmergencyWorkPhoneNumber", NameResourceType = typeof(Resources.Contact))]
        public PhoneViewModel EmergencyWorkPhoneNumber { get; set; }

        [LocalizedDisplayName("Remarks", NameResourceType = typeof(Resources.Common))]
        public string Remarks { get; set; }

        [UIHint("StringTextBox")]
        [LocalizedDisplayName("ScreenerCategory", NameResourceType = typeof(Resources.Account))]
        public string ScreenerCategory { get; set; }

        // List box for
        [LocalizedDisplayName("Roles", NameResourceType = typeof(Resources.Common))]
        public IEnumerable<string> SelectedRoles { get; set; }

        [LocalizedDisplayName("Roles", NameResourceType = typeof(Resources.Common))]
        public List<SelectListItem> Roles { get; set; }

    }
}