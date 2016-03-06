using System.Diagnostics;
using System.Linq.Expressions;
using AutoMapper;
using CVScreeningCore.Error;
using CVScreeningCore.Exception;
using CVScreeningCore.Models;
using CVScreeningCore.Models.AtomicCheckState;
using CVScreeningCore.Models.AtomicCheckValidationState;
using CVScreeningCore.Models.ScreeningState;
using CVScreeningDAL.UnitOfWork;
using CVScreeningService.DTO.Client;
using CVScreeningService.DTO.Common;
using CVScreeningService.DTO.Discussion;
using CVScreeningService.DTO.LookUpDatabase;
using CVScreeningService.DTO.Screening;
using CVScreeningService.DTO.UserManagement;
using CVScreeningService.Filters;
using CVScreeningService.Services.Notification;
using CVScreeningService.Services.Permission;
using CVScreeningService.Services.SystemTime;
using CVScreeningService.Services.UserManagement;
using Microsoft.Ajax.Utilities;
using Nalysa.Common.Log;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;


namespace CVScreeningService.Services.Screening
{
    [Logging(Order = 1), ExceptionHandling(Order = 2)]
    public class ScreeningService : IScreeningService
    {
        private readonly IUnitOfWork _uow;
        private readonly IPermissionService _permissionService;
        private readonly IUserManagementService _userManagementService;
        private readonly ISystemTimeService _systemTimeService;
        private readonly IWebSecurity _webSecurity;
        private readonly INotificationService _notificationService;

        public ScreeningService(IUnitOfWork uow, 
            IPermissionService permissionService,
            IUserManagementService userManagementService,
            ISystemTimeService systemTimeService,
            IWebSecurity webSecurity,
            INotificationService notificationService)
        {
            _uow = uow;
            _permissionService = permissionService;
            _userManagementService = userManagementService;
            _systemTimeService = systemTimeService;
            _webSecurity = webSecurity;
            _notificationService = notificationService;


            Mapper.CreateMap<CVScreeningCore.Models.Screening, ScreeningBaseDTO>()
                .ForMember(dto => dto.ScreeningToReQualify, m => m.MapFrom(poco => poco.HasWronflyQualifiedAtomicChecks()))
                .ForMember(dto => dto.ScreeningToQualify, m => m.MapFrom(poco => !poco.IsQualificationCompleted() && !poco.IsDeactivated()))
                .ForMember(dto => dto.ClientCompanyName, m => m.MapFrom(poco => poco.ScreeningLevelVersion.ScreeningLevel.Contract.ClientCompany.ClientCompanyName))
                .ForMember(dto => dto.ScreeningLevelName, m => m.MapFrom(poco => poco.ScreeningLevelVersion.ScreeningLevel.ScreeningLevelName))
                .ForMember(dto => dto.ClientCompanyId, m => m.MapFrom(poco => poco.ScreeningLevelVersion.ScreeningLevel.Contract.ClientCompany.ClientCompanyId))
                .ForMember(dto => dto.ExternalDiscussionId, m => m.MapFrom(poco => poco.Discussion.First(u => u.DiscussionType == CVScreeningCore.Models.Discussion.kExternalScreeningType).DiscussionId))
                .ForMember(dto => dto.InternalDiscussionId, m => m.MapFrom(poco => poco.Discussion.First(u => u.DiscussionType == CVScreeningCore.Models.Discussion.kInternalScreeningType).DiscussionId))
                .ForMember(dto => dto.State, m => m.MapFrom(poco => poco.State.Name));

            Mapper.CreateMap<CVScreeningCore.Models.Screening, ScreeningDTO>();

            Mapper.CreateMap<AtomicCheck, AtomicCheckBaseDTO>()
                .ForMember(dto => dto.CanBeAssigned, m => m.MapFrom(poco => poco.CanBeAssigned()))
                .ForMember(dto => dto.CanBeProcessed, m => m.MapFrom(poco => poco.CanBeProcessed()))
                .ForMember(dto => dto.CanBeValidated, m => m.MapFrom(poco => poco.CanBeValidated()))
                .ForMember(dto => dto.CanBeRejected, m => m.MapFrom(poco => poco.CanBeRejected()))
                .ForMember(dto => dto.CanBeWronglyQualified, m => m.MapFrom(poco => poco.CanBeWronglyQualified()))
                .ForMember(dto => dto.IsQualified, m => m.MapFrom(poco => poco.IsQualified()))
                .ForMember(dto => dto.IsValidated, m => m.MapFrom(poco => poco.IsValidated()))
                .ForMember(dto => dto.State, m => m.MapFrom(poco => poco.State.Name))
                .ForMember(dto => dto.ValidationState, m => m.MapFrom(poco => poco.ValidationState.Name))
                .ForMember(dto => dto.AtomicCheckVerificationSummary, m => m.MapFrom(poco => poco.GetSummaryVerificationValue()))
                .ForMember(dto => dto.TypeOfCheckComments, m => m.MapFrom(poco => poco.TypeOfCheckScreeningLevelVersion.TypeOfCheckScreeningComments))
                .ForMember(dto => dto.Screening, m => m.MapFrom(poco => poco.Screening))
                .ForMember(dto => dto.InternalDiscussionId, m => m.MapFrom(poco => poco.Discussion.First(u => u.DiscussionType == CVScreeningCore.Models.Discussion.kInternalAtomicCheckType).DiscussionId))
                .ForMember(dto => dto.AssignedTo, m => m.MapFrom(poco => poco.Screener != null ? poco.Screener.FullName : "" ))
                .ForMember(dto => dto.TypeOfCheckCode, m => m.MapFrom(poco => poco.TypeOfCheckScreeningLevelVersion.TypeOfCheck.TypeOfCheckCode))
                .ForMember(dto => dto.TypeOfCheckName, m => m.MapFrom(poco => poco.TypeOfCheckScreeningLevelVersion.TypeOfCheck.CheckName));

            Mapper.CreateMap<AtomicCheck, AtomicCheckDTO>();


            Mapper.CreateMap<Attachment, AttachmentDTO>()
                .ForMember(e => e.ClientCompanyId, e => e.MapFrom(poco => poco.GetClientCompanyId())); Mapper.CreateMap<ScreeningReport, ScreeningReportDTO>();
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
            Mapper.CreateMap<CVScreeningCore.Models.Discussion, DiscussionDTO>();
            Mapper.CreateMap<Message, MessageDTO>();


            Mapper.CreateMap<QualificationPlace, BaseQualificationPlaceDTO>()
                .Include<QualificationPlace, QualificationPlaceDTO>()
                .Include<Court, CourtDTO>()
                .Include<Police, PoliceDTO>()
                .Include<ImmigrationOffice, ImmigrationOfficeDTO>()
                .Include<Faculty, FacultyDTO>()
                .Include<Company, CompanyDTO>()
                .Include<HighSchool, HighSchoolDTO>()
                .Include<DrivingLicenseCheckingOffice, DrivingLicenseOfficeDTO>()
                .Include<CertificationPlace, CertificationPlaceDTO>()
                .Include<PopulationOffice, PopulationOfficeDTO>();

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
            Mapper.CreateMap<PopulationOffice, PopulationOfficeDTO>();
            Mapper.CreateMap<ScreeningQualificationPlaceMeta, ScreeningQualificationPlaceMetaDTO>();
        }

        #region Type of check method

        /// <summary>
        /// Get all the type of checks existing in the application
        /// </summary>
        /// <returns></returns>
        public virtual List<TypeOfCheckDTO> GetAllTypeOfChecks()
        {
            var typeOfChecks = _uow.TypeOfCheckRepository.GetAll();
            return typeOfChecks.Select(Mapper.Map<TypeOfCheck, TypeOfCheckDTO>).ToList();
        }

        /// <summary>
        /// Get type of check 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public virtual TypeOfCheckDTO GetTypeOfCheck(int id)
        {
            if (!_uow.TypeOfCheckRepository.Exist(u => u.TypeOfCheckId == id))
            {
                return null;
            }

            var typeOfCheck = _uow.TypeOfCheckRepository.Single(u => u.TypeOfCheckId == id);
            return Mapper.Map<TypeOfCheck, TypeOfCheckDTO>(typeOfCheck);
        }


        /// <summary>
        /// Create a type of check
        /// </summary>
        /// <param name="typeOfCheckDTO"></param>
        /// <returns></returns>
        public virtual ErrorCode CreateTypeOfCheck(ref TypeOfCheckDTO typeOfCheckDTO)
        {
            var checkName = typeOfCheckDTO.CheckName;
            // Type of check is already existing
            if (_uow.TypeOfCheckRepository.Exist(u => u.CheckName == checkName))
            {
                return ErrorCode.TYPE_OF_CHECK_ALREADY_EXISTS;
            }

            var typeOfCheck = new TypeOfCheck
            {
                CheckName = checkName,
                TypeOfCheckCode = typeOfCheckDTO.TypeOfCheckCode
            };

            // Add type of check to repository
            _uow.TypeOfCheckRepository.Add(typeOfCheck);
            _uow.Commit();

            typeOfCheckDTO = Mapper.Map<TypeOfCheck, TypeOfCheckDTO>(typeOfCheck);
            return ErrorCode.NO_ERROR;
        }

        /// <summary>
        /// Add a type of check metadata (meta key and value)
        /// </summary>
        /// <param name="typeOfCheckName"></param>
        /// <param name="typeOfCheckMetaKey"></param>
        /// <param name="typeOfCheckMetaValue"></param>
        /// <param name="category"></param>
        /// <returns></returns>
        public virtual ErrorCode CreateTypeOfCheckMetaValue(
            string typeOfCheckName, string typeOfCheckMetaKey, string typeOfCheckMetaValue, string category= null)
        {
            // Type of check is already existing
            if (!_uow.TypeOfCheckRepository.Exist(u => u.CheckName == typeOfCheckName))
            {
                return ErrorCode.TYPE_OF_CHECK_NOT_FOUND;
            }

            var typeOfCheck = _uow.TypeOfCheckRepository.First(u => u.CheckName == typeOfCheckName);

            var typeOfCheckMeta = new TypeOfCheckMeta
            {
                TypeOfCheck = typeOfCheck,
                TypeOfCheckMetaKey = typeOfCheckMetaKey,
                TypeOfCheckMetaValue = typeOfCheckMetaValue,
                TypeOfCheckMetaCategory = category
            };

            typeOfCheck.TypeOfCheckMeta.Add(typeOfCheckMeta);

            // Add type of check to repository
            _uow.TypeOfCheckMetaRepository.Add(typeOfCheckMeta);
            _uow.Commit();

            return ErrorCode.NO_ERROR;
        }


        public virtual string GetTypeOfCheckMetaValue(int typeOfCheckId, string typeOfCheckMetaKey)
        {
            if(!_uow.TypeOfCheckRepository.Exist(e => e.TypeOfCheckId == typeOfCheckId)
                || !_uow.TypeOfCheckMetaRepository.Exist(e => e.TypeOfCheckMetaKey.ToLower()
                    .Equals(typeOfCheckMetaKey.ToLower())))
            {
                return null;
            }
            
            var typeOfCheckMeta = _uow.TypeOfCheckMetaRepository.First(
                e => e.TypeOfCheck.TypeOfCheckId == typeOfCheckId && e.TypeOfCheckMetaKey.ToLower().Equals(
                    typeOfCheckMetaKey.ToLower()));
            return typeOfCheckMeta.TypeOfCheckMetaValue;
        }

        #endregion

        #region Screening

        #region Setter for screening


        /// <summary>
        /// Permissions:
        /// Administrator and Qualifier => Authorize
        /// Client => Authorize if company belongs to him
        /// </summary>
        /// <param name="screeningLevelVersionDTO"></param>
        /// <param name="screeningDTO"></param>
        /// <returns></returns>
        [RequirePermission(CVScreeningCore.Models.Permission.kScreeningCreatePermission)]
        public virtual ErrorCode CreateScreening(
            ScreeningLevelVersionDTO screeningLevelVersionDTO, ref ScreeningDTO screeningDTO)
        {
            var sw = new Stopwatch();
            sw.Start();

            // validate whether the same name has been already exist on the same screeningLevelVersion
            var name = screeningDTO.ScreeningFullName;
            var screeningLevelVersionId = screeningLevelVersionDTO.ScreeningLevelVersionId;
            if (IsScreeningExistOnTheSameScreeningLevelVersion(name, screeningLevelVersionId))
                return ErrorCode.SCREENING_ALREADY_EXIST;

            //Map only primitive-type attributes
            var screening = new CVScreeningCore.Models.Screening();
            Mapper.CreateMap<ScreeningDTO, CVScreeningCore.Models.Screening>()
                .ForMember(dto => dto.QualityControl, opt => opt.Ignore())
                .ForMember(dto => dto.QualificationPlace, opt => opt.Ignore())
                .ForMember(dto => dto.ScreeningLevelVersion, opt => opt.Ignore())
                .ForMember(dto => dto.Attachment, opt => opt.Ignore())
                .ForMember(dto => dto.AtomicCheck, opt => opt.Ignore())
                .ForMember(dto => dto.ScreeningReport, opt => opt.Ignore())
                .ForMember(dto => dto.State, opt => opt.Ignore())
                .ForMember(dto => dto.ScreeningQualification, opt => opt.Ignore());
            screening = Mapper.Map<ScreeningDTO, CVScreeningCore.Models.Screening>(screeningDTO);

            LogManager.Instance.Debug(string.Format("Create screening Duration in ms: {0}",sw.ElapsedMilliseconds));


            //set the state to NEW
            screening.ScreeningState = Convert.ToByte(ScreeningStateType.NEW);

            // provide an instance of screening qualification
            screening.ScreeningQualification = new ScreeningQualification();

            // set attachment attribute
            Mapper.CreateMap<AttachmentDTO, Attachment>();
            screening.Attachment = screeningDTO.Attachment.Select(
                Mapper.Map<AttachmentDTO, Attachment>).ToList();

            // set screening level version
            screening.ScreeningLevelVersion = _uow.ScreeningLevelVersionRepository.First(e =>
                e.ScreeningLevelVersionId == screeningLevelVersionId);
            screening.ScreeningLevelVersion.Screening.Add(screening);

            LogManager.Instance.Debug(string.Format("Create screening 2 Duration in ms: {0}", sw.ElapsedMilliseconds));


            // link clientCompany
            var clientCompanyId =
                screeningDTO.ScreeningLevelVersion.ScreeningLevel.Contract.ClientCompany.ClientCompanyId;
            if (clientCompanyId != null)
            {
                screening.ScreeningLevelVersion.ScreeningLevel.Contract.ClientCompany =
                    _uow.ClientCompanyRepository.First(e => e.ClientCompanyId == clientCompanyId);
            }

            // set important dates
            SetDates(ref screening);

            // set reference of screning
            screening.ScreeningReference =
                GenerateScreeningReference(screening.ScreeningLevelVersion.ScreeningLevel.Contract);

            // set and create attachments 
            screening.Attachment = screeningDTO.Attachment.Select(attachment => new Attachment
            {
                AttachmentFilePath = attachment.AttachmentFilePath,
                AttachmentName = attachment.AttachmentName,
                AttachmentCreatedDate = DateTime.Now,
                AttachmentFileType = attachment.AttachmentFileType
            }).ToList();

            LogManager.Instance.Debug(string.Format("Create screening 3 Duration in ms: {0}", sw.ElapsedMilliseconds));
            screening.InitScreeningDiscussions();           // Create default screeening discussion
            LogManager.Instance.Debug(string.Format("Create screening 4 Duration in ms: {0}", sw.ElapsedMilliseconds));

            screening.InsertHistoryWhenScreeningCreated();  // Create history item
            LogManager.Instance.Debug(string.Format("Create screening 5 Duration in ms: {0}", sw.ElapsedMilliseconds));

            screening.InitScreeningAtomicChecks();          // Initialize Atomic Checks
            LogManager.Instance.Debug(string.Format("Create screening 6 Duration in ms: {0}", sw.ElapsedMilliseconds));


            // set quality control based on the minimum workload and grant permission to qc
            screening.AssignQualityControl(GetQualityControlWithMinimumWorkload());
            LogManager.Instance.Debug(string.Format("Create screening 7 Duration in ms: {0}", sw.ElapsedMilliseconds));

            screening.GrantPermissionForAccountManagers();
            LogManager.Instance.Debug(string.Format("Create screening 8 Duration in ms: {0}", sw.ElapsedMilliseconds));

            screening.GrantPermissionForClients();
            LogManager.Instance.Debug(string.Format("Create screening 9 Duration in ms: {0}", sw.ElapsedMilliseconds));

            _uow.ScreeningRepository.Add(screening);

            _uow.Commit();
            screeningDTO.ScreeningId = screening.ScreeningId;

            LogManager.Instance.Debug(string.Format("Create screening 10 Duration in ms: {0}", sw.ElapsedMilliseconds));

            //screeningDTO = Mapper.Map<CVScreeningCore.Models.Screening, ScreeningDTO>(screening);
            return _notificationService.CreateNotificationScreeningCreated(screeningDTO, true);
        }

        /// <summary>
        /// Permissions:
        /// Administrator and Qualifier => Authorize to edit
        /// Client => Authorize if company belongs to him
        /// </summary>
        /// <param name="screeningDTO"></param>
        /// <returns></returns>
        [RequirePermission(CVScreeningCore.Models.Permission.kScreeningManagePermission)]
        public virtual ErrorCode EditScreening(ref ScreeningDTO screeningDTO)
        {
            // validate whether the same name has been already exist on the same screeningLevelVersion
            var id = screeningDTO.ScreeningId;
            if (!_uow.ScreeningRepository.Exist(u => u.ScreeningId == id))
                return ErrorCode.SCREENING_NOT_FOUND;

            var screening = _uow.ScreeningRepository.Single(u => u.ScreeningId == id);
            screening.ScreeningAdditionalRemarks = screeningDTO.ScreeningAdditionalRemarks;

            // set and create attachments 
            foreach (var attachmentDTO in screeningDTO.Attachment)
            {
                screening.Attachment.Add(new Attachment
                {
                    AttachmentFilePath = attachmentDTO.AttachmentFilePath,
                    AttachmentName = attachmentDTO.AttachmentName,
                    AttachmentCreatedDate = DateTime.Now,
                    AttachmentFileType = attachmentDTO.AttachmentFileType
                });
            }
            _uow.Commit();
            screeningDTO = Mapper.Map<CVScreeningCore.Models.Screening, ScreeningDTO>(screening);
            return ErrorCode.NO_ERROR;

        }

        /// <summary>
        /// Permissions:
        /// Only administrator can deactivate a screening
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [RequirePermission(CVScreeningCore.Models.Permission.kScreeningDeactivatePermission)]
        public virtual ErrorCode DeactivateScreening(int id)
        {
            var screening = _uow.ScreeningRepository.First(e => e.ScreeningId == id);
            if (screening == null)
                return ErrorCode.SCREENING_NOT_FOUND;

            if (screening.IsDeactivated())
                return ErrorCode.SCREENING_ALREADY_DEACTIVATED;

            foreach (var atomicCheck in screening.AtomicCheck)
            {
                atomicCheck.UnassignAtomicCheck();
            }

            screening.State.ToDeactivated();
            _uow.Commit();
            return ErrorCode.NO_ERROR;
        }
        /// <summary>
        /// Assign the screening to a screener
        /// </summary>
        /// <param name="userProfileDTO"></param>
        /// <param name="screeningDTO"></param>
        /// <returns></returns>     
        public virtual ErrorCode AssignScreening(ref ScreeningDTO screeningDTO, UserProfileDTO userProfileDTO)
        {
            var qcId = userProfileDTO != null ? userProfileDTO.UserId : 0;
            var screeningId = screeningDTO != null ? screeningDTO.ScreeningId : 0;

            if (!_uow.ScreeningRepository.Exist(e => e.ScreeningId == screeningId))
                return ErrorCode.SCREENING_NOT_FOUND;

            if (!_uow.UserProfileRepository.Exist(e => e.UserId == qcId))
                return ErrorCode.ACCOUNT_USERID_NOT_FOUND;

            var screening = _uow.ScreeningRepository.First(e => e.ScreeningId == screeningId);
            var qc = _uow.UserProfileRepository.First(e => e.UserId == qcId);
            if (!qc.IsQualityControl())
                return ErrorCode.ACCOUNT_NOT_QUALITY_CONTROL;

            screening.AssignQualityControl(qc);
            _uow.Commit();

            return ErrorCode.NO_ERROR;
        }

        #endregion

        #region Getter for screening

        public virtual bool AreAllAtomicChecksValidated(ScreeningBaseDTO screeningDTO)
        {
            var screeningId = screeningDTO != null ? screeningDTO.ScreeningId : 0;

            if (!_uow.ScreeningRepository.Exist(e => e.ScreeningId == screeningId))
                return false;

            return _uow.ScreeningRepository.First(e => e.ScreeningId == screeningId).AreAllAtomicChecksValidated();
        }

        private IEnumerable<CVScreeningCore.Models.Screening> GetGrantedScreenings()
        {
            IEnumerable<CVScreeningCore.Models.Screening> screenings = null;
            if (_permissionService.HasAccessFromRoles(CVScreeningCore.Models.Permission.kScreeningViewPermission))
            {
                screenings = _uow.ScreeningRepository.GetAll();
            }
            else
            {
                var query = _permissionService.GenerateScreeningPermissionQuery(
                    CVScreeningCore.Models.Permission.kScreeningViewPermission);
                screenings = _uow.ScreeningRepository.Find(query);
            }
            return screenings;
        }

        private IEnumerable<AtomicCheck> GetGrantedAtomicChecks()
        {
            IEnumerable<AtomicCheck> atomicChecks = null;
            if (_permissionService.HasAccessFromRoles(CVScreeningCore.Models.Permission.kAtomicCheckViewPermission))
            {
                atomicChecks = _uow.AtomicCheckRepository.GetAll();
            }
            else
            {
                var query = _permissionService.GenerateAtomicCheckPermissionQuery(
                    CVScreeningCore.Models.Permission.kAtomicCheckViewPermission);
                atomicChecks = _uow.AtomicCheckRepository.Find(query);
            }
            return atomicChecks;
        }

        /// <summary>
        /// Retrieve all the screening where the user has permission on
        /// </summary>
        /// <returns></returns>
        public virtual  IEnumerable<ScreeningBaseDTO> GetAllScreenings()
        {
            var screenings = GetGrantedScreenings();

            var name = _webSecurity.GetCurrentUserName();
            var userProfile = _uow.UserProfileRepository.Single(u => u.UserName == name);
            if (userProfile.IsClient())
            {
                screenings = screenings.Where(screening => !screening.IsDeactivated());

                // Client does not have any knowledge about deactivated and validated status
                // !!! COMMIT IS FORBIDDEN HERE BECAUSE IT WILL CHANGE SCREENING STATUS !!!
                // Change is done only in memory
                foreach (var screening in screenings.Where(screening => screening.IsValidated()))
                {
                    screening.State.ToOpen();
                }
            }

            return screenings.Select(Mapper.Map<CVScreeningCore.Models.Screening, ScreeningBaseDTO>).ToList();
        }

  




        /// <summary>
        /// Retrieve all the screening that are assigned to current as qc
        /// </summary>
        /// <returns></returns>
        public virtual IEnumerable<ScreeningBaseDTO> GetAllScreeningsAssignedAsQualityControl()
        {
            var name = _webSecurity.GetCurrentUserName();
            var currentUser = _uow.UserProfileRepository.Single(u => u.UserName == name);
            var screenings = _uow.ScreeningRepository.Find(u => u.QualityControl.UserId == currentUser.UserId);
            return screenings.Select(Mapper.Map<CVScreeningCore.Models.Screening, ScreeningBaseDTO>).ToList();
        }


        /// <summary>
        /// Retrieve all the searched screening data 
        /// </summary>
        /// <returns></returns>
        public virtual IEnumerable<ScreeningBaseDTO> SearchScreening(string name, string client, string startDateStr, string endDateStr, string status)
        {
            var amName = _webSecurity.GetCurrentUserName();
            var am = _uow.UserProfileRepository.Single(u => u.UserName == amName);

            var screenings = GetGrantedScreenings();

            if (am.IsClient())
            {
                screenings = screenings.Where(screening => !screening.IsDeactivated());

                // Client does not have any knowledge about deactivated and validated status
                // !!! COMMIT IS FORBIDDEN HERE BECAUSE IT WILL CHANGE SCREENING STATUS !!!
                // Change is done only in memory
                foreach (var screening in screenings.Where(screening => screening.IsValidated()))
                {
                    screening.State.ToOpen();
                }
            }

            // Search filtering by name, client, start date, end date and status
            if (!name.IsNullOrWhiteSpace())
            {
                screenings = screenings.Where(s => s.ScreeningFullName.Contains(name));
            }
            if (!client.IsNullOrWhiteSpace())
            {
                screenings = screenings.Where(s => s.ScreeningLevelVersion.ScreeningLevelVersionId == int.Parse(client));
            }
            if (!startDateStr.IsNullOrWhiteSpace())
            {
                screenings =
                    screenings.Where(
                        s => s.ScreeningDeliveryDate != null && 
                            (DateTime)(s.ScreeningDeliveryDate.Value).Date >= DateTime.Parse(startDateStr).Date);
            }
            if (!endDateStr.IsNullOrWhiteSpace())
            {
                screenings =
                    screenings.Where(
                        s => s.ScreeningDeliveryDate != null && 
                            (DateTime)(s.ScreeningDeliveryDate.Value).Date <= DateTime.Parse(endDateStr).Date);
            }

            if (!status.IsNullOrWhiteSpace())
            {
                screenings = screenings.Where(s => s.ScreeningState == int.Parse(status));
            }
            return screenings.Select(Mapper.Map<CVScreeningCore.Models.Screening, ScreeningBaseDTO>);
        }



        [RequirePermission(CVScreeningCore.Models.Permission.kScreeningViewPermission)]
        public virtual ScreeningBaseDTO GetBaseScreening(int id)
        {
            var screening = _uow.ScreeningRepository.First(e => e.ScreeningId == id);
            return screening == null ? null : Mapper.Map<CVScreeningCore.Models.Screening, ScreeningBaseDTO>(screening);
        }

        [RequirePermission(CVScreeningCore.Models.Permission.kScreeningViewPermission)]
        public virtual ScreeningDTO GetScreening(int id)
        {
            var screening = _uow.ScreeningRepository.First(e => e.ScreeningId == id);
            return screening == null ? null : Mapper.Map<CVScreeningCore.Models.Screening, ScreeningDTO>(screening);
        }

        public virtual ScreeningDTO GetScreeningForCover(int id)
        {
            var screening = _uow.ScreeningRepository.First(e => e.ScreeningId == id);
            return screening == null ? null : Mapper.Map<CVScreeningCore.Models.Screening, ScreeningDTO>(screening);
        }

        /// <summary>
        /// Permissions:
        /// Administrator, Qualifier => Access to all
        /// Client and AM => Access if belongs to client company
        /// </summary>
        /// <param name="contractId"></param>
        /// <returns></returns>
        [RequirePermission(CVScreeningCore.Models.Permission.kContractViewPermission)]
        public virtual IEnumerable<ScreeningLevelDTO> GetAllScreeningLevelsByContract(int contractId)
        {
            var contract = _uow.ClientContractRepository.First(e => e.ContractId == contractId);
            return contract.ScreeningLevel.Select(Mapper.Map<ScreeningLevel, ScreeningLevelDTO>);
        }

        [RequirePermission(CVScreeningCore.Models.Permission.kScreeningLevelViewPermission)]
        public virtual IEnumerable<ScreeningLevelVersionDTO> GetAllScreeningLevelVersionsByScreeningLevel(int screeningLevelId)
        {
            var screeningLevel = _uow.ScreeningLevelRepository.First(e => e.ScreeningLevelId == screeningLevelId);
            return
                screeningLevel.ScreeningLevelVersion.Select(Mapper.Map<ScreeningLevelVersion, ScreeningLevelVersionDTO>);
        }


        [RequirePermission(CVScreeningCore.Models.Permission.kScreeningLevelViewPermission)]
        public virtual ScreeningLevelVersionDTO GetActiveScreeningLevelVersion(int screeningLevelId)
        {
            var screeningLevelVersions = _uow.ScreeningLevelVersionRepository.Find(e =>
                e.ScreeningLevel.ScreeningLevelId == screeningLevelId);

            // No screening level version are available for today
            if (
                !screeningLevelVersions.Any(
                    u => u.ScreeningLevelVersionStartDate <= _systemTimeService.GetCurrentDateTime()
                         && u.ScreeningLevelVersionEndDate >= _systemTimeService.GetCurrentDateTime()))
            {
                return null;
            }

            return Mapper.Map<
                ScreeningLevelVersion, ScreeningLevelVersionDTO>(screeningLevelVersions.First(
                    u => u.ScreeningLevelVersionStartDate <= _systemTimeService.GetCurrentDateTime()
                         && u.ScreeningLevelVersionEndDate >= _systemTimeService.GetCurrentDateTime()));

        }



        [RequirePermission(CVScreeningCore.Models.Permission.kScreeningLevelViewPermission)]
        public virtual ScreeningLevelDTO GetScreeningLevel(int screeningLevelId)
        {
            var screeningLevel =
                _uow.ScreeningLevelRepository.First(e => e.ScreeningLevelId == screeningLevelId);
            return Mapper.Map<ScreeningLevel, ScreeningLevelDTO>(screeningLevel);
        }


        #endregion

        #region Helper method

        private bool IsScreeningExistOnTheSameScreeningLevelVersion(string name, int screeningLevelVersionId)
        {
            var existingScreening = _uow.ScreeningRepository.First(s => s.ScreeningFullName.ToLower()
                .Equals(name.ToLower()));

            if (existingScreening == null) return false;
            return existingScreening.ScreeningLevelVersion.ScreeningLevelVersionId == screeningLevelVersionId;
        }

        private string GenerateScreeningReference(Contract contract)
        {
            //TODO: Update if there is new tenant
            var countryCode = contract.ContractTenantId == 1 ? "INA" : "TLD";
            var contractYear = contract.ContractYear + "";
            var contractNumber = contract.ContractReference + "";
            var screeningId = contract.GetAllScreenings().Count();  // Screening already added to repository
            return string.Format("INT/{0}/ES/{1}/{2}-{3}", countryCode, contractYear, contractNumber, screeningId);
        }


        /// <summary>
        /// Returns quality control with minimum workload
        /// </summary>
        /// <returns></returns>
        private webpages_UserProfile GetQualityControlWithMinimumWorkload()
        {
            var comparator = int.MaxValue;
            webpages_UserProfile chosenQualityControl = null;

            foreach (var qualityControl in _uow.UserProfileRepository.GetAll().Where(u => u.IsQualityControl()))
            {
                //only users who have screening and which number of screening is less than comparator
                if (qualityControl.Screening == null || qualityControl.Screening.Count >= comparator) continue;
                comparator = qualityControl.Screening.Count;
                chosenQualityControl = qualityControl;
            }
            return chosenQualityControl;
        }

        private void SetDates(ref CVScreeningCore.Models.Screening screening)
        {
            // set upload date = the date of now
            screening.ScreeningUploadedDate = _systemTimeService.GetCurrentDateTime();

            // set start date = upload date + 1 working day
            screening.ScreeningStartingDate = Helpers.DateHelper.Add((DateTime)screening.ScreeningUploadedDate, 1,
                _uow.PublicHolidayRepository.GetAll().ToList());

            // set deadline date = starting date + (contract turnaround time – 1) working day
            var turnAroundDate = screening.ScreeningLevelVersion.ScreeningLevelVersionTurnaroundTime - 1;
            screening.ScreeningDeadlineDate = Helpers.DateHelper.Add((DateTime)screening.ScreeningStartingDate,
                turnAroundDate, _uow.PublicHolidayRepository.GetAll().ToList());
        }



        #endregion

        #endregion

        #region Atomic Check

        public virtual IEnumerable<AtomicCheckDTO> GetAllAtomicChecks()
        {
            var atomicChecks = GetGrantedAtomicChecks();
            return atomicChecks.Select(Mapper.Map<AtomicCheck, AtomicCheckDTO>);
        }

        public virtual IEnumerable<AtomicCheckBaseDTO> GetAllAtomicChecksBase()
        {
            var atomicChecks = GetGrantedAtomicChecks();
            return atomicChecks.Select(Mapper.Map<AtomicCheck, AtomicCheckBaseDTO>);
        }


        public virtual IEnumerable<AtomicCheckBaseDTO> GetAllAtomicChecksToAssign()
        {
            var atomicChecks = _uow.AtomicCheckRepository.Find(u => u.AtomicCheckValidationState != (Byte)AtomicCheckValidationStateType.VALIDATED)
                .Where(atomicCheck => atomicCheck.IsQualified() && !atomicCheck.IsDeactivated() 
                    && !atomicCheck.IsAssigned() && !atomicCheck.IsNotApplicable());
            return atomicChecks.Select(Mapper.Map<AtomicCheck, AtomicCheckBaseDTO>).ToList();
        }



        /// <summary>
        /// Retrieve all the atomic checks that are not yet processed
        /// </summary>
        /// <returns></returns>
        public virtual IEnumerable<AtomicCheckBaseDTO> GetAllAtomicChecksOnGoing()
        {
            var atomicChecks = _uow.AtomicCheckRepository.Find(
                u => u.AtomicCheckValidationState == (Byte)AtomicCheckValidationStateType.NOT_PROCESSED
                || u.AtomicCheckValidationState == (Byte)AtomicCheckValidationStateType.REJECTED).Where(
                atomicCheck => atomicCheck.IsQualified() && atomicCheck.IsAssigned());
            return atomicChecks.Select(Mapper.Map<AtomicCheck, AtomicCheckBaseDTO>).ToList();
        }



        /// <summary>
        /// Retrieve all the atomic checks assigned that are pending validation
        /// </summary>
        /// <returns></returns>
        public virtual IEnumerable<AtomicCheckBaseDTO> GetAllAtomicChecksPendingValidation()
        {
            var atomicChecks = _uow.AtomicCheckRepository.Find(
                 u => u.AtomicCheckValidationState == (Byte)AtomicCheckValidationStateType.PROCESSED);
            return atomicChecks.Select(Mapper.Map<AtomicCheck, AtomicCheckBaseDTO>).ToList();
        }


        /// <summary>
        /// Retrieve all the atomic checks assigned to the current user that are not yet processed
        /// </summary>
        /// <returns></returns>
        public IEnumerable<AtomicCheckBaseDTO> GetAllAtomicChecksOnGoingAssignedAsScreener()
        {
            var name = _webSecurity.GetCurrentUserName();
            var currentUser = _uow.UserProfileRepository.Single(u => u.UserName == name);

            var atomicChecks = _uow.AtomicCheckRepository.Find(u => u.Screener.UserId == currentUser.UserId
                && (u.AtomicCheckValidationState == (Byte)AtomicCheckValidationStateType.NOT_PROCESSED
                || u.AtomicCheckValidationState == (Byte)AtomicCheckValidationStateType.REJECTED))
                .Where(atomicCheck => atomicCheck.IsQualified() && atomicCheck.IsAssigned());

            return atomicChecks.Select(Mapper.Map<AtomicCheck, AtomicCheckBaseDTO>);
        }

        /// <summary>
        /// Retrieve all the atomic checks assigned as screener to the current user that are pending validation
        /// </summary>
        /// <returns></returns>
        public IEnumerable<AtomicCheckDTO> GetAllAtomicChecksPendingValidationAssignedAsScreener()
        {
            var name = _webSecurity.GetCurrentUserName();
            var currentUser = _uow.UserProfileRepository.Single(u => u.UserName == name);

            var atomicChecks = _uow.AtomicCheckRepository.Find(u => u.Screener.UserId == currentUser.UserId)
                .Where(atomicCheck => atomicCheck.IsProcessed());

            return atomicChecks.Select(Mapper.Map<AtomicCheck, AtomicCheckDTO>);
        }

        /// <summary>
        /// Retrieve all the atomic checks assigned as screener to the current user that are pending validation
        /// </summary>
        /// <returns></returns>
        public IEnumerable<AtomicCheckBaseDTO> GetAllAtomicChecksBasePendingValidationAssignedAsScreener()
        {
            var name = _webSecurity.GetCurrentUserName();
            var currentUser = _uow.UserProfileRepository.Single(u => u.UserName == name);

            var atomicChecks = _uow.AtomicCheckRepository.Find(u => u.Screener.UserId == currentUser.UserId
                && u.AtomicCheckValidationState == (Byte)AtomicCheckValidationStateType.PROCESSED);

            return atomicChecks.Select(Mapper.Map<AtomicCheck, AtomicCheckBaseDTO>);
        }




        /// <summary>
        /// Retrieve all the atomic checks assigned as QC to the current user that are pending validation
        /// </summary>
        /// <returns></returns>
        public IEnumerable<AtomicCheckBaseDTO> GetAllAtomicChecksBasePendingValidationAssignedAsQualityControl()
        {
            var name = _webSecurity.GetCurrentUserName();
            var currentUser = _uow.UserProfileRepository.Single(u => u.UserName == name);

            var atomicChecks = _uow.AtomicCheckRepository.Find(u => u.Screening.QualityControl.UserId == currentUser.UserId
                  && u.AtomicCheckValidationState == (Byte)AtomicCheckValidationStateType.PROCESSED);

            return atomicChecks.Select(Mapper.Map<AtomicCheck, AtomicCheckBaseDTO>);
        }

        [RequirePermission(CVScreeningCore.Models.Permission.kAtomicCheckViewPermission)]
        public virtual AtomicCheckDTO GetAtomicCheck(int id)
        {
            var atomicCheck = _uow.AtomicCheckRepository.First(e => e.AtomicCheckId == id);
            return atomicCheck == null ? null : Mapper.Map<AtomicCheck, AtomicCheckDTO>(atomicCheck);
        }


        [RequirePermission(CVScreeningCore.Models.Permission.kScreeningViewPermission)]
        public virtual IEnumerable<AtomicCheckDTO> GetAllAtomicChecksForScreening(ScreeningBaseDTO screeningDTO)
        {
            int id = screeningDTO.ScreeningId;
            // Screening does not exist
            if (!_uow.ScreeningRepository.Exist(u => u.ScreeningId == id))
            {
                return null;
            }
            var screening = _uow.ScreeningRepository.Single(u => u.ScreeningId == id);
            var atomicChecks = screening.AtomicCheck;

            if (atomicChecks == null)
                return null;

            if (_permissionService.HasAccessFromRoles(CVScreeningCore.Models.Permission.kAtomicCheckViewPermission))
            {
                return atomicChecks.Select(Mapper.Map<AtomicCheck, AtomicCheckDTO>).ToList();
            }
            else
            {
                var query =
                    _permissionService.GenerateAtomicCheckPermissionQuery(
                        CVScreeningCore.Models.Permission.kAtomicCheckViewPermission);
                return
                    screening.AtomicCheck.AsQueryable()
                        .Where(query)
                        .Select(Mapper.Map<AtomicCheck, AtomicCheckDTO>).ToList();
            }
        }

        [RequirePermission(CVScreeningCore.Models.Permission.kScreeningViewPermission)]
        public virtual IEnumerable<AtomicCheckBaseDTO> GetAllBaseAtomicChecksForScreening(ScreeningBaseDTO screeningDTO)
        {
            int id = screeningDTO.ScreeningId;
            // Screening does not exist
            if (!_uow.ScreeningRepository.Exist(u => u.ScreeningId == id))
            {
                return null;
            }
            var screening = _uow.ScreeningRepository.Single(u => u.ScreeningId == id);
            var atomicChecks = screening.AtomicCheck;

            if (atomicChecks == null)
                return null;

            if (_permissionService.HasAccessFromRoles(CVScreeningCore.Models.Permission.kAtomicCheckViewPermission))
            {
                return atomicChecks.Select(Mapper.Map<AtomicCheck, AtomicCheckBaseDTO>).ToList();
            }
            else
            {
                var query = _permissionService.GenerateAtomicCheckPermissionQuery(CVScreeningCore.Models.Permission.kAtomicCheckViewPermission);
                return screening.AtomicCheck.AsQueryable().Where(query).Select(Mapper.Map<AtomicCheck, AtomicCheckBaseDTO>).ToList();
            }
        }



        [RequirePermission(CVScreeningCore.Models.Permission.kAtomicCheckManagePermission)]
        public virtual ErrorCode EditAtomicCheck(ref AtomicCheckDTO atomicCheckDTO)
        {
            int atomicCheckId = atomicCheckDTO != null ? atomicCheckDTO.AtomicCheckId : 0;
            AtomicCheck atomicCheck = null;
            bool hasAtomicCheckBeenChanged = false;
            bool hasAtomicCheckValidationBeenChanged = false;
            bool isScreeningPreviouslyValidated = false;
            try
            {
                atomicCheck = _uow.AtomicCheckRepository.First(e => e.AtomicCheckId == atomicCheckId);
                if (atomicCheck == null)
                    return ErrorCode.ATOMIC_CHECK_NOT_FOUND;

                if (atomicCheck.AtomicCheckDeactivated)
                    return ErrorCode.ATOMIC_CHECK_ALREADY_DEACTIVATED;

                if (atomicCheckDTO.AtomicCheckState == (Byte) AtomicCheckStateType.ON_GOING &&
                    atomicCheckDTO.AtomicCheckValidationState == (Byte) AtomicCheckValidationStateType.PROCESSED)
                    return ErrorCode.ATOMIC_CHECK_PROCESSED_CANNOT_BE_ON_GOING;

                if (atomicCheckDTO.AtomicCheckState == (Byte) AtomicCheckStateType.ON_PROCESS_FORWARDED &&
                    atomicCheckDTO.AtomicCheckValidationState == (Byte) AtomicCheckValidationStateType.PROCESSED)
                    return ErrorCode.ATOMIC_CHECK_PROCESSED_CANNOT_BE_ON_PROCESS_FORWARDED;

                if (atomicCheck.IsSummaryMandatory((AtomicCheckStateType) atomicCheckDTO.AtomicCheckState)
                    && string.IsNullOrEmpty(atomicCheckDTO.AtomicCheckSummary))
                    return ErrorCode.ATOMIC_CHECK_SUMMARY_MANDATORY;

                isScreeningPreviouslyValidated = atomicCheck.Screening.IsValidated();
                hasAtomicCheckBeenChanged = atomicCheck.AtomicCheckState != atomicCheckDTO.AtomicCheckState;
                atomicCheck.AtomicCheckSummary = atomicCheckDTO.AtomicCheckSummary;
                atomicCheck.AtomicCheckReport = atomicCheckDTO.AtomicCheckReport;
                atomicCheck.AtomicCheckRemarks = atomicCheckDTO.AtomicCheckRemarks;

                // set attachment attribute
                Mapper.CreateMap<AttachmentDTO, Attachment>();
                atomicCheck.Attachment = atomicCheckDTO.Attachment.Select(
                    Mapper.Map<AttachmentDTO, Attachment>).ToList();

                // Atomic check state has been updated
                if (hasAtomicCheckBeenChanged)
                {
                    atomicCheck.State.GetNextTransitionAsAction((AtomicCheckStateType) atomicCheckDTO.AtomicCheckState)
                        .Invoke();
                    atomicCheck.ResetAddressIfNeighborhoodCheckAndWronglyQualified();
                }

                hasAtomicCheckValidationBeenChanged = atomicCheck.AtomicCheckValidationState != atomicCheckDTO.AtomicCheckValidationState;
                // Atomic check validation state has been updated
                // Check is done to see if atomic check is pending confirmation or wrongly qualified as their validation 
                // status is automatically set to VALIDATED by the application (and not by the user)
                if (!atomicCheck.IsPendingConfirmation() && !atomicCheck.IsWronglyQualified() && hasAtomicCheckValidationBeenChanged)
                {
                    atomicCheck.ValidationState.GetNextTransitionAsAction(
                        (AtomicCheckValidationStateType) atomicCheckDTO.AtomicCheckValidationState).Invoke();
                }

                atomicCheckDTO = Mapper.Map<AtomicCheck, AtomicCheckDTO>(atomicCheck);

                // Push notification
                if (hasAtomicCheckBeenChanged && atomicCheck.IsWronglyQualified())
                    _notificationService.CreateNotificationAtomicCheckWronglyQualified(atomicCheckDTO, false);
                else if (hasAtomicCheckValidationBeenChanged)
                    PushAtomicCheckValidationStatusNotification(atomicCheck);
                if (!isScreeningPreviouslyValidated && atomicCheck.Screening.IsValidated())
                    _notificationService.CreateNotificationScreeningValidated(Mapper.Map<CVScreeningCore.Models.Screening, ScreeningDTO>(atomicCheck.Screening), false);

                _uow.Commit();
                return ErrorCode.NO_ERROR;
            }
            catch (ExceptionAtomicCheckOnProcessForwardImpossible ex)
            {
                LogManager.Instance.Error(
                    string.Format("{0}. Cannot forward atomic check. AtomicCheckDTO: {1}." +
                                  "Error: {2}",
                        MethodBase.GetCurrentMethod().Name, atomicCheckDTO, ex));
                return ErrorCode.ATOMIC_CHECK_FORWARDED_IMPOSSIBLE;
            }
        }


        /// <summary>
        /// Push notification to database if atomic check has been wrongly qualified or atomic check status has been updated
        /// </summary>
        /// <param name="atomicCheck"></param>
        private void PushAtomicCheckValidationStatusNotification(AtomicCheck atomicCheck)
        {
            var atomicCheckDTO = Mapper.Map<AtomicCheck, AtomicCheckDTO>(atomicCheck);
            if (atomicCheck.IsProcessed())
                _notificationService.CreateNotificationAtomicCheckProcessed(atomicCheckDTO, false);
            else if (atomicCheck.IsRejected())
                _notificationService.CreateNotificationAtomicCheckRejected(atomicCheckDTO, false);
            else if (atomicCheck.IsValidated())
                _notificationService.CreateNotificationAtomicCheckValidated(atomicCheckDTO, false);

        }


        /// <summary>
        /// Assign the screening to a screener
        /// </summary>
        /// <param name="userProfileDTO"></param>
        /// <param name="atomicCheckDTO"></param>
        /// <returns></returns>     
        [RequirePermission(CVScreeningCore.Models.Permission.kAtomicCheckAssignPermission)]
        public virtual ErrorCode AssignAtomicCheck(ref AtomicCheckDTO atomicCheckDTO, UserProfileDTO userProfileDTO)
        {
            var screenerId = userProfileDTO != null ? userProfileDTO.UserId : 0;
            var atomicCheckId = atomicCheckDTO != null ? atomicCheckDTO.AtomicCheckId : 0;
            try
            {
                if (!_uow.AtomicCheckRepository.Exist(e => e.AtomicCheckId == atomicCheckId))
                    return ErrorCode.ATOMIC_CHECK_NOT_FOUND;

                if (!_uow.UserProfileRepository.Exist(e => e.UserId == screenerId))
                    return ErrorCode.ACCOUNT_USERID_NOT_FOUND;

                var atomicCheck = _uow.AtomicCheckRepository.First(e => e.AtomicCheckId == atomicCheckId);
                var screener = _uow.UserProfileRepository.First(e => e.UserId == screenerId);
                atomicCheck.AssignAtomicCheck(screener);

                atomicCheckDTO = Mapper.Map<AtomicCheck, AtomicCheckDTO>(atomicCheck);
                _notificationService.CreateNotificationAtomicCheckAssigned(atomicCheckDTO, false);
                _uow.Commit();
                return ErrorCode.NO_ERROR;

            }
            catch (ExceptionAtomicCheckAssignCategoryMismatch ex)
            {
                return ErrorCode.ATOMIC_CHECK_CATEGORY_MISMATCH_ASSIGNEMENT_IMPOSSIBLE;
            }
            catch (ExceptionAtomicCheckNotQualified ex)
            {
                return ErrorCode.ATOMIC_CHECK_NOT_QUALIFIED_ASSIGNEMENT_IMPOSSIBLE;
            }
            catch (ExceptionAtomicCheckNotApplicable ex)
            {
                return ErrorCode.ATOMIC_CHECK_NOT_APPLICABLE_ASSIGNEMENT_IMPOSSIBLE;
            }
            catch (ExceptionAccountNotBelongsToScreenerRole ex)
            {
                return ErrorCode.SCREENING_ACCOUNT_NOT_BELONGS_TO_SCREENER_ROLE;
            }
            catch (ExceptionScreeningDeactivated ex)
            {
                return ErrorCode.SCREENING_ALREADY_DEACTIVATED;
            }
        }
        #endregion

        #region Report

        [RequirePermission(CVScreeningCore.Models.Permission.kReportUploadPermission)]
        public virtual ErrorCode CreateManualReport(ScreeningBaseDTO screening, ref ScreeningReportDTO screeningReportDTO)
        {
            var screeningDT0 = screeningReportDTO.Screening;
            var screeningReport = new ScreeningReport
            {
                Screening = _uow.ScreeningRepository.First(e => e.ScreeningId == screeningDT0.ScreeningId),
                ScreeningReportUrl = screeningReportDTO.ScreeningReportUrl,
                ScreeningReportVersion = 0,
                ScreeningReportSubmittedDate = screeningReportDTO.ScreeningReportSubmittedDate,
                ScreeningReportFilePath = screeningReportDTO.ScreeningReportFilePath,
                ScreeningReportGenerationType = ScreeningReport.kManualGenerationType
            };
            _uow.ScreeningReportRepository.Add(screeningReport);
            screeningReport.GrantPermissionForQualityControl(screeningReport.Screening.QualityControl);
            _uow.Commit();
            return ErrorCode.NO_ERROR;
        }

        /// <summary>
        /// Implementation to get report
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [RequirePermission(CVScreeningCore.Models.Permission.kReportViewPermission)]
        public virtual ScreeningReportDTO GetReport(int id)
        {
            var report = _uow.ScreeningReportRepository.First(e => e.ScreeningReportId == id);
            return report == null ? null : Mapper.Map<ScreeningReport, ScreeningReportDTO>(report);
        }

        [RequirePermission(CVScreeningCore.Models.Permission.kReportManagePermission)]
        public virtual ErrorCode SubmitReport( ref ScreeningReportDTO screeningReportDTO, ref ScreeningDTO screeningDTO)
        {
            var screeningId = screeningDTO != null ? screeningDTO.ScreeningId : 0;
            var screeningReportId = screeningReportDTO != null ? screeningReportDTO.ScreeningReportId : 0;
            if (!_uow.ScreeningRepository.Exist(e => e.ScreeningId == screeningId))
                return ErrorCode.SCREENING_NOT_FOUND;

            var screening = _uow.ScreeningRepository.First(e => e.ScreeningId == screeningId);

            ScreeningReport screeningReport = null;
            switch (screeningReportDTO.ScreeningReportGenerationType)
            {
                // Submit automatic generated report
                case ScreeningReport.kAutomaticGenerationType:
                    screeningReport = new ScreeningReport
                    {
                        ScreeningReportContent = screeningReportDTO.ScreeningReportContent,
                        ScreeningReportGenerationType = ScreeningReport.kAutomaticGenerationType,
                    };
                    _uow.ScreeningReportRepository.Add(screeningReport);
                    screening.ScreeningReport.Add(screeningReport);
                    screeningReport.Screening = screening;
                    break;

                // Submit manual report
                case ScreeningReport.kManualGenerationType:
                    if (!_uow.ScreeningReportRepository.Exist(e => e.ScreeningReportId == screeningReportId))
                        return ErrorCode.REPORT_NOT_FOUND;
                    screeningReport =
                        _uow.ScreeningReportRepository.Single(e => e.ScreeningReportId == screeningReportId);
                    screeningReport.Screening = screening;
                    break;
            }

            screening.SubmitScreeningReport(screeningReport);

            // Screening report date
            screeningReport.ScreeningReportSubmittedDate = _systemTimeService.GetCurrentDateTime();
            screening.ScreeningLastUpdatedDate = screeningReport.ScreeningReportSubmittedDate;
            if (screeningReport.ScreeningReportVersion == 1)
                screening.ScreeningDeliveryDate = screeningReport.ScreeningReportSubmittedDate;

            screeningReport.GrantPermission();
            _uow.Commit();

            screeningReportDTO = Mapper.Map<ScreeningReport, ScreeningReportDTO>(screeningReport);
            screeningDTO = Mapper.Map<CVScreeningCore.Models.Screening, ScreeningDTO>(screening);
            _notificationService.CreateNotificationScreeningSubmitted(screeningDTO, screeningReportDTO, true);

            return ErrorCode.NO_ERROR;
        }


        #endregion

    }
}