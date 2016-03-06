using System.Reflection;
using CVScreeningCore.Error;
using CVScreeningDAL.UnitOfWork;
using CVScreeningService.DTO.Client;
using CVScreeningService.DTO.UserManagement;
using CVScreeningService.Services.Client;
using CVScreeningService.Services.Common;
using CVScreeningService.Services.ErrorHandling;
using CVScreeningService.Services.Permission;
using CVScreeningService.Services.SystemTime;
using CVScreeningService.Services.UserManagement;
using CVScreeningService.Tests.UnitTest;
using Nalysa.Common.Log;
using NUnit.Framework;
using System.Collections.Generic;

namespace CVScreeningService.Tests.IntegrationTest.UserManagement
{
    [TestFixture]
    public class ClientAccount
    {
        // 1. Declare global object
        private IUnitOfWork _unitOfWork;
        private ICommonService _commonService;
        private IUserManagementService _userManagementService;
        private IPermissionService _permissionService;
        private ISystemTimeService _systemTimeService;
        private IClientService _clientService;
        private IErrorMessageFactoryService _errorMessageFactoryService;
        private UserProfileDTO _accountManager;
        private ClientCompanyDTO _clientCompany;

        // 2. Runs Once Before All of The Following Methods
        // Declare Global Objects Which Are Global For Test Class, e.g. Mock Objects
        [TestFixtureSetUp]
        public void RunsOnceBeforeAll()
        {

            LogManager.Instance.Error("Is it working or not");

            if (!WebMatrix.WebData.WebSecurity.Initialized)
                WebMatrix.WebData.WebSecurity.InitializeDatabaseConnection("DefaultConnection",
                    "webpages_UserProfile", "UserId", "UserName", true);

            _unitOfWork = new EfUnitOfWork(1);
            _commonService = new CommonService(_unitOfWork);
            _systemTimeService = new SystemTimeService();
            _userManagementService = new UserManagementService(_unitOfWork, _commonService, _systemTimeService);
            _errorMessageFactoryService = new ErrorMessageFactoryService(new ResourceErrorFactory());

            // Create sample user account for testing
            _accountManager = Utilities.BuildAccountSample();

            var error = _userManagementService.CreateUserProfile(ref _accountManager,
                    new System.Collections.Generic.List<string>(new List<string> { "Administrator" }),
                    "123456");
            Assert.AreEqual(ErrorCode.NO_ERROR, error);

            _permissionService = new PermissionService(_unitOfWork, _accountManager.UserName);
            _clientService = new ClientService(_unitOfWork, _commonService, _userManagementService, _permissionService);

            _clientCompany = Utilities.BuildClientCompanySample();
            _clientCompany.AccountManagers = new List<UserProfileDTO> { _accountManager };
            error = _clientService.CreateClientCompany(ref _clientCompany);
            Assert.AreEqual(ErrorCode.NO_ERROR, error);


        }


        // 3. Runs Twice; Once Before Test Case 1 and Once Before Test Case 2
        // Declare Objects Which Are Shared Among Tests, e.g. Shared Mocks
        [SetUp]
        public void RunOnceBeforeEachTest()
        {

        }


        /// <summary>
        // Test to create a new client account
        /// </summary>
        [Test]
        public void CreateClientAccount()
        {
            var count = _unitOfWork.UserProfileRepository.CountAll();
            var userProfileDTO = Utilities.BuildClientAccountSample();

            var error = _userManagementService.CreateClientProfile(ref userProfileDTO, _clientCompany, "123456");
            Assert.AreEqual(ErrorCode.NO_ERROR, error);
            Assert.AreEqual(_unitOfWork.UserProfileRepository.CountAll(), count + 1);

            error = _userManagementService.DeleteClientProfile(userProfileDTO);
            Assert.AreEqual(ErrorCode.NO_ERROR, error);
        }



        /// <summary>
        // Test to create a new client account with an username (email) already existing
        /// </summary>
        [Test]
        public void CreateClientAccountAlreadyExisting()
        {
            var count = _unitOfWork.UserProfileRepository.CountAll();
            var userProfileDTO1 = Utilities.BuildClientAccountSample();

            var error = _userManagementService.CreateClientProfile(ref userProfileDTO1, _clientCompany  ,"123456");
            Assert.AreEqual(ErrorCode.NO_ERROR, error);

            var userProfileDTO2 = Utilities.BuildClientAccountSample();
            error = _userManagementService.CreateClientProfile(ref userProfileDTO2, _clientCompany,  "123456");
            Assert.AreEqual(ErrorCode.ACCOUNT_EMAIL_ALREADY_EXISTS, error);

            Assert.AreEqual(_unitOfWork.UserProfileRepository.CountAll(), count + 1);
            error = _userManagementService.DeleteClientProfile(userProfileDTO1);
            Assert.AreEqual(ErrorCode.NO_ERROR, error);
        }

        
        /// <summary>
        /// Test to edit an existing client account
        /// </summary>
        [Test]
        public void EditClientAccount()
        {
            var userProfileDTO = Utilities.BuildClientAccountSample();

            var error = _userManagementService.CreateClientProfile(ref userProfileDTO, _clientCompany, "123456");
            Assert.AreEqual(ErrorCode.NO_ERROR, error);


            userProfileDTO.FullName = "My fullname test edit";
            userProfileDTO.Remarks = "My remarks edit";
            userProfileDTO.ContactInfo.MobilePhoneNumber = "622199988000";
            userProfileDTO.ContactInfo.WorkPhoneNumber = "622199988000";

            error = _userManagementService.EditClientProfile(ref userProfileDTO);
            Assert.AreEqual(ErrorCode.NO_ERROR, error);

            var userProfileEditDTO = _userManagementService.GetUserProfileById(userProfileDTO.UserId);
            Assert.AreNotEqual(userProfileEditDTO, null);

            Assert.AreEqual(userProfileEditDTO.FullName, userProfileDTO.FullName);
            Assert.AreEqual(userProfileEditDTO.Remarks, userProfileDTO.Remarks);
            Assert.AreEqual(userProfileEditDTO.ContactInfo.MobilePhoneNumber, userProfileDTO.ContactInfo.MobilePhoneNumber);
            Assert.AreEqual(userProfileEditDTO.ContactInfo.WorkPhoneNumber, userProfileDTO.ContactInfo.WorkPhoneNumber);

            error = _userManagementService.DeleteClientProfile(userProfileDTO);
            Assert.AreEqual(ErrorCode.NO_ERROR, error);

        }

  

        /// <summary>
        // Test to edit a client accout that is not existing
        /// </summary>
        [Test]
        public void EditAccountNotFound()
        {
            var userProfileDTO = new UserProfileDTO
            {
                UserId = 9999
            };

            var error = _userManagementService.EditClientProfile(ref userProfileDTO);
            Assert.AreEqual(ErrorCode.ACCOUNT_USERID_NOT_FOUND, error);
        }

        
        /// <summary>
        // Test to delete a client account
        /// </summary>
        [Test]
        public void DeleteClientAccount()
        {
            var count = _unitOfWork.UserProfileRepository.CountAll();
            var userProfileDTO = Utilities.BuildClientAccountSample();

            var error = _userManagementService.CreateClientProfile(ref userProfileDTO, _clientCompany, "123456");
            Assert.AreEqual(ErrorCode.NO_ERROR, error);
            Assert.AreEqual(_unitOfWork.UserProfileRepository.CountAll(), count + 1);

            error = _userManagementService.DeleteClientProfile(userProfileDTO);
            Assert.AreEqual(ErrorCode.NO_ERROR, error);
            Assert.AreEqual(_unitOfWork.UserProfileRepository.CountAll(), count);
        }

        /// <summary>
        // Test to delete an client  account not found
        /// </summary>
        [Test]
        public void DeleteClientAccountNotFound()
        {
            var userProfileDTO = new UserProfileDTO
            {
                UserId = 9999
            };

            var error = _userManagementService.DeleteClientProfile(userProfileDTO);
            Assert.AreEqual(ErrorCode.ACCOUNT_USERID_NOT_FOUND, error);
        }

        /// <summary>
        // Test to get a client account by id
        /// </summary>
        [Test]
        public void DetailsAccountById()
        {
            var userProfileDTO = Utilities.BuildClientAccountSample();

            var error = _userManagementService.CreateClientProfile(ref userProfileDTO, _clientCompany, "123456");
            Assert.AreEqual(ErrorCode.NO_ERROR, error);

            var userProfileDetailsDTO = _userManagementService.GetClientProfileById(userProfileDTO.UserId);
            Assert.AreNotEqual(userProfileDetailsDTO, null);

            Assert.AreEqual(userProfileDetailsDTO.FullName, userProfileDTO.FullName);
            Assert.AreEqual(userProfileDetailsDTO.Remarks, userProfileDTO.Remarks);
            Assert.AreEqual(userProfileDetailsDTO.ContactInfo.MobilePhoneNumber, userProfileDTO.ContactInfo.MobilePhoneNumber);
            Assert.AreEqual(userProfileDetailsDTO.ContactInfo.WorkPhoneNumber, userProfileDTO.ContactInfo.WorkPhoneNumber);

            error = _userManagementService.DeleteClientProfile(userProfileDetailsDTO);
            Assert.AreEqual(ErrorCode.NO_ERROR, error);
        }


        /// <summary>
        // Test to get a client account
        /// </summary>
        [Test]
        public void DetailsClientAccountByName()
        {
            var userProfileDTO = Utilities.BuildClientAccountSample();

            var error = _userManagementService.CreateClientProfile(ref userProfileDTO, _clientCompany, "123456");
            Assert.AreEqual(ErrorCode.NO_ERROR, error);

            var userProfileDetailsDTO = _userManagementService.GetClientProfilebyName(userProfileDTO.UserName);
            Assert.AreNotEqual(userProfileDetailsDTO, null);

            Assert.AreEqual(userProfileDetailsDTO.FullName, userProfileDTO.FullName);
            Assert.AreEqual(userProfileDetailsDTO.Remarks, userProfileDTO.Remarks);
            Assert.AreEqual(userProfileDetailsDTO.ContactInfo.MobilePhoneNumber, userProfileDTO.ContactInfo.MobilePhoneNumber);
            Assert.AreEqual(userProfileDetailsDTO.ContactInfo.WorkPhoneNumber, userProfileDTO.ContactInfo.WorkPhoneNumber);

            error = _userManagementService.DeleteClientProfile(userProfileDetailsDTO);
            Assert.AreEqual(ErrorCode.NO_ERROR, error);
        }


        /// <summary>
        // Test to get a client account not found by id
        /// </summary>
        [Test]
        public void DetailsClientAccountByIdNotFound()
        {
            var userProfile = _userManagementService.GetClientProfileById(9999);
            Assert.AreEqual(userProfile, null);
        }

        /// <summary>
        // Test to get an user account not found by username
        /// </summary>
        [Test]
        public void DetailsClientAccountByNameNotFound()
        {
            var userProfile = _userManagementService.GetClientProfilebyName("mybadname");
            Assert.AreEqual(userProfile, null);
        }
        

        /// <summary>
        // Test to get all user account
        /// </summary>
        [Test]
        public void GetAllClientAccounts()
        {
            var userProfileDTO1 = Utilities.BuildClientAccountSample();
            var userProfileDTO2 = Utilities.BuildClientAccountSample();
            userProfileDTO2.UserName = "myclient2@email.com";

            var userProfiles = _userManagementService.GetAllClientProfiles();
            var count = userProfiles.Count;

            var error = _userManagementService.CreateClientProfile(ref userProfileDTO1, _clientCompany, "123456");
            Assert.AreEqual(ErrorCode.NO_ERROR, error);

            userProfiles = _userManagementService.GetAllClientProfiles();
            Assert.AreEqual(count + 1, userProfiles.Count);

            error = _userManagementService.CreateClientProfile(ref userProfileDTO2, _clientCompany, "123456");
            Assert.AreEqual(ErrorCode.NO_ERROR, error);

            userProfiles = _userManagementService.GetAllClientProfiles();
            Assert.AreEqual(count + 2, userProfiles.Count);

            error = _userManagementService.DeleteClientProfile(userProfileDTO1);
            Assert.AreEqual(ErrorCode.NO_ERROR, error);
            error = _userManagementService.DeleteClientProfile(userProfileDTO2);
            Assert.AreEqual(ErrorCode.NO_ERROR, error);

            userProfiles = _userManagementService.GetAllClientProfiles();
            Assert.AreEqual(count, userProfiles.Count);

        }
        

        /// <summary>
        // Test to deactivate a client account
        /// </summary>
        [Test]
        public void DeactivateClientAccount()
        {
            var userProfileDTO = Utilities.BuildClientAccountSample();

            var error = _userManagementService.CreateClientProfile(ref userProfileDTO, _clientCompany, "123456");
            Assert.AreEqual(ErrorCode.NO_ERROR, error);

            var userProfileDetailsDTO = _userManagementService.GetClientProfilebyName(userProfileDTO.UserName);
            Assert.AreEqual(userProfileDetailsDTO.UserIsDeactivated, false);

            error = _userManagementService.DeactivateClientProfileByName(userProfileDTO.UserName);
            Assert.AreEqual(ErrorCode.NO_ERROR, error);

            userProfileDetailsDTO = _userManagementService.GetClientProfilebyName(userProfileDTO.UserName);
            Assert.AreEqual(userProfileDetailsDTO.UserIsDeactivated, true);

            error = _userManagementService.DeleteClientProfile(userProfileDTO);
            Assert.AreEqual(ErrorCode.NO_ERROR, error);
        }

        /// <summary>
        // Test to deactivate a client account not found by username
        /// </summary>
        [Test]
        public void DeactivateClientAccountNotFound()
        {
            var error = _userManagementService.DeactivateClientProfileByName("mybadname");
            Assert.AreEqual(error, ErrorCode.ACCOUNT_USERNAME_NOT_FOUND);
        }


        /// <summary>
        // Test to activate a client account
        /// </summary>
        [Test]
        public void ActivateClientAccount()
        {
            var userProfileDTO = Utilities.BuildClientAccountSample();

            var error = _userManagementService.CreateClientProfile(ref userProfileDTO, _clientCompany, "123456");
            Assert.AreEqual(ErrorCode.NO_ERROR, error);

            var userProfileDetailsDTO = _userManagementService.GetClientProfilebyName(userProfileDTO.UserName);
            Assert.AreEqual(userProfileDetailsDTO.UserIsDeactivated, false);

            error = _userManagementService.DeactivateClientProfileByName(userProfileDTO.UserName);
            Assert.AreEqual(ErrorCode.NO_ERROR, error);

            userProfileDetailsDTO = _userManagementService.GetClientProfilebyName(userProfileDTO.UserName);
            Assert.AreEqual(userProfileDetailsDTO.UserIsDeactivated, true);

            error = _userManagementService.ReactivateClientProfileByName(userProfileDTO.UserName);
            Assert.AreEqual(ErrorCode.NO_ERROR, error);

            userProfileDetailsDTO = _userManagementService.GetClientProfilebyName(userProfileDTO.UserName);
            Assert.AreEqual(userProfileDetailsDTO.UserIsDeactivated, false);

            error = _userManagementService.DeleteClientProfile(userProfileDTO);
            Assert.AreEqual(ErrorCode.NO_ERROR, error);
        }

        /// <summary>
        // Test to deactivate an account not found by username
        /// </summary>
        [Test]
        public void ActivateAccountNotFound()
        {
            var error = _userManagementService.ReactivateClientProfileByName("mybadname");
            Assert.AreEqual(error, ErrorCode.ACCOUNT_USERNAME_NOT_FOUND);
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
            var error = _userManagementService.DeleteUserProfile(_accountManager);
            Assert.AreEqual(ErrorCode.NO_ERROR, error);
            error = _clientService.DeleteClientCompany(_clientCompany.ClientCompanyId);
            Assert.AreEqual(ErrorCode.NO_ERROR, error);
        }



    }
}

