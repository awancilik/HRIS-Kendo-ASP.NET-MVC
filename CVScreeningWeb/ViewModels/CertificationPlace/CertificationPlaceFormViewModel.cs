using System.ComponentModel.DataAnnotations;
using CVScreeningWeb.Filters;
using CVScreeningWeb.Resources;
using CVScreeningWeb.ViewModels.Shared;

namespace CVScreeningWeb.ViewModels.CertificationPlace
{
    public class CertificationPlaceFormViewModel : BaseQualificationPlace.QualificationPlaceFormViewModel
    {
        public int ProfessionalQualificationId { get; set; }

        [Required(ErrorMessageResourceName = "Required", ErrorMessageResourceType = typeof (Validation))]
        [LocalizedDisplayName("Object", NameResourceType = typeof (Resources.ProfessionalQualification))]
        public SelectListItemViewModel ProfessionalQualification { get; set; }
    }
}