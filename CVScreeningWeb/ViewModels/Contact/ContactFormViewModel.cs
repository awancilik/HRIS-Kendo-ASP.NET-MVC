using System.ComponentModel.DataAnnotations;
using CVScreeningWeb.Filters;
using CVScreeningWeb.ViewModels.Common;

namespace CVScreeningWeb.ViewModels.Contact
{
    public class ContactFormViewModel
    {
        [LocalizedDisplayName("Id", NameResourceType = typeof (Resources.Common))]
        public int Id { get; set; }

        [Required(ErrorMessageResourceName = "Required", ErrorMessageResourceType = typeof (Resources.Validation))]
        [LocalizedDisplayName("Name", NameResourceType = typeof (Resources.Contact))]
        public string Name { get; set; }

        [UIHint("AddressViewModel")]
        [LocalizedDisplayName("Address", NameResourceType = typeof(Resources.Common))]
        public AddressViewModel AddressViewModel { get; set; }

        [Required(ErrorMessageResourceName = "Required", ErrorMessageResourceType = typeof (Resources.Validation))]
        [LocalizedDisplayName("MobilePhoneNumber", NameResourceType = typeof (Resources.Contact))]
        public PhoneViewModel MobilePhoneNumber { get; set; }

        [LocalizedDisplayName("WorkPhoneNumber", NameResourceType = typeof (Resources.Contact))]
        public PhoneViewModel WorkPhoneNumber { get; set; }

        public int QualificationPlaceId { get; set; }

        public int ClientCompanyId { get; set; }

        [LocalizedDisplayName("Position", NameResourceType = typeof (Resources.Common))]
        public string Position { get; set; }

        [LocalizedDisplayName("Comment", NameResourceType = typeof (Resources.Contact))]
        public string Comment { get; set; }

        [LocalizedDisplayName("EmailAddress", NameResourceType = typeof(Resources.Contact))]
        public string EmailAddress { get; set; }
    }
}