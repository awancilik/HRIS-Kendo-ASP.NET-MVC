using System.Collections.Generic;
using CVScreeningCore.Error;
using CVScreeningService.DTO.Client;
using CVScreeningService.DTO.UserManagement;

namespace CVScreeningService.Services.UserManagement
{
    public interface IUserManagementService
    {
        #region Authentication

        // General  method
        bool IsAuthenticated();
        string GetCurrentUserName();
        int GetCurrentUserId();
        ErrorCode Login(string userName, string password, bool persistCookie = false);
        void Logout();
        bool IsDeactivated(string userName);

        #endregion

        #region Password

        // User profile password method
        ErrorCode UpdatePassword(string userName, string oldPassword, string newPassword);
        string GeneratePasswordResetToken(string userName, int tokenExpirationInMinutesFromNow = 1440);
        ErrorCode RecoverPassword(string passwordResetToken, string newPassword);
        ErrorCode ResetPassword(string userName, string newPassword);

        #endregion

        #region Getter for user profile

        // User profile method
        UserProfileDTO GetCurrentUser();
        List<UserProfileDTO> GetAllUserProfiles();
        UserProfileDTO GetUserProfilebyName(string username);
        UserProfileDTO GetUserProfileById(int id);


        #endregion

        #region User profile CRUD operations

        // User profile CRUD method
        ErrorCode CreateUserProfile(ref UserProfileDTO userProfileDTO, List<string> roles, string password,
            bool createMembership = true);

        ErrorCode EditUserProfile(ref UserProfileDTO userProfile, List<string> roles, bool commit = true);
        ErrorCode DeactivateUserProfileByName(string name, bool commit = true);
        ErrorCode ReactivateUserProfileByName(string name, bool commit = true);
        ErrorCode DeleteUserProfile(UserProfileDTO userProfileDTO, bool commit = true);
        ErrorCode EditPersonalProfile(ref UserProfileDTO userProfile);

        #endregion

        #region Getter for client profile

        // Client profile method
        List<UserProfileDTO> GetAllClientProfiles();
        UserProfileDTO GetClientProfilebyName(string username);
        UserProfileDTO GetClientProfileById(int id);

        #endregion

        IEnumerable<UserProfileDTO> GetAllUserScreeners();

        #region Client profile CRUD operations

        // Client profile CRUD method
        ErrorCode CreateClientProfile(ref UserProfileDTO userProfileDTO, ClientCompanyDTO clientCompanyDTO,
            string password, bool createMembership = true);

        ErrorCode EditClientProfile(ref UserProfileDTO userProfileDTO, bool commit = true);
        ErrorCode DeactivateClientProfileByName(string name, bool commit = true);
        ErrorCode DeactivateClientProfileById(int id, bool commit = true);
        ErrorCode ReactivateClientProfileByName(string name, bool commit = true);
        ErrorCode ReactivateClientProfileById(int id, bool commit = true);
        ErrorCode DeleteClientProfile(UserProfileDTO userProfileDTO, bool commit = true);

        #endregion

        #region Roles

        // Role method
        List<RolesDTO> GetAllRoles(bool includeClientRole = true);
        RolesDTO GetAllRoleById(int id);
        ErrorCode CreateRole(string iRole, bool commit = true);
        ErrorCode DeleteRole(string iRole, bool commit = true);
        List<UserProfileDTO> GetUserProfilesByRoles(string role);
        List<RolesDTO> GetRolesByUserProfile(int userProfileId);
        string GetRolesByUserProfileAsString(int userProfileId);
        bool? IsUserInRole(string iUser, string iRole);
        ErrorCode AddUserToRole(string userName, string role, bool commit = true);
        ErrorCode DeleteUserFromRole(string userName, string role, bool commit = true);

        #endregion

        #region Leave

        // Leaves method
        IEnumerable<UserLeaveDTO> GeAllUserLeavesByUserId(int userId);
        ErrorCode CreateLeave(UserProfileDTO userProfileDTO, ref UserLeaveDTO userLeaveDTO, bool commit = true);
        ErrorCode EditLeave(UserLeaveDTO userLeaveDTO, bool commit = true);
        UserLeaveDTO GetUserLeave(int userLeaveId);
        ErrorCode DeleteUserLeave(UserProfileDTO userProfileDTO, int userLeaveId, bool commit = true);
        int RetrieveNumberOfWorkingDaysToNextLeave(UserProfileDTO userProfileDTO);


        #endregion
    }
}