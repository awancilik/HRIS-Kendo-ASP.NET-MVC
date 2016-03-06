using CVScreeningCore.Error;
using CVScreeningDAL.UnitOfWork;
using CVScreeningService.DTO.UserManagement;
using CVScreeningService.Services.Common;
using CVScreeningService.Services.ErrorHandling;
using CVScreeningService.Services.SystemTime;
using CVScreeningService.Services.UserManagement;
using CVScreeningService.Tests.UnitTest;
using NUnit.Framework;

namespace CVScreeningService.Tests.IntegrationTest.UserManagement
{
    [TestFixture]
    public class Account
    {
        // 1. Declare global object
        private IUnitOfWork _unitOfWork;
        private ICommonService _commonService;
        private IUserManagementService _userManagementService;
        private ISystemTimeService _systemTimeService;
        private IErrorMessageFactoryService _errorMessageFactoryService;

        // 2. Runs Once Before All of The Following Methods
        // Declare Global Objects Which Are Global For Test Class, e.g. Mock Objects
        [TestFixtureSetUp]
        public void RunsOnceBeforeAll()
        {
            if (!WebMatrix.WebData.WebSecurity.Initialized)
                WebMatrix.WebData.WebSecurity.InitializeDatabaseConnection("DefaultConnection",
                    "webpages_UserProfile", "UserId", "UserName", true);

            _unitOfWork = new EfUnitOfWork(1);
            _commonService = new CommonService(_unitOfWork);
            _systemTimeService = new SystemTimeService();
            _userManagementService = new UserManagementService(_unitOfWork, _commonService, _systemTimeService);
            _errorMessageFactoryService = new ErrorMessageFactoryService(new ResourceErrorFactory());

        }


        // 3. Runs Twice; Once Before Test Case 1 and Once Before Test Case 2
        // Declare Objects Which Are Shared Among Tests, e.g. Shared Mocks
        [SetUp]
        public void RunOnceBeforeEachTest()
        {

        }


        /// <summary>
        // Test to create a new account
        /// </summary>
        [Test]
        public void CreateAccount()
        {
            var count = _unitOfWork.UserProfileRepository.CountAll();
            var userProfileDTO = Utilities.BuildAccountSample();
            var aRoles = new System.Collections.Generic.List<string> {"Administrator"};

            var error = _userManagementService.CreateUserProfile(ref userProfileDTO, aRoles, "123456");
            Assert.AreEqual(ErrorCode.NO_ERROR, error);
            Assert.AreEqual(_unitOfWork.UserProfileRepository.CountAll(), count + 1);

            error = _userManagementService.DeleteUserProfile(userProfileDTO);
            Assert.AreEqual(ErrorCode.NO_ERROR, error);
        }

        /// <summary>
        // Test to create a new account with an username (email) already existing
        /// </summary>
        [Test]
        public void CreateAccountAlreadyExisting()
        {
            var count = _unitOfWork.UserProfileRepository.CountAll();
            var userProfileDTO1 = Utilities.BuildAccountSample();
            var aRoles = new System.Collections.Generic.List<string> { "Administrator" };

            var error = _userManagementService.CreateUserProfile(ref userProfileDTO1, aRoles, "123456");
            Assert.AreEqual(ErrorCode.NO_ERROR, error);

            var userProfileDTO2 = Utilities.BuildAccountSample();
            error = _userManagementService.CreateUserProfile(ref userProfileDTO2, aRoles, "123456");
            Assert.AreEqual(ErrorCode.ACCOUNT_EMAIL_ALREADY_EXISTS, error);

            Assert.AreEqual(_unitOfWork.UserProfileRepository.CountAll(), count + 1);
            error = _userManagementService.DeleteUserProfile(userProfileDTO1);
            Assert.AreEqual(ErrorCode.NO_ERROR, error);
        }

        /// <summary>
        /// Test to edit an existing user account
        /// </summary>
        [Test]
        public void EditAccount()
        {
            var userProfileDTO = Utilities.BuildAccountSample();
            var aRoles = new System.Collections.Generic.List<string> { "Administrator" };

            var error = _userManagementService.CreateUserProfile(ref userProfileDTO, aRoles, "123456");
            Assert.AreEqual(ErrorCode.NO_ERROR, error);


            userProfileDTO.FullName = "My fullname test edit";
            userProfileDTO.Remarks = "My remarks edit";
            userProfileDTO.ContactInfo.HomePhoneNumber = "622199988000";
            userProfileDTO.ContactInfo.WorkPhoneNumber = "622199988000";
            userProfileDTO.Address.Street = "Jalan Cipete Edit, 1";
            userProfileDTO.Address.PostalCode = "12000";
            userProfileDTO.Address.Location.LocationId = 14;
            userProfileDTO.ContactPerson.ContactPersonName = "My contact to retrieve edit";

            error = _userManagementService.EditUserProfile(ref userProfileDTO, aRoles);
            Assert.AreEqual(ErrorCode.NO_ERROR, error);

            var userProfileEditDTO = _userManagementService.GetUserProfileById(userProfileDTO.UserId);
            
            Assert.AreNotEqual(userProfileEditDTO, null);
            Assert.AreEqual(userProfileEditDTO.FullName, userProfileDTO.FullName);
            Assert.AreEqual(userProfileEditDTO.Remarks, userProfileDTO.Remarks);
            Assert.AreEqual(userProfileEditDTO.ContactInfo.HomePhoneNumber, userProfileDTO.ContactInfo.HomePhoneNumber);
            Assert.AreEqual(userProfileEditDTO.ContactInfo.WorkPhoneNumber, userProfileDTO.ContactInfo.WorkPhoneNumber);
            Assert.AreEqual(userProfileEditDTO.Address.Street, userProfileDTO.Address.Street);
            Assert.AreEqual(userProfileEditDTO.Address.PostalCode, userProfileDTO.Address.PostalCode);
            Assert.AreEqual(userProfileEditDTO.Address.Location.LocationId, userProfileDTO.Address.Location.LocationId);
            Assert.AreEqual(userProfileEditDTO.ContactPerson.ContactPersonName, userProfileDTO.ContactPerson.ContactPersonName);
            
            error = _userManagementService.DeleteUserProfile(userProfileDTO);
            Assert.AreEqual(ErrorCode.NO_ERROR, error);
        }

        /// <summary>
        // Test to edit an accout that is not existing
        /// </summary>
        [Test]
        public void EditAccountRole()
        {
            var userProfileDTO = Utilities.BuildAccountSample();
            var aRoles = new System.Collections.Generic.List<string> { "Administrator" };
            var error = _userManagementService.CreateUserProfile(ref userProfileDTO, aRoles, "123456");
            Assert.AreEqual(ErrorCode.NO_ERROR, error);

            var roleDTO = _userManagementService.GetRolesByUserProfile(userProfileDTO.UserId);
            Assert.AreEqual(roleDTO.Count, 1);
            Assert.AreEqual(roleDTO[0].RoleName, "Administrator");

            aRoles = new System.Collections.Generic.List<string> {"HR"};
            error = _userManagementService.EditUserProfile(ref userProfileDTO, aRoles);
            Assert.AreEqual(ErrorCode.NO_ERROR, error);

            roleDTO = _userManagementService.GetRolesByUserProfile(userProfileDTO.UserId);
            Assert.AreEqual(roleDTO.Count, 1);
            Assert.AreEqual(roleDTO[0].RoleName, "HR");
            error = _userManagementService.DeleteUserProfile(userProfileDTO);
            Assert.AreEqual(ErrorCode.NO_ERROR, error);
        }

        /// <summary>
        // Test to edit an accout that is not existing
        /// </summary>
        [Test]
        public void EditAccountNotFound()
        {
            var userProfileDTO = new UserProfileDTO
            {
                UserId = 9999
            };

            var aRoles = new System.Collections.Generic.List<string> { "Administrator" };
            var error = _userManagementService.EditUserProfile(ref userProfileDTO, aRoles);
            Assert.AreEqual(ErrorCode.ACCOUNT_USERID_NOT_FOUND, error);
        }

        /// <summary>
        // Test to delete an user account
        /// </summary>
        [Test]
        public void DeleteAccount()
        {
            var count = _unitOfWork.UserProfileRepository.CountAll();
            var userProfileDTO = Utilities.BuildAccountSample();
            var aRoles = new System.Collections.Generic.List<string> { "Administrator" };

            var error = _userManagementService.CreateUserProfile(ref userProfileDTO, aRoles, "123456");
            Assert.AreEqual(ErrorCode.NO_ERROR, error);
            Assert.AreEqual(_unitOfWork.UserProfileRepository.CountAll(), count + 1);

            error = _userManagementService.DeleteUserProfile(userProfileDTO);
            Assert.AreEqual(ErrorCode.NO_ERROR, error);
            Assert.AreEqual(_unitOfWork.UserProfileRepository.CountAll(), count);
        }

        /// <summary>
        // Test to delete an user account not found
        /// </summary>
        [Test]
        public void DeleteAccountNotFound()
        {
            var userProfileDTO = new UserProfileDTO
            {
                UserId = 9999
            };
            var error = _userManagementService.DeleteUserProfile(userProfileDTO);
            Assert.AreEqual(ErrorCode.ACCOUNT_USERID_NOT_FOUND, error);
        }

        /// <summary>
        // Test to get an account by id
        /// </summary>
        [Test]
        public void DetailsAccountById()
        {
            var userProfileDTO = Utilities.BuildAccountSample();
            var aRoles = new System.Collections.Generic.List<string> { "Administrator" };

            var error = _userManagementService.CreateUserProfile(ref userProfileDTO, aRoles, "123456");
            Assert.AreEqual(ErrorCode.NO_ERROR, error);

            var userProfileDetailsDTO = _userManagementService.GetUserProfileById(userProfileDTO.UserId);
            Assert.AreNotEqual(userProfileDetailsDTO, null);

            Assert.AreEqual(userProfileDetailsDTO.FullName, userProfileDTO.FullName);
            Assert.AreEqual(userProfileDetailsDTO.Remarks, userProfileDTO.Remarks);
            Assert.AreEqual(userProfileDetailsDTO.ContactInfo.HomePhoneNumber, userProfileDTO.ContactInfo.HomePhoneNumber);
            Assert.AreEqual(userProfileDetailsDTO.ContactInfo.WorkPhoneNumber, userProfileDTO.ContactInfo.WorkPhoneNumber);
            Assert.AreEqual(userProfileDetailsDTO.Address.Street, userProfileDTO.Address.Street);
            Assert.AreEqual(userProfileDetailsDTO.Address.PostalCode, userProfileDTO.Address.PostalCode);
            Assert.AreEqual(userProfileDetailsDTO.Address.Location.LocationId, userProfileDTO.Address.Location.LocationId);
            Assert.AreEqual(userProfileDetailsDTO.ContactPerson.ContactPersonName, userProfileDTO.ContactPerson.ContactPersonName);

            error = _userManagementService.DeleteUserProfile(userProfileDetailsDTO);
            Assert.AreEqual(ErrorCode.NO_ERROR, error);
        }


        /// <summary>
        // Test to get an account
        /// </summary>
        [Test]
        public void DetailsAccountByName()
        {
            var userProfileDTO = Utilities.BuildAccountSample();
            var aRoles = new System.Collections.Generic.List<string> { "Administrator" };

            var error = _userManagementService.CreateUserProfile(ref userProfileDTO, aRoles, "123456");
            Assert.AreEqual(ErrorCode.NO_ERROR, error);

            var userProfileDetailsDTO = _userManagementService.GetUserProfilebyName(userProfileDTO.UserName);
            Assert.AreNotEqual(userProfileDetailsDTO, null);

            Assert.AreEqual(userProfileDetailsDTO.FullName, userProfileDTO.FullName);
            Assert.AreEqual(userProfileDetailsDTO.Remarks, userProfileDTO.Remarks);
            Assert.AreEqual(userProfileDetailsDTO.ContactInfo.HomePhoneNumber, userProfileDTO.ContactInfo.HomePhoneNumber);
            Assert.AreEqual(userProfileDetailsDTO.ContactInfo.WorkPhoneNumber, userProfileDTO.ContactInfo.WorkPhoneNumber);
            Assert.AreEqual(userProfileDetailsDTO.Address.Street, userProfileDTO.Address.Street);
            Assert.AreEqual(userProfileDetailsDTO.Address.PostalCode, userProfileDTO.Address.PostalCode);
            Assert.AreEqual(userProfileDetailsDTO.Address.Location.LocationId, userProfileDTO.Address.Location.LocationId);
            Assert.AreEqual(userProfileDetailsDTO.ContactPerson.ContactPersonName, userProfileDTO.ContactPerson.ContactPersonName);

            error = _userManagementService.DeleteUserProfile(userProfileDetailsDTO);
            Assert.AreEqual(ErrorCode.NO_ERROR, error);
        }


        /// <summary>
        // Test to get an user account not found by id
        /// </summary>
        [Test]
        public void DetailsAccountByIdNotFound()
        {
            var userProfile = _userManagementService.GetUserProfileById(9999);
            Assert.AreEqual(userProfile, null);
        }

        /// <summary>
        // Test to get an user account not found by username
        /// </summary>
        [Test]
        public void DetailsAccountByNameNotFound()
        {
            var userProfile = _userManagementService.GetUserProfilebyName("mybadname");
            Assert.AreEqual(userProfile, null);
        }

        /// <summary>
        // Test to get all user account
        /// </summary>
        [Test]
        public void GetAllAccounts()
        {
            var userProfileDTO = Utilities.BuildAccountSample();
            var aRoles = new System.Collections.Generic.List<string> { "Administrator" };
            var userProfiles = _userManagementService.GetAllUserProfiles();
            var count = userProfiles.Count;
            var error = _userManagementService.CreateUserProfile(ref userProfileDTO, aRoles, "123456");
            Assert.AreEqual(ErrorCode.NO_ERROR, error);

            userProfiles = _userManagementService.GetAllUserProfiles();
            Assert.AreEqual(userProfiles.Count, count + 1);

            error = _userManagementService.DeleteUserProfile(userProfileDTO);
            Assert.AreEqual(ErrorCode.NO_ERROR, error);

        }


        /// <summary>
        // Test to deactivate an user account
        /// </summary>
        [Test]
        public void DeactivateAccount()
        {
            var userProfileDTO = Utilities.BuildAccountSample();
            var aRoles = new System.Collections.Generic.List<string> { "Administrator" };

            var error = _userManagementService.CreateUserProfile(ref userProfileDTO, aRoles, "123456");
            Assert.AreEqual(ErrorCode.NO_ERROR, error);

            var userProfileDetailsDTO = _userManagementService.GetUserProfilebyName(userProfileDTO.UserName);
            Assert.AreEqual(userProfileDetailsDTO.UserIsDeactivated, false);

            error = _userManagementService.DeactivateUserProfileByName(userProfileDTO.UserName);
            Assert.AreEqual(ErrorCode.NO_ERROR, error);

            userProfileDetailsDTO = _userManagementService.GetUserProfilebyName(userProfileDTO.UserName);
            Assert.AreEqual(userProfileDetailsDTO.UserIsDeactivated, true);

            error = _userManagementService.DeleteUserProfile(userProfileDTO);
            Assert.AreEqual(ErrorCode.NO_ERROR, error);
        }

        /// <summary>
        // Test to deactivate an account not found by username
        /// </summary>
        [Test]
        public void DeactivateAccountNotFound()
        {
            var error = _userManagementService.DeactivateUserProfileByName("mybadname");
            Assert.AreEqual(error, ErrorCode.ACCOUNT_USERNAME_NOT_FOUND);
        }


        /// <summary>
        // Test to activate an user account
        /// </summary>
        [Test]
        public void ActivateAccount()
        {
            var userProfileDTO = Utilities.BuildAccountSample();
            var aRoles = new System.Collections.Generic.List<string> { "Administrator" };

            var error = _userManagementService.CreateUserProfile(ref userProfileDTO, aRoles, "123456");
            Assert.AreEqual(ErrorCode.NO_ERROR, error);

            var userProfileDetailsDTO = _userManagementService.GetUserProfilebyName(userProfileDTO.UserName);
            Assert.AreEqual(userProfileDetailsDTO.UserIsDeactivated, false);

            error = _userManagementService.DeactivateUserProfileByName(userProfileDTO.UserName);
            Assert.AreEqual(ErrorCode.NO_ERROR, error);

            userProfileDetailsDTO = _userManagementService.GetUserProfilebyName(userProfileDTO.UserName);
            Assert.AreEqual(userProfileDetailsDTO.UserIsDeactivated, true);

            error = _userManagementService.ReactivateUserProfileByName(userProfileDTO.UserName);
            Assert.AreEqual(ErrorCode.NO_ERROR, error);

            userProfileDetailsDTO = _userManagementService.GetUserProfilebyName(userProfileDTO.UserName);
            Assert.AreEqual(userProfileDetailsDTO.UserIsDeactivated, false);

            error = _userManagementService.DeleteUserProfile(userProfileDTO);
            Assert.AreEqual(ErrorCode.NO_ERROR, error);
        }

        /// <summary>
        // Test to deactivate an account not found by username
        /// </summary>
        [Test]
        public void ActivateAccountNotFound()
        {
            var error = _userManagementService.ReactivateUserProfileByName("mybadname");
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

        }

    }
}

