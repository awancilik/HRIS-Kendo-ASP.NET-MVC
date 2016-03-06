using System.Collections.Generic;
using System.Linq;
using CVScreeningCore.Error;
using CVScreeningService.DTO.Settings;
using CVScreeningService.Services.Common;
using CVScreeningService.Services.Settings;
using CVScreeningService.Services.SystemTime;
using CVScreeningService.Services.UserManagement;
using NUnit.Framework;
using CVScreeningDAL.UnitOfWork;
using CVScreeningService.Services.ErrorHandling;
using System;
using CVScreeningService.DTO.UserManagement;
using CVScreeningService.Tests.UnitTest;

namespace CVScreeningService.Tests.IntegrationTest.UserManagement
{
    [TestFixture]
    public class UserLeave
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
            var leaves =_userManagementService.GeAllUserLeavesByUserId(_userProfileDTO.UserId);
            if (leaves == null)
                return;

            foreach (var leave in leaves)
            {
                _userManagementService.DeleteUserLeave(_userProfileDTO, leave.UserLeaveId);
            }
        }

        /// <summary>
        // Test to create an user leave
        /// </summary>
        [Test]
        public void CreateUserLeave()
        {
            var count = _userManagementService.GeAllUserLeavesByUserId(_userProfileDTO.UserId).Count();

            // Public holiday from today to today + 1
            var userLeaveDTO = new UserLeaveDTO
            {
                UserLeaveName = "Idul Fitri",
                UserLeaveRemarks = "Selamat Idul Fitri",
                UserLeaveStartDate = DateTime.Now,
                UserLeaveEndDate = DateTime.Now.AddDays(1)
            };

            var error = _userManagementService.CreateLeave(_userProfileDTO, ref userLeaveDTO);
            Assert.AreEqual(ErrorCode.NO_ERROR, error);
            Assert.AreEqual(count + 1, _userManagementService.GeAllUserLeavesByUserId(_userProfileDTO.UserId).Count());

        }

        /// <summary>
        // Test to create an user leave that is overlapping with a previous one
        /// </summary>
        [Test]
        public void CreateUserLeaveOverlapping()
        {
            var count = _userManagementService.GeAllUserLeavesByUserId(_userProfileDTO.UserId).Count();

            // Public holiday from today to today + 1
            var userLeaveDTO1 = new UserLeaveDTO
            {
                UserLeaveName = "Idul Fitri",
                UserLeaveRemarks = "Selamat Idul Fitri",
                UserLeaveStartDate = DateTime.Now,
                UserLeaveEndDate = DateTime.Now.AddDays(1)
            };

            var error = _userManagementService.CreateLeave(_userProfileDTO, ref userLeaveDTO1);
            Assert.AreEqual(ErrorCode.NO_ERROR, error);


            // Public holiday from today to today + 1
            var userLeaveDTO2 = new UserLeaveDTO
            {
                UserLeaveName = "Idul Fitri",
                UserLeaveRemarks = "Selamat Idul Fitri",
                UserLeaveStartDate = DateTime.Now,
                UserLeaveEndDate = DateTime.Now.AddDays(1)
            };

            error = _userManagementService.CreateLeave(_userProfileDTO, ref userLeaveDTO2);
            Assert.AreEqual(ErrorCode.USER_LEAVE_DATE_OVERLAPPING, error);

            Assert.AreEqual(count + 1, _userManagementService.GeAllUserLeavesByUserId(_userProfileDTO.UserId).Count());
        }

        /// <summary>
        /// Test to edit an user leave
        /// </summary>
        [Test]
        public void EditUserLeave()
        {
            // Public holiday from today to today + 1
            var userLeaveDTO1 = new UserLeaveDTO
            {
                UserLeaveName = "Idul Fitri",
                UserLeaveRemarks = "Selamat Idul Fitri",
                UserLeaveStartDate = DateTime.Now,
                UserLeaveEndDate = DateTime.Now.AddDays(1)
            };

            var error = _userManagementService.CreateLeave(_userProfileDTO, ref userLeaveDTO1);
            Assert.AreEqual(ErrorCode.NO_ERROR, error);

            // Public holiday from today to today + 1
            var userLeaveDTO2 = new UserLeaveDTO
            {
                UserLeaveId =  userLeaveDTO1.UserLeaveId,
                UserLeaveName = "Idul Fitri Edit",
                UserLeaveRemarks = "Selamat Idul Fitri Edit",
                UserLeaveStartDate = DateTime.Now.AddDays(2),
                UserLeaveEndDate = DateTime.Now.AddDays(4)
            };

            error = _userManagementService.EditLeave(userLeaveDTO2);
            Assert.AreEqual(ErrorCode.NO_ERROR, error);

            var userLeaveDTO3 = _userManagementService.GetUserLeave(userLeaveDTO1.UserLeaveId);

            Assert.AreEqual(userLeaveDTO2.UserLeaveId, userLeaveDTO3.UserLeaveId);
            Assert.AreEqual(userLeaveDTO2.UserLeaveName, userLeaveDTO3.UserLeaveName);
            Assert.AreEqual(userLeaveDTO2.UserLeaveRemarks, userLeaveDTO3.UserLeaveRemarks);
            Assert.AreEqual(userLeaveDTO2.UserLeaveStartDate.Date, userLeaveDTO3.UserLeaveStartDate);
            Assert.AreEqual(userLeaveDTO2.UserLeaveEndDate.Date, userLeaveDTO3.UserLeaveEndDate);
        }

        /// <summary>
        // Test to edit an user leave that is overlapping with a previous one
        /// </summary>
        [Test]
        public void EditUserLeaveOverlapping()
        {
            // Public holiday from today to today + 1
            var userLeaveDTO1 = new UserLeaveDTO
            {
                UserLeaveName = "Idul Fitri",
                UserLeaveRemarks = "Selamat Idul Fitri",
                UserLeaveStartDate = DateTime.Now,
                UserLeaveEndDate = DateTime.Now.AddDays(1)
            };

            var error = _userManagementService.CreateLeave(_userProfileDTO, ref userLeaveDTO1);
            Assert.AreEqual(ErrorCode.NO_ERROR, error);

            // Public holiday from today to today + 1
            var userLeaveDTO2 = new UserLeaveDTO
            {
                UserLeaveName = "Idul Fitri Edit",
                UserLeaveRemarks = "Selamat Idul Fitri Edit", 
                UserLeaveStartDate = DateTime.Now.AddDays(2),
                UserLeaveEndDate = DateTime.Now.AddDays(4)
            };

            error = _userManagementService.CreateLeave(_userProfileDTO, ref userLeaveDTO2);
            Assert.AreEqual(ErrorCode.NO_ERROR, error);

            // Public holiday from today to today + 1
            var userLeaveDTO3 = new UserLeaveDTO
            {
                UserLeaveId = userLeaveDTO1.UserLeaveId,
                UserLeaveName = "Idul Fitri Edit",
                UserLeaveRemarks = "Selamat Idul Fitri Edit",
                UserLeaveStartDate = DateTime.Now.AddDays(3),
                UserLeaveEndDate = DateTime.Now.AddDays(4)
            };

            error = _userManagementService.EditLeave(userLeaveDTO3);
            Assert.AreEqual(ErrorCode.USER_LEAVE_DATE_OVERLAPPING, error);

        }

        /// <summary>
        // Test to delete an user leav
        /// </summary>
        [Test]
        public void DeleteUserLeave()
        {
            var count = _userManagementService.GeAllUserLeavesByUserId(_userProfileDTO.UserId).Count();

            // Public holiday from today to today + 1
            var userLeaveDTO = new UserLeaveDTO
            {
                UserLeaveName = "Idul Fitri",
                UserLeaveRemarks = "Selamat Idul Fitri",
                UserLeaveStartDate = DateTime.Now,
                UserLeaveEndDate = DateTime.Now.AddDays(1)
            };

            var error = _userManagementService.CreateLeave(_userProfileDTO, ref userLeaveDTO);
            Assert.AreEqual(ErrorCode.NO_ERROR, error);
            Assert.AreEqual(count + 1, _userManagementService.GeAllUserLeavesByUserId(_userProfileDTO.UserId).Count());

            error = _userManagementService.DeleteUserLeave(_userProfileDTO, userLeaveDTO.UserLeaveId);
            Assert.AreEqual(ErrorCode.NO_ERROR, error);

            Assert.AreEqual(count, _userManagementService.GeAllUserLeavesByUserId(_userProfileDTO.UserId).Count());
        }

        /// <summary>
        // Test to delete a public holiday not existing
        /// </summary>
        [Test]
        public void DeleteUserLeaveNotFound()
        {
            var error = _userManagementService.DeleteUserLeave(_userProfileDTO, 1000);
            Assert.AreEqual(ErrorCode.USER_LEAVE_NOT_FOUND, error);

            error = _userManagementService.DeleteUserLeave(new UserProfileDTO
            { UserId = 100000 }, 1000);
            Assert.AreEqual(ErrorCode.ACCOUNT_USERID_NOT_FOUND, error);

            Assert.AreEqual(0, _userManagementService.GeAllUserLeavesByUserId(_userProfileDTO.UserId).Count());
        }

        /// <summary>
        // Test to get a public holiday
        /// </summary>
        [Test]
        public void DetailsUserLeave()
        {
            // Public holiday from today to today + 1
            var userLeaveDTO = new UserLeaveDTO
            {
                UserLeaveName = "Idul Fitri",
                UserLeaveRemarks = "Selamat Idul Fitri",
                UserLeaveStartDate = DateTime.Now,
                UserLeaveEndDate = DateTime.Now.AddDays(1)
            };

            var error = _userManagementService.CreateLeave(_userProfileDTO, ref userLeaveDTO);
            Assert.AreEqual(ErrorCode.NO_ERROR, error);

            var userLeaveDTO2 = _userManagementService.GetUserLeave(userLeaveDTO.UserLeaveId);

            Assert.AreEqual(userLeaveDTO2.UserLeaveId, userLeaveDTO.UserLeaveId);
            Assert.AreEqual(userLeaveDTO2.UserLeaveName, userLeaveDTO.UserLeaveName);
            Assert.AreEqual(userLeaveDTO2.UserLeaveRemarks, userLeaveDTO.UserLeaveRemarks);
            Assert.AreEqual(userLeaveDTO2.UserLeaveStartDate, userLeaveDTO.UserLeaveStartDate.Date);
            Assert.AreEqual(userLeaveDTO2.UserLeaveEndDate, userLeaveDTO.UserLeaveEndDate.Date);
        }

        /// <summary>
        // Test to get a public holiday not existing
        /// </summary>
        [Test]
        public void DetailsUserLeaveNotFound()
        {
            var userLeaveDTO = _userManagementService.GetUserLeave(10000);
            Assert.AreEqual(userLeaveDTO, null);
        }

        /// <summary>
        // Test to get all public holiday
        /// </summary>
        [Test]
        public void GetAllUserLeaves()
        {
            // Public holiday from today to today + 1
            var userLeaveDTO1 = new UserLeaveDTO
            {
                UserLeaveName = "Idul Fitri",
                UserLeaveRemarks = "Selamat Idul Fitri",
                UserLeaveStartDate = DateTime.Now,
                UserLeaveEndDate = DateTime.Now.AddDays(1)
            };

            var error = _userManagementService.CreateLeave(_userProfileDTO, ref userLeaveDTO1);
            Assert.AreEqual(ErrorCode.NO_ERROR, error);

            // Public holiday from today to today + 1
            var userLeaveDTO2 = new UserLeaveDTO
            {
                UserLeaveName = "Idul Fitri Edit",
                UserLeaveRemarks = "Selamat Idul Fitri Edit",
                UserLeaveStartDate = DateTime.Now.AddDays(2),
                UserLeaveEndDate = DateTime.Now.AddDays(4)
            };

            error = _userManagementService.CreateLeave(_userProfileDTO, ref userLeaveDTO2);
            Assert.AreEqual(ErrorCode.NO_ERROR, error);

            var userLeaves = _userManagementService.GeAllUserLeavesByUserId(_userProfileDTO.UserId);

            Assert.AreEqual(userLeaveDTO1.UserLeaveId, userLeaves.ElementAt(0).UserLeaveId);
            Assert.AreEqual(userLeaveDTO1.UserLeaveName, userLeaves.ElementAt(0).UserLeaveName);
            Assert.AreEqual(userLeaveDTO1.UserLeaveStartDate.Date, userLeaves.ElementAt(0).UserLeaveStartDate);
            Assert.AreEqual(userLeaveDTO1.UserLeaveEndDate.Date, userLeaves.ElementAt(0).UserLeaveEndDate);
            Assert.AreEqual(userLeaveDTO1.UserLeaveRemarks, userLeaves.ElementAt(0).UserLeaveRemarks);

            Assert.AreEqual(userLeaveDTO2.UserLeaveId, userLeaves.ElementAt(1).UserLeaveId);
            Assert.AreEqual(userLeaveDTO2.UserLeaveName, userLeaves.ElementAt(1).UserLeaveName);
            Assert.AreEqual(userLeaveDTO2.UserLeaveStartDate.Date, userLeaves.ElementAt(1).UserLeaveStartDate);
            Assert.AreEqual(userLeaveDTO2.UserLeaveEndDate.Date, userLeaves.ElementAt(1).UserLeaveEndDate);
            Assert.AreEqual(userLeaveDTO2.UserLeaveRemarks, userLeaves.ElementAt(1).UserLeaveRemarks);

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
            _userManagementService.DeleteUserProfile(_userProfileDTO);
        }



    }
}

