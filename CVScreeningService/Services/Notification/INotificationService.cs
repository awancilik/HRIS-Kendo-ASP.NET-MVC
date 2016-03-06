using System.Collections.Generic;
using CVScreeningCore.Error;
using CVScreeningService.DTO.Notification;
using CVScreeningService.DTO.Screening;
using CVScreeningService.DTO.UserManagement;

namespace CVScreeningService.Services.Notification
{
    public interface INotificationService
    {

        #region Screening notification

        /// <summary>
        /// Push notification to end user when screening has been created
        /// </summary>
        /// <param name="screeningDTO"></param>
        /// <param name="commit"></param>
        /// <returns></returns>
        ErrorCode CreateNotificationScreeningCreated(ScreeningDTO screeningDTO, bool commit = false);

        /// <summary>
        /// Push notification to end user when screening status has been validated
        /// </summary>
        /// <param name="screeningDTO"></param>
        /// <param name="commit"></param>
        /// <returns></returns>
        ErrorCode CreateNotificationScreeningValidated(ScreeningDTO screeningDTO, bool commit = false);

        /// <summary>
        /// Push notification to end user when screening status has been submitted
        /// </summary>
        /// <param name="screeningDTO"></param>
        /// <param name="screeningReportDTO"></param>
        /// <param name="commit"></param>
        /// <returns></returns>
        ErrorCode CreateNotificationScreeningSubmitted(ScreeningDTO screeningDTO, ScreeningReportDTO screeningReportDTO, bool commit = false);

        #endregion

        #region Atomic check notification

        /// <summary>
        /// Push notification to end user when atomic validated status has been assigned to a screener
        /// </summary>
        /// <param name="screeningDTO"></param>
        /// <param name="commit"></param>
        /// <returns></returns>
        ErrorCode CreateNotificationAtomicCheckAssigned(AtomicCheckDTO screeningDTO, bool commit = false);

        /// <summary>
        /// Push notification to end user when atomic validated status has been processed to a screener
        /// </summary>
        /// <param name="screeningDTO"></param>
        /// <param name="commit"></param>
        /// <returns></returns>
        ErrorCode CreateNotificationAtomicCheckProcessed(AtomicCheckDTO screeningDTO, bool commit = false);

        /// <summary>
        /// Push notification to end user when atomic validated status has been rejected to a screener
        /// </summary>
        /// <param name="screeningDTO"></param>
        /// <param name="commit"></param>
        /// <returns></returns>
        ErrorCode CreateNotificationAtomicCheckRejected(AtomicCheckDTO screeningDTO, bool commit = false);

        /// <summary>
        /// Push notification to end user when atomic validated status has been validated to a screener
        /// </summary>
        /// <param name="screeningDTO"></param>
        /// <param name="commit"></param>
        /// <returns></returns>
        ErrorCode CreateNotificationAtomicCheckValidated(AtomicCheckDTO screeningDTO, bool commit = false);

        /// <summary>
        /// Push notification to end user when atomic validated status has been wrongly qualified by a screener
        /// </summary>
        /// <param name="screeningDTO"></param>
        /// <param name="commit"></param>
        /// <returns></returns>
        ErrorCode CreateNotificationAtomicCheckWronglyQualified(AtomicCheckDTO screeningDTO, bool commit = false);

        #endregion


        #region General method

        /// <summary>
        /// Push a new notification for an user
        /// </summary>
        /// <param name="userProfilesDTO"></param>
        /// <param name="notificationDTO"></param>
        /// <param name="commit"></param>
        /// <returns></returns>
        ErrorCode CreateNotification(IEnumerable<UserProfileDTO> userProfilesDTO,
            ref NotificationDTO notificationDTO, bool commit);

        /// <summary>
        /// Edit notification for a specific user
        /// </summary>
        /// <param name="notificationDTO"></param>
        /// <param name="hasNotificationBeenShown"></param>
        /// <returns></returns>
        ErrorCode EditNotification(ref NotificationOfUserDTO notificationDTO, bool hasNotificationBeenShown);

        /// <summary>
        /// Retrieve all the notifications of a user
        /// </summary>
        /// <param name="userProfileDTO"></param>
        /// <param name="includeOnlyNotYetShown">Include only notifications that has not been seen yet by end user</param>
        /// <returns></returns>
        IEnumerable<NotificationDTO> GetNotificationsByUser(
            UserProfileDTO userProfileDTO, bool includeOnlyNotYetShown = false);

        /// <summary>
        /// Retrieve all the notifications in the application that have not been read yet
        /// </summary>
        /// <returns></returns>
        IDictionary<UserProfileDTO, IList<NotificationDTO>> GetNotifications();

        #endregion


    }
}
