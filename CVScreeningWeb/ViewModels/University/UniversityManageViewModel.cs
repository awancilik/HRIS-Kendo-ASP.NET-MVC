using CVScreeningWeb.Filters;

namespace CVScreeningWeb.ViewModels.University
{
    public class UniversityManageViewModel
    {
        [LocalizedDisplayName("Id", NameResourceType = typeof (Resources.Common))]
        public int Id { get; set; }

        [LocalizedDisplayName("Name", NameResourceType = typeof (Resources.University))]
        public string Name { get; set; }

        [LocalizedDisplayName("Website", NameResourceType = typeof (Resources.Common))]
        public string WebSite { get; set; }

    }
}