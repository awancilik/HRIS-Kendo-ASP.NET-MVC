using CVScreeningWeb.Filters;

namespace CVScreeningWeb.ViewModels.Screening
{
    public class ScreeningManageViewModel
    {
        [LocalizedDisplayName("Id", NameResourceType = typeof (Resources.Common))]
        public int Id { get; set; }

        [LocalizedDisplayName("Reference", NameResourceType = typeof(Resources.Screening))]
        public string Reference { get; set; }

        [LocalizedDisplayName("Name", NameResourceType = typeof (Resources.Common))]
        public string Name { get; set; }

        [LocalizedDisplayName("Object", NameResourceType = typeof (Resources.ScreeningLevel))]
        public string ScreeningLevel { get; set; }

        [LocalizedDisplayName("Deadline", NameResourceType = typeof (Resources.Screening))]
        public string Deadline { get; set; }

        [LocalizedDisplayName("DeliveryDate", NameResourceType = typeof(Resources.Screening))]
        public string DeliveryDate { get; set; }

        [LocalizedDisplayName("DayPending", NameResourceType = typeof (Resources.Screening))]
        public string DayPending { get; set; }

        public int DayPendingInt { get; set; }
        
        [LocalizedDisplayName("Status", NameResourceType = typeof (Resources.Common))]
        public string Status { get; set; }

        public int ExternalDiscussionId { get; set; }

        public int InternalDiscussionId { get; set; }

    }
}