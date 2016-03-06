using CVScreeningCore.Models;
using CVScreeningService.DTO.LookUpDatabase;

namespace CVScreeningService.Services.LookUpDatabase
{
    public interface IQualificationPlaceFactory
    {
        QualificationPlace Create(BaseQualificationPlaceDTO qualificationPlaceDTO);
        QualificationCode GetType(QualificationPlace qualificationPlace);
        QualificationCode GetType(BaseQualificationPlaceDTO qualificationPlaceDTO);
    }
}