using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using AutoMapper;
using CVScreeningCore.Error;
using CVScreeningCore.Models;
using CVScreeningDAL.UnitOfWork;
using CVScreeningService.DTO.Screening;
using CVScreeningService.Filters;
using Nalysa.Common.Log;

namespace CVScreeningService.Services.File
{
    [Logging(Order = 1), ExceptionHandling(Order = 2)]
    public class FileService : IFileService
    {
        private readonly IUnitOfWork _uow;

        public FileService(IUnitOfWork uow)
        {
            _uow = uow;
            Mapper.CreateMap<Attachment, AttachmentDTO>()
                .ForMember(e => e.ClientCompanyId, e => e.MapFrom(poco => poco.GetClientCompanyId()));
        }

        public virtual IEnumerable<AttachmentDTO> GetAllAttachments()
        {
            var attachments = _uow.AttachmentRepository.GetAll();
            return attachments.Select(Mapper.Map<Attachment, AttachmentDTO>);
        }

        public virtual IEnumerable<AttachmentDTO> GetAllAttachmentsByScreening(int screeningId)
        {
            var screening = _uow.ScreeningRepository.First(e => e.ScreeningId == screeningId);
            var attachments = _uow.AttachmentRepository.GetAll().Where(
                e => e.Screening.Equals(screening));
            return attachments.Select(Mapper.Map<Attachment, AttachmentDTO>);
        }

        public virtual AttachmentDTO GetAttachment(int id)
        {
            var attachment = _uow.AttachmentRepository.First(e => e.AttachmentId == id);
            return Mapper.Map<Attachment, AttachmentDTO>(attachment);
        }

        /// <summary>
        ///  Delete attachment in database and in the filesystem
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public virtual ErrorCode DeleteAttachment(int id)
        {
            try
            {
                // Screening does not exist
                if (!_uow.AttachmentRepository.Exist(u => u.AttachmentId == id))
                {
                    return ErrorCode.ATTACHMENT_NOT_FOUND;
                }
                var attachment = _uow.AttachmentRepository.First(e => e.AttachmentId == id);
                 _uow.AttachmentRepository.Delete(attachment);

                 System.IO.File.Delete(attachment.AttachmentFilePath);
                _uow.Commit();
                return ErrorCode.NO_ERROR;

            }
            catch (Exception ex)
            {
                LogManager.Instance.Error(
                string.Format("{0}. Cannot delete attachment. Attachment Id: {1}." +
                              "Error: {2}",
                    MethodBase.GetCurrentMethod().Name, id, ex));
                return ErrorCode.UNKNOWN_ERROR;
            }
        }
    }
}