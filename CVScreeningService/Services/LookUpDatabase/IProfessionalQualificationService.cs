using System.Collections.Generic;
using CVScreeningCore.Error;
using CVScreeningService.DTO.LookUpDatabase;

namespace CVScreeningService.Services.LookUpDatabase
{
    public interface IProfessionalQualificationService
    {
        List<ProfessionalQualificationDTO> GetAllProfessionalQualifications();
        ProfessionalQualificationDTO GetProfessionalQualification(int id);
        ErrorCode CreateOrUpdateProfessionalQualification(ref ProfessionalQualificationDTO professionalQualification);
        ErrorCode DeleteProfessionalQualification(int id);
        List<CertificationPlaceDTO> GetCertificationPlaces(int id);
    }
}