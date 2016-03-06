using System;
using CVScreeningCore.Models;
using CVScreeningDAL.Repo;

namespace CVScreeningDAL.UnitOfWork
{
    public class InMemoryUnitOfWork : IUnitOfWork, IDisposable
    {
        private readonly IRepository<Address> _addressRepository;
        private readonly IRepository<AtomicCheck> _atomicCheckRepository;
        private readonly IRepository<Attachment> _attachmentRepository;
        private readonly IRepository<ClientCompany> _clientCompanyRepository;
        private readonly IRepository<Contract> _clientContractRepository;
        private readonly IRepository<ContactInfo> _contactInfoRepository;
        private readonly IRepository<ContactPerson> _contactPersonRepository;
        private readonly IRepository<DefaultMatrix> _defaultMatrixRepository;
        private readonly IRepository<Discussion> _discussionRepository;
        private readonly IRepository<DispatchingSettings> _dispatchingSettingsRepository;
        private readonly IRepository<History> _historyRepository;
        private readonly IRepository<Location> _locationRepository;
        private readonly IRepository<webpages_Membership> _membershipRepository;
        private readonly IRepository<Message> _messageRepository;
        private readonly IRepository<NotificationOfUser> _notificationOfUserRepository;
        private readonly IRepository<Notification> _notificationRepository;
        private readonly IRepository<webpages_OAuthMembership> _oAuthMembershipRepository;
        private readonly IRepository<Permission> _permissionRepository;
        private readonly IRepository<Post> _postRepository;
        private readonly IRepository<ProfessionalQualification> _professionalQualificationRepository;
        private readonly IRepository<PublicHoliday> _publicHolidayRepository;
        private readonly IRepository<QualificationPlace> _qualificationPlaceRepository;
        private readonly IRepository<webpages_Roles> _roleRepository;
        private readonly IRepository<ScreeningLevel> _screeningLevelRepository;
        private readonly IRepository<ScreeningLevelVersion> _screeningLevelVersionRepository;
        private readonly IRepository<ScreeningQualification> _screeningQualificationRepository;
        private readonly IRepository<ScreeningReport> _screeningReportRepository;
        private readonly IRepository<Screening> _screeningRepository;
        private readonly IRepository<SkillMatrix> _skillMatrixRepository;
        private readonly IRepository<TypeOfCheckMeta> _typeOfCheckMetaRepository;
        private readonly IRepository<TypeOfCheck> _typeOfCheckRepository;
        private readonly IRepository<University> _universityRepository;
        private readonly IRepository<UserLeave> _userLeaveRepository;
        private readonly IRepository<webpages_UserProfile> _userProfileRepository;

        public InMemoryUnitOfWork()
        {
            #region Common

            _addressRepository = new InMemoryRepository<Address>();
            _contactInfoRepository = new InMemoryRepository<ContactInfo>();
            _contactPersonRepository = new InMemoryRepository<ContactPerson>();
            _locationRepository = new InMemoryRepository<Location>();
            _postRepository = new InMemoryRepository<Post>();

            #endregion

            #region User Management

            _membershipRepository = new InMemoryRepository<webpages_Membership>();
            _userProfileRepository = new InMemoryRepository<webpages_UserProfile>();
            _roleRepository = new InMemoryRepository<webpages_Roles>();
            _oAuthMembershipRepository = new InMemoryRepository<webpages_OAuthMembership>();
            _userLeaveRepository = new InMemoryRepository<UserLeave>();
            _permissionRepository = new InMemoryRepository<Permission>();

            #endregion

            #region Client Repository

            _clientContractRepository = new InMemoryRepository<Contract>();
            _clientCompanyRepository = new InMemoryRepository<ClientCompany>();

            #endregion

            #region Database Lookup            

            _qualificationPlaceRepository = new InMemoryRepository<QualificationPlace>();
            _professionalQualificationRepository = new InMemoryRepository<ProfessionalQualification>();
            _universityRepository = new InMemoryRepository<University>();

            #endregion

            #region Screening

            _typeOfCheckRepository = new InMemoryRepository<TypeOfCheck>();
            _typeOfCheckMetaRepository = new InMemoryRepository<TypeOfCheckMeta>();
            _screeningLevelRepository = new InMemoryRepository<ScreeningLevel>();
            _screeningLevelVersionRepository = new InMemoryRepository<ScreeningLevelVersion>();
            _screeningQualificationRepository = new InMemoryRepository<ScreeningQualification>();
            _screeningRepository = new InMemoryRepository<Screening>();
            _screeningQualificationRepository = new InMemoryRepository<ScreeningQualification>();
            _atomicCheckRepository = new InMemoryRepository<AtomicCheck>();
            _attachmentRepository = new InMemoryRepository<Attachment>();
            _screeningReportRepository = new InMemoryRepository<ScreeningReport>();

            #endregion

            #region History Repository

            _historyRepository = new InMemoryRepository<History>();

            #endregion

            #region Discussion and message

            _discussionRepository = new InMemoryRepository<Discussion>();
            _messageRepository = new InMemoryRepository<Message>();

            #endregion

            #region Application settings

            _publicHolidayRepository = new InMemoryRepository<PublicHoliday>();

            #endregion

            #region Dispatching

            _defaultMatrixRepository = new InMemoryRepository<DefaultMatrix>();
            _skillMatrixRepository = new InMemoryRepository<SkillMatrix>();
            _dispatchingSettingsRepository = new InMemoryRepository<DispatchingSettings>();

            #endregion

            #region Notification

            _notificationRepository = new InMemoryRepository<Notification>();
            _notificationOfUserRepository = new InMemoryRepository<NotificationOfUser>();

            #endregion
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }

        public IRepository<webpages_Membership> MembershipRepository
        {
            get { return _membershipRepository; }
        }

        public IRepository<webpages_UserProfile> UserProfileRepository
        {
            get { return _userProfileRepository; }
        }

        public IRepository<webpages_Roles> RoleRepository
        {
            get { return _roleRepository; }
        }

        public IRepository<Permission> PermissionRepository
        {
            get { return _permissionRepository; }
        }

        public IRepository<University> UniversityRepository
        {
            get { return _universityRepository; }
        }

        public IRepository<PublicHoliday> PublicHolidayRepository
        {
            get { return _publicHolidayRepository; }
        }

        public IRepository<DefaultMatrix> DefaultMatrixRepository
        {
            get { return _defaultMatrixRepository; }
        }

        public IRepository<SkillMatrix> SkillMatrixRepository
        {
            get { return _skillMatrixRepository; }
        }


        public IRepository<Address> AddressRepository
        {
            get { return _addressRepository; }
        }

        public IRepository<ContactInfo> ContactInfoRepository
        {
            get { return _contactInfoRepository; }
        }

        public IRepository<Location> LocationRepository
        {
            get { return _locationRepository; }
        }

        public IRepository<ContactPerson> ContactPersonRepository
        {
            get { return _contactPersonRepository; }
        }

        public IRepository<Post> PostRepository
        {
            get { return _postRepository; }
        }

        public IRepository<webpages_OAuthMembership> OAuthMembershipRepository
        {
            get { return _oAuthMembershipRepository; }
        }

        public IRepository<UserLeave> UserLeaveRepository
        {
            get { return _userLeaveRepository; }
        }

        public IRepository<QualificationPlace> QualificationPlaceRepository
        {
            get { return _qualificationPlaceRepository; }
        }

        public IRepository<ProfessionalQualification> ProfessionalQualificationRepository
        {
            get { return _professionalQualificationRepository; }
        }

        public IRepository<ClientCompany> ClientCompanyRepository
        {
            get { return _clientCompanyRepository; }
        }

        public IRepository<Contract> ClientContractRepository
        {
            get { return _clientContractRepository; }
        }

        public IRepository<TypeOfCheck> TypeOfCheckRepository
        {
            get { return _typeOfCheckRepository; }
        }

        public IRepository<ScreeningLevel> ScreeningLevelRepository
        {
            get { return _screeningLevelRepository; }
        }

        public IRepository<ScreeningLevelVersion> ScreeningLevelVersionRepository
        {
            get { return _screeningLevelVersionRepository; }
        }

        public IRepository<ScreeningQualification> ScreeningQualificationRepository
        {
            get { return _screeningQualificationRepository; }
        }

        public IRepository<Screening> ScreeningRepository
        {
            get { return _screeningRepository; }
        }

        public IRepository<AtomicCheck> AtomicCheckRepository
        {
            get { return _atomicCheckRepository; }
        }

        public IRepository<Attachment> AttachmentRepository
        {
            get { return _attachmentRepository; }
        }

        public IRepository<ScreeningReport> ScreeningReportRepository
        {
            get { return _screeningReportRepository; }
        }

        public IRepository<TypeOfCheckMeta> TypeOfCheckMetaRepository
        {
            get { return _typeOfCheckMetaRepository; }
        }

        public IRepository<History> HistoryRepository
        {
            get { return _historyRepository; }
        }

        public IRepository<Discussion> DiscussionRepository
        {
            get { return _discussionRepository; }
        }

        public IRepository<Message> MessageRepository
        {
            get { return _messageRepository; }
        }

        public IRepository<DispatchingSettings> DispatchingSettingsRepository
        {
            get { return _dispatchingSettingsRepository; }
        }

        public IRepository<Notification> NotificationRepository
        {
            get { return _notificationRepository; }
        }

        public IRepository<NotificationOfUser> NotificationOfUserRepository
        {
            get { return _notificationOfUserRepository; }
        }

        public void Commit()
        {
        }
    }
}