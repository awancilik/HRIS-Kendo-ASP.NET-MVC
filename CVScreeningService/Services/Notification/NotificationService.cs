using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using AutoMapper;
using CVScreeningCore.Error;
using CVScreeningCore.Models;
using CVScreeningDAL.UnitOfWork;
using CVScreeningService.DTO.Notification;
using CVScreeningService.DTO.Screening;
using CVScreeningService.DTO.UserManagement;
using CVScreeningService.Filters;
using CVScreeningService.Services.SystemTime;
using CVScreeningService.Services.UserManagement;
using Nalysa.Common.Log;

namespace CVScreeningService.Services.Notification
{
    [Logging(Order = 1), ExceptionHandling(Order = 2)]
    public class NotificationService : INotificationService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IUserManagementService _userManagementService;
        private readonly ISystemTimeService _systemTimeService;
        private readonly IWebSecurity _webSecurity;

        public NotificationService(
            IUnitOfWork unitOfWork, IUserManagementService userManagementService,
            ISystemTimeService systemTimeService,
            IWebSecurity webSecurity)
        {
            _unitOfWork = unitOfWork;
            _userManagementService = userManagementService;
            _systemTimeService =  systemTimeService;
            _webSecurity = webSecurity;

            Mapper.CreateMap<webpages_UserProfile, UserProfileDTO>();
            Mapper.CreateMap<CVScreeningCore.Models.Notification, NotificationDTO>();
            Mapper.CreateMap<NotificationOfUser, NotificationOfUserDTO>();

        }

        /// <summary>
        /// Push notification to end user when screening has been created
        /// </summary>
        /// <param name="screeningDTO"></param>
        /// <param name="commit"></param>
        /// <returns></returns>
        public virtual ErrorCode CreateNotificationScreeningCreated(
            ScreeningDTO screeningDTO, bool commit = false)
        {
            var id = screeningDTO.ScreeningId;
            if (!_unitOfWork.ScreeningRepository.Exist(u => u.ScreeningId == id))
                return ErrorCode.SCREENING_NOT_FOUND;
            var screening = _unitOfWork.ScreeningRepository.Single(u => u.ScreeningId == id);

            var userProfile = screening.GetAccountManagers();

            userProfile.Add(screening.GetQualityControl());

            var userProfilesDTO = userProfile.Select(Mapper.Map<webpages_UserProfile, UserProfileDTO>).ToList();
            userProfilesDTO.AddRange(_userManagementService.GetUserProfilesByRoles(webpages_Roles.kQualifierRole));
            userProfilesDTO.AddRange(_userManagementService.GetUserProfilesByRoles(webpages_Roles.kAdministratorRole));

            // Remove duplicates to avoid double notification
            var distinctUserProfilesDTO = userProfilesDTO.GroupBy(x => x.UserId).Select(y => y.FirstOrDefault());
            string referenceLink = string.Format(CVScreeningService.Resources.Notification.ScreeningDetailLink, screeningDTO.ScreeningId,
                screeningDTO.ScreeningReference);

            var notificationDTO = new NotificationDTO
            {
                NotificationMessage = string.Format(
                    CVScreeningService.Resources.Notification.ScreeningCreated, referenceLink)
            };
            return this.CreateNotification(distinctUserProfilesDTO, ref notificationDTO, commit);
        }

        /// <summary>
        /// Push notification to end user when screening status has been validated
        /// </summary>
        /// <param name="screeningDTO"></param>
        /// <param name="commit"></param>
        /// <returns></returns>
        public virtual ErrorCode CreateNotificationScreeningValidated(ScreeningDTO screeningDTO, bool commit = false)
        {
            var id = screeningDTO.ScreeningId;
            if (!_unitOfWork.ScreeningRepository.Exist(u => u.ScreeningId == id))
                return ErrorCode.SCREENING_NOT_FOUND;
            var screening = _unitOfWork.ScreeningRepository.Single(u => u.ScreeningId == id);

            var userProfile = screening.GetAccountManagers();
            userProfile.Add(screening.GetQualityControl());

            var userProfilesDTO = userProfile.Select(Mapper.Map<webpages_UserProfile, UserProfileDTO>).ToList();
            userProfilesDTO.AddRange(_userManagementService.GetUserProfilesByRoles(webpages_Roles.kAdministratorRole));

            // Remove duplicates to avoid double notification
            var distinctUserProfilesDTO = userProfilesDTO.GroupBy(x => x.UserId).Select(y => y.FirstOrDefault());
            string referenceLink = string.Format(CVScreeningService.Resources.Notification.ScreeningDetailLink, screeningDTO.ScreeningId,
                screeningDTO.ScreeningReference);
            var notificationDTO = new NotificationDTO
            {
                NotificationMessage = string.Format(
                    CVScreeningService.Resources.Notification.ScreeningValidated, referenceLink)
            };
            return this.CreateNotification(distinctUserProfilesDTO, ref notificationDTO, commit);
        }

        /// <summary>
        /// Push notification to end user when screening status has been submitted
        /// </summary>
        /// <param name="screeningDTO"></param>
        /// <param name="screeningReportDTO"></param>
        /// <param name="commit"></param>
        /// <returns></returns>
        public virtual ErrorCode CreateNotificationScreeningSubmitted(
            ScreeningDTO screeningDTO, ScreeningReportDTO screeningReportDTO, bool commit = false)
        {
            var id = screeningDTO.ScreeningId;
            if (!_unitOfWork.ScreeningRepository.Exist(u => u.ScreeningId == id))
                return ErrorCode.SCREENING_NOT_FOUND;
            var screening = _unitOfWork.ScreeningRepository.Single(u => u.ScreeningId == id);

            var userProfile = screening.GetAccountManagers();
            userProfile.Add(screening.GetQualityControl());

            var userProfilesDTO = userProfile.Select(Mapper.Map<webpages_UserProfile, UserProfileDTO>).ToList();
            userProfilesDTO.AddRange(_userManagementService.GetUserProfilesByRoles(webpages_Roles.kAdministratorRole));

            // Remove duplicates to avoid double notification
            var distinctUserProfilesDTO = userProfilesDTO.GroupBy(x => x.UserId).Select(y => y.FirstOrDefault());
            string referenceLink = string.Format(CVScreeningService.Resources.Notification.ManageReportLink, screeningDTO.ScreeningId,
                screeningDTO.ScreeningReference);
            var notificationDTO = new NotificationDTO
            {
                NotificationMessage = string.Format(
                    CVScreeningService.Resources.Notification.ScreeningSubmitted, referenceLink)
            };
            return this.CreateNotification(distinctUserProfilesDTO, ref notificationDTO, commit);
        }

        /// <summary>
        /// Push notification to end user when atomic validated status has been assigned to a screener
        /// </summary>
        /// <param name="atomicCheckDTO"></param>
        /// <param name="commit"></param>
        /// <returns></returns>
        public virtual ErrorCode CreateNotificationAtomicCheckAssigned(AtomicCheckDTO atomicCheckDTO, bool commit = false)
        {
            var id = atomicCheckDTO.AtomicCheckId;
            if (!_unitOfWork.AtomicCheckRepository.Exist(u => u.AtomicCheckId == id))
                return ErrorCode.ATOMIC_CHECK_NOT_FOUND;
            var atomicCheck = _unitOfWork.AtomicCheckRepository.Single(u => u.AtomicCheckId == id);

            // Notification send to screener only
            var userProfilesDTO = new List<UserProfileDTO>
            {
                Mapper.Map<webpages_UserProfile, UserProfileDTO>(atomicCheck.Screener)
            };

            // Remove duplicates to avoid double notification
            var distinctUserProfilesDTO = userProfilesDTO.GroupBy(x => x.UserId).Select(y => y.FirstOrDefault());
            var notificationDTO = new NotificationDTO
            {
                NotificationMessage = string.Format(
                    CVScreeningService.Resources.Notification.AtomicCheckAssigned, atomicCheck.AtomicCheckId)
            };
            return this.CreateNotification(distinctUserProfilesDTO, ref notificationDTO, commit);
        }

        /// <summary>
        /// Push notification to end user when atomic validated status has been processed to a screener
        /// </summary>
        /// <param name="atomicCheckDTO"></param>
        /// <param name="commit"></param>
        /// <returns></returns>
        public virtual ErrorCode CreateNotificationAtomicCheckProcessed(AtomicCheckDTO atomicCheckDTO, bool commit = false)
        {
            var id = atomicCheckDTO.AtomicCheckId;
            if (!_unitOfWork.AtomicCheckRepository.Exist(u => u.AtomicCheckId == id))
                return ErrorCode.ATOMIC_CHECK_NOT_FOUND;
            var atomicCheck = _unitOfWork.AtomicCheckRepository.Single(u => u.AtomicCheckId == id);

            // Notification send to screener only
            var userProfilesDTO = new List<UserProfileDTO>
            {
                Mapper.Map<webpages_UserProfile, UserProfileDTO>(atomicCheck.Screening.QualityControl)
            };

            // Remove duplicates to avoid double notification
            var distinctUserProfilesDTO = userProfilesDTO.GroupBy(x => x.UserId).Select(y => y.FirstOrDefault());
            var notificationDTO = new NotificationDTO
            {
                NotificationMessage = string.Format(
                    CVScreeningService.Resources.Notification.AtomicCheckProcessed, atomicCheck.AtomicCheckId)
            };
            return this.CreateNotification(distinctUserProfilesDTO, ref notificationDTO, commit);
        }

        /// <summary>
        /// Push notification to end user when atomic validated status has been rejected to a screener
        /// </summary>
        /// <param name="atomicCheckDTO"></param>
        /// <param name="commit"></param>
        /// <returns></returns>
        public virtual ErrorCode CreateNotificationAtomicCheckRejected(AtomicCheckDTO atomicCheckDTO, bool commit = false)
        {
            var id = atomicCheckDTO.AtomicCheckId;
            if (!_unitOfWork.AtomicCheckRepository.Exist(u => u.AtomicCheckId == id))
                return ErrorCode.ATOMIC_CHECK_NOT_FOUND;
            var atomicCheck = _unitOfWork.AtomicCheckRepository.Single(u => u.AtomicCheckId == id);

            // Notification send to screener only
            var userProfilesDTO = new List<UserProfileDTO>
            {
                Mapper.Map<webpages_UserProfile, UserProfileDTO>(atomicCheck.Screener)
            };

            // Remove duplicates to avoid double notification
            var distinctUserProfilesDTO = userProfilesDTO.GroupBy(x => x.UserId).Select(y => y.FirstOrDefault());
            var notificationDTO = new NotificationDTO
            {
                NotificationMessage = string.Format(
                    CVScreeningService.Resources.Notification.AtomicCheckRejected, atomicCheck.AtomicCheckId)
            };
            return this.CreateNotification(distinctUserProfilesDTO, ref notificationDTO, commit);
        }

        /// <summary>
        /// Push notification to end user when atomic validated status has been validated to a screener
        /// </summary>
        /// <param name="atomicCheckDTO"></param>
        /// <param name="commit"></param>
        /// <returns></returns>
        public virtual ErrorCode CreateNotificationAtomicCheckValidated(AtomicCheckDTO atomicCheckDTO, bool commit = false)
        {
            var id = atomicCheckDTO.AtomicCheckId;
            if (!_unitOfWork.AtomicCheckRepository.Exist(u => u.AtomicCheckId == id))
                return ErrorCode.ATOMIC_CHECK_NOT_FOUND;
            var atomicCheck = _unitOfWork.AtomicCheckRepository.Single(u => u.AtomicCheckId == id);

            // Notification send to screener only
            var userProfilesDTO = new List<UserProfileDTO>
            {
                Mapper.Map<webpages_UserProfile, UserProfileDTO>(atomicCheck.Screener)
            };

            // Remove duplicates to avoid double notification
            var distinctUserProfilesDTO = userProfilesDTO.GroupBy(x => x.UserId).Select(y => y.FirstOrDefault());
            var notificationDTO = new NotificationDTO
            {
                NotificationMessage = string.Format(
                    CVScreeningService.Resources.Notification.AtomicCheckValidated, atomicCheck.AtomicCheckId)
            };
            return this.CreateNotification(distinctUserProfilesDTO, ref notificationDTO, commit);
        }

        /// <summary>
        /// Push notification to end user when atomic validated status has been wrongly qualified by a screener
        /// </summary>
        /// <param name="atomicCheckDTO"></param>
        /// <param name="commit"></param>
        /// <returns></returns>
        public virtual ErrorCode CreateNotificationAtomicCheckWronglyQualified(AtomicCheckDTO atomicCheckDTO, bool commit = false)
        {
            var id = atomicCheckDTO.AtomicCheckId;
            if (!_unitOfWork.AtomicCheckRepository.Exist(u => u.AtomicCheckId == id))
                return ErrorCode.ATOMIC_CHECK_NOT_FOUND;
            var atomicCheck = _unitOfWork.AtomicCheckRepository.Single(u => u.AtomicCheckId == id);

            // Notification send to qualifier only
            var userProfilesDTO = _userManagementService.GetUserProfilesByRoles(webpages_Roles.kQualifierRole);

            // Remove duplicates to avoid double notification
            var distinctUserProfilesDTO = userProfilesDTO.GroupBy(x => x.UserId).Select(y => y.FirstOrDefault());
            var notificationDTO = new NotificationDTO
            {
                NotificationMessage = string.Format(
                    CVScreeningService.Resources.Notification.AtomicCheckWrongQualified, atomicCheck.AtomicCheckId)
            };
            return this.CreateNotification(distinctUserProfilesDTO, ref notificationDTO, commit);
        }

        /// <summary>
        /// Push a new notification for an user
        /// </summary>
        /// <param name="userProfilesDTO"></param>
        /// <param name="notificationDTO"></param>
        /// <param name="commit"></param>
        /// <returns></returns>
        public virtual ErrorCode CreateNotification(IEnumerable<UserProfileDTO> userProfilesDTO,
            ref NotificationDTO notificationDTO, bool commit)
        {
            var notification = new CVScreeningCore.Models.Notification
            {
                NotificationMessage = notificationDTO.NotificationMessage,
                NotificationCreatedDate = _systemTimeService.GetCurrentDateTime()
            };
            _unitOfWork.NotificationRepository.Add(notification);

            foreach (var userId in userProfilesDTO.Select(userProfileDTO => userProfileDTO.UserId))
            {
                if (!_unitOfWork.UserProfileRepository.Exist(u => u.UserId == userId))
                {
                    return ErrorCode.ACCOUNT_USERID_NOT_FOUND;
                }

                var userProfile = _unitOfWork.UserProfileRepository.Single(u => u.UserId == userId);
                var notificationOfUser = new NotificationOfUser
                    {
                        NotificationId = notification.NotificationId,
                        UserId = userProfile.UserId,
                        Notification = notification,
                        webpages_UserProfile = userProfile,
                        IsNotificationShown = false
                    };
                notification.NotificationOfUser.Add(notificationOfUser);
                userProfile.NotificationOfUser.Add(notificationOfUser);

                _unitOfWork.NotificationOfUserRepository.Add(notificationOfUser);

                LogManager.Instance.Info(String.Format("Notification pushed for {0}: {1}", userProfile.UserName,
                    notification.NotificationMessage));
            }

            if (commit)
                _unitOfWork.Commit();

            notificationDTO =  Mapper.Map<CVScreeningCore.Models.Notification,NotificationDTO>(notification);
            return ErrorCode.NO_ERROR;
        }



        /// <summary>
        /// Edit notification for a specific user
        /// </summary>
        /// <param name="notificationDTO"></param>
        /// <param name="hasNotificationBeenShown"></param>
        /// <returns></returns>
        public virtual ErrorCode EditNotification(ref NotificationOfUserDTO notificationDTO, bool hasNotificationBeenShown)
        {
            int userId = notificationDTO.UserId;
            int notificationId = notificationDTO.Notification.NotificationId;

            if (!_unitOfWork.UserProfileRepository.Exist(u => u.UserId == userId))
            {
                return ErrorCode.ACCOUNT_USERID_NOT_FOUND;
            }
            var userProfile = _unitOfWork.UserProfileRepository.Single(u => u.UserId == userId);
            if (!userProfile.NotificationOfUser.Any(u => u.NotificationId == notificationId))
            {
                return ErrorCode.NOTIFICATION_NOT_FOUND;
            }

            var notification = userProfile.NotificationOfUser.Single(u => u.NotificationId == notificationId);
            notification.IsNotificationShown = hasNotificationBeenShown;
            _unitOfWork.Commit();

            notificationDTO = Mapper.Map<NotificationOfUser, NotificationOfUserDTO>(notification);
            return ErrorCode.NO_ERROR;
        }


        /// <summary>
        /// Retrieve all the notifications of a user
        /// </summary>
        /// <param name="userProfileDTO"></param>
        /// <param name="includeOnlyNotYetShown">Include only notifications that has not been seen yet by end user</param>
        /// <returns></returns>
        public virtual IEnumerable<NotificationDTO> GetNotificationsByUser(
            UserProfileDTO userProfileDTO, bool includeOnlyNotYetShown = false)
        {
            int userId = userProfileDTO.UserId;
            if (!_unitOfWork.UserProfileRepository.Exist(u => u.UserId == userId))
            {
                return null;
            }
            var userProfile = _unitOfWork.UserProfileRepository.Single(u => u.UserId == userId);

            Expression<Func<NotificationOfUser, bool>> filter = u => u.webpages_UserProfile.UserId == userProfile.UserId;
            if (includeOnlyNotYetShown)
                filter = u => u.webpages_UserProfile.UserId == userProfile.UserId && u.IsNotificationShown == false;

            var notifcations = _unitOfWork.NotificationOfUserRepository.Find(filter)
                .OrderByDescending(u => u.Notification.NotificationCreatedDate).Take(10)
                .Select(n => n.Notification).ToList();

            return notifcations.Select(Mapper.Map<CVScreeningCore.Models.Notification, NotificationDTO>).ToList();
        }


        /// <summary>
        /// Retrieve all the notifications in the application that has not been read yet
        /// </summary>
        /// <returns></returns>
        public virtual IDictionary<UserProfileDTO, IList<NotificationDTO>> GetNotifications()
        {
            var dictionnary = new Dictionary<webpages_UserProfile, IList<CVScreeningCore.Models.Notification>>();

            var notifcations = _unitOfWork.NotificationOfUserRepository.Find(u => u.IsNotificationShown == false);
            foreach (var notificationOfUser in notifcations)
            {
                var userProfile = notificationOfUser.webpages_UserProfile;
                var notification = notificationOfUser.Notification;

                if (dictionnary.ContainsKey(userProfile))
                    dictionnary[userProfile].Add(notification);
                else
                    dictionnary[userProfile] = new List<CVScreeningCore.Models.Notification> {notification};
            }

            return Mapper.Map<Dictionary<webpages_UserProfile, IList<CVScreeningCore.Models.Notification>>,
                Dictionary<UserProfileDTO, IList<NotificationDTO>>>(dictionnary);
        }

    }
}