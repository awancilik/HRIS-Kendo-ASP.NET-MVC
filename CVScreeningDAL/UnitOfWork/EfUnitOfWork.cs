using System;
using System.Data;
using System.Data.Entity.Validation;
using System.Linq;
using System.Reflection;
using System.Transactions;
using CVScreeningDAL.EntityFramework;
using CVScreeningDAL.Repo;
using CVScreeningCore.Models;
using Nalysa.Common.Log;

namespace CVScreeningDAL.UnitOfWork
{
    public class EfUnitOfWork : IUnitOfWork, IDisposable
    {
        /// <summary>
        /// Entity framework db context
        /// </summary>
        private readonly CVScreeningEFContext _dbContext;

        /// <summary>
        /// Tenant ID
        /// </summary>
        private readonly Byte _tenantId;

        private readonly IRepository<Address> _addressRepository;
        private readonly IRepository<ContactInfo> _contactInfoRepository;
        private readonly IRepository<ContactPerson> _contactPersonRepository;
        private readonly IRepository<Location> _locationRepository;
        private readonly IRepository<Post> _postRepository;
        private readonly IRepository<webpages_Membership> _membershipRepository;
        private readonly IRepository<webpages_UserProfile> _userProfileRepository;
        private readonly IRepository<webpages_Roles> _roleRepository;
        private readonly IRepository<Permission> _permissionRepository;
        private readonly IRepository<PublicHoliday> _publicHolidayRepository;
        private readonly IRepository<webpages_OAuthMembership> _oAuthMembershipRepository;
        private readonly IRepository<UserLeave> _userLeaveRepository;
        private readonly IRepository<QualificationPlace> _qualificationPlaceRepository;
        private readonly IRepository<ProfessionalQualification> _professionalQualificationRepository;
        private readonly IRepository<ClientCompany> _clientCompanyRepository;
        private readonly IRepository<Contract> _clientContractRepository;
        private readonly IRepository<University> _universityRepository;
        private readonly IRepository<TypeOfCheck> _typeOfCheckRepository;
        private readonly IRepository<TypeOfCheckMeta> _typeOfCheckMetaRepository;
        private readonly IRepository<ScreeningLevel> _screeningLevelRepository;
        private readonly IRepository<ScreeningLevelVersion> _screeningLevelVersionRepository;
        private readonly IRepository<ScreeningQualification> _screeningQualificationRepository;
        private readonly IRepository<Screening> _screeningRepository;
        private readonly IRepository<AtomicCheck> _atomicCheckRepository;
        private readonly IRepository<Attachment> _attachmentRepository;
        private readonly IRepository<ScreeningReport> _screeningReportRepository;
        private readonly IRepository<History> _historyRepository;
        private readonly IRepository<Discussion> _discussionRepository;
        private readonly IRepository<Message> _messageRepository;
        private readonly IRepository<DefaultMatrix> _defaultMatrixRepository;
        private readonly IRepository<SkillMatrix> _skillMatrixRepository;
        private readonly IRepository<DispatchingSettings> _dispatchingSettingsRepository;
        private readonly IRepository<Notification> _notificationRepository;
        private readonly IRepository<NotificationOfUser> _notificationOfUserRepository;


        public EfUnitOfWork(Byte tenantId)
        {
            _dbContext = new CVScreeningEFContext();
            _tenantId = tenantId;
            
            #region Common

            _addressRepository = new EntityRepository<Address>(_dbContext, tenantId, e => e.AddressTenantId == _tenantId);
            _contactInfoRepository = new EntityRepository<ContactInfo>(_dbContext, tenantId,
                e => e.ContactInfoTenantId == _tenantId);
            _contactPersonRepository = new EntityRepository<ContactPerson>(_dbContext, tenantId,
                e => e.ContactPersonTenantId == _tenantId);
            _locationRepository = new EntityRepository<Location>(_dbContext, tenantId,
                e => e.LocationTenantId == _tenantId);
            _postRepository = new EntityRepository<Post>(_dbContext, tenantId, e => e.PostTenantId == _tenantId);

            #endregion

            #region User Management

            _membershipRepository = new EntityRepository<webpages_Membership>(_dbContext);
            _userProfileRepository = new EntityRepository<webpages_UserProfile>(_dbContext, tenantId, e => e.UserProfileTenantId == _tenantId);
            _roleRepository = new EntityRepository<webpages_Roles>(_dbContext);
            _permissionRepository = new EntityRepository<Permission>(_dbContext, tenantId, e => e.PermissionTenantId == _tenantId);
            _oAuthMembershipRepository = new EntityRepository<webpages_OAuthMembership>(_dbContext);
            _userLeaveRepository = new EntityRepository<UserLeave>(_dbContext, tenantId,
                e => e.UserLeaveTenantId == _tenantId);

            #endregion

            #region Client Repository

            _clientContractRepository = new EntityRepository<Contract>(_dbContext, tenantId,
                e => e.ContractTenantId == _tenantId);
            _clientCompanyRepository = new EntityRepository<ClientCompany>(_dbContext, tenantId,
                e => e.ClientCompanyTenantId == _tenantId);

            #endregion

            #region Screening

            _typeOfCheckRepository = new EntityRepository<TypeOfCheck>(_dbContext, tenantId,
                e => e.TypeOfCheckTenantId == _tenantId);
            _typeOfCheckMetaRepository = new EntityRepository<TypeOfCheckMeta>(_dbContext, tenantId,
                e => e.TypeOfCheckMetaTenantId == _tenantId);
            _screeningLevelRepository = new EntityRepository<ScreeningLevel>(_dbContext, tenantId,
                e => e.ScreeningLevelTenantId == _tenantId);
            _screeningLevelVersionRepository = new EntityRepository<ScreeningLevelVersion>(_dbContext, tenantId,
                e => e.ScreeningLevelVersionTenantId == _tenantId);
            _screeningRepository = new EntityRepository<Screening>(_dbContext, tenantId,
                e => e.ScreeningTenantId == _tenantId);
            _screeningQualificationRepository = new EntityRepository<ScreeningQualification>(_dbContext, tenantId,
                e => e.ScreeningQualificationTenantId == _tenantId);
            _atomicCheckRepository = new EntityRepository<AtomicCheck>(_dbContext, tenantId,
                e => e.AtomicCheckTenantId == _tenantId);
            _attachmentRepository = new EntityRepository<Attachment>(_dbContext, tenantId,
                e => e.AttachmentTenantId == _tenantId);
            _screeningReportRepository = new EntityRepository<ScreeningReport>(_dbContext, tenantId,
                e => e.ScreeningReportTenantId == _tenantId);

            #endregion

            #region Database Lookup            

            _qualificationPlaceRepository = new EntityRepository<QualificationPlace>(_dbContext, tenantId,
                e => e.QualificationPlaceTenantId == _tenantId);
            _professionalQualificationRepository = new EntityRepository<ProfessionalQualification>(_dbContext, tenantId,
                e => e.ProfessionalQualificationTenantId == _tenantId);
            _universityRepository = new EntityRepository<University>(_dbContext, tenantId,
                e => e.UniversityTenantId == _tenantId);

            #endregion

            #region History Repository

            _historyRepository = new EntityRepository<History>(_dbContext, tenantId, e => e.HistoryTenantId == _tenantId);

            #endregion

            #region Discussion and message

            _discussionRepository = new EntityRepository<Discussion>(_dbContext, tenantId, e => e.DiscussionTenantId == _tenantId);
            _messageRepository = new EntityRepository<Message>(_dbContext, tenantId, e => e.MessageTenantId == _tenantId);

            #endregion

            #region Application settings

            _publicHolidayRepository = new EntityRepository<PublicHoliday>(_dbContext, tenantId,
                e => e.PublicHolidayTenantId == _tenantId);

            #endregion
            
            #region Dispatching

            _defaultMatrixRepository = new EntityRepository<DefaultMatrix>(_dbContext, tenantId, 
                e => e.DefaultMatrixTenantId == _tenantId);
            _skillMatrixRepository = new EntityRepository<SkillMatrix>(_dbContext, tenantId, 
                e => e.SkillMatrixTenantId == _tenantId);
            _dispatchingSettingsRepository = new EntityRepository<DispatchingSettings>(_dbContext, tenantId,
                e => e.DispatchingSettingsTenantId == _tenantId);

            #endregion

            #region Notification

            _notificationRepository = new EntityRepository<Notification>(
                _dbContext, tenantId, e => e.NotificationTenantId == _tenantId);
            _notificationOfUserRepository = new EntityRepository<NotificationOfUser>(
                _dbContext, tenantId, e => e.NotificationOfUserTenantId == _tenantId);

            #endregion
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

        public IRepository<PublicHoliday> PublicHolidayRepository
        {
            get { return _publicHolidayRepository; }
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

        public IRepository<University> UniversityRepository
        {
            get { return _universityRepository; }
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

        public IRepository<DefaultMatrix> DefaultMatrixRepository
        {
            get { return _defaultMatrixRepository; }
        }

        public IRepository<SkillMatrix> SkillMatrixRepository
        {
            get { return _skillMatrixRepository; }
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

        public void Dispose()
        {
            if (_dbContext != null)
            {
                _dbContext.Dispose();
            }
            GC.SuppressFinalize(this);
        }


        public void Commit()
        {
            var changeInfo = _dbContext.ChangeTracker.Entries()
                .Where(t => t.State == EntityState.Added);
            foreach (var entity in changeInfo.Select(change => (IEntity) change.Entity))
            {
                entity.SetTenantId(_tenantId);
            }
            using (var transaction = new TransactionScope())
            {
                try
                {
                    _dbContext.SaveChanges();
                }
                // to get error exception from raw SQL Server
                catch (DbEntityValidationException e)
                {
                    foreach (var eve in e.EntityValidationErrors)
                    {
                        var message = string.Format(
                            "Entity of type \"{0}\" in state \"{1}\" has the following validation errors:",
                            eve.Entry.Entity.GetType().Name, eve.Entry.State);
                        foreach (var ve in eve.ValidationErrors)
                        {
                            var subMessage = string.Format("Property: \"{0}\", Error: \"{1}\"",
                                ve.PropertyName, ve.ErrorMessage);
                        }
                        LogManager.Instance.Error(
                            string.Format("Function: {0}. Error: {1}",
                                MethodBase.GetCurrentMethod().Name, message));
                    }
                    throw;
                }
                transaction.Complete();
            }
        }
    }
}