using System.ComponentModel.DataAnnotations;
using CVScreeningWeb.Filters;
using CVScreeningWeb.Resources;
using CVScreeningWeb.ViewModels.Shared;

namespace CVScreeningWeb.ViewModels.ProfessionalQualification
{
    public class ProfessionalQualificationFormViewModel
    {
        [LocalizedDisplayName("Id", NameResourceType = typeof(Resources.Common))]
        public int Id { get; set; }

        [Required(ErrorMessageResourceName = "Required", ErrorMessageResourceType = typeof (Resources.Validation))]
        [LocalizedDisplayName("Name", NameResourceType = typeof(Resources.Common))]
        public string Name { get; set; }

        [LocalizedDisplayName("Code", NameResourceType = typeof(Resources.CertificationPlace))]
        public string Code { get; set; }

        [LocalizedDisplayName("Center", NameResourceType = typeof(Resources.CertificationPlace))]
        public SelectListItemViewModel Centers { get; set; }

        [LocalizedDisplayName("Description", NameResourceType = typeof(Resources.Common))]
        public string Description { get; set; }

    }
}