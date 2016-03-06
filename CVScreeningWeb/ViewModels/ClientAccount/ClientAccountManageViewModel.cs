using CVScreeningWeb.Filters;

namespace CVScreeningWeb.ViewModels.ClientAccount
{
    public class ClientAccountManageViewModel
    {
        [LocalizedDisplayName("Id", NameResourceType = typeof (Resources.Common))]
        public int Id { get; set; }

        [LocalizedDisplayName("FullName", NameResourceType = typeof (Resources.Common))]
        public string FullName { get; set; }

        [LocalizedDisplayName("Email", NameResourceType = typeof (Resources.Common))]
        public string Email { get; set; }

        [LocalizedDisplayName("Company", NameResourceType = typeof (Resources.ClientCompany))]
        public string Company { get; set; }

        [LocalizedDisplayName("IsDeactivated", NameResourceType = typeof(Resources.Account))]
        public bool IsDeactivated { get; set; }

        [LocalizedDisplayName("Position", NameResourceType = typeof (Resources.Common))]
        public string Position { get; set; }
    }
}