using System.Collections.Generic;
using CVScreeningWeb.Filters;

namespace CVScreeningWeb.ViewModels.Contract
{
    public class ContractManageViewModel
    {
        [LocalizedDisplayName("Id", NameResourceType = typeof (Resources.Common))]
        public int Id { get; set; }

        [LocalizedDisplayName("IsEnabled", NameResourceType = typeof (Resources.ClientCompany))]
        public bool IsEnabled { get; set; }

        [LocalizedDisplayName("ReferenceNumber", NameResourceType = typeof (Resources.ClientCompany))]
        public string ReferenceNumber { get; set; }

        [LocalizedDisplayName("Description", NameResourceType = typeof (Resources.Common))]
        public string Description { get; set; }

        [LocalizedDisplayName("Year", NameResourceType = typeof (Resources.Common))]
        public string Year { get; set; }
    }

    public class ContractClientCompanyManageViewModel
    {
        public int ClientCompanyId { get; set; }

        public IEnumerable<ContractManageViewModel> ContractManageViewModels { get; set; }
 
    }
}