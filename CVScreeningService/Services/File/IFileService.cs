using System.Collections.Generic;
using CVScreeningCore.Error;
using CVScreeningService.DTO.Screening;

namespace CVScreeningService.Services.File
{
    public interface IFileService
    {
        IEnumerable<AttachmentDTO> GetAllAttachments();
        IEnumerable<AttachmentDTO> GetAllAttachmentsByScreening(int screeningId);
        AttachmentDTO GetAttachment(int id);

        /// <summary>
        ///  Delete attachment in database and in the filesystem
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        ErrorCode DeleteAttachment(int id);
    }
}