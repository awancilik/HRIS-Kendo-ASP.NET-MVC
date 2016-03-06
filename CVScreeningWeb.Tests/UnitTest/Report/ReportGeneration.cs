using System;
using System.Collections.Generic;
using System.Linq;
using CVScreeningCore.Error;
using CVScreeningCore.Models;
using CVScreeningCore.Models.AtomicCheckState;
using CVScreeningDAL.UnitOfWork;
using CVScreeningService.DTO.Client;
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
using CVScreeningService.Tests.UnitTest;
using CVScreeningWeb.Helpers;
using FakeItEasy;
using NUnit.Framework;


namespace CVScreeningWeb.Tests.UnitTest.Report
{
    [TestFixture]
    public class ReportGeneration
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
        private ILookUpDatabaseService<DrivingLicenseOfficeDTO> _drivingLicenseOfficeService;
        private IWebSecurity _webSecurity;

        private UserProfileDTO _admin;
        private UserProfileDTO _accountManager;
        private UserProfileDTO _qualityControl;
        private ClientCompanyDTO _clientCompany;
        private ClientContractDTO _clientContract;
        private ScreeningLevelDTO _screeningLevelDTO;
        private ScreeningLevelVersionDTO _screeningLevelVersionDTO;
        private ScreeningDTO _screeningDTO;
        private ImmigrationOfficeDTO _immigrationOfficeDTO;
        private PoliceDTO _police1DTO;
        private PoliceDTO _police2DTO;
        private CompanyDTO _companyDTO;
        private ScreeningQualificationDTO _qualificationBaseDTO;
        private HighSchoolDTO _highSchoolDTO;
        private FacultyDTO _facultyDTO;
        private IEnumerable<BaseQualificationPlaceDTO> _qualificationPlacesDTO;


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
            _screeningService = new ScreeningService(_unitOfWork, _permissionService, _userManagementService, systemTime, _webSecurity, notificationService);
            _clientService = new ClientService(_unitOfWork, _commonService, _userManagementService, null);
            _policeService = new PoliceLookUpDatabaseService(_unitOfWork, _factory);
            _immigrationOfficeService = new ImmigrationOfficeLookUpDatabaseService(_unitOfWork, _factory);
            _companyService = new CompanyLookUpDatabaseService(_unitOfWork, _factory);
            _highSchoolService = new HighSchoolLookUpDatabaseService(_unitOfWork, _factory);
            _facultyService = new FacultyLookUpDatabaseService(_unitOfWork, _factory);
            _drivingLicenseOfficeService = new DrivingLicenseOfficeLookUpDatabaseService(_unitOfWork, _factory);
            _qualificationService = new QualificationService(_unitOfWork, _commonService);

            _admin = Utilities.BuildAdminAccountSample();
            var error = _userManagementService.CreateUserProfile(ref _admin,
                    new System.Collections.Generic.List<string>(new List<string> { "Administrator" }),
                    "123456", false);
            _permissionService = new PermissionService(_unitOfWork, _admin.UserName);

            InitializeTest();

        }

        // 3. Runs Twice; Once Before Test Case 1 and Once Before Test Case 2
        // Declare Objects Which Are Shared Among Tests, e.g. Shared Mocks
        [SetUp]
        public void RunOnceBeforeEachTest()
        {
            //InitializeScreening();
        }


        private void InitializeTest()
        {
            Utilities.InitLocations(_commonService);
            Utilities.InitTypeOfCheck(_screeningService, _unitOfWork);

            // Create sample user account for testing
            _accountManager = Utilities.BuildAccountSample();
            var error = _userManagementService.CreateUserProfile(ref _accountManager,
                    new List<string>(new List<string> { "Account manager" }),
                    "123456", false);
            Assert.AreEqual(ErrorCode.NO_ERROR, error);

            _qualityControl = Utilities.BuildAccountSample();
            _qualityControl.UserName = "quality@control.com";
            error = _userManagementService.CreateUserProfile(ref _qualityControl,
                    new System.Collections.Generic.List<string>(new List<string> { "Quality control" }),
                    "123456", false);
            Assert.AreEqual(ErrorCode.NO_ERROR, error);

            _clientCompany = Utilities.BuildClientCompanySample();
            _clientCompany.AccountManagers = new List<UserProfileDTO> { _accountManager };
            error = _clientService.CreateClientCompany(ref _clientCompany);
            Assert.AreEqual(ErrorCode.NO_ERROR, error);

            _clientContract = Utilities.BuildClientContractDTO();
            error = _clientService.CreateClientContract(_clientCompany, ref _clientContract);
            Assert.AreEqual(ErrorCode.NO_ERROR, error);

            _screeningLevelDTO = Utilities.BuildScreeningLevelDTO();

            _screeningLevelVersionDTO = Utilities.BuildScreeningLevelVersionDTO();
            _screeningLevelVersionDTO.TypeOfCheckScreeningLevelVersion = Utilities.BuildTypeOfCheckListForScreeningListDTO();
            _screeningLevelVersionDTO.ScreeningLevelVersionTurnaroundTime = 10;

            error = _clientService.CreateScreeningLevel(_clientContract, ref _screeningLevelDTO, ref _screeningLevelVersionDTO);
            Assert.AreEqual(ErrorCode.NO_ERROR, error);

            _screeningDTO = Utilities.BuildScreeningDTO(_screeningLevelVersionDTO, _qualityControl);
            error = _screeningService.CreateScreening(_screeningDTO.ScreeningLevelVersion, ref _screeningDTO);
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

            _qualificationBaseDTO = Utilities.BuildScreeningQualificationDTO();
            _qualificationBaseDTO.CurrentAddress = _commonService.GetAllAddresses().ToArray()[0];
            _qualificationBaseDTO.CVAddress = _commonService.GetAllAddresses().ToArray()[0];
            _qualificationBaseDTO.IDCardAddress = _commonService.GetAllAddresses().ToArray()[0];
            _screeningDTO.ScreeningQualification = _qualificationBaseDTO;
            error = _qualificationService.SetQualificationBase(_screeningDTO, ref _qualificationBaseDTO);
            Assert.AreEqual(ErrorCode.NO_ERROR, error);

            _qualificationPlacesDTO = new List<BaseQualificationPlaceDTO> { _police1DTO, _immigrationOfficeDTO, _companyDTO };
            error = _qualificationService.SetQualificationPlaces(_screeningDTO, _qualificationPlacesDTO);
            Assert.AreEqual(ErrorCode.NO_ERROR, error);


            _screeningDTO.AtomicCheck.ToArray()[0].Attachment.Add(new AttachmentDTO
            {
                AttachmentName = "asasa",
                AttachmentCreatedDate = DateTime.Now,
                AttachmentFilePath = "asokaos",
                AttachmentFileType = "asas"
            });
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
        /// <summary>
        /// Make sure title is shown
        /// </summary>
        [Test]
        public void Report_Should_Contain_Title()
        {
            var viewModels = ReportHelper.GenerateReportViewModel(_screeningDTO);
            Assert.AreNotEqual(null, viewModels.Title);
        }

        /// <summary>
        /// Make sure atomic check which has attachment is shown in appendix
        /// </summary>
        [Test]
        public void Report_Should_Contain_Appendix()
        {
            var viewModels = ReportHelper.GenerateReportViewModel(_screeningDTO);
            Assert.Greater(viewModels.Appendices.Count(), 0);
        }

        /// <summary>
        /// Make sure that summary is shown
        /// </summary>
        [Test]
        public void Report_Should_Contain_Summary()
        {
            var viewModels = ReportHelper.GenerateReportViewModel(_screeningDTO);
            Assert.AreNotEqual(null, viewModels.Summary);
        }

        /// <summary>
        /// Make sure that the number of type of check based on above Dummy Data is right
        /// </summary>
        [Test]
        public void Report_Should_Contain_TypeOfChecks()
        {
            var viewModels = ReportHelper.GenerateReportViewModel(_screeningDTO);
            Assert.AreEqual(8, viewModels.TypeOfCheckReports.Count());
        }

        [Test]
        public void Wrongly_Qualified_Atomic_Check_Should_Not_Be_Displayed()
        {
            _screeningDTO.AtomicCheck.ToArray()[0].AtomicCheckState = (byte)AtomicCheckStateType.WRONGLY_QUALIFIED;
            var viewModels = ReportHelper.GenerateReportViewModel(_screeningDTO);

            Assert.AreEqual(0, viewModels.Summary.TypeOfChecks.ToArray()[0].Verifications.Count());
        }

        [Test]
        public void Status_Of_Atomic_Check_Should_Be_Displayed_In_Right_Color()
        {
            // Check NEW state
            _screeningDTO.AtomicCheck.ToArray()[0].AtomicCheckState = (byte) AtomicCheckStateType.NEW;
            var viewModels = ReportHelper.GenerateReportViewModel(_screeningDTO);
            
            var color = viewModels.Summary.TypeOfChecks.ToArray()[0].Verifications.ToArray()[0].Status;
            Assert.AreEqual(AtomicCheckStateType.NEW.ToString(), color);
            

            // Check ON GOING state
            _screeningDTO.AtomicCheck.ToArray()[0].AtomicCheckState = (byte)AtomicCheckStateType.ON_GOING;
            viewModels = ReportHelper.GenerateReportViewModel(_screeningDTO);

            color = viewModels.Summary.TypeOfChecks.ToArray()[0].Verifications.ToArray()[0].Status;
            Assert.AreEqual(AtomicCheckStateType.ON_GOING.ToString(), color);

        }

    }
}