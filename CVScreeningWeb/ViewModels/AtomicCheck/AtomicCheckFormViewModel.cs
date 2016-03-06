using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web;
using System.Web.Mvc;
using CVScreeningWeb.Filters;
using CVScreeningWeb.Resources;
using CVScreeningWeb.ViewModels.Shared;

namespace CVScreeningWeb.ViewModels.AtomicCheck
{
    public class AtomicCheckFormViewModel
    {
        public int Id { get; set; }

        public string PreviousPage { get; set; }

        public int ScreeningId { get; set; }

        public string QualificationPlaceName { get; set; }

        public int QualificationPlaceId { get; set; }

        public bool CanBeProcessed { get; set; }

        public bool CanBeValidated { get; set; }

        public bool CanBeRejected { get; set; }

        public bool IsValidated { get; set; }

        public string ScreeningPhysicalPath { get; set; }

        public string AtomicCheckReportPhysicalPath { get; set; }

        public string AtomicCheckPictureReportPhysicalPath { get; set; }

        public string AtomicCheckReportVirtualPath { get; set; }

        public string AtomicCheckPictureReportVirtualPath { get; set; }

        [UIHint("StringTextBox")]
        [LocalizedDisplayName("Name", NameResourceType = typeof (Resources.Common))]
        public string ScreeningFullName { get; set; }

        [UIHint("StringTextBox")]
        [LocalizedDisplayName("TypeOfCheck", NameResourceType = typeof (Resources.Screening))]
        public string TypeOfCheck { get; set; }

        [UIHint("StringTextBox")]
        [LocalizedDisplayName("AssignedTo", NameResourceType = typeof(Resources.Screening))]
        public string AssignedTo { get; set; }

        [UIHint("StringTextBox")]
        [LocalizedDisplayName("Category", NameResourceType = typeof(Resources.AtomicCheck))]
        public string Category { get; set; }

        [UIHint("StringTextBox")]
        [LocalizedDisplayName("Type", NameResourceType = typeof(Resources.AtomicCheck))]
        public string Type { get; set; }

        [UIHint("StringTextArea")]
        [LocalizedDisplayName("Remarks", NameResourceType = typeof(Resources.AtomicCheck))]
        public string Remarks { get; set; }

        [UIHint("StringTextBox")]
        [LocalizedDisplayName("Summary", NameResourceType = typeof(Resources.AtomicCheck))]
        public string Summary { get; set; }

        [AllowHtml] 
        [LocalizedDisplayName("Report", NameResourceType = typeof(Resources.AtomicCheck))]
        public string Report { get; set; }

        [UIHint("AttachmentFilesViewModel")]
        [LocalizedDisplayName("AttachmentsAlreadyUploaded", NameResourceType = typeof(Resources.AtomicCheck))]
        public IEnumerable<AttachmentViewModel> Attachments { get; set; }

        [UIHint("AttachmentFilesViewModel")]
        [LocalizedDisplayName("Attachments", NameResourceType = typeof(Resources.Common))]
        public IEnumerable<HttpPostedFileBase> AttachmentFiles { get; set; }

        [UIHint("StringTextArea")]
        [LocalizedDisplayName("Comments", NameResourceType = typeof(Resources.Common))]
        public string Comments { get; set; }

        [LocalizedDisplayName("Status", NameResourceType = typeof (Resources.Common))]
        [Required(ErrorMessageResourceName = "Required", ErrorMessageResourceType = typeof(Validation))]
        public DropDownListViewModel Status { get; set; }

        [LocalizedDisplayName("ValidationStatus", NameResourceType = typeof(Resources.AtomicCheck))]
        [Required(ErrorMessageResourceName = "Required", ErrorMessageResourceType = typeof(Validation))]
        public DropDownListViewModel ValidationStatus { get; set; }

        [UIHint("StringTextArea")]
        [LocalizedDisplayName("Description", NameResourceType = typeof(Resources.Common))]
        public string Description { get; set; }

    }
}