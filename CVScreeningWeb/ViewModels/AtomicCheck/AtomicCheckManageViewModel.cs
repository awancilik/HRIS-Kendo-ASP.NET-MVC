using CVScreeningWeb.Filters;

namespace CVScreeningWeb.ViewModels.AtomicCheck
{
    public class AtomicCheckManageViewModel
    {
        public int Id { get; set; }

        public int ScreeningId { get; set; }

        public bool CanBeAssigned { get; set; }

        public bool CanBeProcessed { get; set; }

        public bool CanBeValidated { get; set; }

        public bool CanBeRejected { get; set; }

        [LocalizedDisplayName("Name", NameResourceType = typeof (Resources.Common))]
        public string ScreeningFullName { get; set; }

        [LocalizedDisplayName("TypeOfCheck", NameResourceType = typeof (Resources.Screening))]
        public string TypeOfCheck { get; set; }

        [LocalizedDisplayName("Deadline", NameResourceType = typeof(Resources.Screening))]
        public string Deadline { get; set; }

        [LocalizedDisplayName("DayPending", NameResourceType = typeof (Resources.Screening))]
        public string DayPending { get; set; }

        public int DayPendingInt { get; set; }

        [LocalizedDisplayName("AssignedTo", NameResourceType = typeof(Resources.Screening))]
        public string AssignedTo { get; set; }

        [LocalizedDisplayName("Status", NameResourceType = typeof (Resources.Common))]
        public string Status { get; set; }

        [LocalizedDisplayName("ValidationStatus", NameResourceType = typeof(Resources.AtomicCheck))]
        public string ValidationStatus { get; set; }

        [LocalizedDisplayName("Qualified", NameResourceType = typeof(Resources.Common))]
        public bool Qualified { get; set; }

        public int InternalDiscussionId { get; set; }
    }
}