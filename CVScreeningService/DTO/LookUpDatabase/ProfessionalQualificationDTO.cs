using System.Collections.Generic;

namespace CVScreeningService.DTO.LookUpDatabase
{
    public class ProfessionalQualificationDTO
    {
        public int ProfessionalQualificationId { get; set; }
        public string ProfessionalQualificationName { get; set; }
        public string ProfessionalQualificationCode { get; set; }
        public string ProfessionalQualificationDescription { get; set; }
        public ICollection<CertificationPlaceDTO> QualificationPlace { get; set; }
    }
}