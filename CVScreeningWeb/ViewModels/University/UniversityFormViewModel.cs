using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using CVScreeningWeb.Filters;
using Resources;

namespace CVScreeningWeb.ViewModels.University
{
    public class UniversityFormViewModel
    {
        [LocalizedDisplayName("Id", NameResourceType = typeof (Resources.Common))]
        public int Id { get; set; }

        [Required(ErrorMessageResourceName = "Required", ErrorMessageResourceType = typeof (Resources.Validation))]
        [LocalizedDisplayName("Name", NameResourceType = typeof (Resources.University))]
        public string Name { get; set; }

        [LocalizedDisplayName("WebSite", NameResourceType = typeof (Resources.Common))]
        public string WebSite { get; set; }

        [LocalizedDisplayName("Faculty", NameResourceType = typeof (Resources.University))]
        public IEnumerable<FacultyManageViewModel> Faculties { get; set; }
    }
}