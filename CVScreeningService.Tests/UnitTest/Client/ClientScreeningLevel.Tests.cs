using System.Linq;
using System.Threading;
using CVScreeningCore.Error;
using CVScreeningCore.Models;
using CVScreeningDAL.UnitOfWork;
using CVScreeningService.DTO.Client;
using CVScreeningService.DTO.UserManagement;
using CVScreeningService.Services.Client;
using CVScreeningService.Services.Common;
using CVScreeningService.Services.ErrorHandling;
using CVScreeningService.Services.Notification;
using CVScreeningService.Services.Permission;
using CVScreeningService.Services.Screening;
using CVScreeningService.Services.SystemTime;
using CVScreeningService.Services.UserManagement;
using FakeItEasy;
using NUnit.Framework;
using System.Collections.Generic;
using NUnit.Framework.Constraints;
using System;

namespace CVScreeningService.Tests.UnitTest.Client
{
    [TestFixture]
    public class ClientScreeningLevel
    {
        // 1. Declare global object
        private IUnitOfWork _unitOfWork;
        private IScreeningService _screeningService;
        private IClientService _clientService;
        private IPermissionService _permissionService;
        private ICommonService _commonService;
        private IUserManagementService _userManagementService;
        private ISystemTimeService _systemTimeService;
        private IWebSecurity _webSecurity;
        private IErrorMessageFactoryService _errorMessageFactoryService;
        private UserProfileDTO _admin;
        private UserProfileDTO _accountManager;
        private ClientCompanyDTO _clientCompany;
        private ClientContractDTO _clientContract1;
        private ClientContractDTO _clientContract2;

        // 2. Runs Once Before All of The Following Methods
        // Declare Global Objects Which Are Global For Test Class, e.g. Mock Objects
        [TestFixtureSetUp]
        public void RunsOnceBeforeAll()
        {
            var notificationService = A.Fake<INotificationService>();
            var systemTime = A.Fake<ISystemTimeService>();
            A.CallTo(() => systemTime.GetCurrentDateTime()) .Returns(new DateTime(2014, 12, 1));
            _webSecurity = A.Fake<IWebSecurity>();
            A.CallTo(() => _webSecurity.GetCurrentUserName()).Returns("Administrator");

            _unitOfWork = new InMemoryUnitOfWork();

            _commonService = new CommonService(_unitOfWork);
            _systemTimeService = new SystemTimeService();

            _userManagementService = new UserManagementService(_unitOfWork, _commonService, _systemTimeService);
            _errorMessageFactoryService = new ErrorMessageFactoryService(new ResourceErrorFactory());

            Utilities.InitLocations(_commonService);
            Utilities.InitRoles(_userManagementService);

            // Create sample user account for testing
            _admin = Utilities.BuildAdminAccountSample();
            var error = _userManagementService.CreateUserProfile(ref _admin,
                    new System.Collections.Generic.List<string>(new List<string> { "Administrator" }),
                    "123456", false);

            _permissionService = new PermissionService(_unitOfWork, _admin.UserName);
            _screeningService = new ScreeningService(
                _unitOfWork, _permissionService, _userManagementService, systemTime, _webSecurity, notificationService);

            _clientService = new ClientService(_unitOfWork, _commonService, _userManagementService, _permissionService);
            Utilities.InitTypeOfCheck(_screeningService, _unitOfWork);


            _accountManager = Utilities.BuildAccountSample();
            error = _userManagementService.CreateUserProfile(ref _accountManager,
                    new System.Collections.Generic.List<string>(new List<string> { "Administrator" }),
                    "123456", false);



            _clientCompany = Utilities.BuildClientCompanySample();
            _clientCompany.AccountManagers = new List<UserProfileDTO> { _accountManager };
            error = _clientService.CreateClientCompany(ref _clientCompany);


            _clientContract1 = Utilities.BuildClientContractDTO();
            error = _clientService.CreateClientContract(_clientCompany, ref _clientContract1);

            _clientContract2 = Utilities.BuildClientContractDTO();
            _clientContract2.ContractYear = "2015";
            error = _clientService.CreateClientContract(_clientCompany, ref _clientContract2);


        }


        // 3. Runs Twice; Once Before Test Case 1 and Once Before Test Case 2
        // Declare Objects Which Are Shared Among Tests, e.g. Shared Mocks
        [SetUp]
        public void RunOnceBeforeEachTest()
        {
            IList<ScreeningLevelDTO> screeningLevels = _clientService.GetScreeningLevelsByContract(_clientContract1);
            foreach (var screeningLevel in screeningLevels.Reverse())
            {
                _clientService.DeleteScreeningLevel(_clientContract1, screeningLevel);
            }

            screeningLevels = _clientService.GetScreeningLevelsByContract(_clientContract2);
            foreach (var screeningLevel in screeningLevels.Reverse())
            {
                _clientService.DeleteScreeningLevel(_clientContract2, screeningLevel);
            }
        }

        /// <summary>
        // Test to create a screening level
        /// </summary>
        [Test]
        public void CreateScreeningLevel()
        {
            int count = _unitOfWork.ScreeningLevelRepository.CountAll();

            var screeningLevelDTO = Utilities.BuildScreeningLevelDTO();
            var screeningLevelVersionDTO = Utilities.BuildScreeningLevelVersionDTO();
            screeningLevelVersionDTO.TypeOfCheckScreeningLevelVersion =
                Utilities.BuildTypeOfCheckListForScreeningListDTO();

            var error = _clientService.CreateScreeningLevel(_clientContract1, ref screeningLevelDTO,
               ref screeningLevelVersionDTO);
            Assert.AreEqual(ErrorCode.NO_ERROR, error);
            Assert.AreEqual(count + 1, _unitOfWork.ScreeningLevelRepository.CountAll());

        }

        /// <summary>
        // Test to create a screening level with a name already existing for this contract
        /// </summary>
        [Test]
        public void CreateScreeningLevelAlreadyExisting()
        {
            int count = _unitOfWork.ScreeningLevelRepository.CountAll();

            var screeningLevelDTO = Utilities.BuildScreeningLevelDTO();
            var screeningLevelVersionDTO = Utilities.BuildScreeningLevelVersionDTO();
            screeningLevelVersionDTO.TypeOfCheckScreeningLevelVersion =
                Utilities.BuildTypeOfCheckListForScreeningListDTO();

            var error = _clientService.CreateScreeningLevel(_clientContract1, ref screeningLevelDTO,
               ref screeningLevelVersionDTO);
            Assert.AreEqual(ErrorCode.NO_ERROR, error);
            Assert.AreEqual(count + 1, _unitOfWork.ScreeningLevelRepository.CountAll());

            error = _clientService.CreateScreeningLevel(_clientContract1, ref screeningLevelDTO,
                ref screeningLevelVersionDTO);
            Assert.AreEqual(ErrorCode.SCREENING_LEVEL_ALREADY_EXISTS, error);
            Assert.AreEqual(count + 1, _unitOfWork.ScreeningLevelRepository.CountAll());

        }


        /// <summary>
        // Test to create a screening level with a contract not existing
        /// </summary>
        [Test]
        public void CreateScreeningLevelContractNotExisting()
        {
            int count = _unitOfWork.ScreeningLevelRepository.CountAll();

            var screeningLevelDTO = Utilities.BuildScreeningLevelDTO();
            var screeningLevelVersionDTO = Utilities.BuildScreeningLevelVersionDTO();
            screeningLevelVersionDTO.TypeOfCheckScreeningLevelVersion =
                Utilities.BuildTypeOfCheckListForScreeningListDTO();

            var error = _clientService.CreateScreeningLevel(new ClientContractDTO{ContractId = 20}, ref screeningLevelDTO,
               ref screeningLevelVersionDTO);
            Assert.AreEqual(ErrorCode.CLIENT_COMPANY_CONTRACT_NOT_FOUND, error);
            Assert.AreEqual(count, _unitOfWork.ScreeningLevelRepository.CountAll());

        }


        /// <summary>
        // Test to edit a screening level
        /// </summary>
        [Test]
        public void EditScreeningLevel()
        {
            int count = _unitOfWork.ScreeningLevelRepository.CountAll();

            var screeningLevelDTO = Utilities.BuildScreeningLevelDTO();
            var screeningLevelVersionDTO = Utilities.BuildScreeningLevelVersionDTO();
            screeningLevelVersionDTO.TypeOfCheckScreeningLevelVersion =
                Utilities.BuildTypeOfCheckListForScreeningListDTO();

            var error = _clientService.CreateScreeningLevel(_clientContract1, ref screeningLevelDTO,
               ref screeningLevelVersionDTO);
            Assert.AreEqual(ErrorCode.NO_ERROR, error);
            Assert.AreEqual(count + 1, _unitOfWork.ScreeningLevelRepository.CountAll());

            var screeningLevelDTOEdit = new ScreeningLevelDTO
            {
                ScreeningLevelId = screeningLevelDTO.ScreeningLevelId,
                ScreeningLevelName = "Employee Edit",
                ScreeningLevelVersion = new List<ScreeningLevelVersionDTO>()
            };

            var screeningLevelVersionDTOEdit = new ScreeningLevelVersionDTO
            {
                ScreeningLevelVersionId = screeningLevelVersionDTO.ScreeningLevelVersionId,
                ScreeningLevelVersionDescription = "Employee description Edit",
                ScreeningLevelVersionLanguage = "Bahasa Indonesia Edit",
                ScreeningLevelVersionAllowedToContactCurrentCompany = "Current Company",
                ScreeningLevelVersionStartDate = DateTime.Now.AddDays(10),
                ScreeningLevelVersionEndDate = DateTime.Now.AddDays(20),
                ScreeningLevelVersionTurnaroundTime = 6
            };
            screeningLevelDTOEdit.ScreeningLevelVersion.Add(screeningLevelVersionDTOEdit);
            error = _clientService.EditScreeningLevel(ref screeningLevelDTOEdit, ref screeningLevelVersionDTOEdit);
            Assert.AreEqual(ErrorCode.NO_ERROR, error);
            Assert.AreEqual(count + 1, _unitOfWork.ScreeningLevelRepository.CountAll());

            var screeningLevelDTOGet = _clientService.GetScreeningLevel(screeningLevelDTO.ScreeningLevelId);
            Assert.AreEqual("Employee Edit", screeningLevelDTOGet.ScreeningLevelName);
            Assert.AreEqual("Employee description Edit", screeningLevelDTOGet.ScreeningLevelVersion[0].ScreeningLevelVersionDescription);
            Assert.AreEqual("Bahasa Indonesia Edit", screeningLevelDTOGet.ScreeningLevelVersion[0].ScreeningLevelVersionLanguage);
            Assert.AreEqual(false, screeningLevelDTOGet.ScreeningLevelVersion[0].ScreeningLevelVersionAllowedToContactCandidate);
            Assert.AreEqual(DateTime.Now.AddDays(10).Date, screeningLevelDTOGet.ScreeningLevelVersion[0].ScreeningLevelVersionStartDate);
            Assert.AreEqual(DateTime.Now.AddDays(20).Date, screeningLevelDTOGet.ScreeningLevelVersion[0].ScreeningLevelVersionEndDate);
            Assert.AreEqual(6, screeningLevelDTOGet.ScreeningLevelVersion[0].ScreeningLevelVersionTurnaroundTime);

        }

        /// <summary>
        // Test to edit a screening level that is not existing
        /// </summary>
        [Test]
        public void EditScreeningLevelNotExisting()
        {
            int count = _unitOfWork.ScreeningLevelRepository.CountAll();

            var screeningLevelDTO = Utilities.BuildScreeningLevelDTO();
            var screeningLevelVersionDTO = Utilities.BuildScreeningLevelVersionDTO();
            screeningLevelVersionDTO.TypeOfCheckScreeningLevelVersion =
                Utilities.BuildTypeOfCheckListForScreeningListDTO();

            var error = _clientService.CreateScreeningLevel(_clientContract1, ref screeningLevelDTO,
               ref screeningLevelVersionDTO);
            Assert.AreEqual(ErrorCode.NO_ERROR, error);
            Assert.AreEqual(count + 1, _unitOfWork.ScreeningLevelRepository.CountAll());

            var screeningLevelNotExistingDTO = new ScreeningLevelDTO
            {
                ScreeningLevelId = 20
            };
            error = _clientService.EditScreeningLevel(ref screeningLevelNotExistingDTO, ref screeningLevelVersionDTO);
            Assert.AreEqual(ErrorCode.SCREENING_LEVEL_NOT_FOUND, error);

            var screeningLevelVersionNotExistingDTO = new ScreeningLevelVersionDTO
            {
                ScreeningLevelVersionId = 20
            };
            error = _clientService.EditScreeningLevel(ref screeningLevelDTO, ref screeningLevelVersionNotExistingDTO);
            Assert.AreEqual(ErrorCode.SCREENING_LEVEL_VERSION_NOT_FOUND, error);
        }



        /// <summary>
        // Test to get a screening level by id
        /// </summary>
        [Test]
        public void GetScreeningLevelById()
        {
            int count = _unitOfWork.ScreeningLevelRepository.CountAll();

            var screeningLevelDTO = Utilities.BuildScreeningLevelDTO();
            var screeningLevelVersionDTO = Utilities.BuildScreeningLevelVersionDTO();
            screeningLevelVersionDTO.TypeOfCheckScreeningLevelVersion =
                Utilities.BuildTypeOfCheckListForScreeningListDTO();

            var error = _clientService.CreateScreeningLevel(_clientContract1, ref screeningLevelDTO,
               ref screeningLevelVersionDTO);
            Assert.AreEqual(ErrorCode.NO_ERROR, error);
            Assert.AreEqual(count + 1, _unitOfWork.ScreeningLevelRepository.CountAll());

            var screeningLevelDTOGet = _clientService.GetScreeningLevel(screeningLevelDTO.ScreeningLevelId);
            Assert.AreEqual("Employee", screeningLevelDTOGet.ScreeningLevelName);
            Assert.AreEqual("Employee description", screeningLevelDTOGet.ScreeningLevelVersion[0].ScreeningLevelVersionDescription);
            Assert.AreEqual("Bahasa Indonesia", screeningLevelDTOGet.ScreeningLevelVersion[0].ScreeningLevelVersionLanguage);
            Assert.AreEqual(true, screeningLevelDTOGet.ScreeningLevelVersion[0].ScreeningLevelVersionAllowedToContactCandidate);
            Assert.AreEqual(DateTime.Now.Date, screeningLevelDTOGet.ScreeningLevelVersion[0].ScreeningLevelVersionStartDate);
            Assert.AreEqual(DateTime.Now.AddDays(10).Date, screeningLevelDTOGet.ScreeningLevelVersion[0].ScreeningLevelVersionEndDate);
            Assert.AreEqual(5, screeningLevelDTOGet.ScreeningLevelVersion[0].ScreeningLevelVersionTurnaroundTime);

        }

        /// <summary>
        // Test to get a screening level by id not existing
        /// </summary>
        [Test]
        public void GetScreeningLevelByIdNotExisting()
        {
            var screeningLevelDTO = _clientService.GetScreeningLevel(20);
            Assert.AreEqual(null, screeningLevelDTO);
        }

        /// <summary>
        // Test to get a screening level by contract
        /// </summary>
        [Test]
        public void GetScreeningLevelByContract()
        {
            int count = _unitOfWork.ScreeningLevelRepository.CountAll();

            var screeningLevel1Contract1DTO = Utilities.BuildScreeningLevelDTO();
            var screeningLevelVersion1Contract1DTO = Utilities.BuildScreeningLevelVersionDTO();
            screeningLevelVersion1Contract1DTO.TypeOfCheckScreeningLevelVersion =
                Utilities.BuildTypeOfCheckListForScreeningListDTO();

            var error = _clientService.CreateScreeningLevel(_clientContract1, ref screeningLevel1Contract1DTO,
               ref screeningLevelVersion1Contract1DTO);
            Assert.AreEqual(ErrorCode.NO_ERROR, error);
            Assert.AreEqual(count + 1, _unitOfWork.ScreeningLevelRepository.CountAll());

            var screeningLevel2Contract1DTO = Utilities.BuildScreeningLevelDTO();
            screeningLevel2Contract1DTO.ScreeningLevelName = "Manager";
            var screeningLevelVersion2Contract1DTO = Utilities.BuildScreeningLevelVersionDTO();
            screeningLevelVersion2Contract1DTO.TypeOfCheckScreeningLevelVersion =
                Utilities.BuildTypeOfCheckListForScreeningListDTO();

            error = _clientService.CreateScreeningLevel(_clientContract1, ref screeningLevel2Contract1DTO,
               ref screeningLevelVersion2Contract1DTO);
            Assert.AreEqual(ErrorCode.NO_ERROR, error);
            Assert.AreEqual(count + 2, _unitOfWork.ScreeningLevelRepository.CountAll());

            var screeningLevel1Contract2DTO = Utilities.BuildScreeningLevelDTO();
            var screeningLevelVersion1Contract2DTO = Utilities.BuildScreeningLevelVersionDTO();
            screeningLevelVersion1Contract2DTO.TypeOfCheckScreeningLevelVersion =
                Utilities.BuildTypeOfCheckListForScreeningListDTO();

            screeningLevel1Contract2DTO.ScreeningLevelName = "Employee contract 2";
            error = _clientService.CreateScreeningLevel(_clientContract2, ref screeningLevel1Contract2DTO,
               ref screeningLevelVersion1Contract2DTO);
            Assert.AreEqual(ErrorCode.NO_ERROR, error);
            Assert.AreEqual(count + 3, _unitOfWork.ScreeningLevelRepository.CountAll());

            var screeningLevelsContract1DTO = _clientService.GetScreeningLevelsByContract(_clientContract1);
            Assert.AreEqual(screeningLevelsContract1DTO.Count, 2);

            Assert.AreEqual("Employee", screeningLevelsContract1DTO[0].ScreeningLevelName);
            Assert.AreEqual("Employee description", screeningLevelsContract1DTO[0].ScreeningLevelVersion[0].ScreeningLevelVersionDescription);
            Assert.AreEqual("Bahasa Indonesia", screeningLevelsContract1DTO[0].ScreeningLevelVersion[0].ScreeningLevelVersionLanguage);
            Assert.AreEqual(true, screeningLevelsContract1DTO[0].ScreeningLevelVersion[0].ScreeningLevelVersionAllowedToContactCandidate);
            Assert.AreEqual(DateTime.Now.Date, screeningLevelsContract1DTO[0].ScreeningLevelVersion[0].ScreeningLevelVersionStartDate);
            Assert.AreEqual(DateTime.Now.AddDays(10).Date, screeningLevelsContract1DTO[0].ScreeningLevelVersion[0].ScreeningLevelVersionEndDate);
            Assert.AreEqual(5, screeningLevelsContract1DTO[0].ScreeningLevelVersion[0].ScreeningLevelVersionTurnaroundTime);


            Assert.AreEqual("Manager", screeningLevelsContract1DTO[1].ScreeningLevelName);
            Assert.AreEqual("Employee description", screeningLevelsContract1DTO[1].ScreeningLevelVersion[0].ScreeningLevelVersionDescription);
            Assert.AreEqual("Bahasa Indonesia", screeningLevelsContract1DTO[1].ScreeningLevelVersion[0].ScreeningLevelVersionLanguage);
            Assert.AreEqual(true, screeningLevelsContract1DTO[1].ScreeningLevelVersion[0].ScreeningLevelVersionAllowedToContactCandidate);
            Assert.AreEqual(DateTime.Now.Date, screeningLevelsContract1DTO[1].ScreeningLevelVersion[0].ScreeningLevelVersionStartDate);
            Assert.AreEqual(DateTime.Now.AddDays(10).Date, screeningLevelsContract1DTO[1].ScreeningLevelVersion[0].ScreeningLevelVersionEndDate);
            Assert.AreEqual(5, screeningLevelsContract1DTO[1].ScreeningLevelVersion[0].ScreeningLevelVersionTurnaroundTime);

            var screeningLevelsContract2DTO = _clientService.GetScreeningLevelsByContract(_clientContract2);
            Assert.AreEqual(screeningLevelsContract2DTO.Count, 1);

            Assert.AreEqual("Employee contract 2", screeningLevelsContract2DTO[0].ScreeningLevelName);
            Assert.AreEqual("Employee description", screeningLevelsContract2DTO[0].ScreeningLevelVersion[0].ScreeningLevelVersionDescription);
            Assert.AreEqual("Bahasa Indonesia", screeningLevelsContract2DTO[0].ScreeningLevelVersion[0].ScreeningLevelVersionLanguage);
            Assert.AreEqual(true, screeningLevelsContract2DTO[0].ScreeningLevelVersion[0].ScreeningLevelVersionAllowedToContactCandidate);
            Assert.AreEqual(DateTime.Now.Date, screeningLevelsContract2DTO[0].ScreeningLevelVersion[0].ScreeningLevelVersionStartDate);
            Assert.AreEqual(DateTime.Now.AddDays(10).Date, screeningLevelsContract2DTO[0].ScreeningLevelVersion[0].ScreeningLevelVersionEndDate);
            Assert.AreEqual(5, screeningLevelsContract2DTO[0].ScreeningLevelVersion[0].ScreeningLevelVersionTurnaroundTime);
        }

        /// <summary>
        // Test to get a screening level by contract not existing
        /// </summary>
        [Test]
        public void GetScreeningLevelByContractNotExisting()
        {
            var screeningLevelDTO = _clientService.GetScreeningLevelsByContract(new ClientContractDTO{ContractId = 20});
            Assert.AreEqual(null, screeningLevelDTO);
        }


        /// <summary>
        // Test to get a screening level version by screening level
        /// </summary>
        [Test]
        public void GetScreeningLevelVersionsByScreeningLevel()
        {
            int count = _unitOfWork.ScreeningLevelRepository.CountAll();

            var screeningLevelDTO = Utilities.BuildScreeningLevelDTO();
            var screeningLevelVersion1DTO = Utilities.BuildScreeningLevelVersionDTO();
            screeningLevelVersion1DTO.TypeOfCheckScreeningLevelVersion =
                Utilities.BuildTypeOfCheckListForScreeningListDTO();

            var error = _clientService.CreateScreeningLevel(_clientContract1, ref screeningLevelDTO,
               ref screeningLevelVersion1DTO);
            Assert.AreEqual(ErrorCode.NO_ERROR, error);
            Assert.AreEqual(1, screeningLevelDTO.ScreeningLevelVersion[0].ScreeningLevelVersionNumber);
            Assert.AreEqual(count + 1, _unitOfWork.ScreeningLevelRepository.CountAll());

            screeningLevelVersion1DTO.TypeOfCheckScreeningLevelVersion =
                Utilities.BuildTypeOfCheckList2ForScreeningListDTO();

            screeningLevelVersion1DTO.ScreeningLevelVersionDescription = "Employee description update";
            screeningLevelVersion1DTO.ScreeningLevelVersionLanguage = "Bahasa Prancis";
            screeningLevelVersion1DTO.ScreeningLevelVersionAllowedToContactCurrentCompany = ScreeningLevelVersion.kHR;
            screeningLevelVersion1DTO.ScreeningLevelVersionAllowedToContactCandidate = false;
            screeningLevelVersion1DTO.ScreeningLevelVersionStartDate = DateTime.Now.AddDays(10);
            screeningLevelVersion1DTO.ScreeningLevelVersionEndDate = DateTime.Now.AddDays(20);
            screeningLevelVersion1DTO.ScreeningLevelVersionTurnaroundTime = 3;

            error = _clientService.UpdateScreeningLevel(ref screeningLevelDTO,
                ref screeningLevelVersion1DTO);
            Assert.AreEqual(ErrorCode.NO_ERROR, error);
            Assert.AreEqual(screeningLevelDTO.ScreeningLevelVersion[1].ScreeningLevelVersionNumber, 2);
            Assert.AreEqual(count + 1, _unitOfWork.ScreeningLevelRepository.CountAll());

            var screeningLevelVersionsDTO = _clientService.GetScreeningLevelVersionsByScreeningLevel(screeningLevelDTO);
            Assert.AreEqual(2, screeningLevelVersionsDTO.Count);
            Assert.AreEqual(1, screeningLevelVersionsDTO[0].ScreeningLevelVersionNumber);
            Assert.AreEqual("Employee description", screeningLevelVersionsDTO[0].ScreeningLevelVersionDescription);
            Assert.AreEqual("Bahasa Indonesia", screeningLevelVersionsDTO[0].ScreeningLevelVersionLanguage);
            Assert.AreEqual(true, screeningLevelVersionsDTO[0].ScreeningLevelVersionAllowedToContactCandidate);
            Assert.AreEqual(DateTime.Now.Date, screeningLevelVersionsDTO[0].ScreeningLevelVersionStartDate);
            Assert.AreEqual(DateTime.Now.AddDays(10).Date, screeningLevelVersionsDTO[0].ScreeningLevelVersionEndDate);
            Assert.AreEqual(5, screeningLevelVersionsDTO[0].ScreeningLevelVersionTurnaroundTime);
            Assert.AreEqual(9, screeningLevelVersionsDTO[0].TypeOfCheckScreeningLevelVersion.Count);


            Assert.AreEqual(2, screeningLevelVersionsDTO[1].ScreeningLevelVersionNumber);
            Assert.AreEqual("Employee description update", screeningLevelVersionsDTO[1].ScreeningLevelVersionDescription);
            Assert.AreEqual("Bahasa Prancis", screeningLevelVersionsDTO[1].ScreeningLevelVersionLanguage);
            Assert.AreEqual(false, screeningLevelVersionsDTO[1].ScreeningLevelVersionAllowedToContactCandidate);
            Assert.AreEqual(DateTime.Now.AddDays(10).Date, screeningLevelVersionsDTO[1].ScreeningLevelVersionStartDate);
            Assert.AreEqual(DateTime.Now.AddDays(20).Date, screeningLevelVersionsDTO[1].ScreeningLevelVersionEndDate);
            Assert.AreEqual(3, screeningLevelVersionsDTO[1].ScreeningLevelVersionTurnaroundTime);
            Assert.AreEqual(3, screeningLevelVersionsDTO[1].TypeOfCheckScreeningLevelVersion.Count);

        }

        /// <summary>
        // Test to get a screening level by contract not existing
        /// </summary>
        [Test]
        public void GetScreeningLevelVersionsByScreeningLevelNotExisting()
        {
            var screeningLevelDTO = _clientService.GetScreeningLevelVersionsByScreeningLevel(new ScreeningLevelDTO { ScreeningLevelId= 20 });
            Assert.AreEqual(null, screeningLevelDTO);
        }


        /// <summary>
        // Test to get a screening level version by screening level
        /// </summary>
        [Test]
        public void UpdateScreeningLevel()
        {
            int count = _unitOfWork.ScreeningLevelRepository.CountAll();

            var screeningLevelDTO = Utilities.BuildScreeningLevelDTO();
            var screeningLevelVersion1DTO = Utilities.BuildScreeningLevelVersionDTO();
            screeningLevelVersion1DTO.TypeOfCheckScreeningLevelVersion =
                Utilities.BuildTypeOfCheckListForScreeningListDTO();

            var error = _clientService.CreateScreeningLevel(_clientContract1, ref screeningLevelDTO,
               ref screeningLevelVersion1DTO);
            Assert.AreEqual(ErrorCode.NO_ERROR, error);
            Assert.AreEqual(1, screeningLevelDTO.ScreeningLevelVersion[0].ScreeningLevelVersionNumber);
            Assert.AreEqual(count + 1, _unitOfWork.ScreeningLevelRepository.CountAll());

            screeningLevelVersion1DTO.TypeOfCheckScreeningLevelVersion =
                Utilities.BuildTypeOfCheckList2ForScreeningListDTO();
            screeningLevelVersion1DTO.ScreeningLevelVersionStartDate = DateTime.Now.AddDays(20).Date;
            screeningLevelVersion1DTO.ScreeningLevelVersionEndDate = DateTime.Now.AddDays(30).Date;
            screeningLevelVersion1DTO.ScreeningLevelVersionDescription = "Employee description update";
            error = _clientService.UpdateScreeningLevel(ref screeningLevelDTO,
                ref screeningLevelVersion1DTO);
            Assert.AreEqual(ErrorCode.NO_ERROR, error);
            Assert.AreEqual(screeningLevelDTO.ScreeningLevelVersion[1].ScreeningLevelVersionNumber, 2);
            Assert.AreEqual(count + 1, _unitOfWork.ScreeningLevelRepository.CountAll());

            var screeningLevelVersionsDTO = _clientService.GetScreeningLevelVersionsByScreeningLevel(screeningLevelDTO);
            Assert.AreEqual(2, screeningLevelVersionsDTO.Count);
            Assert.AreEqual(1, screeningLevelVersionsDTO[0].ScreeningLevelVersionNumber);
            Assert.AreEqual(2, screeningLevelVersionsDTO[1].ScreeningLevelVersionNumber);

        }


        /// <summary>
        // Test to upddate a screening level version that is overlapping with a previous one
        /// </summary>
        [Test]
        public void UpdateScreeningLevelOverlapping()
        {
            int count = _unitOfWork.ScreeningLevelVersionRepository.CountAll();

            var screeningLevelDTO = Utilities.BuildScreeningLevelDTO();
            // Start date from now to now + 10
            var screeningLevelVersion1DTO = Utilities.BuildScreeningLevelVersionDTO();
            screeningLevelVersion1DTO.TypeOfCheckScreeningLevelVersion =
                Utilities.BuildTypeOfCheckListForScreeningListDTO();

            var error = _clientService.CreateScreeningLevel(_clientContract1, ref screeningLevelDTO,
               ref screeningLevelVersion1DTO);
            Assert.AreEqual(ErrorCode.NO_ERROR, error);
            Assert.AreEqual(count + 1, _unitOfWork.ScreeningLevelVersionRepository.CountAll());

            screeningLevelVersion1DTO.TypeOfCheckScreeningLevelVersion =
                Utilities.BuildTypeOfCheckList2ForScreeningListDTO();
            // Start date from now + 5 to now + 15
            screeningLevelVersion1DTO.ScreeningLevelVersionStartDate = DateTime.Now.AddDays(5).Date;
            screeningLevelVersion1DTO.ScreeningLevelVersionEndDate = DateTime.Now.AddDays(15).Date;
            error = _clientService.UpdateScreeningLevel(ref screeningLevelDTO,
                ref screeningLevelVersion1DTO);
            Assert.AreEqual(ErrorCode.SCREENING_LEVEL_VERSION_OVERLAPPING, error);
            Assert.AreEqual(count + 1, _unitOfWork.ScreeningLevelVersionRepository.CountAll());

            screeningLevelVersion1DTO.ScreeningLevelVersionStartDate = DateTime.Now.AddDays(-5).Date;
            screeningLevelVersion1DTO.ScreeningLevelVersionEndDate = DateTime.Now.AddDays(15).Date;
            error = _clientService.UpdateScreeningLevel(ref screeningLevelDTO,
                ref screeningLevelVersion1DTO);
            Assert.AreEqual(ErrorCode.SCREENING_LEVEL_VERSION_OVERLAPPING, error);
            Assert.AreEqual(count + 1, _unitOfWork.ScreeningLevelVersionRepository.CountAll());

            screeningLevelVersion1DTO.ScreeningLevelVersionStartDate = DateTime.Now.AddDays(8).Date;
            screeningLevelVersion1DTO.ScreeningLevelVersionEndDate = DateTime.Now.AddDays(12).Date;
            error = _clientService.UpdateScreeningLevel(ref screeningLevelDTO,
                ref screeningLevelVersion1DTO);
            Assert.AreEqual(ErrorCode.SCREENING_LEVEL_VERSION_OVERLAPPING, error);
            Assert.AreEqual(count + 1, _unitOfWork.ScreeningLevelVersionRepository.CountAll());

            screeningLevelVersion1DTO.ScreeningLevelVersionStartDate = DateTime.Now.AddDays(10).Date;
            screeningLevelVersion1DTO.ScreeningLevelVersionEndDate = DateTime.Now.AddDays(20).Date;
            error = _clientService.UpdateScreeningLevel(ref screeningLevelDTO,
                ref screeningLevelVersion1DTO);
            Assert.AreEqual(ErrorCode.NO_ERROR, error);
            Assert.AreEqual(count + 2, _unitOfWork.ScreeningLevelVersionRepository.CountAll());

            screeningLevelVersion1DTO.ScreeningLevelVersionStartDate = DateTime.Now.AddDays(20).Date;
            screeningLevelVersion1DTO.ScreeningLevelVersionEndDate = DateTime.Now.AddDays(30).Date;
            error = _clientService.UpdateScreeningLevel(ref screeningLevelDTO,
                ref screeningLevelVersion1DTO);
            Assert.AreEqual(ErrorCode.NO_ERROR, error);
            Assert.AreEqual(count + 3, _unitOfWork.ScreeningLevelVersionRepository.CountAll());

            screeningLevelVersion1DTO.ScreeningLevelVersionStartDate = DateTime.Now.AddDays(15).Date;
            screeningLevelVersion1DTO.ScreeningLevelVersionEndDate = DateTime.Now.AddDays(30).Date;
            error = _clientService.EditScreeningLevel(ref screeningLevelDTO,
                ref screeningLevelVersion1DTO);
            Assert.AreEqual(ErrorCode.SCREENING_LEVEL_VERSION_OVERLAPPING, error);
            Assert.AreEqual(count + 3, _unitOfWork.ScreeningLevelVersionRepository.CountAll());
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



    }
}

