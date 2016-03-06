using CVScreeningWeb.Filters;

namespace CVScreeningWeb.ViewModels.University
{
    public class FacultyFormViewModel : BaseQualificationPlace.QualificationPlaceFormViewModel
    {
        public int UniversityId { get; set; }

        [LocalizedDisplayName("AlumniWebsite", NameResourceType = typeof(Resources.University))]
        public string AlumniWebsite { get; set; }
    }
}