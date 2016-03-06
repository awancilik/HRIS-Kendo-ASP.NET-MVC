using CVScreeningWeb.Filters;

namespace CVScreeningWeb.ViewModels.ProfessionalQualification
{
    public class ProfessionalQualificationManageViewModel
    {
        [LocalizedDisplayName("Id", NameResourceType = typeof (Resources.Common))]
        public int Id { get; set; }

        [LocalizedDisplayName("Name", NameResourceType = typeof (Resources.Common))]
        public string Name{ get; set; }

        [LocalizedDisplayName("Code", NameResourceType = typeof (Resources.ProfessionalQualification))]
        public string Code { get; set; }
    }
}