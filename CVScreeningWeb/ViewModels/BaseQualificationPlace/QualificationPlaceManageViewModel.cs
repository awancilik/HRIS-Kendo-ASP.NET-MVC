using CVScreeningWeb.Filters;

namespace CVScreeningWeb.ViewModels.BaseQualificationPlace
{
    public class QualificationPlaceManageViewModel
    {
        [LocalizedDisplayName("Id", NameResourceType = typeof (Resources.Common))]
        public int Id { get; set; }

        [LocalizedDisplayName("Name", NameResourceType = typeof (Resources.Common))]
        public string Name { get; set; }

        [LocalizedDisplayName("Address", NameResourceType = typeof (Resources.Common))]
        public string Address { get; set; }

        [LocalizedDisplayName("District", NameResourceType = typeof (Resources.Common))]
        public string District { get; set; }

        [LocalizedDisplayName("City", NameResourceType = typeof (Resources.Common))]
        public string City { get; set; }

        [LocalizedDisplayName("Category", NameResourceType = typeof(Resources.Common))]
        public string Category { get; set; }
    }
}