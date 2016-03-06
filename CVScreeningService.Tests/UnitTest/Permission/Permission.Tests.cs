using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using CVScreeningCore.Error;
using CVScreeningCore.Models;
using CVScreeningCore.Models.AtomicCheckState;
using CVScreeningCore.Models.AtomicCheckValidationState;
using CVScreeningCore.Models.ScreeningState;
using CVScreeningDAL.UnitOfWork;
using CVScreeningService.DTO.Client;
using CVScreeningService.DTO.Common;
using CVScreeningService.DTO.LookUpDatabase;
using CVScreeningService.DTO.Screening;
using CVScreeningService.DTO.UserManagement;
using CVScreeningService.Services.Client;
using CVScreeningService.Services.Common;
using CVScreeningService.Services.LookUpDatabase;
using CVScreeningService.Services.Notification;
using CVScreeningService.Services.Permission;
using CVScreeningService.Services.Screening;
using CVScreeningService.Services.SystemTime;
using CVScreeningService.Services.UserManagement;
using FakeItEasy;
using NUnit.Framework;

namespace CVScreeningService.Tests.UnitTest.Permission
{
    [TestFixture]
    public class Permission
    {
        #region NUnit Test Template

        // 1. Declare global object
        private IUnitOfWork _unitOfWork;
        private IQualificationPlaceFactory _factory;
        private ICommonService _commonService;
        private IScreeningService _screeningService;
        private IClientService _clientService;
        private IPermissionService _permissionService;
        private IQualificationService _qualificationService;
        private IUserManagementService _userManagementService;
        private ILookUpDatabaseService<PoliceDTO> _policeService;
        private ILookUpDatabaseService<ImmigrationOfficeDTO> _immigrationOfficeService;
        private ILookUpDatabaseService<CompanyDTO> _companyService;
        private ILookUpDatabaseService<HighSchoolDTO> _highSchoolService;
        private ILookUpDatabaseService<FacultyDTO> _facultyService;
        private ILookUpDatabaseService<CertificationPlaceDTO> _certificationService;
        private ILookUpDatabaseService<CourtDTO> _courtLookUpDatabaseService;
        private ILookUpDatabaseService<PopulationOfficeDTO> _populationOfficeService;
        private IProfessionalQualificationService _professionalQualificationService;

        private UserProfileDTO _admin;
        private UserProfileDTO _accountManager1;
        private UserProfileDTO _qualityControl1;
        private UserProfileDTO _accountManager2;
        private UserProfileDTO _qualityControl2;
        private UserProfileDTO _productionManager;
        private UserProfileDTO _qualifier;
        private UserProfileDTO _screenerOffice1;
        private UserProfileDTO _screenerOffice2;
        private UserProfileDTO _screenerOnField;
        private UserProfileDTO _client1;
        private UserProfileDTO _client2;
        private UserProfileDTO _hr;


        private ClientCompanyDTO _clientCompany1;
        private ClientCompanyDTO _clientCompany2;
        private ClientContractDTO _clientContract1;
        private ScreeningLevelDTO _screeningLevel1;
        private ScreeningLevelVersionDTO _screeningLevelVersion1;
        private ImmigrationOfficeDTO _immigrationOfficeDTO;
        private IWebSecurity _webSecurity;
        private PoliceDTO _police1DTO;
        private PoliceDTO _police2DTO;
        private CompanyDTO _companyDTO;
        private CompanyDTO _currentCmpanyDTO;
        private ScreeningQualificationDTO _qualificationBaseDTO;
        private HighSchoolDTO _highSchoolDTO;
        private FacultyDTO _facultyDTO;
        private CertificationPlaceDTO _certificationPlaceDTO;
        private CourtDTO _commercialCourtDTO;
        private IEnumerable<BaseQualificationPlaceDTO> _qualificationPlacesDTO;
        private PopulationOfficeDTO _populationOfficeDTO;


        // 2. Runs Once Before All of The Following Methods
        // Declare Global Objects Which Are Global For Test Class, e.g. Mock Objects
        [TestFixtureSetUp]
        public void RunsOnceBeforeAll()
        {
            var notificationService = A.Fake<INotificationService>();
            var systemTime = A.Fake<ISystemTimeService>();
            A.CallTo(() => systemTime.GetCurrentDateTime()).Returns(new DateTime(2014, 12, 1));
            _webSecurity = A.Fake<IWebSecurity>();
            A.CallTo(() => _webSecurity.GetCurrentUserName()).Returns("Administrator");

            _factory = new QualificationPlaceFactory(_unitOfWork);
            _unitOfWork = new InMemoryUnitOfWork();
            _commonService = new CommonService(_unitOfWork);
            _userManagementService = new UserManagementService(_unitOfWork, _commonService, systemTime);
            _clientService = new ClientService(_unitOfWork, _commonService, _userManagementService, _permissionService);
            _policeService = new PoliceLookUpDatabaseService(_unitOfWork, _factory);
            _immigrationOfficeService = new ImmigrationOfficeLookUpDatabaseService(_unitOfWork, _factory);
            _companyService = new CompanyLookUpDatabaseService(_unitOfWork, _factory);
            _highSchoolService = new HighSchoolLookUpDatabaseService(_unitOfWork, _factory);
            _facultyService = new FacultyLookUpDatabaseService(_unitOfWork, _factory);
            _certificationService = new CertificationPlaceLookUpDatabaseService(_unitOfWork, _factory);
            _courtLookUpDatabaseService = new CourtLookUpDatabaseService(_unitOfWork, _factory);
            _qualificationService = new QualificationService(_unitOfWork, _commonService);
            _professionalQualificationService = new Services.LookUpDatabase.ProfessionalQualificationService(_unitOfWork);
            _populationOfficeService = new PopulationOfficeLookUpDatabaseService(_unitOfWork,_factory);

            Utilities.InitLocations(_commonService);
            Utilities.InitRoles(_userManagementService);

            _admin = Utilities.BuildAdminAccountSample();
            var error = _userManagementService.CreateUserProfile(ref _admin,
                    new System.Collections.Generic.List<string>(new List<string> { "Administrator" }),
                    "123456", false);
            _permissionService = new PermissionService(_unitOfWork, _admin.UserName);
            _screeningService = new ScreeningService(
                _unitOfWork, _permissionService, _userManagementService, systemTime, _webSecurity, notificationService);

            InitializeTest();

        }

        // 3. Runs Twice; Once Before Test Case 1 and Once Before Test Case 2
        // Declare Objects Which Are Shared Among Tests, e.g. Shared Mocks
        [SetUp]
        public void RunOnceBeforeEachTest()
        {
            var screenings = _unitOfWork.ScreeningRepository.GetAll();
            foreach (var screening in screenings.Reverse())
            {
                _unitOfWork.ScreeningRepository.Delete(screening);
            }
        }

        [Test]
        public void Administrator_Should_Have_Full_Access()
        {
            _permissionService.SwitchCurrentUser(_admin.UserName, _admin.UserId);
            var screeningDTO = CreateScreening();
            ResetPermissionRepository(screeningDTO);

            Assert.AreEqual(true, _permissionService.IsGranted(CVScreeningCore.Models.Permission.kScreeningViewPermission,
                screeningDTO.ScreeningId));
            Assert.AreEqual(true, _permissionService.IsGranted(CVScreeningCore.Models.Permission.kScreeningManagePermission,
                screeningDTO.ScreeningId));
            Assert.AreEqual(true, _permissionService.IsGranted(CVScreeningCore.Models.Permission.kScreeningDeactivatePermission,
                screeningDTO.ScreeningId));
            Assert.AreEqual(true, _permissionService.IsGranted(CVScreeningCore.Models.Permission.kReportUploadPermission,
                screeningDTO.ScreeningId));

            Assert.AreEqual(true, _permissionService.IsGranted(CVScreeningCore.Models.Permission.kScreeningCreatePermission,
                screeningDTO.ScreeningLevelVersion.ScreeningLevelVersionId));
            Assert.AreEqual(true, _permissionService.IsGranted(CVScreeningCore.Models.Permission.kScreeningLevelVersionViewPermission,
                screeningDTO.ScreeningLevelVersion.ScreeningLevelVersionId));
            Assert.AreEqual(true, _permissionService.IsGranted(CVScreeningCore.Models.Permission.kScreeningLevelViewPermission,
                screeningDTO.ScreeningLevelVersion.ScreeningLevel.ScreeningLevelId));
            Assert.AreEqual(true, _permissionService.IsGranted(CVScreeningCore.Models.Permission.kContractViewPermission,
                screeningDTO.ScreeningLevelVersion.ScreeningLevel.Contract.ContractId));

            Assert.AreEqual(true, _permissionService.IsGranted(CVScreeningCore.Models.Permission.kExternalScreeningDiscussionManagePermission,
                screeningDTO.Discussion.First(u => u.DiscussionType == Discussion.kExternalScreeningType).DiscussionId));
            Assert.AreEqual(true, _permissionService.IsGranted(CVScreeningCore.Models.Permission.kInternalScreeningDiscussionManagePermission,
                screeningDTO.Discussion.First(u => u.DiscussionType == Discussion.kInternalScreeningType).DiscussionId));

            Assert.AreEqual(true, _permissionService.IsGranted(CVScreeningCore.Models.Permission.kAtomicCheckManagePermission,
                screeningDTO.AtomicCheck.ElementAt(0).AtomicCheckId));
            Assert.AreEqual(true, _permissionService.IsGranted(CVScreeningCore.Models.Permission.kAtomicCheckViewPermission,
                screeningDTO.AtomicCheck.ElementAt(0).AtomicCheckId));            
            Assert.AreEqual(true, _permissionService.IsGranted(CVScreeningCore.Models.Permission.kAtomicCheckAssignPermission,
                screeningDTO.AtomicCheck.ElementAt(0).AtomicCheckId));
        }

        [Test]
        public void ProductionManager_Should_Have_Access()
        {
            _permissionService.SwitchCurrentUser(_admin.UserName, _admin.UserId);
            var screeningDTO = CreateScreening();
            ResetPermissionRepository(screeningDTO);

            _permissionService.SwitchCurrentUser(_productionManager.UserName, _productionManager.UserId);
            Assert.AreEqual(true, _permissionService.IsGranted(CVScreeningCore.Models.Permission.kScreeningViewPermission,
                screeningDTO.ScreeningId));
            Assert.AreEqual(true, _permissionService.IsGranted(CVScreeningCore.Models.Permission.kAtomicCheckViewPermission,
                screeningDTO.AtomicCheck.ElementAt(0).AtomicCheckId));
            Assert.AreEqual(true, _permissionService.IsGranted(CVScreeningCore.Models.Permission.kAtomicCheckAssignPermission,
                screeningDTO.AtomicCheck.ElementAt(0).AtomicCheckId));
        }

        [Test]
        public void ProductionManager_Should_Not_Have_Access()
        {
            _permissionService.SwitchCurrentUser(_admin.UserName, _admin.UserId);
            var screeningDTO = CreateScreening();
            ResetPermissionRepository(screeningDTO);

            _permissionService.SwitchCurrentUser(_productionManager.UserName, _productionManager.UserId);
            Assert.AreEqual(false, _permissionService.IsGranted(CVScreeningCore.Models.Permission.kScreeningManagePermission,
                screeningDTO.ScreeningId));
            Assert.AreEqual(false, _permissionService.IsGranted(CVScreeningCore.Models.Permission.kScreeningDeactivatePermission,
                screeningDTO.ScreeningId));
            Assert.AreEqual(false, _permissionService.IsGranted(CVScreeningCore.Models.Permission.kReportUploadPermission,
                screeningDTO.ScreeningId));

            Assert.AreEqual(false, _permissionService.IsGranted(CVScreeningCore.Models.Permission.kScreeningCreatePermission,
                screeningDTO.ScreeningLevelVersion.ScreeningLevelVersionId));
            Assert.AreEqual(false, _permissionService.IsGranted(CVScreeningCore.Models.Permission.kScreeningLevelVersionViewPermission,
                screeningDTO.ScreeningLevelVersion.ScreeningLevelVersionId));
            Assert.AreEqual(false, _permissionService.IsGranted(CVScreeningCore.Models.Permission.kScreeningLevelViewPermission,
                screeningDTO.ScreeningLevelVersion.ScreeningLevel.ScreeningLevelId));
            Assert.AreEqual(false, _permissionService.IsGranted(CVScreeningCore.Models.Permission.kContractViewPermission,
                screeningDTO.ScreeningLevelVersion.ScreeningLevel.Contract.ContractId));


            Assert.AreEqual(false, _permissionService.IsGranted(CVScreeningCore.Models.Permission.kExternalScreeningDiscussionManagePermission,
                screeningDTO.Discussion.First(u => u.DiscussionType == Discussion.kExternalScreeningType).DiscussionId));
            Assert.AreEqual(false, _permissionService.IsGranted(CVScreeningCore.Models.Permission.kInternalScreeningDiscussionManagePermission,
                screeningDTO.Discussion.First(u => u.DiscussionType == Discussion.kInternalScreeningType).DiscussionId));

            Assert.AreEqual(false, _permissionService.IsGranted(CVScreeningCore.Models.Permission.kAtomicCheckManagePermission,
                screeningDTO.AtomicCheck.ElementAt(0).AtomicCheckId));
        }

        [Test]
        public void Qualifier_Should_Have_Access()
        {
            _permissionService.SwitchCurrentUser(_admin.UserName, _admin.UserId);
            var screeningDTO = CreateScreening();
            ResetPermissionRepository(screeningDTO);

            _permissionService.SwitchCurrentUser(_qualifier.UserName, _qualifier.UserId);

            Assert.AreEqual(true, _permissionService.IsGranted(CVScreeningCore.Models.Permission.kScreeningCreatePermission,
                screeningDTO.ScreeningLevelVersion.ScreeningLevelVersionId));
            Assert.AreEqual(true, _permissionService.IsGranted(CVScreeningCore.Models.Permission.kScreeningManagePermission,
                screeningDTO.ScreeningId));
            Assert.AreEqual(true, _permissionService.IsGranted(CVScreeningCore.Models.Permission.kScreeningViewPermission,
                screeningDTO.ScreeningId));

            Assert.AreEqual(true, _permissionService.IsGranted(CVScreeningCore.Models.Permission.kScreeningLevelVersionViewPermission,
                screeningDTO.ScreeningLevelVersion.ScreeningLevelVersionId));
            Assert.AreEqual(true, _permissionService.IsGranted(CVScreeningCore.Models.Permission.kScreeningLevelViewPermission,
                screeningDTO.ScreeningLevelVersion.ScreeningLevel.ScreeningLevelId));
            Assert.AreEqual(true, _permissionService.IsGranted(CVScreeningCore.Models.Permission.kContractViewPermission,
                screeningDTO.ScreeningLevelVersion.ScreeningLevel.Contract.ContractId));

            Assert.AreEqual(true, _permissionService.IsGranted(CVScreeningCore.Models.Permission.kAtomicCheckViewPermission,
                screeningDTO.AtomicCheck.ElementAt(0).AtomicCheckId));
            Assert.AreEqual(true, _permissionService.IsGranted(CVScreeningCore.Models.Permission.kInternalScreeningDiscussionManagePermission,
                screeningDTO.Discussion.First(u => u.DiscussionType == Discussion.kInternalScreeningType).DiscussionId));
        }

        [Test]
        public void Qualifier_Should_Not_Have_Access()
        {
            _permissionService.SwitchCurrentUser(_admin.UserName, _admin.UserId);
            var screeningDTO = CreateScreening();
            ResetPermissionRepository(screeningDTO);

            _permissionService.SwitchCurrentUser(_qualifier.UserName, _qualifier.UserId);

            Assert.AreEqual(false, _permissionService.IsGranted(CVScreeningCore.Models.Permission.kScreeningDeactivatePermission,
                screeningDTO.ScreeningId));
            Assert.AreEqual(false, _permissionService.IsGranted(CVScreeningCore.Models.Permission.kAtomicCheckManagePermission,
                screeningDTO.AtomicCheck.ElementAt(0).AtomicCheckId));
            Assert.AreEqual(false, _permissionService.IsGranted(CVScreeningCore.Models.Permission.kAtomicCheckAssignPermission,
                screeningDTO.AtomicCheck.ElementAt(0).AtomicCheckId));
            Assert.AreEqual(false, _permissionService.IsGranted(CVScreeningCore.Models.Permission.kReportUploadPermission,
                screeningDTO.ScreeningId));

            Assert.AreEqual(false, _permissionService.IsGranted(CVScreeningCore.Models.Permission.kExternalScreeningDiscussionManagePermission,
                screeningDTO.Discussion.First(u => u.DiscussionType == Discussion.kExternalScreeningType).DiscussionId));

        }

        [Test]
        public void Hr_Should_Not_Have_Full_Access()
        {
            _permissionService.SwitchCurrentUser(_admin.UserName, _admin.UserId);
            var screeningDTO = CreateScreening();
            ResetPermissionRepository(screeningDTO);

            _permissionService.SwitchCurrentUser(_hr.UserName, _hr.UserId);

            Assert.AreEqual(false, _permissionService.IsGranted(CVScreeningCore.Models.Permission.kScreeningViewPermission,
                screeningDTO.ScreeningId));
            Assert.AreEqual(false, _permissionService.IsGranted(CVScreeningCore.Models.Permission.kScreeningManagePermission,
                screeningDTO.ScreeningId));
            Assert.AreEqual(false, _permissionService.IsGranted(CVScreeningCore.Models.Permission.kScreeningDeactivatePermission,
                screeningDTO.ScreeningId));
            Assert.AreEqual(false, _permissionService.IsGranted(CVScreeningCore.Models.Permission.kReportUploadPermission,
                screeningDTO.ScreeningId));
            Assert.AreEqual(false, _permissionService.IsGranted(CVScreeningCore.Models.Permission.kScreeningCreatePermission,
                screeningDTO.ScreeningLevelVersion.ScreeningLevelVersionId));
            Assert.AreEqual(false, _permissionService.IsGranted(CVScreeningCore.Models.Permission.kScreeningLevelVersionViewPermission,
                screeningDTO.ScreeningLevelVersion.ScreeningLevelVersionId));
            Assert.AreEqual(false, _permissionService.IsGranted(CVScreeningCore.Models.Permission.kScreeningLevelViewPermission,
                screeningDTO.ScreeningLevelVersion.ScreeningLevel.ScreeningLevelId));
            Assert.AreEqual(false, _permissionService.IsGranted(CVScreeningCore.Models.Permission.kContractViewPermission,
                screeningDTO.ScreeningLevelVersion.ScreeningLevel.Contract.ContractId));
            Assert.AreEqual(false, _permissionService.IsGranted(CVScreeningCore.Models.Permission.kExternalScreeningDiscussionManagePermission,
                screeningDTO.Discussion.First(u => u.DiscussionType == Discussion.kExternalScreeningType).DiscussionId));
            Assert.AreEqual(false, _permissionService.IsGranted(CVScreeningCore.Models.Permission.kInternalScreeningDiscussionManagePermission,
                screeningDTO.Discussion.First(u => u.DiscussionType == Discussion.kInternalScreeningType).DiscussionId));
            Assert.AreEqual(false, _permissionService.IsGranted(CVScreeningCore.Models.Permission.kAtomicCheckManagePermission,
                screeningDTO.AtomicCheck.ElementAt(0).AtomicCheckId));
            Assert.AreEqual(false, _permissionService.IsGranted(CVScreeningCore.Models.Permission.kAtomicCheckViewPermission,
                screeningDTO.AtomicCheck.ElementAt(0).AtomicCheckId));
            Assert.AreEqual(false, _permissionService.IsGranted(CVScreeningCore.Models.Permission.kAtomicCheckAssignPermission,
                screeningDTO.AtomicCheck.ElementAt(0).AtomicCheckId));
        }

        [Test]
        public void Screener_Should_Have_Access()
        {
            _permissionService.SwitchCurrentUser(_admin.UserName, _admin.UserId);
            var screeningDTO = CreateQualifyAndAssignScreening();
            ResetPermissionRepository(screeningDTO);
            _permissionService.SwitchCurrentUser(_screenerOffice1.UserName, _screenerOffice1.UserId);

            // Screener office that has access to atomic check office
            var atomicChecksDTO = _screeningService.GetAllAtomicChecksForScreening(screeningDTO);
            foreach (var atomicCheckDTO in atomicChecksDTO.Where(u => u.AtomicCheckCategory == CVScreeningCore.Models.AtomicCheck.kOfficeCategory))
            {
                Assert.AreEqual(true, _permissionService.IsGranted(CVScreeningCore.Models.Permission.kAtomicCheckManagePermission,
                    atomicCheckDTO.AtomicCheckId));
                Assert.AreEqual(true, _permissionService.IsGranted(CVScreeningCore.Models.Permission.kAtomicCheckViewPermission,
                    atomicCheckDTO.AtomicCheckId));
                Assert.AreEqual(true, _permissionService.IsGranted(CVScreeningCore.Models.Permission.kInternalAtomicCheckDiscussionManagePermission,
                    atomicCheckDTO.Discussion.First(u => u.DiscussionType == Discussion.kInternalAtomicCheckType).DiscussionId));
            }

            Assert.AreEqual(true, _permissionService.IsGranted(CVScreeningCore.Models.Permission.kScreeningViewPermission,
                screeningDTO.ScreeningId));

            // Switch to screener on field that has access to atomic check on field
            _permissionService.SwitchCurrentUser(_screenerOnField.UserName, _screenerOnField.UserId);

            atomicChecksDTO = _screeningService.GetAllAtomicChecksForScreening(screeningDTO);
            foreach (var atomicCheckDTO in atomicChecksDTO.Where(u => u.AtomicCheckCategory == CVScreeningCore.Models.AtomicCheck.kOnFieldCategory))
            {
                Assert.AreEqual(true, _permissionService.IsGranted(CVScreeningCore.Models.Permission.kAtomicCheckManagePermission,
                    atomicCheckDTO.AtomicCheckId));
                Assert.AreEqual(true, _permissionService.IsGranted(CVScreeningCore.Models.Permission.kAtomicCheckViewPermission,
                    atomicCheckDTO.AtomicCheckId));
                Assert.AreEqual(true, _permissionService.IsGranted(CVScreeningCore.Models.Permission.kInternalAtomicCheckDiscussionManagePermission,
                    atomicCheckDTO.Discussion.First(u => u.DiscussionType == Discussion.kInternalAtomicCheckType).DiscussionId));
            }

            Assert.AreEqual(true, _permissionService.IsGranted(CVScreeningCore.Models.Permission.kScreeningViewPermission,
                screeningDTO.ScreeningId));

        }

        [Test]
        public void Screener_Should_Not_Have_Access()
        {
            _permissionService.SwitchCurrentUser(_admin.UserName, _admin.UserId);
            var screeningDTO = CreateQualifyAndAssignScreening();
            ResetPermissionRepository(screeningDTO);

            _permissionService.SwitchCurrentUser(_screenerOffice1.UserName, _screenerOffice1.UserId);

            Assert.AreEqual(false, _permissionService.IsGranted(CVScreeningCore.Models.Permission.kScreeningManagePermission,
                screeningDTO.ScreeningId));
            Assert.AreEqual(false, _permissionService.IsGranted(CVScreeningCore.Models.Permission.kScreeningDeactivatePermission,
                screeningDTO.ScreeningId));
            Assert.AreEqual(false, _permissionService.IsGranted(CVScreeningCore.Models.Permission.kReportUploadPermission,
                screeningDTO.ScreeningId));
            Assert.AreEqual(false, _permissionService.IsGranted(CVScreeningCore.Models.Permission.kScreeningCreatePermission,
                screeningDTO.ScreeningLevelVersion.ScreeningLevelVersionId));
            Assert.AreEqual(false, _permissionService.IsGranted(CVScreeningCore.Models.Permission.kScreeningLevelVersionViewPermission,
                screeningDTO.ScreeningLevelVersion.ScreeningLevelVersionId));
            Assert.AreEqual(false, _permissionService.IsGranted(CVScreeningCore.Models.Permission.kScreeningLevelViewPermission,
                screeningDTO.ScreeningLevelVersion.ScreeningLevel.ScreeningLevelId));
            Assert.AreEqual(false, _permissionService.IsGranted(CVScreeningCore.Models.Permission.kContractViewPermission,
                screeningDTO.ScreeningLevelVersion.ScreeningLevel.Contract.ContractId));
            Assert.AreEqual(false, _permissionService.IsGranted(CVScreeningCore.Models.Permission.kExternalScreeningDiscussionManagePermission,
                screeningDTO.Discussion.First(u => u.DiscussionType == Discussion.kExternalScreeningType).DiscussionId));
            Assert.AreEqual(false, _permissionService.IsGranted(CVScreeningCore.Models.Permission.kInternalScreeningDiscussionManagePermission,
                screeningDTO.Discussion.First(u => u.DiscussionType == Discussion.kInternalScreeningType).DiscussionId));
            Assert.AreEqual(false, _permissionService.IsGranted(CVScreeningCore.Models.Permission.kAtomicCheckAssignPermission,
                screeningDTO.AtomicCheck.ElementAt(0).AtomicCheckId));

            // Screener onffice that does not have access to atomic check on field
            var atomicChecksDTO = _screeningService.GetAllAtomicChecksForScreening(screeningDTO);
            foreach (var atomicCheckDTO in atomicChecksDTO.Where(u => u.AtomicCheckCategory == CVScreeningCore.Models.AtomicCheck.kOnFieldCategory))
            {
                Assert.AreEqual(false, _permissionService.IsGranted(CVScreeningCore.Models.Permission.kAtomicCheckManagePermission,
                    atomicCheckDTO.AtomicCheckId));
                Assert.AreEqual(false, _permissionService.IsGranted(CVScreeningCore.Models.Permission.kAtomicCheckViewPermission,
                    atomicCheckDTO.AtomicCheckId));
                Assert.AreEqual(false, _permissionService.IsGranted(CVScreeningCore.Models.Permission.kInternalAtomicCheckDiscussionManagePermission,
                    atomicCheckDTO.Discussion.First(u => u.DiscussionType == Discussion.kInternalAtomicCheckType).DiscussionId));
            }

            // Switch to screener on field that does not have access to atomic check office
            _permissionService.SwitchCurrentUser(_screenerOnField.UserName, _screenerOnField.UserId);

            atomicChecksDTO = _screeningService.GetAllAtomicChecksForScreening(screeningDTO);
            foreach (var atomicCheckDTO in atomicChecksDTO.Where(u => u.AtomicCheckCategory == CVScreeningCore.Models.AtomicCheck.kOfficeCategory))
            {
                Assert.AreEqual(false, _permissionService.IsGranted(CVScreeningCore.Models.Permission.kAtomicCheckManagePermission,
                    atomicCheckDTO.AtomicCheckId));
                Assert.AreEqual(false, _permissionService.IsGranted(CVScreeningCore.Models.Permission.kAtomicCheckViewPermission,
                    atomicCheckDTO.AtomicCheckId));
                Assert.AreEqual(false, _permissionService.IsGranted(CVScreeningCore.Models.Permission.kInternalAtomicCheckDiscussionManagePermission,
                    atomicCheckDTO.Discussion.First(u => u.DiscussionType == Discussion.kInternalAtomicCheckType).DiscussionId));
            }



        }

        [Test]
        public void Screener_Should_Lost_Access_To_Screening_When_All_Atomic_Checks_Reassigned()
        {
            _permissionService.SwitchCurrentUser(_admin.UserName, _admin.UserId);
            var screeningDTO = CreateQualifyAndAssignScreening();
            ResetPermissionRepository(screeningDTO);
            _permissionService.SwitchCurrentUser(_screenerOffice1.UserName, _screenerOffice1.UserId);

            // Screener office that has access to atomic check office
            var atomicChecksDTO = _screeningService.GetAllAtomicChecksForScreening(screeningDTO);
            foreach (var atomicCheckDTO in atomicChecksDTO.Where(u => u.AtomicCheckCategory == CVScreeningCore.Models.AtomicCheck.kOfficeCategory))
            {
                Assert.AreEqual(true, _permissionService.IsGranted(CVScreeningCore.Models.Permission.kAtomicCheckManagePermission,
                    atomicCheckDTO.AtomicCheckId));
                Assert.AreEqual(true, _permissionService.IsGranted(CVScreeningCore.Models.Permission.kAtomicCheckViewPermission,
                    atomicCheckDTO.AtomicCheckId));
                Assert.AreEqual(true, _permissionService.IsGranted(CVScreeningCore.Models.Permission.kInternalAtomicCheckDiscussionManagePermission,
                    atomicCheckDTO.Discussion.First(u => u.DiscussionType == Discussion.kInternalAtomicCheckType).DiscussionId));
            }

            Assert.AreEqual(true, _permissionService.IsGranted(CVScreeningCore.Models.Permission.kScreeningViewPermission,
                screeningDTO.ScreeningId));

            foreach (var atomicCheckDTO in atomicChecksDTO.Where(u => u.AtomicCheckCategory == CVScreeningCore.Models.AtomicCheck.kOfficeCategory))
            {
                var localAtomicCheckDTO = atomicCheckDTO;
                var error = _screeningService.AssignAtomicCheck(ref localAtomicCheckDTO, _screenerOffice2);
                Assert.AreEqual(ErrorCode.NO_ERROR, error);
            }

            ResetPermissionRepository(screeningDTO);

            foreach (var atomicCheckDTO in atomicChecksDTO.Where(u => u.AtomicCheckCategory == CVScreeningCore.Models.AtomicCheck.kOfficeCategory))
            {
                _permissionService.SwitchCurrentUser(_screenerOffice2.UserName, _screenerOffice2.UserId);
                Assert.AreEqual(true, _permissionService.IsGranted(CVScreeningCore.Models.Permission.kAtomicCheckManagePermission,
                    atomicCheckDTO.AtomicCheckId));
                Assert.AreEqual(true, _permissionService.IsGranted(CVScreeningCore.Models.Permission.kAtomicCheckViewPermission,
                    atomicCheckDTO.AtomicCheckId));
                Assert.AreEqual(true, _permissionService.IsGranted(CVScreeningCore.Models.Permission.kInternalAtomicCheckDiscussionManagePermission,
                    atomicCheckDTO.Discussion.First(u => u.DiscussionType == Discussion.kInternalAtomicCheckType).DiscussionId));

                _permissionService.SwitchCurrentUser(_screenerOffice1.UserName, _screenerOffice1.UserId);
                Assert.AreEqual(false, _permissionService.IsGranted(CVScreeningCore.Models.Permission.kAtomicCheckManagePermission,
                    atomicCheckDTO.AtomicCheckId));
                Assert.AreEqual(false, _permissionService.IsGranted(CVScreeningCore.Models.Permission.kAtomicCheckViewPermission,
                    atomicCheckDTO.AtomicCheckId));
                Assert.AreEqual(false, _permissionService.IsGranted(CVScreeningCore.Models.Permission.kInternalAtomicCheckDiscussionManagePermission,
                    atomicCheckDTO.Discussion.First(u => u.DiscussionType == Discussion.kInternalAtomicCheckType).DiscussionId));
            }

            _permissionService.SwitchCurrentUser(_screenerOffice1.UserName, _screenerOffice1.UserId);
            Assert.AreEqual(false, _permissionService.IsGranted(CVScreeningCore.Models.Permission.kScreeningViewPermission,
                screeningDTO.ScreeningId));

            _permissionService.SwitchCurrentUser(_screenerOffice2.UserName, _screenerOffice2.UserId);
            Assert.AreEqual(true, _permissionService.IsGranted(CVScreeningCore.Models.Permission.kScreeningViewPermission,
                screeningDTO.ScreeningId));
        }

        [Test]
        public void QualityControl_Should_Lost_Access_To_Screening_When_QualityControl_Reassigned()
        {
            _permissionService.SwitchCurrentUser(_admin.UserName, _admin.UserId);
            var screeningDTO = CreateQualifyAndAssignScreening();
            ResetPermissionRepository(screeningDTO);
            _permissionService.SwitchCurrentUser(_qualityControl1.UserName, _qualityControl1.UserId);

            var atomicChecksDTO = _screeningService.GetAllAtomicChecksForScreening(screeningDTO);
            foreach (var atomicCheckDTO in atomicChecksDTO)
            {
                Assert.AreEqual(true, _permissionService.IsGranted(CVScreeningCore.Models.Permission.kAtomicCheckManagePermission,
                    atomicCheckDTO.AtomicCheckId));
                Assert.AreEqual(true, _permissionService.IsGranted(CVScreeningCore.Models.Permission.kAtomicCheckViewPermission,
                    atomicCheckDTO.AtomicCheckId));
                Assert.AreEqual(true, _permissionService.IsGranted(CVScreeningCore.Models.Permission.kAtomicCheckAssignPermission,
                    atomicCheckDTO.AtomicCheckId));
                Assert.AreEqual(true, _permissionService.IsGranted(CVScreeningCore.Models.Permission.kInternalAtomicCheckDiscussionManagePermission,
                    atomicCheckDTO.Discussion.First(u => u.DiscussionType == Discussion.kInternalAtomicCheckType).DiscussionId));
            }

            Assert.AreEqual(true, _permissionService.IsGranted(CVScreeningCore.Models.Permission.kReportUploadPermission,
                screeningDTO.ScreeningId));
            Assert.AreEqual(true, _permissionService.IsGranted(CVScreeningCore.Models.Permission.kScreeningManagePermission,
                screeningDTO.ScreeningId));
            Assert.AreEqual(true, _permissionService.IsGranted(CVScreeningCore.Models.Permission.kScreeningViewPermission,
                screeningDTO.ScreeningId));
            Assert.AreEqual(true, _permissionService.IsGranted(CVScreeningCore.Models.Permission.kInternalScreeningDiscussionManagePermission,
                screeningDTO.Discussion.First(u => u.DiscussionType == Discussion.kInternalScreeningType).DiscussionId));

            var error = _screeningService.AssignScreening(ref screeningDTO, _qualityControl2);
            Assert.AreEqual(ErrorCode.NO_ERROR, error);

            ResetPermissionRepository(screeningDTO);

            Assert.AreEqual(false, _permissionService.IsGranted(CVScreeningCore.Models.Permission.kScreeningViewPermission,
            screeningDTO.ScreeningId));
            Assert.AreEqual(false, _permissionService.IsGranted(CVScreeningCore.Models.Permission.kScreeningManagePermission,
                screeningDTO.ScreeningId));
            Assert.AreEqual(false, _permissionService.IsGranted(CVScreeningCore.Models.Permission.kScreeningDeactivatePermission,
                screeningDTO.ScreeningId));
            Assert.AreEqual(false, _permissionService.IsGranted(CVScreeningCore.Models.Permission.kReportUploadPermission,
                screeningDTO.ScreeningId));
            Assert.AreEqual(false, _permissionService.IsGranted(CVScreeningCore.Models.Permission.kScreeningCreatePermission,
                screeningDTO.ScreeningLevelVersion.ScreeningLevelVersionId));
            Assert.AreEqual(false, _permissionService.IsGranted(CVScreeningCore.Models.Permission.kScreeningLevelVersionViewPermission,
                screeningDTO.ScreeningLevelVersion.ScreeningLevelVersionId));
            Assert.AreEqual(false, _permissionService.IsGranted(CVScreeningCore.Models.Permission.kScreeningLevelViewPermission,
                screeningDTO.ScreeningLevelVersion.ScreeningLevel.ScreeningLevelId));
            Assert.AreEqual(false, _permissionService.IsGranted(CVScreeningCore.Models.Permission.kContractViewPermission,
                screeningDTO.ScreeningLevelVersion.ScreeningLevel.Contract.ContractId));
            Assert.AreEqual(false, _permissionService.IsGranted(CVScreeningCore.Models.Permission.kExternalScreeningDiscussionManagePermission,
                screeningDTO.Discussion.First(u => u.DiscussionType == Discussion.kExternalScreeningType).DiscussionId));
            Assert.AreEqual(false, _permissionService.IsGranted(CVScreeningCore.Models.Permission.kInternalScreeningDiscussionManagePermission,
                screeningDTO.Discussion.First(u => u.DiscussionType == Discussion.kInternalScreeningType).DiscussionId));
            Assert.AreEqual(false, _permissionService.IsGranted(CVScreeningCore.Models.Permission.kAtomicCheckManagePermission,
                screeningDTO.AtomicCheck.ElementAt(0).AtomicCheckId));
            Assert.AreEqual(false, _permissionService.IsGranted(CVScreeningCore.Models.Permission.kAtomicCheckViewPermission,
                screeningDTO.AtomicCheck.ElementAt(0).AtomicCheckId));
            Assert.AreEqual(false, _permissionService.IsGranted(CVScreeningCore.Models.Permission.kAtomicCheckAssignPermission,
                screeningDTO.AtomicCheck.ElementAt(0).AtomicCheckId));

            _permissionService.SwitchCurrentUser(_qualityControl2.UserName, _qualityControl2.UserId);

            foreach (var atomicCheckDTO in atomicChecksDTO)
            {
                Assert.AreEqual(true, _permissionService.IsGranted(CVScreeningCore.Models.Permission.kAtomicCheckManagePermission,
                    atomicCheckDTO.AtomicCheckId));
                Assert.AreEqual(true, _permissionService.IsGranted(CVScreeningCore.Models.Permission.kAtomicCheckViewPermission,
                    atomicCheckDTO.AtomicCheckId));
                Assert.AreEqual(true, _permissionService.IsGranted(CVScreeningCore.Models.Permission.kAtomicCheckAssignPermission,
                    atomicCheckDTO.AtomicCheckId));
                Assert.AreEqual(true, _permissionService.IsGranted(CVScreeningCore.Models.Permission.kInternalAtomicCheckDiscussionManagePermission,
                    atomicCheckDTO.Discussion.First(u => u.DiscussionType == Discussion.kInternalAtomicCheckType).DiscussionId));
            }

            Assert.AreEqual(true, _permissionService.IsGranted(CVScreeningCore.Models.Permission.kReportUploadPermission,
                screeningDTO.ScreeningId));
            Assert.AreEqual(true, _permissionService.IsGranted(CVScreeningCore.Models.Permission.kScreeningManagePermission,
                screeningDTO.ScreeningId));
            Assert.AreEqual(true, _permissionService.IsGranted(CVScreeningCore.Models.Permission.kScreeningViewPermission,
                screeningDTO.ScreeningId));
            Assert.AreEqual(true, _permissionService.IsGranted(CVScreeningCore.Models.Permission.kInternalScreeningDiscussionManagePermission,
                screeningDTO.Discussion.First(u => u.DiscussionType == Discussion.kInternalScreeningType).DiscussionId));


        }

        [Test]
        public void AccountManager_Should_Have_Access()
        {
            _permissionService.SwitchCurrentUser(_admin.UserName, _admin.UserId);
            var screeningDTO = CreateQualifyAndAssignScreening();
            ResetPermissionRepository(screeningDTO);
            _permissionService.SwitchCurrentUser(_accountManager1.UserName, _accountManager1.UserId);

            var atomicChecksDTO = _screeningService.GetAllAtomicChecksForScreening(screeningDTO);
            foreach (var atomicCheckDTO in atomicChecksDTO.Where(u => u.AtomicCheckCategory == CVScreeningCore.Models.AtomicCheck.kOfficeCategory))
            {
                Assert.AreEqual(true, _permissionService.IsGranted(CVScreeningCore.Models.Permission.kAtomicCheckManagePermission,
                    atomicCheckDTO.AtomicCheckId));
                Assert.AreEqual(true, _permissionService.IsGranted(CVScreeningCore.Models.Permission.kAtomicCheckViewPermission,
                    atomicCheckDTO.AtomicCheckId));
                Assert.AreEqual(true, _permissionService.IsGranted(CVScreeningCore.Models.Permission.kAtomicCheckAssignPermission,
                    atomicCheckDTO.AtomicCheckId));
                Assert.AreEqual(true, _permissionService.IsGranted(CVScreeningCore.Models.Permission.kInternalAtomicCheckDiscussionManagePermission,
                    atomicCheckDTO.Discussion.First(u => u.DiscussionType == Discussion.kInternalAtomicCheckType).DiscussionId));
            }

            Assert.AreEqual(true, _permissionService.IsGranted(CVScreeningCore.Models.Permission.kScreeningManagePermission,
                screeningDTO.ScreeningId));
            Assert.AreEqual(true, _permissionService.IsGranted(CVScreeningCore.Models.Permission.kScreeningViewPermission,
                screeningDTO.ScreeningId));
            Assert.AreEqual(true, _permissionService.IsGranted(CVScreeningCore.Models.Permission.kScreeningLevelViewPermission,
                screeningDTO.ScreeningId));
            Assert.AreEqual(true, _permissionService.IsGranted(CVScreeningCore.Models.Permission.kScreeningLevelVersionViewPermission,
                screeningDTO.ScreeningId));
            Assert.AreEqual(true, _permissionService.IsGranted(CVScreeningCore.Models.Permission.kContractViewPermission,
                screeningDTO.ScreeningId));
            Assert.AreEqual(true, _permissionService.IsGranted(CVScreeningCore.Models.Permission.kExternalScreeningDiscussionManagePermission,
                screeningDTO.Discussion.First(u => u.DiscussionType == Discussion.kExternalScreeningType).DiscussionId));
            Assert.AreEqual(true, _permissionService.IsGranted(CVScreeningCore.Models.Permission.kInternalScreeningDiscussionManagePermission,
                screeningDTO.Discussion.First(u => u.DiscussionType == Discussion.kInternalScreeningType).DiscussionId));


            var accountManager3 = Utilities.BuildAccountManagerAccountSample();
            accountManager3.UserName = "am3@am.com";
            var error = _userManagementService.CreateUserProfile(ref accountManager3,
                    new System.Collections.Generic.List<string>(new List<string> { "Account manager" }),
                    "123456", false);
            Assert.AreEqual(ErrorCode.NO_ERROR, error);
            _permissionService.SwitchCurrentUser(accountManager3.UserName, accountManager3.UserId);
           
            _clientCompany1.AccountManagers.Add(accountManager3);
            error = _clientService.EditClientCompany(ref _clientCompany1);
            Assert.AreEqual(ErrorCode.NO_ERROR, error);

            ResetPermissionRepository(screeningDTO);

            // Assign new AM to company. Check that he has access to company data
            atomicChecksDTO = _screeningService.GetAllAtomicChecksForScreening(screeningDTO);
            foreach (var atomicCheckDTO in atomicChecksDTO.Where(u => u.AtomicCheckCategory == CVScreeningCore.Models.AtomicCheck.kOfficeCategory))
            {
                Assert.AreEqual(true, _permissionService.IsGranted(CVScreeningCore.Models.Permission.kAtomicCheckManagePermission,
                    atomicCheckDTO.AtomicCheckId));
                Assert.AreEqual(true, _permissionService.IsGranted(CVScreeningCore.Models.Permission.kAtomicCheckViewPermission,
                    atomicCheckDTO.AtomicCheckId));
                Assert.AreEqual(true, _permissionService.IsGranted(CVScreeningCore.Models.Permission.kAtomicCheckAssignPermission,
                    atomicCheckDTO.AtomicCheckId));
                Assert.AreEqual(true, _permissionService.IsGranted(CVScreeningCore.Models.Permission.kInternalAtomicCheckDiscussionManagePermission,
                    atomicCheckDTO.Discussion.First(u => u.DiscussionType == Discussion.kInternalAtomicCheckType).DiscussionId));
            }

            Assert.AreEqual(true, _permissionService.IsGranted(CVScreeningCore.Models.Permission.kScreeningManagePermission,
                screeningDTO.ScreeningId));
            Assert.AreEqual(true, _permissionService.IsGranted(CVScreeningCore.Models.Permission.kScreeningViewPermission,
                screeningDTO.ScreeningId));
            Assert.AreEqual(true, _permissionService.IsGranted(CVScreeningCore.Models.Permission.kScreeningLevelViewPermission,
                screeningDTO.ScreeningId));
            Assert.AreEqual(true, _permissionService.IsGranted(CVScreeningCore.Models.Permission.kScreeningLevelVersionViewPermission,
                screeningDTO.ScreeningId));
            Assert.AreEqual(true, _permissionService.IsGranted(CVScreeningCore.Models.Permission.kContractViewPermission,
                screeningDTO.ScreeningId));
            Assert.AreEqual(true, _permissionService.IsGranted(CVScreeningCore.Models.Permission.kExternalScreeningDiscussionManagePermission,
                screeningDTO.Discussion.First(u => u.DiscussionType == Discussion.kExternalScreeningType).DiscussionId));
            Assert.AreEqual(true, _permissionService.IsGranted(CVScreeningCore.Models.Permission.kInternalScreeningDiscussionManagePermission,
                screeningDTO.Discussion.First(u => u.DiscussionType == Discussion.kInternalScreeningType).DiscussionId));

        }

        [Test]
        public void AccountManager_Should_Not_Have_Access()
        {
            _permissionService.SwitchCurrentUser(_admin.UserName, _admin.UserId);
            var screeningDTO = CreateQualifyAndAssignScreening();
            ResetPermissionRepository(screeningDTO);
            _permissionService.SwitchCurrentUser(_accountManager1.UserName, _accountManager1.UserId);

            Assert.AreEqual(false, _permissionService.IsGranted(CVScreeningCore.Models.Permission.kScreeningCreatePermission,
                screeningDTO.ScreeningId));
            Assert.AreEqual(false, _permissionService.IsGranted(CVScreeningCore.Models.Permission.kScreeningDeactivatePermission,
                screeningDTO.ScreeningId));
            Assert.AreEqual(false, _permissionService.IsGranted(CVScreeningCore.Models.Permission.kReportUploadPermission,
                screeningDTO.ScreeningId));


            // Switch to am 2 that does not have access to this screening
            _permissionService.SwitchCurrentUser(_accountManager2.UserName, _accountManager2.UserId);
            Assert.AreEqual(false, _permissionService.IsGranted(CVScreeningCore.Models.Permission.kScreeningViewPermission,
                screeningDTO.ScreeningId));
            Assert.AreEqual(false, _permissionService.IsGranted(CVScreeningCore.Models.Permission.kScreeningManagePermission,
                screeningDTO.ScreeningId));
            Assert.AreEqual(false, _permissionService.IsGranted(CVScreeningCore.Models.Permission.kScreeningDeactivatePermission,
                screeningDTO.ScreeningId));
            Assert.AreEqual(false, _permissionService.IsGranted(CVScreeningCore.Models.Permission.kReportUploadPermission,
                screeningDTO.ScreeningId));

            Assert.AreEqual(false, _permissionService.IsGranted(CVScreeningCore.Models.Permission.kScreeningCreatePermission,
                screeningDTO.ScreeningLevelVersion.ScreeningLevelVersionId));
            Assert.AreEqual(false, _permissionService.IsGranted(CVScreeningCore.Models.Permission.kScreeningLevelVersionViewPermission,
                screeningDTO.ScreeningLevelVersion.ScreeningLevelVersionId));
            Assert.AreEqual(false, _permissionService.IsGranted(CVScreeningCore.Models.Permission.kScreeningLevelViewPermission,
                screeningDTO.ScreeningLevelVersion.ScreeningLevel.ScreeningLevelId));
            Assert.AreEqual(false, _permissionService.IsGranted(CVScreeningCore.Models.Permission.kContractViewPermission,
                screeningDTO.ScreeningLevelVersion.ScreeningLevel.Contract.ContractId));

            Assert.AreEqual(false, _permissionService.IsGranted(CVScreeningCore.Models.Permission.kExternalScreeningDiscussionManagePermission,
                screeningDTO.Discussion.First(u => u.DiscussionType == Discussion.kExternalScreeningType).DiscussionId));
            Assert.AreEqual(false, _permissionService.IsGranted(CVScreeningCore.Models.Permission.kInternalScreeningDiscussionManagePermission,
                screeningDTO.Discussion.First(u => u.DiscussionType == Discussion.kInternalScreeningType).DiscussionId));

            Assert.AreEqual(false, _permissionService.IsGranted(CVScreeningCore.Models.Permission.kAtomicCheckManagePermission,
                screeningDTO.AtomicCheck.ElementAt(0).AtomicCheckId));
            Assert.AreEqual(false, _permissionService.IsGranted(CVScreeningCore.Models.Permission.kAtomicCheckViewPermission,
                screeningDTO.AtomicCheck.ElementAt(0).AtomicCheckId));
            Assert.AreEqual(false, _permissionService.IsGranted(CVScreeningCore.Models.Permission.kAtomicCheckAssignPermission,
                screeningDTO.AtomicCheck.ElementAt(0).AtomicCheckId));

            var accountManager4 = Utilities.BuildAccountManagerAccountSample();
            accountManager4.UserName = "am4@am.com";
            var error = _userManagementService.CreateUserProfile(ref accountManager4,
                    new System.Collections.Generic.List<string>(new List<string> { "Account manager" }),
                    "123456", false);

            ResetPermissionRepository(screeningDTO);
            _permissionService.SwitchCurrentUser(accountManager4.UserName, accountManager4.UserId);
            // Create new AM that is not assigned to any company. Check that he cannot access to anything

            var atomicChecksDTO = _screeningService.GetAllAtomicChecksForScreening(screeningDTO);
            Assert.AreEqual(0, atomicChecksDTO.Count());

            Assert.AreEqual(false, _permissionService.IsGranted(CVScreeningCore.Models.Permission.kScreeningManagePermission,
                screeningDTO.ScreeningId));
            Assert.AreEqual(false, _permissionService.IsGranted(CVScreeningCore.Models.Permission.kScreeningViewPermission,
                screeningDTO.ScreeningId));
            Assert.AreEqual(false, _permissionService.IsGranted(CVScreeningCore.Models.Permission.kScreeningLevelViewPermission,
                screeningDTO.ScreeningId));
            Assert.AreEqual(false, _permissionService.IsGranted(CVScreeningCore.Models.Permission.kScreeningLevelVersionViewPermission,
                screeningDTO.ScreeningId));
            Assert.AreEqual(false, _permissionService.IsGranted(CVScreeningCore.Models.Permission.kContractViewPermission,
                screeningDTO.ScreeningId));
            Assert.AreEqual(false, _permissionService.IsGranted(CVScreeningCore.Models.Permission.kExternalScreeningDiscussionManagePermission,
                screeningDTO.Discussion.First(u => u.DiscussionType == Discussion.kExternalScreeningType).DiscussionId));
            Assert.AreEqual(false, _permissionService.IsGranted(CVScreeningCore.Models.Permission.kInternalScreeningDiscussionManagePermission,
                screeningDTO.Discussion.First(u => u.DiscussionType == Discussion.kInternalScreeningType).DiscussionId));

        }

        [Test]
        public void QualityControl_Should_Have_Access()
        {
            _permissionService.SwitchCurrentUser(_admin.UserName, _admin.UserId);
            var screeningDTO = CreateQualifyAndAssignScreening();
            ResetPermissionRepository(screeningDTO);
            _permissionService.SwitchCurrentUser(_qualityControl1.UserName, _qualityControl1.UserId);

            var atomicChecksDTO = _screeningService.GetAllAtomicChecksForScreening(screeningDTO);
            foreach (var atomicCheckDTO in atomicChecksDTO)
            {
                Assert.AreEqual(true, _permissionService.IsGranted(CVScreeningCore.Models.Permission.kAtomicCheckManagePermission,
                    atomicCheckDTO.AtomicCheckId));
                Assert.AreEqual(true, _permissionService.IsGranted(CVScreeningCore.Models.Permission.kAtomicCheckViewPermission,
                    atomicCheckDTO.AtomicCheckId));
                Assert.AreEqual(true, _permissionService.IsGranted(CVScreeningCore.Models.Permission.kAtomicCheckAssignPermission,
                    atomicCheckDTO.AtomicCheckId));
                Assert.AreEqual(true, _permissionService.IsGranted(CVScreeningCore.Models.Permission.kInternalAtomicCheckDiscussionManagePermission,
                    atomicCheckDTO.Discussion.First(u => u.DiscussionType == Discussion.kInternalAtomicCheckType).DiscussionId));
            }

            Assert.AreEqual(true, _permissionService.IsGranted(CVScreeningCore.Models.Permission.kReportUploadPermission,
                screeningDTO.ScreeningId));
            Assert.AreEqual(true, _permissionService.IsGranted(CVScreeningCore.Models.Permission.kScreeningManagePermission,
                screeningDTO.ScreeningId));
            Assert.AreEqual(true, _permissionService.IsGranted(CVScreeningCore.Models.Permission.kScreeningViewPermission,
                screeningDTO.ScreeningId));
            Assert.AreEqual(true, _permissionService.IsGranted(CVScreeningCore.Models.Permission.kInternalScreeningDiscussionManagePermission,
                screeningDTO.Discussion.First(u => u.DiscussionType == Discussion.kInternalScreeningType).DiscussionId));

        }
        
        [Test]
        public void QualityControl_Should_Not_Have_Access()
        {
            _permissionService.SwitchCurrentUser(_admin.UserName, _admin.UserId);
            var screeningDTO = CreateQualifyAndAssignScreening();
            ResetPermissionRepository(screeningDTO);
            _permissionService.SwitchCurrentUser(_qualityControl1.UserName, _qualityControl1.UserId);

            Assert.AreEqual(false, _permissionService.IsGranted(CVScreeningCore.Models.Permission.kScreeningLevelViewPermission,
                screeningDTO.ScreeningId));
            Assert.AreEqual(false, _permissionService.IsGranted(CVScreeningCore.Models.Permission.kScreeningLevelVersionViewPermission,
                screeningDTO.ScreeningId));
            Assert.AreEqual(false, _permissionService.IsGranted(CVScreeningCore.Models.Permission.kContractViewPermission,
                screeningDTO.ScreeningId));

            Assert.AreEqual(false, _permissionService.IsGranted(CVScreeningCore.Models.Permission.kScreeningCreatePermission,
                screeningDTO.ScreeningId));
            Assert.AreEqual(false, _permissionService.IsGranted(CVScreeningCore.Models.Permission.kScreeningDeactivatePermission,
                screeningDTO.ScreeningId));
            Assert.AreEqual(false, _permissionService.IsGranted(CVScreeningCore.Models.Permission.kExternalScreeningDiscussionManagePermission,
                screeningDTO.Discussion.First(u => u.DiscussionType == Discussion.kExternalScreeningType).DiscussionId));


            // Switch to qc 2 that does not have access to this screening
            _permissionService.SwitchCurrentUser(_qualityControl2.UserName, _qualityControl2.UserId);
            Assert.AreEqual(false, _permissionService.IsGranted(CVScreeningCore.Models.Permission.kScreeningViewPermission,
                screeningDTO.ScreeningId));
            Assert.AreEqual(false, _permissionService.IsGranted(CVScreeningCore.Models.Permission.kScreeningManagePermission,
                screeningDTO.ScreeningId));
            Assert.AreEqual(false, _permissionService.IsGranted(CVScreeningCore.Models.Permission.kScreeningDeactivatePermission,
                screeningDTO.ScreeningId));
            Assert.AreEqual(false, _permissionService.IsGranted(CVScreeningCore.Models.Permission.kReportUploadPermission,
                screeningDTO.ScreeningId));

            Assert.AreEqual(false, _permissionService.IsGranted(CVScreeningCore.Models.Permission.kScreeningCreatePermission,
                screeningDTO.ScreeningLevelVersion.ScreeningLevelVersionId));
            Assert.AreEqual(false, _permissionService.IsGranted(CVScreeningCore.Models.Permission.kScreeningLevelVersionViewPermission,
                screeningDTO.ScreeningLevelVersion.ScreeningLevelVersionId));
            Assert.AreEqual(false, _permissionService.IsGranted(CVScreeningCore.Models.Permission.kScreeningLevelViewPermission,
                screeningDTO.ScreeningLevelVersion.ScreeningLevel.ScreeningLevelId));
            Assert.AreEqual(false, _permissionService.IsGranted(CVScreeningCore.Models.Permission.kContractViewPermission,
                screeningDTO.ScreeningLevelVersion.ScreeningLevel.Contract.ContractId));

            Assert.AreEqual(false, _permissionService.IsGranted(CVScreeningCore.Models.Permission.kExternalScreeningDiscussionManagePermission,
                screeningDTO.Discussion.First(u => u.DiscussionType == Discussion.kExternalScreeningType).DiscussionId));
            Assert.AreEqual(false, _permissionService.IsGranted(CVScreeningCore.Models.Permission.kInternalScreeningDiscussionManagePermission,
                screeningDTO.Discussion.First(u => u.DiscussionType == Discussion.kInternalScreeningType).DiscussionId));

            Assert.AreEqual(false, _permissionService.IsGranted(CVScreeningCore.Models.Permission.kAtomicCheckManagePermission,
                screeningDTO.AtomicCheck.ElementAt(0).AtomicCheckId));
            Assert.AreEqual(false, _permissionService.IsGranted(CVScreeningCore.Models.Permission.kAtomicCheckViewPermission,
                screeningDTO.AtomicCheck.ElementAt(0).AtomicCheckId));
            Assert.AreEqual(false, _permissionService.IsGranted(CVScreeningCore.Models.Permission.kAtomicCheckAssignPermission,
                screeningDTO.AtomicCheck.ElementAt(0).AtomicCheckId));

        }

        [Test]
        public void Client_Should_Have_Access()
        {
            _permissionService.SwitchCurrentUser(_admin.UserName, _admin.UserId);
            var screeningDTO = CreateQualifyAndAssignScreening();
            ResetPermissionRepository(screeningDTO);
            _permissionService.SwitchCurrentUser(_client1.UserName, _client1.UserId);

            Assert.AreEqual(true, _permissionService.IsGranted(CVScreeningCore.Models.Permission.kScreeningCreatePermission,
                screeningDTO.ScreeningLevelVersion.ScreeningLevelVersionId));
            Assert.AreEqual(true, _permissionService.IsGranted(CVScreeningCore.Models.Permission.kScreeningManagePermission,
                screeningDTO.ScreeningId));
            Assert.AreEqual(true, _permissionService.IsGranted(CVScreeningCore.Models.Permission.kScreeningViewPermission,
                screeningDTO.ScreeningId));
            Assert.AreEqual(true, _permissionService.IsGranted(CVScreeningCore.Models.Permission.kScreeningLevelViewPermission,
                screeningDTO.ScreeningLevelVersion.ScreeningLevel.ScreeningLevelId));
            Assert.AreEqual(true, _permissionService.IsGranted(CVScreeningCore.Models.Permission.kScreeningLevelVersionViewPermission,
                screeningDTO.ScreeningLevelVersion.ScreeningLevelVersionId));
            Assert.AreEqual(true, _permissionService.IsGranted(CVScreeningCore.Models.Permission.kContractViewPermission,
                screeningDTO.ScreeningLevelVersion.ScreeningLevel.Contract.ContractId));
            Assert.AreEqual(true, _permissionService.IsGranted(CVScreeningCore.Models.Permission.kExternalScreeningDiscussionManagePermission,
                screeningDTO.Discussion.First(u => u.DiscussionType == Discussion.kExternalScreeningType).DiscussionId));

            // Create new client and check that he has automatically access to previous screening
            var client3 = Utilities.BuildClientAccountSample();
            client3.UserName = "myclient3@mytest.com";
            var error = _userManagementService.CreateClientProfile(ref client3, _clientCompany1, "123456", false);
            Assert.AreEqual(ErrorCode.NO_ERROR, error);

            ResetPermissionRepository(screeningDTO);
            _permissionService.SwitchCurrentUser(client3.UserName, client3.UserId);

            Assert.AreEqual(true, _permissionService.IsGranted(CVScreeningCore.Models.Permission.kScreeningCreatePermission,
                screeningDTO.ScreeningLevelVersion.ScreeningLevelVersionId));
            Assert.AreEqual(true, _permissionService.IsGranted(CVScreeningCore.Models.Permission.kScreeningManagePermission,
                screeningDTO.ScreeningId));
            Assert.AreEqual(true, _permissionService.IsGranted(CVScreeningCore.Models.Permission.kScreeningViewPermission,
                screeningDTO.ScreeningId));
            Assert.AreEqual(true, _permissionService.IsGranted(CVScreeningCore.Models.Permission.kScreeningLevelViewPermission,
                screeningDTO.ScreeningLevelVersion.ScreeningLevel.ScreeningLevelId));
            Assert.AreEqual(true, _permissionService.IsGranted(CVScreeningCore.Models.Permission.kScreeningLevelVersionViewPermission,
                screeningDTO.ScreeningLevelVersion.ScreeningLevelVersionId));
            Assert.AreEqual(true, _permissionService.IsGranted(CVScreeningCore.Models.Permission.kContractViewPermission,
                screeningDTO.ScreeningLevelVersion.ScreeningLevel.Contract.ContractId));
            Assert.AreEqual(true, _permissionService.IsGranted(CVScreeningCore.Models.Permission.kExternalScreeningDiscussionManagePermission,
                screeningDTO.Discussion.First(u => u.DiscussionType == Discussion.kExternalScreeningType).DiscussionId));

        }

        [Test]
        public void Client_Should_Not_Have_Access()
        {
            _permissionService.SwitchCurrentUser(_admin.UserName, _admin.UserId);
            var screeningDTO = CreateQualifyAndAssignScreening();
            ResetPermissionRepository(screeningDTO);
            _permissionService.SwitchCurrentUser(_client1.UserName, _client1.UserId);
            
            var atomicChecksDTO = _screeningService.GetAllAtomicChecksForScreening(screeningDTO);
            Assert.AreEqual(0, atomicChecksDTO.Count());
            
            Assert.AreEqual(false, _permissionService.IsGranted(CVScreeningCore.Models.Permission.kReportUploadPermission,
                screeningDTO.ScreeningId));
            Assert.AreEqual(false, _permissionService.IsGranted(CVScreeningCore.Models.Permission.kScreeningDeactivatePermission,
                screeningDTO.ScreeningId));
            Assert.AreEqual(false, _permissionService.IsGranted(CVScreeningCore.Models.Permission.kInternalScreeningDiscussionManagePermission,
                screeningDTO.Discussion.First(u => u.DiscussionType == Discussion.kInternalScreeningType).DiscussionId));

            // Switch to client 2 that does not have access to this company
            _permissionService.SwitchCurrentUser(_client2.UserName, _client2.UserId);
            Assert.AreEqual(false, _permissionService.IsGranted(CVScreeningCore.Models.Permission.kScreeningViewPermission,
                screeningDTO.ScreeningId));
            Assert.AreEqual(false, _permissionService.IsGranted(CVScreeningCore.Models.Permission.kScreeningManagePermission,
                screeningDTO.ScreeningId));
            Assert.AreEqual(false, _permissionService.IsGranted(CVScreeningCore.Models.Permission.kScreeningDeactivatePermission,
                screeningDTO.ScreeningId));
            Assert.AreEqual(false, _permissionService.IsGranted(CVScreeningCore.Models.Permission.kReportUploadPermission,
                screeningDTO.ScreeningId));

            Assert.AreEqual(false, _permissionService.IsGranted(CVScreeningCore.Models.Permission.kScreeningCreatePermission,
                screeningDTO.ScreeningLevelVersion.ScreeningLevelVersionId));
            Assert.AreEqual(false, _permissionService.IsGranted(CVScreeningCore.Models.Permission.kScreeningLevelVersionViewPermission,
                screeningDTO.ScreeningLevelVersion.ScreeningLevelVersionId));
            Assert.AreEqual(false, _permissionService.IsGranted(CVScreeningCore.Models.Permission.kScreeningLevelViewPermission,
                screeningDTO.ScreeningLevelVersion.ScreeningLevel.ScreeningLevelId));
            Assert.AreEqual(false, _permissionService.IsGranted(CVScreeningCore.Models.Permission.kContractViewPermission,
                screeningDTO.ScreeningLevelVersion.ScreeningLevel.Contract.ContractId));

            Assert.AreEqual(false, _permissionService.IsGranted(CVScreeningCore.Models.Permission.kExternalScreeningDiscussionManagePermission,
                screeningDTO.Discussion.First(u => u.DiscussionType == Discussion.kExternalScreeningType).DiscussionId));
            Assert.AreEqual(false, _permissionService.IsGranted(CVScreeningCore.Models.Permission.kInternalScreeningDiscussionManagePermission,
                screeningDTO.Discussion.First(u => u.DiscussionType == Discussion.kInternalScreeningType).DiscussionId));

            Assert.AreEqual(false, _permissionService.IsGranted(CVScreeningCore.Models.Permission.kAtomicCheckManagePermission,
                screeningDTO.AtomicCheck.ElementAt(0).AtomicCheckId));
            Assert.AreEqual(false, _permissionService.IsGranted(CVScreeningCore.Models.Permission.kAtomicCheckViewPermission,
                screeningDTO.AtomicCheck.ElementAt(0).AtomicCheckId));
            Assert.AreEqual(false, _permissionService.IsGranted(CVScreeningCore.Models.Permission.kAtomicCheckAssignPermission,
                screeningDTO.AtomicCheck.ElementAt(0).AtomicCheckId));

            var client4 = Utilities.BuildClientAccountSample();
            client4.UserName = "myclient4@mytest.com";
            var error = _userManagementService.CreateClientProfile(ref client4, _clientCompany2, "123456", false);
            Assert.AreEqual(ErrorCode.NO_ERROR, error);

            ResetPermissionRepository(screeningDTO);
            _permissionService.SwitchCurrentUser(client4.UserName, client4.UserId);

            Assert.AreEqual(false, _permissionService.IsGranted(CVScreeningCore.Models.Permission.kScreeningCreatePermission,
                screeningDTO.ScreeningLevelVersion.ScreeningLevelVersionId));
            Assert.AreEqual(false, _permissionService.IsGranted(CVScreeningCore.Models.Permission.kScreeningManagePermission,
                screeningDTO.ScreeningId));
            Assert.AreEqual(false, _permissionService.IsGranted(CVScreeningCore.Models.Permission.kScreeningViewPermission,
                screeningDTO.ScreeningId));
            Assert.AreEqual(false, _permissionService.IsGranted(CVScreeningCore.Models.Permission.kScreeningLevelViewPermission,
                screeningDTO.ScreeningLevelVersion.ScreeningLevel.ScreeningLevelId));
            Assert.AreEqual(false, _permissionService.IsGranted(CVScreeningCore.Models.Permission.kScreeningLevelVersionViewPermission,
                screeningDTO.ScreeningLevelVersion.ScreeningLevelVersionId));
            Assert.AreEqual(false, _permissionService.IsGranted(CVScreeningCore.Models.Permission.kContractViewPermission,
                screeningDTO.ScreeningLevelVersion.ScreeningLevel.Contract.ContractId));
            Assert.AreEqual(false, _permissionService.IsGranted(CVScreeningCore.Models.Permission.kExternalScreeningDiscussionManagePermission,
                screeningDTO.Discussion.First(u => u.DiscussionType == Discussion.kExternalScreeningType).DiscussionId));


        }


        // 4. Runs Twice; Once after Test Case 1 and Once After Test Case 2
        // Dispose Objects Used in Each Test which are no longer required
        [TearDown]
        public void RunOnceAfterEachTests()
        {
        }

        // 5. Runs Once After All of The Aformentioned Methods
        // Dispose all Mocks and Global Objects
        [TestFixtureTearDown]
        public void RunOnceAfterAll()
        {

        }

        #endregion

        #region Test Helpers

        private void InitializeTest()
        {
            Utilities.InitTypeOfCheck(_screeningService, _unitOfWork);
            Utilities.InitPermissionForProductionManager(_unitOfWork);
            Utilities.InitPermissionForQualifier(_unitOfWork);

            // Create sample user account for testing
            _accountManager1 = Utilities.BuildAccountManagerAccountSample();
            var error = _userManagementService.CreateUserProfile(ref _accountManager1,
                    new System.Collections.Generic.List<string>(new List<string> { "Account manager" }),
                    "123456", false);
            Assert.AreEqual(ErrorCode.NO_ERROR, error);

            _accountManager2 = Utilities.BuildAccountManagerAccountSample();
            _accountManager2.UserName = "am2@am.com";
            error = _userManagementService.CreateUserProfile(ref _accountManager2,
                    new System.Collections.Generic.List<string>(new List<string> { "Account manager" }),
                    "123456", false);
            Assert.AreEqual(ErrorCode.NO_ERROR, error);

            _qualityControl1 = Utilities.BuildQualityControlAccountSample();
            error = _userManagementService.CreateUserProfile(ref _qualityControl1,
                    new System.Collections.Generic.List<string>(new List<string> { "Quality control" }),
                    "123456", false);
            Assert.AreEqual(ErrorCode.NO_ERROR, error);

            _qualityControl2 = Utilities.BuildQualityControlAccountSample();
            _qualityControl2.UserName = "qc2@qc.com";
            error = _userManagementService.CreateUserProfile(ref _qualityControl2,
                    new System.Collections.Generic.List<string>(new List<string> { "Quality control" }),
                    "123456", false);
            Assert.AreEqual(ErrorCode.NO_ERROR, error);

            _productionManager = Utilities.BuildProductionManagerAccountSample();
            error = _userManagementService.CreateUserProfile(ref _productionManager,
                    new System.Collections.Generic.List<string>(new List<string> { "Production manager" }),
                    "123456", false);
            Assert.AreEqual(ErrorCode.NO_ERROR, error);

            _qualifier = Utilities.BuildQualifierAccountSample();
            error = _userManagementService.CreateUserProfile(ref _qualifier,
                    new System.Collections.Generic.List<string>(new List<string> { "Qualifier" }),
                    "123456", false);
            Assert.AreEqual(ErrorCode.NO_ERROR, error);

            _screenerOffice1 = Utilities.BuildScreenerAccountOfficeDTO();
            error = _userManagementService.CreateUserProfile(ref _screenerOffice1,
                    new System.Collections.Generic.List<string>(new List<string> { "Screener" }),
                    "123456", false);
            Assert.AreEqual(ErrorCode.NO_ERROR, error);


            _screenerOffice2 = Utilities.BuildScreenerAccountOfficeDTO();
            _screenerOffice2.UserName = "screener2@office.com";
            error = _userManagementService.CreateUserProfile(ref _screenerOffice2,
                    new System.Collections.Generic.List<string>(new List<string> { "Screener" }),
                    "123456", false);
            Assert.AreEqual(ErrorCode.NO_ERROR, error);

            _screenerOnField = Utilities.BuildScreenerAccountOnFieldDTO();
            error = _userManagementService.CreateUserProfile(ref _screenerOnField,
                    new System.Collections.Generic.List<string>(new List<string> { "Screener" }),
                    "123456", false);
            Assert.AreEqual(ErrorCode.NO_ERROR, error);

            _hr = Utilities.BuildHrAccountSample();
            error = _userManagementService.CreateUserProfile(ref _hr,
                    new System.Collections.Generic.List<string>(new List<string> { "HR" }),
                    "123456", false);
            Assert.AreEqual(ErrorCode.NO_ERROR, error);

            _clientCompany1 = Utilities.BuildClientCompanySample();
            _clientCompany1.AccountManagers = new List<UserProfileDTO> { _accountManager1 };
            error = _clientService.CreateClientCompany(ref _clientCompany1);
            Assert.AreEqual(ErrorCode.NO_ERROR, error);

            _client1 = Utilities.BuildClientAccountSample();
            error = _userManagementService.CreateClientProfile(ref _client1, _clientCompany1, "123456", false);
            Assert.AreEqual(ErrorCode.NO_ERROR, error);

            _clientCompany2 = Utilities.BuildClientCompanySample();
            _clientCompany2.ClientCompanyName = "My client company 2";
            _clientCompany2.AccountManagers = new List<UserProfileDTO> { _accountManager2 };
            error = _clientService.CreateClientCompany(ref _clientCompany2);
            Assert.AreEqual(ErrorCode.NO_ERROR, error);

            _client2 = Utilities.BuildClientAccountSample();
            _client2.UserName = "myclient2@mytest.com";
            error = _userManagementService.CreateClientProfile(ref _client2, _clientCompany2, "123456", false);
            Assert.AreEqual(ErrorCode.NO_ERROR, error);

            _clientContract1 = Utilities.BuildClientContractDTO();
            error = _clientService.CreateClientContract(_clientCompany1, ref _clientContract1);
            Assert.AreEqual(ErrorCode.NO_ERROR, error);

            _screeningLevel1 = Utilities.BuildScreeningLevelDTO();

            _screeningLevelVersion1 = Utilities.BuildScreeningLevelVersionDTO();
            _screeningLevelVersion1.TypeOfCheckScreeningLevelVersion = Utilities.BuildTypeOfCheckListForScreeningListDTO();
            _screeningLevelVersion1.ScreeningLevelVersionTurnaroundTime = 10;

            error = _clientService.CreateScreeningLevel(_clientContract1, ref _screeningLevel1, ref _screeningLevelVersion1);
            Assert.AreEqual(ErrorCode.NO_ERROR, error);

            _immigrationOfficeDTO = Utilities.BuildImmigrationOfficeDTO();
            error = _immigrationOfficeService.CreateOrEditQualificationPlace(ref _immigrationOfficeDTO);
            Assert.AreEqual(ErrorCode.NO_ERROR, error);

            _police1DTO = Utilities.BuildPoliceOfficeDTO();
            error = _policeService.CreateOrEditQualificationPlace(ref _police1DTO);
            Assert.AreEqual(ErrorCode.NO_ERROR, error);

            _police2DTO = Utilities.BuildPoliceOfficeDTO();
            _police2DTO.QualificationPlaceName = "Police 2";
            error = _policeService.CreateOrEditQualificationPlace(ref _police2DTO);
            Assert.AreEqual(ErrorCode.NO_ERROR, error);

            _companyDTO = Utilities.BuildCompanyDTO();
            error = _companyService.CreateOrEditQualificationPlace(ref _companyDTO);
            Assert.AreEqual(ErrorCode.NO_ERROR, error);

            _currentCmpanyDTO = Utilities.BuildCurrentCompanyDTO();
            error = _companyService.CreateOrEditQualificationPlace(ref _currentCmpanyDTO);
            Assert.AreEqual(ErrorCode.NO_ERROR, error);

            _highSchoolDTO = Utilities.BuildHighSchoolDTO();
            error = _highSchoolService.CreateOrEditQualificationPlace(ref _highSchoolDTO);
            Assert.AreEqual(ErrorCode.NO_ERROR, error);

            _populationOfficeDTO = Utilities.BuildPopulationOfficeDTO();
            error = _populationOfficeService.CreateOrEditQualificationPlace(ref _populationOfficeDTO);
            Assert.AreEqual(ErrorCode.NO_ERROR, error);

            _unitOfWork.UniversityRepository.Add(new University
            {
                UniversityId = 1,
                UniversityName = "ITS"
            });

            _facultyDTO = Utilities.BuildFaculyDTO();
            error = _facultyService.CreateOrEditQualificationPlace(ref _facultyDTO);
            Assert.AreEqual(ErrorCode.NO_ERROR, error);

            var professionalQualificationDTO = Utilities.BuildProfessionalQualificationDTO();
            _certificationPlaceDTO = Utilities.BuildCertificationPlaceDTO();

            error = _certificationService.CreateOrEditQualificationPlace(ref _certificationPlaceDTO);
            Assert.AreEqual(ErrorCode.NO_ERROR, error);

            professionalQualificationDTO.QualificationPlace = new List<CertificationPlaceDTO>
            {
                _certificationPlaceDTO
            };
            error = _professionalQualificationService.CreateOrUpdateProfessionalQualification(
                    ref professionalQualificationDTO);
            Assert.AreEqual(ErrorCode.NO_ERROR, error);

            // Retrieve screening directly from repository
            var professionalQualification = _unitOfWork.ProfessionalQualificationRepository.Single(u => u.ProfessionalQualificationId == professionalQualificationDTO.ProfessionalQualificationId);

            _commercialCourtDTO = Utilities.BuildCommercialCourtDTO();
            error = _courtLookUpDatabaseService.CreateOrEditQualificationPlace(ref _commercialCourtDTO);
            Assert.AreEqual(ErrorCode.NO_ERROR, error);
        }

        /// <summary>
        /// Reset and copy permission to repository. Needed as permission as handled in POCO class and as EF is not used
        /// repository is not filled automatically when commit is done
        /// </summary>
        /// <param name="screeningDTO"></param>
        private void ResetPermissionRepository(ScreeningDTO screeningDTO)
        {
            foreach (var permission in _unitOfWork.PermissionRepository.GetAll().Reverse())
            {
                _unitOfWork.PermissionRepository.Delete(permission);
            }

            Utilities.InitPermissionForQualifier(_unitOfWork);
            Utilities.InitPermissionForProductionManager(_unitOfWork);

            // Retrieve screening directly from repository
            var screening = _unitOfWork.ScreeningRepository.Single(u => u.ScreeningId == screeningDTO.ScreeningId);
            screening.Permission.ToList().ForEach(p => _unitOfWork.PermissionRepository.Add(p));
            screening.ScreeningReport.ToList().ForEach(r => r.Permission.ToList().ForEach(p => _unitOfWork.PermissionRepository.Add(p)));

            screening.AtomicCheck.ToList().ForEach(a => a.Permission.ToList().ForEach(
                p => _unitOfWork.PermissionRepository.Add(p)));

            screening.ScreeningLevelVersion.Permission.ToList().ForEach(p => _unitOfWork.PermissionRepository.Add(p));
            screening.ScreeningLevelVersion.ScreeningLevel.Permission.ToList().ForEach(p => _unitOfWork.PermissionRepository.Add(p));
            screening.ScreeningLevelVersion.ScreeningLevel.Contract.Permission.ToList().ForEach(p => _unitOfWork.PermissionRepository.Add(p));

            screening.AtomicCheck.ToList().ForEach(a => a.Discussion.ToList().ForEach(d => d.Permission.ToList().ForEach(
            p => _unitOfWork.PermissionRepository.Add(p))));
            screening.Discussion.ToList().ForEach(d => d.Permission.ToList().ForEach(p => _unitOfWork.PermissionRepository.Add(p)));

            screening.ScreeningLevelVersion.Permission.ToList().ForEach(p => _unitOfWork.PermissionRepository.Add(p));
        }

        /// <summary>
        /// Create a screening the application, used by test only
        /// </summary>
        /// <returns></returns>
        private ScreeningDTO CreateScreening()
        {
            // Create screening
            var screeningDTO = Utilities.BuildScreeningDTO(_screeningLevelVersion1, _qualityControl1);
            var error = _screeningService.CreateScreening(screeningDTO.ScreeningLevelVersion, ref screeningDTO);
            Assert.AreEqual(ErrorCode.NO_ERROR, error);

            // Retrieve screening directly from repository
            var screening = _unitOfWork.ScreeningRepository.Single(u => u.ScreeningId == screeningDTO.ScreeningId);
            foreach (var atomicCheck in screening.AtomicCheck)
            {
                _unitOfWork.AtomicCheckRepository.Add(atomicCheck);
            }

            return screeningDTO;
        }


        /// <summary>
        /// Create a screening the application and qualify it, used by test only
        /// </summary>
        /// <returns></returns>
        private ScreeningDTO CreateAndQualifyScreening()
        {
            // Create screening
            var screeningDTO = CreateScreening();
            var screening = _unitOfWork.ScreeningRepository.Single(u => u.ScreeningId == screeningDTO.ScreeningId);

            // Qualification started
            var qualificationBaseDTO = Utilities.BuildScreeningQualificationDTO();
            var error = _qualificationService.SetQualificationBase(screeningDTO, ref qualificationBaseDTO);
            Assert.AreEqual(ErrorCode.NO_ERROR, error);

            // All the atomic check are now qualified
            var qualifications = new List<BaseQualificationPlaceDTO>
            {
                _companyDTO, _currentCmpanyDTO, _police1DTO, _immigrationOfficeDTO, _highSchoolDTO, _facultyDTO, _commercialCourtDTO, _certificationPlaceDTO, _populationOfficeDTO
            };
            error = _qualificationService.SetQualificationPlaces(screeningDTO, qualifications);
            Assert.AreEqual(ErrorCode.NO_ERROR, error);

            return screeningDTO;
        }

        /// <summary>
        /// Create a screening the application, qualify it and process it, used by test only
        /// </summary>
        /// <returns></returns>
        private ScreeningDTO CreateQualifyAndAssignScreening()
        {
            // Create screening
            var screeningDTO = CreateAndQualifyScreening();
            var screening = _unitOfWork.ScreeningRepository.Single(u => u.ScreeningId == screeningDTO.ScreeningId);

            var atomicChecksDTO = _screeningService.GetAllAtomicChecksForScreening(screeningDTO);

            // Assign on field screener to office atomic check
            foreach (var atomicCheckDTO in atomicChecksDTO.Where(u => u.AtomicCheckCategory == CVScreeningCore.Models.AtomicCheck.kOfficeCategory))
            {
                var refAtomicCheckTO = atomicCheckDTO;
                var error = _screeningService.AssignAtomicCheck(ref refAtomicCheckTO, _screenerOffice1);
                Assert.AreEqual(ErrorCode.NO_ERROR, error);
            }

            foreach (var atomicCheckDTO in atomicChecksDTO.Where(u => u.AtomicCheckCategory == CVScreeningCore.Models.AtomicCheck.kOnFieldCategory))
            {
                var refAtomicCheckTO = atomicCheckDTO;
                var error = _screeningService.AssignAtomicCheck(ref refAtomicCheckTO, _screenerOnField);
                Assert.AreEqual(ErrorCode.NO_ERROR, error);
            }

            // Atomic checks are all in the status ON GOING
            Assert.AreEqual(0, screening.GetNewAtomicChecks().Count);
            return screeningDTO;
        }

        /// <summary>
        /// Create a screening the application, qualify it and process it, used by test only
        /// </summary>
        /// <returns></returns>
        private ScreeningDTO CreateQualifyAssignAndProcessScreening()
        {
            // Create screening
            var screeningDTO = CreateQualifyAndAssignScreening();
            var screening = _unitOfWork.ScreeningRepository.Single(u => u.ScreeningId == screeningDTO.ScreeningId);

            foreach (var atomicCheckDTO in _screeningService.GetAllAtomicChecksForScreening(screeningDTO))
            {
                var refAtomicCheckTO = atomicCheckDTO;
                refAtomicCheckTO.AtomicCheckState = (Byte)AtomicCheckStateType.DONE_OK;
                refAtomicCheckTO.AtomicCheckValidationState = (Byte)AtomicCheckValidationStateType.PROCESSED;
                var error = _screeningService.EditAtomicCheck(ref refAtomicCheckTO);
                Assert.AreEqual(ErrorCode.NO_ERROR, error);
            }
            return screeningDTO;
        }

        /// <summary>
        /// Create a screening the application, qualify it, process it and validate it, used by test only
        /// </summary>
        /// <returns></returns>
        private ScreeningDTO CreateQualifyAssignProcessAndValidateScreening()
        {
            // Create screening
            var screeningDTO = CreateQualifyAssignAndProcessScreening();
            var screening = _unitOfWork.ScreeningRepository.Single(u => u.ScreeningId == screeningDTO.ScreeningId);

            foreach (var atomicCheckDTO in _screeningService.GetAllAtomicChecksForScreening(screeningDTO))
            {
                var refAtomicCheckTO = atomicCheckDTO;
                refAtomicCheckTO.AtomicCheckState = (Byte)AtomicCheckStateType.DONE_OK;
                refAtomicCheckTO.AtomicCheckValidationState = (Byte)AtomicCheckValidationStateType.VALIDATED;
                var error = _screeningService.EditAtomicCheck(ref refAtomicCheckTO);
                Assert.AreEqual(ErrorCode.NO_ERROR, error);
            }
            return screeningDTO;
        }

        #endregion
    }
}