using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web;
using CVScreeningWeb.Filters;
using CVScreeningWeb.Resources;
using CVScreeningWeb.ViewModels.Shared;

namespace CVScreeningWeb.ViewModels.Screening
{
    public class ScreeningFormViewModel
    {
        [LocalizedDisplayName("Id", NameResourceType = typeof (Resources.Common))]
        public int Id { get; set; }

        [Required(ErrorMessageResourceName = "Required", ErrorMessageResourceType = typeof (Validation))]
        [LocalizedDisplayName("Name", NameResourceType = typeof (Resources.Common))]
        public string Name { get; set; }

        [UIHint("ScreeningLevelVersionViewModel")]
        public ScreeningLevelVersionViewModel ScreeningLevelVersion { get; set; }

        [UIHint("CVFileViewModel")]
        [LocalizedDisplayName("CV", NameResourceType = typeof (Resources.Screening))]
        public AttachmentViewModel CV { get; set; }

        [UIHint("AttachmentFilesViewModel")]
        [LocalizedDisplayName("AttachmentsAlreadyUploaded", NameResourceType = typeof(Resources.Screening))]
        public IEnumerable<AttachmentViewModel> Attachments { get; set; }

        [UIHint("AttachmentFilesViewModel")]
        [LocalizedDisplayName("Attachments", NameResourceType = typeof(Resources.Common))]
        public IEnumerable<HttpPostedFileBase> AttachmentFiles { get; set; }

        [Required(ErrorMessageResourceName = "ScreeningCVFileRequired", ErrorMessageResourceType = typeof(Resources.Error))]
        [UIHint("CVFileViewModel")]
        [LocalizedDisplayName("CV", NameResourceType = typeof (Resources.Common))]
        public HttpPostedFileBase CVFile { get; set; }

        [UIHint("StringTextBox")]
        [LocalizedDisplayName("Comments", NameResourceType = typeof(Resources.Screening))]
        public string Comments { get; set; }

        [LocalizedDisplayName("AdditionnalRemarks", NameResourceType = typeof(Resources.Screening))]
        public string AdditionnalRemarks { get; set; }

        public string ScreeningPhysicalPath { get; set; }
        public string ScreeningVirtualPath { get; set; }

        public string PreviousPage { get; set; }


    }
}