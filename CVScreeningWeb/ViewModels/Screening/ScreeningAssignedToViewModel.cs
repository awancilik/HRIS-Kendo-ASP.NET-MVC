using CVScreeningWeb.Filters;

namespace CVScreeningWeb.ViewModels.Screening
{
    public class ScreeningAssignedToViewModel
    {
        [LocalizedDisplayName("QualityControl", NameResourceType = typeof(Resources.Screening))]
        public int UserProfileId { get; set; }

        public int ScreeningId { get; set; }

        public string PreviousPage { get; set; }

    }
}