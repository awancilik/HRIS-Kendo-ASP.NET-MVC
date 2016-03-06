using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using CVScreeningCore.Error;
using CVScreeningDAL.UnitOfWork;
using CVScreeningService.DTO.Client;
using CVScreeningService.DTO.Screening;
using CVScreeningService.DTO.UserManagement;
using CVScreeningService.Services.Client;
using CVScreeningService.Services.Common;
using CVScreeningService.Services.Notification;
using CVScreeningService.Services.Permission;
using CVScreeningService.Services.Screening;
using CVScreeningService.Services.SystemTime;
using CVScreeningService.Services.UserManagement;
using FakeItEasy;
using NUnit.Framework;

namespace CVScreeningService.Tests.UnitTest.Screening
{
    [TestFixture]
    public class Screening
    {
        #region NUnit Test Template

        // 1. Declare global object
        private IUnitOfWork _unitOfWork;
        private ICommonService _commonService;
        private IScreeningService _screeningService;
        private IClientService _clientService;
        private IUserManagementService _userManagementService;
        private IPermissionService _permissionService;
        private ISystemTimeService _systemTimeService;
        private IWebSecurity _webSecurity;

        private UserProfileDTO _admin;
        private UserProfileDTO _accountManager;
        private UserProfileDTO _qualityControl;
        private ClientCompanyDTO _clientCompany;
        private ClientContractDTO _clientContract;
        private ScreeningLevelDTO _screeningLevelDTO;
        private ScreeningLevelVersionDTO _screeningLevelVersion1DTO;
        private ScreeningLevelVersionDTO _screeningLevelVersion2DTO;


        // 2. Runs Once Before All of The Following Methods
        // Declare Global Objects Which Are Global For Test Class, e.g. Mock Objects
        [TestFixtureSetUp]
        public void RunsOnceBeforeAll()
        {
            var notificationService = A.Fake<INotificationService>();
            _systemTimeService = A.Fake<ISystemTimeService>();
            A.CallTo(() => _systemTimeService.GetCurrentDateTime()).Returns(new DateTime(2014, 12, 1));
            _webSecurity = A.Fake<IWebSecurity>();

            _unitOfWork = new InMemoryUnitOfWork();
            _commonService = new CommonService(_unitOfWork);
            _userManagementService = new UserManagementService(_unitOfWork, _commonService, _systemTimeService);
            _clientService = new ClientService(_unitOfWork, _commonService, _userManagementService, _permissionService);

            Utilities.InitLocations(_commonService);
            Utilities.InitRoles(_userManagementService);

            _admin = Utilities.BuildAdminAccountSample();
            var error = _userManagementService.CreateUserProfile(ref _admin,
                    new System.Collections.Generic.List<string>(new List<string> { "Administrator" }),
                    "123456", false);

            _permissionService = new PermissionService(_unitOfWork, _admin.UserName);
            A.CallTo(() => _webSecurity.GetCurrentUserName()).Returns("admin@admin.com");
            _screeningService = new ScreeningService(
                _unitOfWork, _permissionService, _userManagementService, _systemTimeService, _webSecurity, notificationService);

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
            Utilities.InitRoles(_userManagementService);
            Utilities.InitTypeOfCheck(_screeningService, _unitOfWork);

            // Create sample user account for testing
            _accountManager = Utilities.BuildAccountSample();

            var error = _userManagementService.CreateUserProfile(ref _accountManager,
                    new List<string>(new List<string> { "Account manager" }),
                    "123456", false);
            Assert.AreEqual(ErrorCode.NO_ERROR, error);

            _qualityControl = Utilities.BuildQualifierAccountSample();

            error = _userManagementService.CreateUserProfile(ref _qualityControl,
                    new List<string>(new List<string> { "Quality control" }),
                    "123456", false);
            Assert.AreEqual(ErrorCode.NO_ERROR, error);

            _clientCompany = Utilities.BuildClientCompanySample();
            _clientCompany.AccountManagers = new List<UserProfileDTO> { _accountManager };
            error = _clientService.CreateClientCompany(ref _clientCompany);
            Assert.AreEqual(ErrorCode.NO_ERROR, error);

            _clientContract = Utilities.BuildClientContractDTO();
            _clientContract.ClientCompany = _clientCompany;
            error = _clientService.CreateClientContract(_clientCompany, ref _clientContract);
            Assert.AreEqual(ErrorCode.NO_ERROR, error);

            _screeningLevelDTO = Utilities.BuildScreeningLevelDTO();
            _screeningLevelDTO.Contract = _clientContract;

            _screeningLevelVersion1DTO = Utilities.BuildScreeningLevelVersionDTO();
            _screeningLevelVersion1DTO.TypeOfCheckScreeningLevelVersion = Utilities.BuildTypeOfCheckListForScreeningListDTO();
            _screeningLevelVersion1DTO.ScreeningLevelVersionTurnaroundTime = 10;
            _screeningLevelVersion1DTO.ScreeningLevel = _screeningLevelDTO;
            
            error = _clientService.CreateScreeningLevel(_clientContract, ref _screeningLevelDTO, ref _screeningLevelVersion1DTO);
            Assert.AreEqual(ErrorCode.NO_ERROR, error);

            _screeningLevelVersion2DTO = Utilities.BuildScreeningLevelVersionDTO();
            _screeningLevelVersion2DTO.TypeOfCheckScreeningLevelVersion = Utilities.BuildTypeOfCheckListForScreeningListDTO();
            _screeningLevelVersion2DTO.ScreeningLevelVersionTurnaroundTime = 20;
            _screeningLevelVersion2DTO.ScreeningLevelVersionStartDate = _screeningLevelVersion1DTO.ScreeningLevelVersionStartDate.AddDays(20).Date;
            _screeningLevelVersion2DTO.ScreeningLevelVersionEndDate = _screeningLevelVersion1DTO.ScreeningLevelVersionStartDate.AddDays(30);

            error = _clientService.UpdateScreeningLevel(ref _screeningLevelDTO, ref _screeningLevelVersion2DTO);
            Assert.AreEqual(ErrorCode.NO_ERROR, error);
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

        [Test]
        public void DeactivateScreening()
        {
            
        }

        [Test]
        public void CreateScreening()
        {
            var insertedScreening = new ScreeningDTO
            {
                ScreeningFullName = "Screening 4 Full Name",
                ScreeningLevelVersion = _screeningLevelVersion1DTO,
                QualityControl = _qualityControl,
                Attachment = new Collection<AttachmentDTO>
                {
                    new AttachmentDTO
                    {
                        AttachmentId = 1,
                        AttachmentName = "My File 1"
                    }
                }
            };
            insertedScreening.ScreeningLevelVersion.ScreeningLevel.Contract.ClientCompany = _clientCompany;
            
            var errorCode = _screeningService.CreateScreening(insertedScreening.ScreeningLevelVersion, ref insertedScreening);
            Assert.AreEqual(ErrorCode.NO_ERROR, errorCode);

            //get the latest inserted object from the repository
            var actualScreening = _unitOfWork.ScreeningRepository.First(
                e => e.ScreeningId == insertedScreening.ScreeningId);

            Assert.AreNotEqual(null, actualScreening);
            Assert.AreEqual(insertedScreening.ScreeningFullName, actualScreening.ScreeningFullName);
            Assert.AreEqual(insertedScreening.ScreeningLevelVersion.ScreeningLevelVersionTurnaroundTime,
                actualScreening.ScreeningLevelVersion.ScreeningLevelVersionTurnaroundTime);
            Assert.AreEqual(insertedScreening.Attachment.ToArray()[0].AttachmentName, actualScreening.Attachment.ToArray()[0].AttachmentName);
        }



        [Test]
        public void GetAllScreenings()
        {
            // IEnumerable does not support count
            var screenings = _screeningService.GetAllScreenings();
            Assert.AreNotEqual(null, screenings);
        }

        [Test]
        public void GetScreening()
        {
            var actualScreeningDTO = _screeningService.GetScreening(1);
            var expectedScreeningDTO = new ScreeningDTO
            {
                ScreeningFullName = "Screening 4 Full Name",
                QualityControl = _qualityControl,
                ScreeningLevelVersion = _screeningLevelVersion1DTO,
                Attachment = new Collection<AttachmentDTO>()
                {
                    new AttachmentDTO()
                    {
                        AttachmentName = "My File 1"
                    }
                }
            };

            Assert.AreNotEqual(null, actualScreeningDTO);
            Assert.AreEqual(expectedScreeningDTO.ScreeningFullName, actualScreeningDTO.ScreeningFullName);
            Assert.AreEqual(expectedScreeningDTO.QualityControl.UserName, actualScreeningDTO.QualityControl.UserName);
            Assert.AreEqual(expectedScreeningDTO.Attachment.Count, actualScreeningDTO.Attachment.Count);
            Assert.AreEqual(expectedScreeningDTO.Attachment.ToArray()[0].AttachmentName,
                actualScreeningDTO.Attachment.ToArray()[0].AttachmentName);
            Assert.AreEqual(expectedScreeningDTO.ScreeningLevelVersion.ScreeningLevelVersionTurnaroundTime, 
                actualScreeningDTO.ScreeningLevelVersion.ScreeningLevelVersionTurnaroundTime);
            Assert.AreEqual("INT/TLD/ES/2014/INT-BC-1", actualScreeningDTO.ScreeningReference);
        }
    }
}