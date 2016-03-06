using System.ComponentModel.DataAnnotations;
using CVScreeningWeb.Filters;
using CVScreeningWeb.Resources;
using CVScreeningWeb.ViewModels.Shared;
using NUnit.Framework.Constraints;

namespace CVScreeningWeb.ViewModels.Screening
{
    public class ScreeningLevelVersionViewModel
    {
        public bool IsClientMode { get; set; }
        
        [LocalizedDisplayName("Object", NameResourceType = typeof (Resources.ClientCompany))]
        public string ClientCompanyId { get; set; }
        public string ClientCompanyName { get; set; }

        [LocalizedDisplayName("Contract", NameResourceType = typeof (Resources.ClientCompany))]
        public string ContractId { get; set; }
        public string  ContractName { get; set; }

        [Required(ErrorMessageResourceName = "Required", ErrorMessageResourceType = typeof (Validation))]
        [LocalizedDisplayName("ScreeningLevel", NameResourceType = typeof (Resources.Screening))]
        public string ScreeningLevelId { get; set; }
        public string ScreeningLevelName { get; set; }

        [LocalizedDisplayName("ScreeningLevelVersion", NameResourceType = typeof (Resources.Screening))]
        public string ScreeningLevelVersionId { get; set; }
        public string ScreeningLevelVersionName { get; set; }

        [LocalizedDisplayName("AllowedToContact", NameResourceType = typeof(Resources.ScreeningLevel))]
        public DropDownListViewModel AllowedToContact { get; set; }
    }
}