using System.Collections.Generic;
using CVScreeningCore.Error;
using CVScreeningService.Services.Common;
using CVScreeningService.Services.SystemTime;
using CVScreeningService.Services.UserManagement;
using NUnit.Framework;
using CVScreeningDAL.UnitOfWork;
using CVScreeningService.Services.ErrorHandling;
using CVScreeningService.DTO.UserManagement;
using CVScreeningService.Tests.UnitTest;

namespace CVScreeningService.Tests.IntegrationTest.UserManagement
{
    [TestFixture]
    public class Role
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
            _userManagementService.DeleteRole("Role 1");
            _userManagementService.DeleteRole("Role 2");
            _userManagementService.DeleteRole("Role 3");
            _userManagementService.DeleteRole("Role 4");
            _userManagementService.DeleteRole("Role 5");
            _userManagementService.DeleteRole("Role 6");
            _userManagementService.DeleteRole("Role 7");
        }

        /// <summary>
        // Test to create a new role
        /// </summary>
        [Test]
        public void CreateRole()
        {
            var roleCount = _unitOfWork.RoleRepository.CountAll();
            var error = _userManagementService.CreateRole("Role 1");
            Assert.AreEqual(ErrorCode.NO_ERROR, error);

            error = _userManagementService.CreateRole("Role 2");
            Assert.AreEqual(ErrorCode.NO_ERROR, error);

            Assert.AreEqual(roleCount + 2, _unitOfWork.RoleRepository.CountAll());
        }

        /// <summary>
        // Test to create a role already existing
        /// </summary>
        [Test]
        public void CreateRoleAlreadyExisting()
        {
            var roleCount = _unitOfWork.RoleRepository.CountAll();
            var error = _userManagementService.CreateRole("Role 3");
            Assert.AreEqual(ErrorCode.NO_ERROR, error);
            Assert.AreEqual(roleCount + 1, _unitOfWork.RoleRepository.CountAll());

            error = _userManagementService.CreateRole("Role 3");
            Assert.AreEqual(ErrorCode.ROLE_ALREADY_EXISTS, error);
            Assert.AreEqual(roleCount + 1, _unitOfWork.RoleRepository.CountAll());

        }

        /// <summary>
        /// Test to delete an existing role
        /// </summary>
        [Test]
        public void DeleteRole()
        {
            var roleCount = _unitOfWork.RoleRepository.CountAll();
            var error = _userManagementService.CreateRole("Role 4");
            Assert.AreEqual(ErrorCode.NO_ERROR, error);
            Assert.AreEqual(roleCount + 1, _unitOfWork.RoleRepository.CountAll());

            error = _userManagementService.DeleteRole("Role 4");
            Assert.AreEqual(ErrorCode.NO_ERROR, error);
            Assert.AreEqual(roleCount, _unitOfWork.RoleRepository.CountAll());
        }

        /// <summary>
        // Test to delete a role that is not existing
        /// </summary>
        [Test]
        public void DeleteRoleNotFound()
        {
            var roleCount = _unitOfWork.RoleRepository.CountAll();
            var error = _userManagementService.DeleteRole("Role 1114");
            Assert.AreEqual(ErrorCode.ROLE_NOT_FOUND, error);
            Assert.AreEqual(roleCount, _unitOfWork.RoleRepository.CountAll());
        }

        /// <summary>
        // Test to get all the roles
        /// </summary>
        [Test]
        public void GetAllRoles()
        {
            var roleCount = _unitOfWork.RoleRepository.CountAll();

            var error = _userManagementService.CreateRole("Role 5");
            Assert.AreEqual(ErrorCode.NO_ERROR, error);
            error = _userManagementService.CreateRole("Role 6");
            Assert.AreEqual(ErrorCode.NO_ERROR, error);
            error = _userManagementService.CreateRole("Role 7");
            Assert.AreEqual(ErrorCode.NO_ERROR, error);

            var roles = _userManagementService.GetAllRoles();
            Assert.AreEqual(roles.Count, roleCount + 3);

        }

        /// <summary>
        // Test to add an user to the role
        /// </summary>
        [Test]
        public void AddUserToRole()
        {
            var error = _userManagementService.CreateRole("Role 1");
            Assert.AreEqual(ErrorCode.NO_ERROR, error);

            var roles = _userManagementService.GetRolesByUserProfile(_userProfileDTO.UserId);
            Assert.AreEqual(roles.Count, 1);

            error = _userManagementService.AddUserToRole(_userProfileDTO.UserName, "Role 1");
            Assert.AreEqual(ErrorCode.NO_ERROR, error);

            roles = _userManagementService.GetRolesByUserProfile(_userProfileDTO.UserId);
            Assert.AreEqual(roles.Count, 2);
            Assert.AreEqual(roles[1].RoleName, "Role 1");
        }

        /// <summary>
        // Test to add an user to the role that is already added to the user
        /// </summary>
        [Test]
        public void AddUserToRoleAlreadyBelongs()
        {
            var error = _userManagementService.CreateRole("Role 1");
            Assert.AreEqual(ErrorCode.NO_ERROR, error);

            var roles = _userManagementService.GetRolesByUserProfile(_userProfileDTO.UserId);
            Assert.AreEqual(roles.Count, 1);

            error = _userManagementService.AddUserToRole(_userProfileDTO.UserName, "Role 1");
            Assert.AreEqual(ErrorCode.NO_ERROR, error);

            error = _userManagementService.AddUserToRole(_userProfileDTO.UserName, "Role 1");
            Assert.AreEqual(ErrorCode.ROLE_ALREADY_BELONGS_TO_USER, error);

            roles = _userManagementService.GetRolesByUserProfile(_userProfileDTO.UserId);
            Assert.AreEqual(roles.Count, 2);
            Assert.AreEqual(roles[1].RoleName, "Role 1");
        }

        /// <summary>
        /// Test to add a non existing role to an user
        /// </summary>
        [Test]
        public void AddUserToRoleNotFound()
        {
            var error = _userManagementService.AddUserToRole(_userProfileDTO.UserName, "ROLE NOT FOUND");
            Assert.AreEqual(ErrorCode.ROLE_NOT_FOUND, error);
        }

        /// <summary>
        /// Test to add a non existing user to a role
        /// </summary>
        [Test]
        public void AddUserNotFoundToRole()
        {
            var error = _userManagementService.CreateRole("Role 1");
            Assert.AreEqual(ErrorCode.NO_ERROR, error);

            error = _userManagementService.AddUserToRole("User not existing", "Role 1");
            Assert.AreEqual(ErrorCode.ACCOUNT_USERNAME_NOT_FOUND, error);
        }

        /// <summary>
        // Test to remove an user from the role
        /// </summary>
        [Test]
        public void DeleteUserFromRole()
        {
            var error = _userManagementService.CreateRole("Role 1");
            Assert.AreEqual(ErrorCode.NO_ERROR, error);

            var roles = _userManagementService.GetRolesByUserProfile(_userProfileDTO.UserId);
            Assert.AreEqual(roles.Count, 1);

            error = _userManagementService.AddUserToRole(_userProfileDTO.UserName, "Role 1");
            Assert.AreEqual(ErrorCode.NO_ERROR, error);

            roles = _userManagementService.GetRolesByUserProfile(_userProfileDTO.UserId);
            Assert.AreEqual(roles.Count, 2);
            Assert.AreEqual(roles[1].RoleName, "Role 1");

            error = _userManagementService.DeleteUserFromRole(_userProfileDTO.UserName, "Role 1");
            Assert.AreEqual(ErrorCode.NO_ERROR, error);

            roles = _userManagementService.GetRolesByUserProfile(_userProfileDTO.UserId);
            Assert.AreEqual(roles.Count, 1);
        }

        /// <summary>
        // Test to remove a role that does not belongs yet to an user
        /// </summary>
        [Test]
        public void DeleteUserFromRoleNotBelongs()
        {
            var roles = _userManagementService.GetRolesByUserProfile(_userProfileDTO.UserId);
            Assert.AreEqual(roles.Count, 1);

            var error = _userManagementService.CreateRole("Role 1");
            Assert.AreEqual(ErrorCode.NO_ERROR, error);

            error = _userManagementService.DeleteUserFromRole(_userProfileDTO.UserName, "Role 1");
            Assert.AreEqual(ErrorCode.ROLE_NOT_BELONGS_TO_USER, error);
        }

        /// <summary>
        /// Test to delete a user from a non existing role
        /// </summary>
        [Test]
        public void DeleteUserToRoleNotFound()
        {
            var error = _userManagementService.DeleteUserFromRole(_userProfileDTO.UserName, "ROLE NOT FOUND");
            Assert.AreEqual(ErrorCode.ROLE_NOT_FOUND, error);
        }

        /// <summary>
        /// Test to delete a role to a non existing user
        /// </summary>
        [Test]
        public void DeleteUserNotFoundToRole()
        {
            var error = _userManagementService.CreateRole("Role 1");
            Assert.AreEqual(ErrorCode.NO_ERROR, error);

            error = _userManagementService.AddUserToRole("User not existing", "Role 1");
            Assert.AreEqual(ErrorCode.ACCOUNT_USERNAME_NOT_FOUND, error);
        }

        /// <summary>
        // Test to check whether an user belongs to a role
        /// </summary>
        [Test]
        public void IsUserInRoleTrue()
        {
            var error = _userManagementService.CreateRole("Role 1");
            Assert.AreEqual(ErrorCode.NO_ERROR, error);

            error = _userManagementService.AddUserToRole(_userProfileDTO.UserName, "Role 1");
            Assert.AreEqual(ErrorCode.NO_ERROR, error);

            Assert.AreEqual(_userManagementService.IsUserInRole(_userProfileDTO.UserName, "Role 1"), true);
        }

        /// <summary>
        // Test to check whether an user belongs to a role
        /// </summary>
        [Test]
        public void IsUserInRoleFalseOrNull()
        {
            var error = _userManagementService.CreateRole("Role 1");
            Assert.AreEqual(ErrorCode.NO_ERROR, error);

            error = _userManagementService.CreateRole("Role 2");
            Assert.AreEqual(ErrorCode.NO_ERROR, error);

            error = _userManagementService.AddUserToRole(_userProfileDTO.UserName, "Role 1");
            Assert.AreEqual(ErrorCode.NO_ERROR, error);

            Assert.AreEqual(_userManagementService.IsUserInRole(_userProfileDTO.UserName, "Role 2"), false);
            Assert.AreEqual(_userManagementService.IsUserInRole(_userProfileDTO.UserName, "Role 3"), null);
            Assert.AreEqual(_userManagementService.IsUserInRole("User not existing", "Role 2"), null);
        }


        /// <summary>
        // Test to get role by user profile and user profile by role
        /// </summary>
        [Test]
        public void GetUserProfileByRoleAndGetRoleByUserProfile()
        {
            var error = _userManagementService.CreateRole("Role 1");
            Assert.AreEqual(ErrorCode.NO_ERROR, error);

            var users = _userManagementService.GetUserProfilesByRoles("Role 1");
            Assert.AreEqual(users.Count, 0);

            var roles = _userManagementService.GetRolesByUserProfile(_userProfileDTO.UserId);
            Assert.AreEqual(roles.Count, 1);

            error = _userManagementService.AddUserToRole(_userProfileDTO.UserName, "Role 1");
            Assert.AreEqual(ErrorCode.NO_ERROR, error);

            users = _userManagementService.GetUserProfilesByRoles("Role 1");
            Assert.AreEqual(users.Count, 1);

            roles = _userManagementService.GetRolesByUserProfile(_userProfileDTO.UserId);
            Assert.AreEqual(roles.Count, 2);

            Assert.AreEqual(users[0].UserId, _userProfileDTO.UserId);
            Assert.AreEqual(roles[1].RoleName, "Role 1");

        }


        /// <summary>
        // Test to get role by user profile 
        /// </summary>
        [Test]
        public void GetRoleByUserProfileAsString()
        {
            var roles = _userManagementService.GetRolesByUserProfileAsString(_userProfileDTO.UserId);
            Assert.AreEqual(roles, "Administrator");

            var error = _userManagementService.CreateRole("Role 1");
            Assert.AreEqual(ErrorCode.NO_ERROR, error);

            error = _userManagementService.AddUserToRole(_userProfileDTO.UserName, "Role 1");
            Assert.AreEqual(ErrorCode.NO_ERROR, error);

            roles = _userManagementService.GetRolesByUserProfileAsString(_userProfileDTO.UserId);
            Assert.AreEqual(roles, "Administrator, Role 1");

            error = _userManagementService.CreateRole("Role 2");
            Assert.AreEqual(ErrorCode.NO_ERROR, error);

            error = _userManagementService.AddUserToRole(_userProfileDTO.UserName, "Role 2");
            Assert.AreEqual(ErrorCode.NO_ERROR, error);

            roles = _userManagementService.GetRolesByUserProfileAsString(_userProfileDTO.UserId);
            Assert.AreEqual(roles, "Administrator, Role 1, Role 2");

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
            _userManagementService.DeleteRole("Role 1");
            _userManagementService.DeleteRole("Role 2");
            _userManagementService.DeleteRole("Role 3");
            _userManagementService.DeleteRole("Role 4");
            _userManagementService.DeleteRole("Role 5");
            _userManagementService.DeleteRole("Role 6");
            _userManagementService.DeleteRole("Role 7");
            _userManagementService.DeleteUserProfile(_userProfileDTO);
        }



    }
}

