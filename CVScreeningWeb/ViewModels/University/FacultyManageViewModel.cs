using System.Collections.Generic;

namespace CVScreeningWeb.ViewModels.University
{
    public class FacultyManageViewModel : BaseQualificationPlace.QualificationPlaceManageViewModel
    {

    }

    public class FacultyManageUniversityViewModel
    {
        public int UniversityId { get; set; }
        public string UniversityName { get; set; }
        public IEnumerable<FacultyManageViewModel> Faculties { get; set; }
    }
}