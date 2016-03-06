using System;

namespace CVScreeningService.DTO.Screening
{
    public class AttachmentDTO
    {
        public int AttachmentId { get; set; }
        public string AttachmentName { get; set; }
        public DateTime AttachmentCreatedDate { get; set; }
        public string AttachmentFilePath { get; set; }
        public string AttachmentFileType { get; set; }
        public int ClientCompanyId { get; set; }
    }
}