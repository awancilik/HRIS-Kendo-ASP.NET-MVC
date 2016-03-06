using System.Collections.Generic;
using CVScreeningCore.Error;
using CVScreeningService.DTO.LookUpDatabase;

namespace CVScreeningService.Services.LookUpDatabase
{
    public interface IUniversityLookUpDatabaseService
    {
        List<UniversityDTO> GetAllUniversities();

        UniversityDTO GetUniversity(int id);

        ErrorCode CreateOrUpdateUniversity(ref UniversityDTO university);

        ErrorCode DeleteUniversity(int id);
    }
}