using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using CVScreeningCore.Error;
using CVScreeningCore.Models;
using CVScreeningService.DTO.Notification;
using CVScreeningService.DTO.Settings;
using CVScreeningService.DTO.UserManagement;
using CVScreeningService.Services.Common;
using CVScreeningService.Services.Notification;
using CVScreeningService.Services.Settings;
using CVScreeningService.Services.SystemTime;
using CVScreeningService.Services.UserManagement;
using FakeItEasy;
using NUnit.Framework;
using CVScreeningDAL.UnitOfWork;
using CVScreeningService.Services.ErrorHandling;
using System;

namespace CVScreeningService.Tests.UnitTest.Notification
{
    [TestFixture]
    public class Notification
    {
        // 1. Declare global object
        private IUnitOfWork _unitOfWork;
        private ICommonService _commonService;
        private ISystemTimeService _systemTimeService;
        private INotificationService _notificationService;
        private IWebSecurity _webSecurity;
        private IUserManagementService _userManagementService;
        private IErrorMessageFactoryService _errorMessageFactoryService;
        private UserProfileDTO _admin;
        private UserProfileDTO _qc;


        // 2. Runs Once Before All of The Following Methods
        // Declare Global Objects Which Are Global For Test Class, e.g. Mock Objects
        [TestFixtureSetUp]
        public void RunsOnceBeforeAll()
        {
            // Fake it easy
            _systemTimeService = A.Fake<ISystemTimeService>();
            _webSecurity = A.Fake<IWebSecurity>();
            A.CallTo(() => _webSecurity.GetCurrentUserName()).Returns("Administrator");
            A.CallTo(() => _systemTimeService.GetCurrentDateTime()).Returns(new DateTime(2014, 12, 1));

            _unitOfWork = new InMemoryUnitOfWork();
            _commonService = new CommonService(_unitOfWork);
            _userManagementService = new UserManagementService(_unitOfWork, _commonService, _systemTimeService);
            _notificationService = new NotificationService(_unitOfWork, _userManagementService, _systemTimeService, _webSecurity);
            _errorMessageFactoryService = new ErrorMessageFactoryService(new ResourceErrorFactory());

            _unitOfWork.UserProfileRepository.Add(new webpages_UserProfile
            {
                UserId = 1,
                UserIsDeactivated = false,
                UserName = "admin@admin.com",
                FullName = "Administrator",
            });
            _admin = new UserProfileDTO{UserId = 1};

            _unitOfWork.UserProfileRepository.Add(new webpages_UserProfile
            {
                UserId = 2,
                UserIsDeactivated = false,
                UserName = "qc@qc.com",
                FullName = "Quality control",
            });
            _qc = new UserProfileDTO { UserId = 2 };
        }


        // 3. Runs Twice; Once Before Test Case 1 and Once Before Test Case 2
        // Declare Objects Which Are Shared Among Tests, e.g. Shared Mocks
        [SetUp]
        public void RunOnceBeforeEachTest()
        {
            var notificationOfUsers = _unitOfWork.NotificationOfUserRepository.GetAll();
            foreach (var notificationOfUser in notificationOfUsers.Reverse())
            {
                _unitOfWork.NotificationOfUserRepository.Delete(notificationOfUser);
            }

            var notifications = _unitOfWork.NotificationRepository.GetAll();
            foreach (var notification in notifications.Reverse())
            {
                _unitOfWork.NotificationRepository.Delete(notification);
            }
        }

        /// <summary>
        // Test to create a new notification
        /// </summary>
        [Test]
        public void CreateNotifcation_Should_Be_Successfull()
        {
            IEnumerable<UserProfileDTO> userProfiles = new List<UserProfileDTO>
            {
                _admin
            };
            var notification = new NotificationDTO
            {
                NotificationMessage = "This is my first notification",
            };
            var error =_notificationService.CreateNotification(userProfiles, ref notification, true);
            Assert.AreEqual(ErrorCode.NO_ERROR, error);
            Assert.AreEqual("This is my first notification", notification.NotificationMessage);
            Assert.AreEqual(_systemTimeService.GetCurrentDateTime(), notification.NotificationCreatedDate);
        }

        /// <summary>
        // Test to create a new notification that failed
        /// </summary>
        [Test]
        public void CreateNotifcation_With_User_Unknown()
        {
            IEnumerable<UserProfileDTO> userProfiles = new List<UserProfileDTO>
            {
                new UserProfileDTO
                {
                    UserId = 10,
                }
            };
            var notification = new NotificationDTO
            {
                NotificationMessage = "This is my first notification",
            };
            var error =_notificationService.CreateNotification(userProfiles, ref notification, true);
            Assert.AreEqual(ErrorCode.ACCOUNT_USERID_NOT_FOUND, error);
        }

        /// <summary>
        // Test to create a new notification that failed
        /// </summary>
        [Test]
        public void EditNotifcation_With_User_Unknown()
        {
            IEnumerable<UserProfileDTO> userProfiles = new List<UserProfileDTO>
            {
                _admin

            };
            var notification = new NotificationDTO
            {
                NotificationMessage = "This is my first notification",
            };
            var error = _notificationService.CreateNotification(userProfiles, ref notification, true);
            Assert.AreEqual(ErrorCode.NO_ERROR, error);

            var notifcationByUser = new NotificationOfUserDTO
            {
                Notification = notification,
            };
            Assert.AreEqual(false, notifcationByUser.IsNotificationShown);
            error = _notificationService.EditNotification(ref notifcationByUser, true);
            Assert.AreEqual(ErrorCode.NO_ERROR, error);
            Assert.AreEqual(true, notifcationByUser.IsNotificationShown);

        }

        /// <summary>
        // Test to retrieve notification
        /// </summary>
        [Test]
        public void GetNotifcation_Should_Be_Successfull()
        {
            IEnumerable<UserProfileDTO> userProfiles = new List<UserProfileDTO> {_admin, _qc};
            var notification1 = new NotificationDTO { NotificationMessage = "This is my first notification" };
            var error = _notificationService.CreateNotification(userProfiles, ref notification1, true);
            Assert.AreEqual(ErrorCode.NO_ERROR, error);
            var notification2 = new NotificationDTO { NotificationMessage = "This is my first notification" };
            error = _notificationService.CreateNotification(new List<UserProfileDTO> { _admin }, ref notification2, true);
            Assert.AreEqual(ErrorCode.NO_ERROR, error);


            Assert.AreEqual(ErrorCode.NO_ERROR, error);
            var notifications = _notificationService.GetNotifications();
            Assert.AreEqual(2, notifications.Count);
            Assert.AreEqual(2, notifications.Single(u => u.Key.UserId == _admin.UserId).Value.Count);
            Assert.AreEqual(1, notifications.Single(u => u.Key.UserId == _qc.UserId).Value.Count);

            var notifcationByUser = new NotificationOfUserDTO
            {
                Notification = notification1,
            };
            error = _notificationService.EditNotification(ref notifcationByUser, true);
            Assert.AreEqual(ErrorCode.NO_ERROR, error);
            notifications = _notificationService.GetNotifications();
            Assert.AreEqual(2, notifications.Count);
            Assert.AreEqual(1, notifications.Single(u => u.Key.UserId == _admin.UserId).Value.Count);
            Assert.AreEqual(1, notifications.Single(u => u.Key.UserId == _qc.UserId).Value.Count);

        }

        /// <summary>
        // Test to retrieve notification
        /// </summary>
        [Test]
        public void GetNotifcation_When_Empty_Should_Be_Successfull()
        {
            var notifications = _notificationService.GetNotifications();
            Assert.AreEqual(0, notifications.Count);
        }


        /// <summary>
        // Test to retrieve notification by user
        /// </summary>
        [Test]
        public void GetNotifcation_By_User_Should_Be_Successfull()
        {
            IEnumerable<UserProfileDTO> userProfiles = new List<UserProfileDTO> { _admin, _qc };
            var notification1 = new NotificationDTO { NotificationMessage = "This is my first notification" };
            var error = _notificationService.CreateNotification(userProfiles, ref notification1, true);
            Assert.AreEqual(ErrorCode.NO_ERROR, error);
            var notification2 = new NotificationDTO { NotificationMessage = "This is my second notification" };
            error = _notificationService.CreateNotification(new List<UserProfileDTO> { _admin }, ref notification2, true);
            Assert.AreEqual(ErrorCode.NO_ERROR, error);


            Assert.AreEqual(ErrorCode.NO_ERROR, error);
            var notifications = _notificationService.GetNotificationsByUser(_admin);
            Assert.AreEqual(2, notifications.Count());
            Assert.AreEqual("This is my first notification", notifications.ElementAt(0).NotificationMessage);
            Assert.AreEqual("This is my second notification", notifications.ElementAt(1).NotificationMessage);

            notifications = _notificationService.GetNotificationsByUser(_qc);
            Assert.AreEqual(1, notifications.Count());
            Assert.AreEqual("This is my first notification", notifications.ElementAt(0).NotificationMessage);


            var notifcationByUser = new NotificationOfUserDTO
            {
                Notification = notification1,
            };
            error = _notificationService.EditNotification(ref notifcationByUser, true);
            Assert.AreEqual(ErrorCode.NO_ERROR, error);
            notifications = _notificationService.GetNotificationsByUser(_admin, true);
            Assert.AreEqual(1, notifications.Count());
            Assert.AreEqual("This is my second notification", notifications.ElementAt(0).NotificationMessage);


        }

        /// <summary>
        // Test to retrieve notification by user
        /// </summary>
        [Test]
        public void GetNotifcation_By_User_When_Empty_Should_Be_Successfull()
        {
            var notifications = _notificationService.GetNotificationsByUser(_admin);
            Assert.AreEqual(0, notifications.Count());

            notifications = _notificationService.GetNotificationsByUser(_qc);
            Assert.AreEqual(0, notifications.Count());
        }

        /// <summary>
        // Test to retrieve notification by user
        /// </summary>
        [Test]
        public void GetNotifcation_By_User_When_User_Does_Not_Exit_Should_Be_Null()
        {
            var notifications = _notificationService.GetNotificationsByUser(new UserProfileDTO{UserId = 10});
            Assert.AreEqual(null, notifications);

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

