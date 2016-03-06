using CVScreeningWeb.Filters;
using CVScreeningWeb.Resources;
using CVScreeningWeb.ViewModels.Shared;

namespace CVScreeningWeb.ViewModels.ClientCompany
{
    public class ClientCompanyManageViewModel
    {
        [LocalizedDisplayName("Id", NameResourceType = typeof(Resources.Common))]
        public int Id { get; set; }

        [LocalizedDisplayName("Company", NameResourceType = typeof(Resources.ClientCompany))]
        public string Company { get; set; }

        [LocalizedDisplayName("Category", NameResourceType = typeof(Resources.Common))]
        public string Category { get; set; }

        [LocalizedDisplayName("PhoneNumber", NameResourceType = typeof(Resources.Common))]
        public string PhoneNumber { get; set; }

        [LocalizedDisplayName("IsDeactivated", NameResourceType = typeof (Resources.Account))]
        public bool IsDeactivated { get; set; }

        [LocalizedDisplayName("City", NameResourceType = typeof(Resources.Common))]
        public string City { get; set; }

        [LocalizedDisplayName("AccountManager", NameResourceType = typeof(Resources.ClientCompany))]
        public InlineListViewModel AccountManager { get; set; } 
    }
}