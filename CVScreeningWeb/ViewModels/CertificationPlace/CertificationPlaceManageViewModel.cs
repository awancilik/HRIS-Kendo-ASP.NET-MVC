using System.Collections.Generic;

namespace CVScreeningWeb.ViewModels.CertificationPlace
{
    public class CertificationPlaceManageViewModel : BaseQualificationPlace.QualificationPlaceManageViewModel
    {
         
    }

    public class CertificationPlaceForQualificationViewModel
    {
        public IEnumerable<CertificationPlaceManageViewModel> CertificationPlaceManageViewModel { get; set; }

        public int ProfessionalQualificationId { get; set; }

    }
}