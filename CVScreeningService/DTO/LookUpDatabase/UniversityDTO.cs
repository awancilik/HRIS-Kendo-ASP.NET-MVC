using System.Collections.Generic;

namespace CVScreeningService.DTO.LookUpDatabase
{
    public class UniversityDTO
    {
        public int UniversityId { get; set; }
        public string UniversityName { get; set; }
        public string UniversityWebSite { get; set; }

        public List<FacultyDTO> QualificationPlace { get; set; }
    }
}