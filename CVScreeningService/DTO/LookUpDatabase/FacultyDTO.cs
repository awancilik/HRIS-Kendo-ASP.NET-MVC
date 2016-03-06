namespace CVScreeningService.DTO.LookUpDatabase
{
    public class FacultyDTO : BaseQualificationPlaceDTO
    {
        public UniversityDTO University { get; set; }

        public string QualificationPlaceAlumniWebSite { get; set; }
    }
}