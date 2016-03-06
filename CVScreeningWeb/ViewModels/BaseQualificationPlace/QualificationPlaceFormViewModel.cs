using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using CVScreeningWeb.Filters;
using CVScreeningWeb.ViewModels.Account;
using CVScreeningWeb.ViewModels.Common;
using CVScreeningWeb.ViewModels.Shared;

namespace CVScreeningWeb.ViewModels.BaseQualificationPlace
{
    public class QualificationPlaceFormViewModel
    {
        [LocalizedDisplayName("Id", NameResourceType = typeof(Resources.Common))]
        public int Id { get; set; }

        [Required(ErrorMessageResourceName = "Required", ErrorMessageResourceType = typeof(Resources.Validation))]
        [LocalizedDisplayName("Name", NameResourceType = typeof(Resources.Common))]
        public string Name { get; set; }

        [LocalizedDisplayName("Category", NameResourceType = typeof(Resources.Common))]
        public DropDownListViewModel Category { get; set; }

        [LocalizedDisplayName("Description", NameResourceType = typeof(Resources.Common))]
        public string Description { get; set; }

        [AddressMandatory]
        [UIHint("AddressViewModel")]
        [LocalizedDisplayName("Address", NameResourceType = typeof (Resources.Common))]
        public AddressViewModel AddressViewModel { get; set; }

        [LocalizedDisplayName("Website", NameResourceType = typeof(Resources.Common))]
        public string Website { get; set; }

        [LocalizedDisplayName("AccountManager", NameResourceType = typeof (Resources.ClientCompany))]
        public IEnumerable<AccountManagerManageViewModel> AccountManagers { get; set; } 
    }
}