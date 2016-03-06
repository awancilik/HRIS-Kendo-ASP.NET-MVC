using System.ComponentModel.DataAnnotations;
using CVScreeningWeb.Filters;
using CVScreeningWeb.Resources;
using CVScreeningWeb.ViewModels.Common;
using CVScreeningWeb.ViewModels.Shared;

namespace CVScreeningWeb.ViewModels.ClientCompany
{
    public class ClientCompanyFormViewModel
    {
        [LocalizedDisplayName("Id", NameResourceType = typeof (Resources.Common))]
        public int? Id { get; set; }

        [Required(ErrorMessageResourceName = "Required", ErrorMessageResourceType = typeof (Resources.Validation))]
        [LocalizedDisplayName("Company", NameResourceType = typeof (Resources.ClientCompany))]
        public string Company { get; set; }

        [Required(ErrorMessageResourceName = "Required", ErrorMessageResourceType = typeof(Validation))]
        [LocalizedDisplayName("Category", NameResourceType = typeof(Resources.ClientCompany))]
        public DropDownListViewModel Category { get; set; }

        [LocalizedDisplayName("Description", NameResourceType = typeof (Resources.Common))]
        public string Description { get; set; }

        [AddressMandatory]
        [UIHint("AddressViewModel")]
        [LocalizedDisplayName("Address", NameResourceType = typeof(Resources.Common))]
        public AddressViewModel AddressViewModel { get; set; }

        [LocalizedDisplayName("Website", NameResourceType = typeof(Resources.Common))]
        public string Website { get; set; }

        [Required(ErrorMessageResourceName = "Required", ErrorMessageResourceType = typeof (Validation))]
        [LocalizedDisplayName("AccountManagers", NameResourceType = typeof (Resources.Account))]
        public SelectListItemViewModel AccountManagers { get; set; }

        public bool IsDeactivated { get; set; }
    }
}