using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using AutoMapper;
using CVScreeningCore.Error;
using CVScreeningCore.Models;
using CVScreeningDAL.UnitOfWork;
using CVScreeningService.DTO.Client;
using CVScreeningService.DTO.Common;
using CVScreeningService.DTO.Discussion;
using CVScreeningService.DTO.LookUpDatabase;
using CVScreeningService.DTO.Screening;
using CVScreeningService.DTO.UserManagement;
using CVScreeningService.Filters;
using CVScreeningService.Services.UserManagement;
using Nalysa.Common.Log;
using UserProfile = CVScreeningCore.Models.webpages_UserProfile;

namespace CVScreeningService.Services.Discussion
{
    [Logging(Order = 1), ExceptionHandling(Order = 2)]
    public class DiscussionService : IDiscussionService
    {
        private readonly IUnitOfWork _uow;
        private readonly IUserManagementService _userManagementService;

        public DiscussionService(IUnitOfWork uow, IUserManagementService userManagementService)
        {
            _uow = uow;
            _userManagementService = userManagementService;

            Mapper.CreateMap<CVScreeningCore.Models.Discussion, DiscussionDTO>();
            Mapper.CreateMap<Message, MessageDTO>();
            Mapper.CreateMap<CVScreeningCore.Models.Screening, ScreeningDTO>();
            Mapper.CreateMap<AtomicCheck, AtomicCheckDTO>();
            Mapper.CreateMap<webpages_UserProfile, UserProfileDTO>();

            Mapper.CreateMap<Attachment, AttachmentDTO>();
            Mapper.CreateMap<ScreeningQualification, ScreeningQualificationDTO>();
            Mapper.CreateMap<ScreeningLevelVersion, ScreeningLevelVersionDTO>();
            Mapper.CreateMap<webpages_UserProfile, UserProfileDTO>();
            Mapper.CreateMap<Contract, ClientContractDTO>();
            Mapper.CreateMap<ScreeningLevel, ScreeningLevelDTO>();
            Mapper.CreateMap<TypeOfCheck, TypeOfCheckDTO>();
            Mapper.CreateMap<ClientCompany, ClientCompanyDTO>();
            Mapper.CreateMap<TypeOfCheckScreeningLevelVersion, TypeOfCheckScreeningLevelVersionDTO>();
            Mapper.CreateMap<Address, AddressDTO>();
            Mapper.CreateMap<Location, LocationDTO>();
            Mapper.CreateMap<ContactInfo, ContactInfoDTO>();
            Mapper.CreateMap<ContactPerson, ContactPersonDTO>();

            Mapper.CreateMap<QualificationPlace, BaseQualificationPlaceDTO>()
                .Include<QualificationPlace, QualificationPlaceDTO>()
                .Include<Court, CourtDTO>()
                .Include<Police, PoliceDTO>()
                .Include<ImmigrationOffice, ImmigrationOfficeDTO>()
                .Include<Faculty, FacultyDTO>()
                .Include<Company, CompanyDTO>()
                .Include<HighSchool, HighSchoolDTO>()
                .Include<DrivingLicenseCheckingOffice, DrivingLicenseOfficeDTO>()
                .Include<CertificationPlace, CertificationPlaceDTO>();

            Mapper.CreateMap<Court, CourtDTO>();
            Mapper.CreateMap<Police, PoliceDTO>();
            Mapper.CreateMap<Faculty, FacultyDTO>();
            Mapper.CreateMap<Company, CompanyDTO>();
            Mapper.CreateMap<ImmigrationOffice, ImmigrationOfficeDTO>();
            Mapper.CreateMap<HighSchool, HighSchoolDTO>();
            Mapper.CreateMap<DrivingLicenseCheckingOffice, DrivingLicenseOfficeDTO>();
            Mapper.CreateMap<ProfessionalQualification, ProfessionalQualificationDTO>();
            Mapper.CreateMap<CertificationPlace, CertificationPlaceDTO>();
            Mapper.CreateMap<University, UniversityDTO>();
        }

        #region Discussion method

        /// <summary>
        /// Create discussion in the application
        /// </summary>
        /// <param name="discussionDTO"></param>
        /// <param name="messageDTO"></param>
        /// <returns></returns>
        [RequirePermission(
            CVScreeningCore.Models.Permission.kExternalScreeningDiscussionManagePermission + "," +
            CVScreeningCore.Models.Permission.kInternalScreeningDiscussionManagePermission + "," +
            CVScreeningCore.Models.Permission.kInternalAtomicCheckDiscussionManagePermission)]
        public virtual ErrorCode CreateMessage(ref DiscussionDTO discussionDTO, ref MessageDTO messageDTO)
        {
            int? discussionId = discussionDTO != null ? discussionDTO.DiscussionId : 0;
            try
            {
                // Discussion not found
                if (!_uow.DiscussionRepository.Exist(u => u.DiscussionId == discussionId))
                {
                    return ErrorCode.DISCUSSION_NOT_FOUND;
                }

                var currentUserId = _userManagementService.GetCurrentUserId();
                var discussion = _uow.DiscussionRepository.Single(u => u.DiscussionId == discussionId);
                var createdBy = _uow.UserProfileRepository.Single(u => u.UserId == currentUserId);

                var message = new Message
                {
                    MessageContent = messageDTO.MessageContent,
                    MessageCreatedDate = DateTime.Now,
                    MessageCreatedBy = createdBy
                };

                discussion.Message.Add(_uow.MessageRepository.Add(message));
                _uow.Commit();

                discussionDTO = Mapper.Map<CVScreeningCore.Models.Discussion, DiscussionDTO>(discussion);
                messageDTO = Mapper.Map<Message, MessageDTO>(message);

                return ErrorCode.NO_ERROR;
            }
            catch (Exception ex)
            {
                LogManager.Instance.Error(
                    string.Format("Function: {0}. Discussion: {1}. Message: {2}. Exception: {3}",
                        MethodBase.GetCurrentMethod().Name,
                        discussionDTO, messageDTO, ex.Message));
                return ErrorCode.UNKNOWN_ERROR;
            }
        }

        /// <summary>
        /// Get all the messages of a discussion
        /// </summary>
        /// <param name="discussionDTO"></param>
        /// <returns></returns>
        [RequirePermission(
            CVScreeningCore.Models.Permission.kExternalScreeningDiscussionManagePermission + "," +
            CVScreeningCore.Models.Permission.kInternalScreeningDiscussionManagePermission + "," +
            CVScreeningCore.Models.Permission.kInternalAtomicCheckDiscussionManagePermission)]
        public virtual IEnumerable<MessageDTO> GetMessages(DiscussionDTO discussionDTO)
        {
            int? discussionId = discussionDTO != null ? discussionDTO.DiscussionId : -1;

            try
            {
                // Discussion not existing
                if (!_uow.DiscussionRepository.Exist(u => u.DiscussionId == discussionId))
                    return null;

                var discussion = _uow.DiscussionRepository.Single(
                    u => u.DiscussionId == discussionId);

                return discussion.Message.Select(
                    Mapper.Map<Message, MessageDTO>).ToList();
            }
            catch (Exception ex)
            {
                LogManager.Instance.Error(
                    string.Format("Function: {0}. Discussion: {1}. Error: {2}",
                        MethodBase.GetCurrentMethod().Name, discussionDTO, ex.Message));
                return null;
            }            
        }


        /// <summary>
        /// Get information of a discussion
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [RequirePermission(
            CVScreeningCore.Models.Permission.kExternalScreeningDiscussionManagePermission + "," +
            CVScreeningCore.Models.Permission.kInternalScreeningDiscussionManagePermission + "," +
            CVScreeningCore.Models.Permission.kInternalAtomicCheckDiscussionManagePermission)]
        public virtual DiscussionDTO GetDiscussion(int id)
        {
            try
            {
                // Discussion not existing
                if (!_uow.DiscussionRepository.Exist(u => u.DiscussionId == id))
                    return null;

                var discussion = _uow.DiscussionRepository.Single(
                    u => u.DiscussionId == id);

                return Mapper.Map<CVScreeningCore.Models.Discussion, DiscussionDTO>(discussion);
            }
            catch (Exception ex)
            {
                LogManager.Instance.Error(
                    string.Format("Function: {0}. DiscussionId: {1}. Error: {2}",
                        MethodBase.GetCurrentMethod().Name, id, ex.Message));
                return null;
            }   
        }

        #endregion
    }
}