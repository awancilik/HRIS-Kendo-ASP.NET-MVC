using System.Collections.Generic;

namespace CVScreeningService.DTO.LookUpDatabase
{
    public class CertificationPlaceDTO : BaseQualificationPlaceDTO
    {
        public IEnumerable<ProfessionalQualificationDTO> ProfessionalQualification { get; set; }
    }
}