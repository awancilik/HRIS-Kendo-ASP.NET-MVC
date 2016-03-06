using CVScreeningWeb.Filters;

namespace CVScreeningWeb.ViewModels.AtomicCheck
{
    public class AtomicCheckAssignToViewModel
    {
        [LocalizedDisplayName("Screener", NameResourceType = typeof(Resources.AtomicCheck))]
        public int UserProfileId { get; set; }

        public int AtomicCheckId { get; set; }

        public int ScreeningId { get; set; }

        public string PreviousPage { get; set; }

    }
}