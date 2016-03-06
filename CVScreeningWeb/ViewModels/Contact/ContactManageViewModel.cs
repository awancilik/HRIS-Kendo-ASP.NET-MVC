using System.Collections.Generic;
using CVScreeningWeb.Filters;

namespace CVScreeningWeb.ViewModels.Contact
{
    public class ContactManageViewModel
    {
        [LocalizedDisplayName("Id", NameResourceType = typeof (Resources.Common))]
        public int Id { get; set; }

        [LocalizedDisplayName("Name", NameResourceType = typeof (Resources.Contact))]
        public string Name { get; set; }

        [LocalizedDisplayName("MobilePhoneNumber", NameResourceType = typeof (Resources.Contact))]
        public string PhoneNumber { get; set; }

        [LocalizedDisplayName("Address", NameResourceType = typeof (Resources.Common))]
        public string Address { get; set; }

        [LocalizedDisplayName("City", NameResourceType = typeof (Resources.Common))]
        public string City { get; set; }

        [LocalizedDisplayName("Position", NameResourceType = typeof (Resources.Common))]
        public string Position { get; set; }

        [LocalizedDisplayName("EmailAddress", NameResourceType = typeof(Resources.Contact))]
        public string EmailAddress { get; set; }

        [LocalizedDisplayName("IsDeactivated", NameResourceType = typeof(Resources.Contact))]
        public bool IsDeactivated { get; set; }

    }

    public class ContactManageQualificationPlaceViewModel
    {
        public int QualificationPlaceId { get; set; }
        public string QualificationPlaceName { get; set; }
        public IEnumerable<ContactManageViewModel> Contacts { get; set; }
    }

    public class ContactManageClientCompanyViewModel
    {
        public int ClientCompanyId { get; set; }
        public string  ClientCompanyName { get; set; }
        public IEnumerable<ContactManageViewModel> Contacts { get; set; }
    }
}