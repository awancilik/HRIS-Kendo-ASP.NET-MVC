using System;
using System.Collections.Generic;
using System.Linq;
using CVScreeningCore.Error;
using CVScreeningCore.Models;
using CVScreeningDAL.UnitOfWork;
using CVScreeningService.DTO.Client;
using CVScreeningService.DTO.LookUpDatabase;
using CVScreeningService.DTO.Screening;
using CVScreeningService.DTO.UserManagement;
using CVScreeningService.Services.Client;
using CVScreeningService.Services.Common;
using CVScreeningService.Services.DispatchingManagement;
using CVScreeningService.Services.LookUpDatabase;
using CVScreeningService.Services.Notification;
using CVScreeningService.Services.Permission;
using CVScreeningService.Services.Screening;
using CVScreeningService.Services.SystemTime;
using CVScreeningService.Services.UserManagement;
using FakeItEasy;
using NUnit.Framework;

namespace CVScreeningService.Tests.UnitTest.Dispatching
{
    [TestFixture]
    public class Availibility
    {
        #region NUnit Test Template

        // 1. Declare global object
        private IUnitOfWork _unitOfWork;
        private IQualificationPlaceFactory _factory;
        private ICommonService _commonService;
        private IScreeningService _screeningService;
        private IClientService _clientService;
        private IPermissionService _permissionService;
        private ISystemTimeService _systemTimeService;
        private IQualificationService _qualificationService;
        private IUserManagementService _userManagementService;
        private ILookUpDatabaseService<PoliceDTO> _policeService;
        private ILookUpDatabaseService<ImmigrationOfficeDTO> _immigrationOfficeService;
        private ILookUpDatabaseService<CompanyDTO> _companyService;
        private ILookUpDatabaseService<HighSchoolDTO> _highSchoolService;
        private ILookUpDatabaseService<FacultyDTO> _facultyService;
        private ILookUpDatabaseService<CertificationPlaceDTO> _certificationService;
        private ILookUpDatabaseService<CourtDTO> _courtLookUpDatabaseService;
        private IProfessionalQualificationService _professionalQualificationService;
        private IDispatchingManagementService _dispatchingManagementService;
        private IWebSecurity _webSecurity;

        private UserProfileDTO _admin;
        private UserProfileDTO _accountManager1;
        private UserProfileDTO _qualityControl1;
        private UserProfileDTO _accountManager2;
        private UserProfileDTO _qualityControl2;
        private UserProfileDTO _productionManager;
        private UserProfileDTO _qualifier;
        private UserProfileDTO _screenerOffice1;
        private UserProfileDTO _screenerOffice2;
        private UserProfileDTO _screenerOffice3;
        private UserProfileDTO _screenerOffice4;
        private UserProfileDTO _screenerOffice5;
        private UserProfileDTO _screenerOffice6;
        private UserProfileDTO _screenerOnField1;
        private UserProfileDTO _screenerOnField2;
        private UserProfileDTO _screenerOnField3;
        private UserProfileDTO _client1;
        private UserProfileDTO _client2;
        private UserProfileDTO _hr;


        private ClientCompanyDTO _clientCompany1;
        private ClientCompanyDTO _clientCompany2;
        private ClientContractDTO _clientContract1;
        private ScreeningLevelDTO _screeningLevel1;
        private ScreeningLevelVersionDTO _screeningLevelVersion1;
        private ImmigrationOfficeDTO _immigrationOfficeDTO;
        private PoliceDTO _police1DTO;
        private PoliceDTO _police2DTO;
        private CompanyDTO _companyDTO;
        private ScreeningQualificationDTO _qualificationBaseDTO;
        private HighSchoolDTO _highSchoolDTO;
        private FacultyDTO _facultyDTO;
        private CertificationPlaceDTO _certificationPlaceDTO;
        private CourtDTO _commercialCourtDTO;
        private IEnumerable<BaseQualificationPlaceDTO> _qualificationPlacesDTO;


        // 2. Runs Once Before All of The Following Methods
        // Declare Global Objects Which Are Global For Test Class, e.g. Mock Objects
        [TestFixtureSetUp]
        public void RunsOnceBeforeAll()
        {
            var notificationService = A.Fake<INotificationService>();
            _systemTimeService = A.Fake<ISystemTimeService>();
            A.CallTo(() => _systemTimeService.GetCurrentDateTime()).Returns(new DateTime(2014, 12, 1));
            _webSecurity = A.Fake<IWebSecurity>();
            A.CallTo(() => _webSecurity.GetCurrentUserName()).Returns("Administrator");

            _factory = new QualificationPlaceFactory(_unitOfWork);
            _unitOfWork = new InMemoryUnitOfWork();
            _commonService = new CommonService(_unitOfWork);
            _userManagementService = new UserManagementService(_unitOfWork, _commonService, _systemTimeService);
            _clientService = new ClientService(_unitOfWork, _commonService, _userManagementService, _permissionService);
            _policeService = new PoliceLookUpDatabaseService(_unitOfWork, _factory);
            _immigrationOfficeService = new ImmigrationOfficeLookUpDatabaseService(_unitOfWork, _factory);
            _companyService = new CompanyLookUpDatabaseService(_unitOfWork, _factory);
            _highSchoolService = new HighSchoolLookUpDatabaseService(_unitOfWork, _factory);
            _facultyService = new FacultyLookUpDatabaseService(_unitOfWork, _factory);
            _certificationService = new CertificationPlaceLookUpDatabaseService(_unitOfWork, _factory);
            _courtLookUpDatabaseService = new CourtLookUpDatabaseService(_unitOfWork, _factory);
            _qualificationService = new QualificationService(_unitOfWork, _commonService);
            _professionalQualificationService = new ProfessionalQualificationService(_unitOfWork);
            _dispatchingManagementService = new DispatchingManagementService(_unitOfWork, _userManagementService,
                _systemTimeService, _screeningService);

            Utilities.InitLocations(_commonService);
            Utilities.InitRoles(_userManagementService);

            _admin = Utilities.BuildAdminAccountSample();
            var error = _userManagementService.CreateUserProfile(ref _admin,
                new System.Collections.Generic.List<string>(new List<string> {"Administrator"}),
                "123456", false);
            _permissionService = new PermissionService(_unitOfWork, _admin.UserName);
            _screeningService = new ScreeningService(_unitOfWork, _permissionService, _userManagementService,
                _systemTimeService, _webSecurity, notificationService);

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

            var atomicChecks = _unitOfWork.AtomicCheckRepository.GetAll();
            foreach (var atomicCheck in atomicChecks.Reverse())
            {
                _unitOfWork.AtomicCheckRepository.Delete(atomicCheck);
            }

            var screeners = _unitOfWork.UserProfileRepository.Find(u => u.IsScreener());
            foreach (var screener in screeners.Reverse())
            {
                screener.AtomicCheck.Clear();
                screener.UserLeave.Clear();
            }

            var leaves = _unitOfWork.UserLeaveRepository.GetAll();
            foreach (var leave in leaves.Reverse())
            {
                _unitOfWork.UserLeaveRepository.Delete(leave);
            }
        }

        [Test]
        public void Retrieve_Availibility_Screeners()
        {
            var screeningDTO = CreateAndQualifyScreening();
            var atomicChecks = _screeningService.GetAllAtomicChecksForScreening(screeningDTO);

            var educationAtomicCheck = atomicChecks.First(
                u => u.TypeOfCheck.TypeOfCheckCode == (Byte) TypeOfCheckEnum.EDUCATION_CHECK_STANDARD);

            var availibity = _dispatchingManagementService.GetScreenerAvailabilityWeight(educationAtomicCheck);

            Assert.AreEqual(100, availibity.ElementAt(0).Value);
            Assert.AreEqual(100, availibity.ElementAt(1).Value);
            Assert.AreEqual(100, availibity.ElementAt(2).Value);
            Assert.AreEqual(100, availibity.ElementAt(3).Value);
        }


        [Test]
        public void Retrieve_Availibility_Screeners_Should_Be_Not_Available()
        {
            A.CallTo(() => _systemTimeService.GetCurrentDateTime()).Returns(new DateTime(2014, 9, 15));
            var screeningDTO = CreateAndQualifyScreening();
            var atomicChecks = _screeningService.GetAllAtomicChecksForScreening(screeningDTO);
            var educationAtomicCheck = atomicChecks.First(
                u => u.TypeOfCheck.TypeOfCheckCode == (Byte) TypeOfCheckEnum.EDUCATION_CHECK_STANDARD);

            var userLeave1DTO = new UserLeaveDTO
            {
                UserLeaveName = "Annual holiday",
                UserLeaveStartDate = new DateTime(2014, 9, 16),
                UserLeaveEndDate = new DateTime(2014, 9, 18)
            };
            var userLeave2DTO = new UserLeaveDTO
            {
                UserLeaveName = "Annual holiday",
                UserLeaveStartDate = new DateTime(2014, 9, 16),
                UserLeaveEndDate = new DateTime(2014, 9, 18)
            };

            var error = _userManagementService.CreateLeave(_screenerOffice1, ref userLeave1DTO);
            Assert.AreEqual(ErrorCode.NO_ERROR, error);
            error = _userManagementService.CreateLeave(_screenerOnField1, ref userLeave2DTO);
            Assert.AreEqual(ErrorCode.NO_ERROR, error);

            var availibity = _dispatchingManagementService.GetScreenerAvailabilityWeight(educationAtomicCheck);

            Assert.AreEqual(availibity.ElementAt(0).Value, 0);
            Assert.AreEqual(availibity.ElementAt(1).Value, 100);
            Assert.AreEqual(availibity.ElementAt(2).Value, 0);
            Assert.AreEqual(availibity.ElementAt(3).Value, 100);
        }


        [Test]
        public void Retrieve_Availibility_Screeners_Should_Be_Partially_Available_1()
        {
            A.CallTo(() => _systemTimeService.GetCurrentDateTime()).Returns(new DateTime(2014, 9, 15));
            var screeningDTO = CreateAndQualifyScreening();
            var atomicChecks = _screeningService.GetAllAtomicChecksForScreening(screeningDTO);
            var educationAtomicCheck = atomicChecks.First(
                u => u.TypeOfCheck.TypeOfCheckCode == (Byte) TypeOfCheckEnum.EDUCATION_CHECK_STANDARD);

            var userLeave1DTO = new UserLeaveDTO
            {
                UserLeaveName = "Annual holiday",
                UserLeaveStartDate = new DateTime(2014, 9, 17), // Wednesday 17th September
                UserLeaveEndDate = new DateTime(2014, 9, 17)
            };
            var userLeave2DTO = new UserLeaveDTO
            {
                UserLeaveName = "Annual holiday",
                UserLeaveStartDate = new DateTime(2014, 9, 18), // Thursday 18th September
                UserLeaveEndDate = new DateTime(2014, 9, 18)
            };

            var userLeave3DTO = new UserLeaveDTO
            {
                UserLeaveName = "Annual holiday",
                UserLeaveStartDate = new DateTime(2014, 9, 19), // Friday 19th September
                UserLeaveEndDate = new DateTime(2014, 9, 19)
            };

            var userLeave4DTO = new UserLeaveDTO
            {
                UserLeaveName = "Annual holiday",
                UserLeaveStartDate = new DateTime(2014, 9, 22), // Monday 22th September
                UserLeaveEndDate = new DateTime(2014, 9, 22)
            };

            var userLeave5DTO = new UserLeaveDTO
            {
                UserLeaveName = "Annual holiday",
                UserLeaveStartDate = new DateTime(2014, 9, 23), // Tuesday 23th September
                UserLeaveEndDate = new DateTime(2014, 9, 23)
            };
            var userLeave6DTO = new UserLeaveDTO
            {
                UserLeaveName = "Annual holiday",
                UserLeaveStartDate = new DateTime(2014, 9, 24), // Wednesday 24th September
                UserLeaveEndDate = new DateTime(2014, 9, 24)
            };

            var userLeave7DTO = new UserLeaveDTO
            {
                UserLeaveName = "Annual holiday",
                UserLeaveStartDate = new DateTime(2014, 9, 25), // Thursday 25th September
                UserLeaveEndDate = new DateTime(2014, 9, 25)
            };

            var userLeave8DTO = new UserLeaveDTO
            {
                UserLeaveName = "Annual holiday",
                UserLeaveStartDate = new DateTime(2014, 9, 26), // Friday 26th September
                UserLeaveEndDate = new DateTime(2014, 9, 26)
            };


            var userLeave9DTO = new UserLeaveDTO
            {
                UserLeaveName = "Annual holiday",
                UserLeaveStartDate = new DateTime(2014, 9, 29), // Monday 29th September
                UserLeaveEndDate = new DateTime(2014, 9, 29)
            };


            var error = _userManagementService.CreateLeave(_screenerOffice1, ref userLeave1DTO);
            Assert.AreEqual(ErrorCode.NO_ERROR, error);
            error = _userManagementService.CreateLeave(_screenerOffice2, ref userLeave2DTO);
            Assert.AreEqual(ErrorCode.NO_ERROR, error);
            error = _userManagementService.CreateLeave(_screenerOnField1, ref userLeave3DTO);
            Assert.AreEqual(ErrorCode.NO_ERROR, error);
            error = _userManagementService.CreateLeave(_screenerOnField2, ref userLeave4DTO);
            Assert.AreEqual(ErrorCode.NO_ERROR, error);
            error = _userManagementService.CreateLeave(_screenerOffice3, ref userLeave5DTO);
            Assert.AreEqual(ErrorCode.NO_ERROR, error);
            error = _userManagementService.CreateLeave(_screenerOffice4, ref userLeave6DTO);
            Assert.AreEqual(ErrorCode.NO_ERROR, error);
            error = _userManagementService.CreateLeave(_screenerOffice5, ref userLeave7DTO);
            Assert.AreEqual(ErrorCode.NO_ERROR, error);
            error = _userManagementService.CreateLeave(_screenerOffice6, ref userLeave8DTO);
            Assert.AreEqual(ErrorCode.NO_ERROR, error);
            error = _userManagementService.CreateLeave(_screenerOnField3, ref userLeave9DTO);
            Assert.AreEqual(ErrorCode.NO_ERROR, error);

            var availibity = _dispatchingManagementService.GetScreenerAvailabilityWeight(educationAtomicCheck);

            Assert.AreEqual(0, availibity.ElementAt(0).Value);
            Assert.AreEqual(0, availibity.ElementAt(1).Value);
            Assert.AreEqual(14, (int) availibity.ElementAt(2).Value);
            Assert.AreEqual(28, (int) availibity.ElementAt(3).Value);
            Assert.AreEqual(42, (int) availibity.ElementAt(4).Value);
            Assert.AreEqual(57, (int) availibity.ElementAt(5).Value);
            Assert.AreEqual(71, (int) availibity.ElementAt(6).Value);
            Assert.AreEqual(85, (int) availibity.ElementAt(7).Value);
            Assert.AreEqual(100, availibity.ElementAt(8).Value);
        }


        [Test]
        public void Retrieve_Availibility_Screeners_Should_Be_Partially_Available_2()
        {
            A.CallTo(() => _systemTimeService.GetCurrentDateTime()).Returns(new DateTime(2014, 9, 15));
            var screeningDTO = CreateAndQualifyScreening();
            var atomicChecks = _screeningService.GetAllAtomicChecksForScreening(screeningDTO);
            var educationAtomicCheck = atomicChecks.First(
                u => u.TypeOfCheck.TypeOfCheckCode == (Byte) TypeOfCheckEnum.EDUCATION_CHECK_STANDARD);

            var userLeave1DTO = new UserLeaveDTO
            {
                UserLeaveName = "Annual holiday",
                UserLeaveStartDate = new DateTime(2014, 9, 17), // Wednesday 17th September
                UserLeaveEndDate = new DateTime(2014, 9, 17)
            };
            var userLeave2DTO = new UserLeaveDTO
            {
                UserLeaveName = "Annual holiday",
                UserLeaveStartDate = new DateTime(2014, 9, 18), // Thursday 18th September
                UserLeaveEndDate = new DateTime(2014, 9, 18)
            };

            var userLeave3DTO = new UserLeaveDTO
            {
                UserLeaveName = "Annual holiday",
                UserLeaveStartDate = new DateTime(2014, 9, 19), // Friday 19th September
                UserLeaveEndDate = new DateTime(2014, 9, 19)
            };

            var userLeave4DTO = new UserLeaveDTO
            {
                UserLeaveName = "Annual holiday",
                UserLeaveStartDate = new DateTime(2014, 9, 22), // Monday 22th September
                UserLeaveEndDate = new DateTime(2014, 9, 22)
            };

            var userLeave5DTO = new UserLeaveDTO
            {
                UserLeaveName = "Annual holiday",
                UserLeaveStartDate = new DateTime(2014, 9, 23), // Tuesday 23th September
                UserLeaveEndDate = new DateTime(2014, 9, 23)
            };
            var userLeave6DTO = new UserLeaveDTO
            {
                UserLeaveName = "Annual holiday",
                UserLeaveStartDate = new DateTime(2014, 9, 24), // Wednesday 24th September
                UserLeaveEndDate = new DateTime(2014, 9, 24)
            };

            var userLeave7DTO = new UserLeaveDTO
            {
                UserLeaveName = "Annual holiday",
                UserLeaveStartDate = new DateTime(2014, 9, 25), // Thursday 25th September
                UserLeaveEndDate = new DateTime(2014, 9, 25)
            };

            var userLeave8DTO = new UserLeaveDTO
            {
                UserLeaveName = "Annual holiday",
                UserLeaveStartDate = new DateTime(2014, 9, 26), // Friday 26th September
                UserLeaveEndDate = new DateTime(2014, 9, 26)
            };


            var userLeave9DTO = new UserLeaveDTO
            {
                UserLeaveName = "Annual holiday",
                UserLeaveStartDate = new DateTime(2014, 9, 29), // Monday 29th September
                UserLeaveEndDate = new DateTime(2014, 9, 29)
            };


            var error = _userManagementService.CreateLeave(_screenerOffice1, ref userLeave1DTO);
            Assert.AreEqual(ErrorCode.NO_ERROR, error);
            error = _userManagementService.CreateLeave(_screenerOffice2, ref userLeave2DTO);
            Assert.AreEqual(ErrorCode.NO_ERROR, error);
            error = _userManagementService.CreateLeave(_screenerOnField1, ref userLeave3DTO);
            Assert.AreEqual(ErrorCode.NO_ERROR, error);
            error = _userManagementService.CreateLeave(_screenerOnField2, ref userLeave4DTO);
            Assert.AreEqual(ErrorCode.NO_ERROR, error);
            error = _userManagementService.CreateLeave(_screenerOffice3, ref userLeave5DTO);
            Assert.AreEqual(ErrorCode.NO_ERROR, error);
            error = _userManagementService.CreateLeave(_screenerOffice4, ref userLeave6DTO);
            Assert.AreEqual(ErrorCode.NO_ERROR, error);
            error = _userManagementService.CreateLeave(_screenerOffice5, ref userLeave7DTO);
            Assert.AreEqual(ErrorCode.NO_ERROR, error);
            error = _userManagementService.CreateLeave(_screenerOffice6, ref userLeave8DTO);
            Assert.AreEqual(ErrorCode.NO_ERROR, error);
            error = _userManagementService.CreateLeave(_screenerOnField3, ref userLeave9DTO);
            Assert.AreEqual(ErrorCode.NO_ERROR, error);

            A.CallTo(() => _systemTimeService.GetCurrentDateTime()).Returns(new DateTime(2014, 9, 22));
            var availibity = _dispatchingManagementService.GetScreenerAvailabilityWeight(educationAtomicCheck);

            Assert.AreEqual(100, availibity.ElementAt(0).Value);
            Assert.AreEqual(100, availibity.ElementAt(1).Value);
            Assert.AreEqual(100, (int) availibity.ElementAt(2).Value);
            Assert.AreEqual(0, (int) availibity.ElementAt(3).Value); // Screener in leave today, availibility is null
            Assert.AreEqual(0, (int) availibity.ElementAt(4).Value);
            Assert.AreEqual(0, (int) availibity.ElementAt(5).Value);
            Assert.AreEqual(0, (int) availibity.ElementAt(6).Value);
            Assert.AreEqual(50, (int) availibity.ElementAt(7).Value);
            Assert.AreEqual(100, availibity.ElementAt(8).Value);
        }


        [Test]
        public void Retrieve_Availibility_Screeners_Should_Be_Partially_Available_3()
        {
            A.CallTo(() => _systemTimeService.GetCurrentDateTime()).Returns(new DateTime(2014, 9, 15));
            var screeningDTO = CreateAndQualifyScreening();
            var atomicChecks = _screeningService.GetAllAtomicChecksForScreening(screeningDTO);
            var educationAtomicCheck = atomicChecks.First(
                u => u.TypeOfCheck.TypeOfCheckCode == (Byte) TypeOfCheckEnum.EDUCATION_CHECK_STANDARD);

            var userLeave1DTO = new UserLeaveDTO
            {
                UserLeaveName = "Annual holiday",
                UserLeaveStartDate = new DateTime(2014, 9, 17), // Wednesday 17th September to Thursday 18th September
                UserLeaveEndDate = new DateTime(2014, 9, 18)
            };
            var userLeave2DTO = new UserLeaveDTO
            {
                UserLeaveName = "Annual holiday",
                UserLeaveStartDate = new DateTime(2014, 9, 18), // Thursday 18th September
                UserLeaveEndDate = new DateTime(2014, 9, 18)
            };

            var userLeave3DTO = new UserLeaveDTO
            {
                UserLeaveName = "Annual holiday",
                UserLeaveStartDate = new DateTime(2014, 9, 19), // Friday 19th September
                UserLeaveEndDate = new DateTime(2014, 9, 19)
            };

            var userLeave4DTO = new UserLeaveDTO
            {
                UserLeaveName = "Annual holiday",
                UserLeaveStartDate = new DateTime(2014, 9, 22), // Monday 22th September
                UserLeaveEndDate = new DateTime(2014, 9, 22)
            };

            var userLeave5DTO = new UserLeaveDTO
            {
                UserLeaveName = "Annual holiday",
                UserLeaveStartDate = new DateTime(2014, 9, 23), // Tuesday 23th September
                UserLeaveEndDate = new DateTime(2014, 9, 23)
            };
            var userLeave6DTO = new UserLeaveDTO
            {
                UserLeaveName = "Annual holiday",
                UserLeaveStartDate = new DateTime(2014, 9, 24), // Wednesday 24th September
                UserLeaveEndDate = new DateTime(2014, 9, 24)
            };

            var userLeave7DTO = new UserLeaveDTO
            {
                UserLeaveName = "Annual holiday",
                UserLeaveStartDate = new DateTime(2014, 9, 25), // Thursday 25th September
                UserLeaveEndDate = new DateTime(2014, 9, 25)
            };

            var userLeave8DTO = new UserLeaveDTO
            {
                UserLeaveName = "Annual holiday",
                UserLeaveStartDate = new DateTime(2014, 9, 26), // Friday 26th September
                UserLeaveEndDate = new DateTime(2014, 9, 26)
            };


            var userLeave9DTO = new UserLeaveDTO
            {
                UserLeaveName = "Annual holiday",
                UserLeaveStartDate = new DateTime(2014, 9, 29), // Monday 29th September
                UserLeaveEndDate = new DateTime(2014, 9, 29)
            };


            var error = _userManagementService.CreateLeave(_screenerOffice1, ref userLeave1DTO);
            Assert.AreEqual(ErrorCode.NO_ERROR, error);
            error = _userManagementService.CreateLeave(_screenerOffice2, ref userLeave2DTO);
            Assert.AreEqual(ErrorCode.NO_ERROR, error);
            error = _userManagementService.CreateLeave(_screenerOnField1, ref userLeave3DTO);
            Assert.AreEqual(ErrorCode.NO_ERROR, error);
            error = _userManagementService.CreateLeave(_screenerOnField2, ref userLeave4DTO);
            Assert.AreEqual(ErrorCode.NO_ERROR, error);
            error = _userManagementService.CreateLeave(_screenerOffice3, ref userLeave5DTO);
            Assert.AreEqual(ErrorCode.NO_ERROR, error);
            error = _userManagementService.CreateLeave(_screenerOffice4, ref userLeave6DTO);
            Assert.AreEqual(ErrorCode.NO_ERROR, error);
            error = _userManagementService.CreateLeave(_screenerOffice5, ref userLeave7DTO);
            Assert.AreEqual(ErrorCode.NO_ERROR, error);
            error = _userManagementService.CreateLeave(_screenerOffice6, ref userLeave8DTO);
            Assert.AreEqual(ErrorCode.NO_ERROR, error);
            error = _userManagementService.CreateLeave(_screenerOnField3, ref userLeave9DTO);
            Assert.AreEqual(ErrorCode.NO_ERROR, error);

            A.CallTo(() => _systemTimeService.GetCurrentDateTime()).Returns(new DateTime(2014, 9, 18));
            var availibity = _dispatchingManagementService.GetScreenerAvailabilityWeight(educationAtomicCheck);

            Assert.AreEqual(0, availibity.ElementAt(0).Value);
            Assert.AreEqual(0, availibity.ElementAt(1).Value);
            Assert.AreEqual(0, (int) availibity.ElementAt(2).Value);
            Assert.AreEqual(0, (int) availibity.ElementAt(3).Value); // Screener in leave today, availibility is null
            Assert.AreEqual(0, (int) availibity.ElementAt(4).Value);
            Assert.AreEqual(25, (int) availibity.ElementAt(5).Value);
            Assert.AreEqual(50, (int) availibity.ElementAt(6).Value);
            Assert.AreEqual(75, (int) availibity.ElementAt(7).Value);
            Assert.AreEqual(100, availibity.ElementAt(8).Value);
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
            Utilities.InitRoles(_userManagementService);
            Utilities.InitTypeOfCheck(_screeningService, _unitOfWork);
            Utilities.InitPermissionForProductionManager(_unitOfWork);
            Utilities.InitPermissionForQualifier(_unitOfWork);

            // Create sample user account for testing
            _accountManager1 = Utilities.BuildAccountManagerAccountSample();
            var error = _userManagementService.CreateUserProfile(ref _accountManager1,
                new List<string>(new List<string> {"Account manager"}), "123456", false);
            Assert.AreEqual(ErrorCode.NO_ERROR, error);

            _accountManager2 = Utilities.BuildAccountManagerAccountSample();
            _accountManager2.UserName = "am2@am.com";
            error = _userManagementService.CreateUserProfile(ref _accountManager2,
                new List<string>(new List<string> {"Account manager"}),
                "123456", false);
            Assert.AreEqual(ErrorCode.NO_ERROR, error);

            _qualityControl1 = Utilities.BuildQualityControlAccountSample();
            error = _userManagementService.CreateUserProfile(ref _qualityControl1,
                new List<string>(new List<string> {"Quality control"}),
                "123456", false);
            Assert.AreEqual(ErrorCode.NO_ERROR, error);

            _qualityControl2 = Utilities.BuildQualityControlAccountSample();
            _qualityControl2.UserName = "qc2@qc.com";
            error = _userManagementService.CreateUserProfile(ref _qualityControl2,
                new System.Collections.Generic.List<string>(new List<string> {"Quality control"}),
                "123456", false);
            Assert.AreEqual(ErrorCode.NO_ERROR, error);

            _productionManager = Utilities.BuildProductionManagerAccountSample();
            error = _userManagementService.CreateUserProfile(ref _productionManager,
                new System.Collections.Generic.List<string>(new List<string> {"Production manager"}),
                "123456", false);
            Assert.AreEqual(ErrorCode.NO_ERROR, error);

            _qualifier = Utilities.BuildQualifierAccountSample();
            error = _userManagementService.CreateUserProfile(ref _qualifier,
                new System.Collections.Generic.List<string>(new List<string> {"Qualifier"}),
                "123456", false);
            Assert.AreEqual(ErrorCode.NO_ERROR, error);

            _screenerOffice1 = Utilities.BuildScreenerAccountOfficeDTO();
            error = _userManagementService.CreateUserProfile(ref _screenerOffice1,
                new System.Collections.Generic.List<string>(new List<string> {"Screener"}),
                "123456", false);
            Assert.AreEqual(ErrorCode.NO_ERROR, error);


            _screenerOffice2 = Utilities.BuildScreenerAccountOfficeDTO();
            _screenerOffice2.UserName = "screener2@office.com";
            error = _userManagementService.CreateUserProfile(ref _screenerOffice2,
                new System.Collections.Generic.List<string>(new List<string> {"Screener"}),
                "123456", false);
            Assert.AreEqual(ErrorCode.NO_ERROR, error);


            _screenerOnField1 = Utilities.BuildScreenerAccountOnFieldDTO();
            error = _userManagementService.CreateUserProfile(ref _screenerOnField1,
                new System.Collections.Generic.List<string>(new List<string> {"Screener"}),
                "123456", false);
            Assert.AreEqual(ErrorCode.NO_ERROR, error);

            _screenerOnField2 = Utilities.BuildScreenerAccountOnFieldDTO();
            _screenerOnField2.UserName = "screeneronfield2@office.com";
            error = _userManagementService.CreateUserProfile(ref _screenerOnField2,
                new System.Collections.Generic.List<string>(new List<string> {"Screener"}),
                "123456", false);
            Assert.AreEqual(ErrorCode.NO_ERROR, error);


            _screenerOffice3 = Utilities.BuildScreenerAccountOfficeDTO();
            _screenerOffice3.UserName = "screener3@office.com";
            error = _userManagementService.CreateUserProfile(ref _screenerOffice3,
                new System.Collections.Generic.List<string>(new List<string> {"Screener"}),
                "123456", false);
            Assert.AreEqual(ErrorCode.NO_ERROR, error);

            _screenerOffice4 = Utilities.BuildScreenerAccountOfficeDTO();
            _screenerOffice4.UserName = "screener4@office.com";
            error = _userManagementService.CreateUserProfile(ref _screenerOffice4,
                new System.Collections.Generic.List<string>(new List<string> {"Screener"}),
                "123456", false);
            Assert.AreEqual(ErrorCode.NO_ERROR, error);

            _screenerOffice5 = Utilities.BuildScreenerAccountOfficeDTO();
            _screenerOffice5.UserName = "screener5@office.com";
            error = _userManagementService.CreateUserProfile(ref _screenerOffice5,
                new System.Collections.Generic.List<string>(new List<string> {"Screener"}),
                "123456", false);
            Assert.AreEqual(ErrorCode.NO_ERROR, error);

            _screenerOffice6 = Utilities.BuildScreenerAccountOfficeDTO();
            _screenerOffice6.UserName = "screener6@office.com";
            error = _userManagementService.CreateUserProfile(ref _screenerOffice6,
                new System.Collections.Generic.List<string>(new List<string> {"Screener"}),
                "123456", false);
            Assert.AreEqual(ErrorCode.NO_ERROR, error);


            _screenerOnField3 = Utilities.BuildScreenerAccountOnFieldDTO();
            _screenerOnField3.UserName = "screeneronfield3@office.com";
            error = _userManagementService.CreateUserProfile(ref _screenerOnField3,
                new System.Collections.Generic.List<string>(new List<string> {"Screener"}),
                "123456", false);
            Assert.AreEqual(ErrorCode.NO_ERROR, error);


            _hr = Utilities.BuildHrAccountSample();
            error = _userManagementService.CreateUserProfile(ref _hr,
                new System.Collections.Generic.List<string>(new List<string> {"HR"}),
                "123456", false);
            Assert.AreEqual(ErrorCode.NO_ERROR, error);

            _clientCompany1 = Utilities.BuildClientCompanySample();
            _clientCompany1.AccountManagers = new List<UserProfileDTO> {_accountManager1};
            error = _clientService.CreateClientCompany(ref _clientCompany1);
            Assert.AreEqual(ErrorCode.NO_ERROR, error);

            _client1 = Utilities.BuildClientAccountSample();
            error = _userManagementService.CreateClientProfile(ref _client1, _clientCompany1, "123456", false);
            Assert.AreEqual(ErrorCode.NO_ERROR, error);

            _clientCompany2 = Utilities.BuildClientCompanySample();
            _clientCompany2.ClientCompanyName = "My client company 2";
            _clientCompany2.AccountManagers = new List<UserProfileDTO> {_accountManager2};
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
            _screeningLevelVersion1.TypeOfCheckScreeningLevelVersion =
                Utilities.BuildTypeOfCheckListForScreeningListDTO();
            _screeningLevelVersion1.ScreeningLevelVersionTurnaroundTime = 10;

            error = _clientService.CreateScreeningLevel(_clientContract1, ref _screeningLevel1,
                ref _screeningLevelVersion1);
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

            _highSchoolDTO = Utilities.BuildHighSchoolDTO();
            error = _highSchoolService.CreateOrEditQualificationPlace(ref _highSchoolDTO);
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
            var professionalQualification =
                _unitOfWork.ProfessionalQualificationRepository.Single(
                    u => u.ProfessionalQualificationId == professionalQualificationDTO.ProfessionalQualificationId);

            _commercialCourtDTO = Utilities.BuildCommercialCourtDTO();
            error = _courtLookUpDatabaseService.CreateOrEditQualificationPlace(ref _commercialCourtDTO);
            Assert.AreEqual(ErrorCode.NO_ERROR, error);
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
                _companyDTO,
                _police1DTO,
                _immigrationOfficeDTO,
                _highSchoolDTO,
                _facultyDTO,
                _commercialCourtDTO,
                _certificationPlaceDTO
            };
            error = _qualificationService.SetQualificationPlaces(screeningDTO, qualifications);
            Assert.AreEqual(ErrorCode.NO_ERROR, error);

            return screeningDTO;
        }

        /// <summary>
        /// Create a screening the application, qualify it and process it, used by test only
        /// </summary>
        /// <returns></returns>
        private ScreeningDTO CreateQualifyAndAssignScreeningEqually()
        {
            var error = ErrorCode.NO_ERROR;

            // Create screening
            var screeningDTO = CreateAndQualifyScreening();
            var screening = _unitOfWork.ScreeningRepository.Single(u => u.ScreeningId == screeningDTO.ScreeningId);

            var atomicChecksDTO = _screeningService.GetAllAtomicChecksForScreening(screeningDTO);

            // Assign on field screener to office atomic check
            foreach (var atomicCheckDTO in atomicChecksDTO.Where(
                u => u.AtomicCheckCategory == CVScreeningCore.Models.AtomicCheck.kOfficeCategory))
            {
                var refAtomicCheckTO = atomicCheckDTO;
                error = refAtomicCheckTO.AtomicCheckId%2 == 0
                    ? _screeningService.AssignAtomicCheck(ref refAtomicCheckTO, _screenerOffice1)
                    : _screeningService.AssignAtomicCheck(ref refAtomicCheckTO, _screenerOffice2);

                Assert.AreEqual(ErrorCode.NO_ERROR, error);
            }

            foreach (
                var atomicCheckDTO in
                    atomicChecksDTO.Where(
                        u => u.AtomicCheckCategory == CVScreeningCore.Models.AtomicCheck.kOnFieldCategory))
            {
                var refAtomicCheckTO = atomicCheckDTO;

                error = refAtomicCheckTO.AtomicCheckId%2 == 0
                    ? _screeningService.AssignAtomicCheck(ref refAtomicCheckTO, _screenerOnField1)
                    : _screeningService.AssignAtomicCheck(ref refAtomicCheckTO, _screenerOnField2);

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
        private ScreeningDTO CreateQualifyAndAssignScreeningForScreenerOffice1AndScreenerOnField1()
        {
            var error = ErrorCode.NO_ERROR;

            // Create screening
            var screeningDTO = CreateAndQualifyScreening();
            var screening = _unitOfWork.ScreeningRepository.Single(u => u.ScreeningId == screeningDTO.ScreeningId);

            var atomicChecksDTO = _screeningService.GetAllAtomicChecksForScreening(screeningDTO);

            // Assign on field screener to office atomic check
            foreach (var atomicCheckDTO in atomicChecksDTO.Where(
                u => u.AtomicCheckCategory == CVScreeningCore.Models.AtomicCheck.kOfficeCategory))
            {
                var refAtomicCheckTO = atomicCheckDTO;
                error = _screeningService.AssignAtomicCheck(ref refAtomicCheckTO, _screenerOffice1);


                Assert.AreEqual(ErrorCode.NO_ERROR, error);
            }

            foreach (
                var atomicCheckDTO in
                    atomicChecksDTO.Where(
                        u => u.AtomicCheckCategory == AtomicCheck.kOnFieldCategory))
            {
                var refAtomicCheckTO = atomicCheckDTO;
                error = _screeningService.AssignAtomicCheck(ref refAtomicCheckTO, _screenerOnField1);
                
                Assert.AreEqual(ErrorCode.NO_ERROR, error);
            }

            // Atomic checks are all in the status ON GOING
            Assert.AreEqual(0, screening.GetNewAtomicChecks().Count);
            return screeningDTO;
        }

        #endregion
    }
}