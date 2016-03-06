using System.ComponentModel.DataAnnotations;
using CVScreeningWeb.Filters;
using Resources;

namespace CVScreeningWeb.ViewModels.Contract
{
    public class ContractFormViewModel
    {
        [LocalizedDisplayName("Id", NameResourceType = typeof(Resources.Common))]
        public int? Id { get; set; }

        public int ClientCompanyId { get; set; }

        [Required(ErrorMessageResourceName = "Required", ErrorMessageResourceType = typeof (Resources.Validation))]
        [LocalizedDisplayName("ReferenceNumber", NameResourceType = typeof(Resources.ClientCompany))]
        public string ReferenceNumber { get; set; }

        [LocalizedDisplayName("Description", NameResourceType = typeof(Resources.Common))]
        public string Description { get; set; }

        [Required(ErrorMessageResourceName = "Required", ErrorMessageResourceType = typeof(Resources.Validation))]
        [LocalizedDisplayName("Year", NameResourceType = typeof(Resources.Common))]
        [Range(0, 9999, ErrorMessageResourceName = "YearValid", ErrorMessageResourceType = typeof(Resources.Validation))]
        public string Year { get; set; }

        public bool IsEnabled { get; set; }
    }
}