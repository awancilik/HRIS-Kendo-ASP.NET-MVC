using System.Collections.Generic;
using CVScreeningCore.Error;
using CVScreeningService.Services.Common;
using CVScreeningService.Services.SystemTime;
using CVScreeningService.Services.UserManagement;
using CVScreeningService.Tests.UnitTest;
using NUnit.Framework;
using CVScreeningDAL.UnitOfWork;
using CVScreeningService.Services.ErrorHandling;
using CVScreeningService.DTO.UserManagement;

namespace CVScreeningService.Tests.IntegrationTest.UserManagement
{
    [TestFixture]
    public class Password
    {
        // 1. Declare global object
        private IUnitOfWork _unitOfWork;
        private ICommonService _commonService;
        private IUserManagementService _userManagementService;
        private IErrorMessageFactoryService _errorMessageFactoryService;
        private ISystemTimeService _systemTimeService;
        private UserProfileDTO _userProfileDTO;

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

            // Create sample user account for testing
            _userProfileDTO = Utilities.BuildAccountSample();
            if (_userManagementService.GetUserProfilebyName(_userProfileDTO.UserName) == null)
            {
                var error = _userManagementService.CreateUserProfile(ref _userProfileDTO,
                    new System.Collections.Generic.List<string>(new List<string> { "Administrator" }),
                    "123456");
            }
            else
            {
                _userProfileDTO = _userManagementService.GetUserProfilebyName(_userProfileDTO.UserName);
            }
        }


        // 3. Runs Twice; Once Before Test Case 1 and Once Before Test Case 2
        // Declare Objects Which Are Shared Among Tests, e.g. Shared Mocks
        [SetUp]
        public void RunOnceBeforeEachTest()
        {

        }


        /// <summary>
        // Test to update personal password
        /// </summary>
        [Test]
        public void UpdatePasswordOk()
        {
            var error = _userManagementService.UpdatePassword(
                _userProfileDTO.UserName, "123456", "1234567");
            Assert.AreEqual(ErrorCode.NO_ERROR, error);

            error = _userManagementService.UpdatePassword(
                _userProfileDTO.UserName, "1234567", "123456");
            Assert.AreEqual(ErrorCode.NO_ERROR, error);
        }


        /// <summary>
        // Test to update personal password failure
        /// </summary>
        [Test]
        public void UpdatePasswordKo()
        {
            var error = _userManagementService.UpdatePassword(
                _userProfileDTO.UserName, "1234567", "12345678");
            Assert.AreEqual(ErrorCode.ACCOUNT_WRONG_PASSWORD, error);
        }

        /// <summary>
        // Test to update personal password failure
        /// </summary>
        [Test]
        public void UpdatePasswordUsernameNotFound()
        {
           var error = _userManagementService.UpdatePassword(
                "Username not existing", "1234567", "12345678");
            Assert.AreEqual(ErrorCode.ACCOUNT_USERNAME_NOT_FOUND, error);
        }

        /// <summary>
        // Test to reset password
        /// </summary>
        [Test]
        public void ResetPasswordOk()
        {
            var error = _userManagementService.ResetPassword(
                 _userProfileDTO.UserName, "1234567");
            Assert.AreEqual(ErrorCode.NO_ERROR, error);

            error = _userManagementService.UpdatePassword(
                _userProfileDTO.UserName, "1234567", "123456");
            Assert.AreEqual(ErrorCode.NO_ERROR, error);
        }

        /// <summary>
        // Test to reset password with username not existing
        /// </summary>
        [Test]
        public void ResetPasswordUsernameNotFound()
        {
            var error = _userManagementService.ResetPassword(
                 "Username not existing", "1234567");
            Assert.AreEqual(ErrorCode.ACCOUNT_USERNAME_NOT_FOUND, error);
        }

        /// <summary>
        // Test to recover password using generated token
        /// </summary>
        [Test]
        public void RecoverPassword()
        {
            var token = _userManagementService.GeneratePasswordResetToken(
                 _userProfileDTO.UserName);

            var error = _userManagementService.RecoverPassword(token, "1234567");
            Assert.AreEqual(ErrorCode.NO_ERROR, error);

            error = _userManagementService.UpdatePassword(
                _userProfileDTO.UserName, "1234567", "123456");
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
            var error = _userManagementService.DeleteUserProfile(_userProfileDTO);
        }



    }
}

