using System.ComponentModel.DataAnnotations;
using CVScreeningWeb.Filters;
using CVScreeningWeb.ViewModels.Common;
using CVScreeningWeb.Resources;
using CVScreeningWeb.ViewModels.Contact;
using CVScreeningWeb.ViewModels.Shared;
using Foolproof;

namespace CVScreeningWeb.ViewModels.ClientAccount
{
    public class ClientAccountFormViewModel
    {
        [LocalizedDisplayName("Id", NameResourceType = typeof (Resources.Common))]
        public int? Id { get; set; }

        [Required(ErrorMessageResourceName = "Required", ErrorMessageResourceType = typeof (Resources.Validation))]
        [LocalizedDisplayName("Email", NameResourceType = typeof (Resources.Common))]
        public string Email { get; set; }

        [LocalizedDisplayName("FullName", NameResourceType = typeof (Resources.Common))]
        public string FullName { get; set; }

        [LocalizedDisplayName("Object", NameResourceType = typeof(Resources.ClientCompany))]
        public DropDownListViewModel ClientCompany { get; set; }

        [UIHint("PhoneViewModel")]
        [LocalizedDisplayName("HomePhoneNumber", NameResourceType = typeof (Resources.Contact))]
        public PhoneViewModel HomePhoneNumber { get; set; }

        [UIHint("PhoneViewModel")]
        [LocalizedDisplayName("MobilePhoneNumber", NameResourceType = typeof(Resources.Contact))]
        public PhoneViewModel MobilePhoneNumber { get; set; }

        [LocalizedDisplayName("Comment", NameResourceType = typeof (Resources.Common))]
        public string Comment { get; set; }

        [LocalizedDisplayName("Position", NameResourceType = typeof (Resources.Common))]
        public string Position { get; set; }

        [AddressMandatory]
        [UIHint("AddressViewModel")]
        [LocalizedDisplayName("Address", NameResourceType = typeof(Resources.Common))]
        public AddressViewModel AddressViewModel { get; set; }
    }
}