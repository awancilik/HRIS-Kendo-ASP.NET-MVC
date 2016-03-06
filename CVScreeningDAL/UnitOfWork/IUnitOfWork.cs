using CVScreeningCore.Models;
using CVScreeningDAL.Repo;

namespace CVScreeningDAL.UnitOfWork
{
    public interface IUnitOfWork
    {
        void Commit();

        #region Common Repository
        IRepository<Address> AddressRepository { get; }
        IRepository<Location> LocationRepository { get; }
        IRepository<ContactInfo> ContactInfoRepository { get; }
        IRepository<ContactPerson> ContactPersonRepository { get; }
        IRepository<Post> PostRepository { get; } 
        #endregion

        #region User Management Repository
        IRepository<webpages_Membership> MembershipRepository { get; }
        IRepository<webpages_UserProfile> UserProfileRepository { get; }
        IRepository<webpages_Roles> RoleRepository { get; }
        IRepository<webpages_OAuthMembership> OAuthMembershipRepository { get; }
        IRepository<UserLeave> UserLeaveRepository { get; }
        IRepository<Permission> PermissionRepository { get; }
        #endregion

        #region Lookup database

        IRepository<QualificationPlace> QualificationPlaceRepository { get; }
        IRepository<ProfessionalQualification> ProfessionalQualificationRepository { get; }
        IRepository<University> UniversityRepository { get; }

        #endregion

        #region Client Repositoy
        IRepository<ClientCompany> ClientCompanyRepository { get; }
        IRepository<Contract> ClientContractRepository { get; } 
        #endregion

        #region Screening
        IRepository<TypeOfCheck> TypeOfCheckRepository { get; }
        IRepository<TypeOfCheckMeta> TypeOfCheckMetaRepository { get; }
        IRepository<ScreeningQualification> ScreeningQualificationRepository { get; }
        IRepository<ScreeningLevel> ScreeningLevelRepository { get; }
        IRepository<ScreeningLevelVersion> ScreeningLevelVersionRepository { get; }
        IRepository<Screening> ScreeningRepository { get; }
        IRepository<AtomicCheck> AtomicCheckRepository { get; }
        IRepository<Attachment> AttachmentRepository { get; }
        IRepository<ScreeningReport> ScreeningReportRepository { get; }

        #endregion

        #region History Repository

        IRepository<History> HistoryRepository { get; }

        #endregion

        #region Discussion and message

        IRepository<Discussion> DiscussionRepository { get; }
        IRepository<Message> MessageRepository { get; }

        #endregion

        #region Application settings
        IRepository<PublicHoliday> PublicHolidayRepository { get; }
        #endregion

        #region Dispatching

        IRepository<DefaultMatrix> DefaultMatrixRepository { get; }
        IRepository<SkillMatrix> SkillMatrixRepository { get; }
        IRepository<DispatchingSettings> DispatchingSettingsRepository { get; }

        #endregion

        #region Notification

        IRepository<Notification> NotificationRepository { get; }
        IRepository<NotificationOfUser> NotificationOfUserRepository { get; }

        #endregion
    }
}