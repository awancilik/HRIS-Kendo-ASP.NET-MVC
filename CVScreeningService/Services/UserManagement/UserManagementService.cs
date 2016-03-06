using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web.Security;
using AutoMapper;
using CVScreeningCore.Error;
using CVScreeningCore.Models;
using CVScreeningDAL.UnitOfWork;
using CVScreeningService.DTO.Client;
using CVScreeningService.DTO.Common;
using CVScreeningService.DTO.Settings;
using CVScreeningService.DTO.UserManagement;
using CVScreeningService.Filters;
using CVScreeningService.Helpers;
using CVScreeningService.Services.Common;
using CVScreeningService.Services.SystemTime;
using Nalysa.Common.Log;
using WebMatrix.WebData;
using Contract = System.Diagnostics.Contracts.Contract;
using UserProfile = CVScreeningCore.Models.webpages_UserProfile;
using Roles = CVScreeningCore.Models.webpages_Roles;

namespace CVScreeningService.Services.UserManagement
{
    [Logging(Order = 1), ExceptionHandling(Order = 2)]
    public class UserManagementService : IUserManagementService
    {
        private readonly ICommonService _commonService;
        private readonly ISystemTimeService _systemTimeService;
        private readonly IUnitOfWork _uow;

        public UserManagementService(
            IUnitOfWork uow, ICommonService commonService, ISystemTimeService systemTimeService)
        {
            _uow = uow;
            _commonService = commonService;
            _systemTimeService = systemTimeService;

            Mapper.CreateMap<UserProfile, UserProfileDTO>();
            Mapper.CreateMap<ClientCompany, ClientCompanyDTO>()
                .ForMember(e => e.Contract, opt => opt.Ignore());
            Mapper.CreateMap<Address, AddressDTO>();
            Mapper.CreateMap<Location, LocationDTO>();
            Mapper.CreateMap<ContactInfo, ContactInfoDTO>();
            Mapper.CreateMap<ContactPerson, ContactPersonDTO>();
            Mapper.CreateMap<UserLeave, UserLeaveDTO>();
            Mapper.CreateMap<Roles, RolesDTO>();
        }

        #region Authentication

        /// <summary>
        ///     Check if the current user is authenticated
        /// </summary>
        /// <returns></returns>
        public virtual bool IsAuthenticated()
        {
            return WebMatrix.WebData.WebSecurity.IsAuthenticated;
        }

        /// <summary>
        ///     Get the user name (email) of the current user
        /// </summary>
        /// <returns></returns>
        public virtual string GetCurrentUserName()
        {
            return WebMatrix.WebData.WebSecurity.CurrentUserName;
        }

        /// <summary>
        ///     Get the id of the current user
        /// </summary>
        /// <returns></returns>
        public virtual int GetCurrentUserId()
        {
            return WebMatrix.WebData.WebSecurity.CurrentUserId;
        }

        /// <summary>
        ///     Login
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="password"></param>
        /// <param name="persistCookie"></param>
        /// <returns></returns>
        public virtual ErrorCode Login(string userName, string password, bool persistCookie = false)
        {
            if (GetUserProfilebyName(userName) == null && GetClientProfilebyName(userName) == null)
                return ErrorCode.ACCOUNT_USERNAME_NOT_FOUND;

            if (IsDeactivated(userName))
                return ErrorCode.ACCOUNT_DEACTIVATED;

            return WebMatrix.WebData.WebSecurity.Login(userName, password, persistCookie)
                ? ErrorCode.NO_ERROR
                : ErrorCode.ACCOUNT_WRONG_PASSWORD;
        }

        public virtual void Logout()
        {
            WebMatrix.WebData.WebSecurity.Logout();
        }

        public virtual bool IsDeactivated(string userName)
        {
            UserProfileDTO aUserProfile = GetUserProfilebyName(userName);
            return aUserProfile != null && aUserProfile.UserIsDeactivated;
        }

        #endregion

        #region Password

        public virtual ErrorCode UpdatePassword(string userName, string oldPassword, string newPassword)
        {
            if (GetUserProfilebyName(userName) == null)
                return ErrorCode.ACCOUNT_USERNAME_NOT_FOUND;

            return WebMatrix.WebData.WebSecurity.ChangePassword(userName, oldPassword, newPassword)
                ? ErrorCode.NO_ERROR
                : ErrorCode.ACCOUNT_WRONG_PASSWORD;
        }

        public virtual string GeneratePasswordResetToken(string userName, int tokenExpirationInMinutesFromNow = 1440)
        {
            return WebMatrix.WebData.WebSecurity.GeneratePasswordResetToken(userName, tokenExpirationInMinutesFromNow);
        }

        public virtual ErrorCode RecoverPassword(string passwordResetToken, string newPassword)
        {
            return WebMatrix.WebData.WebSecurity.ResetPassword(passwordResetToken, newPassword)
                ? ErrorCode.NO_ERROR
                : ErrorCode.ACCOUNT_PASSWORD_RECOVERY_ERROR;
        }

        public virtual ErrorCode ResetPassword(string userName, string newPassword)
        {
            if (GetUserProfilebyName(userName) == null)
                return ErrorCode.ACCOUNT_USERNAME_NOT_FOUND;

            string passwordResetToken = WebMatrix.WebData.WebSecurity.GeneratePasswordResetToken(userName);

            return WebMatrix.WebData.WebSecurity.ResetPassword(passwordResetToken, newPassword)
                ? ErrorCode.NO_ERROR
                : ErrorCode.ACCOUNT_WRONG_PASSWORD;
        }

        #endregion

        #region Getter for user profile

        /// <summary>
        ///     Get the current user as UserProfileDTO
        /// </summary>
        /// <returns></returns>
        public virtual UserProfileDTO GetCurrentUser()
        {
            return GetUserProfilebyName(GetCurrentUserName());
        }

        public virtual List<UserProfileDTO> GetAllUserProfiles()
        {
            List<webpages_UserProfile> allProfiles = _uow.UserProfileRepository.GetAll().ToList();
            IEnumerable<webpages_UserProfile> userProfiles =
                allProfiles.Where(u => u.webpages_Roles.Any(r => r.RoleName != "Client"));
            return userProfiles.Select(Mapper.Map<UserProfile, UserProfileDTO>).ToList();
        }

        public virtual UserProfileDTO GetUserProfilebyName(string username)
        {
            if (_uow.UserProfileRepository.Exist(
                u => string.Compare(u.UserName, username, StringComparison.OrdinalIgnoreCase) == 0))
            {
                webpages_UserProfile userProfile = _uow.UserProfileRepository.Single(
                    u => string.Compare(u.UserName, username, StringComparison.OrdinalIgnoreCase) == 0);
                Mapper.CreateMap<UserProfile, UserProfileDTO>();
                return Mapper.Map<UserProfile, UserProfileDTO>(userProfile);
            }
            return null;
        }

        public virtual UserProfileDTO GetUserProfileById(int id)
        {
            if (_uow.UserProfileRepository.Exist(u => u.UserId == id))
            {
                webpages_UserProfile userProfile = _uow.UserProfileRepository.First(
                    u => u.UserId == id);
                Mapper.CreateMap<UserProfile, UserProfileDTO>();
                return Mapper.Map<UserProfile, UserProfileDTO>(userProfile);
            }
            return null;
        }

        #endregion

        #region User profile CRUD operations

        public virtual ErrorCode DeactivateUserProfileByName(string name, bool commit = true)
        {
            if (GetUserProfilebyName(name) == null)
                return ErrorCode.ACCOUNT_USERNAME_NOT_FOUND;

            webpages_UserProfile userProfile = _uow.UserProfileRepository.Single(
                u => u.UserName == name);
            userProfile.UserIsDeactivated = true;

            if (commit)
                _uow.Commit();

            return ErrorCode.NO_ERROR;
        }

        public virtual ErrorCode ReactivateUserProfileByName(string name, bool commit = true)
        {
            if (GetUserProfilebyName(name) == null)
                return ErrorCode.ACCOUNT_USERNAME_NOT_FOUND;

            webpages_UserProfile userProfile = _uow.UserProfileRepository.Single(
                u => u.UserName == name);
            userProfile.UserIsDeactivated = false;

            if (commit)
                _uow.Commit();

            return ErrorCode.NO_ERROR;
        }


        public virtual ErrorCode CreateUserProfile(ref UserProfileDTO userProfileDTO, List<string> roles,
            string password,
            bool createMembership = true)
        {
            int? clientCompanyId = null;
            int? contactPersonId = null;
            int? addressId = null;
            string userName = userProfileDTO.UserName;

            // Check if the account exists. Note that repository is not filtered by tenant. SimpleMembership contains 
            // all the account for all the tenant
            if (_uow.UserProfileRepository.Exist(u => u.UserName == userName, true))
                return ErrorCode.ACCOUNT_EMAIL_ALREADY_EXISTS;

            if (roles == null || roles.Count == 0)
                return ErrorCode.ROLE_MISSING;

            // Screener roles should contain screener category
            var dispatchingSettings = _uow.DispatchingSettingsRepository.GetAll()
                .ToDictionary(e => e.DispatchingSettingsKey, f => f.DispatchingSettingsValue);
            if (roles.Contains(webpages_Roles.kScreenerRole))
            {
                if (String.IsNullOrEmpty(userProfileDTO.ScreenerCategory))
                    return ErrorCode.ACCOUNT_SCREENER_CATEGORY_MISSING;

                userProfileDTO.ScreenerCapabilities =
                    Convert.ToByte(dispatchingSettings[DispatchingSettings.kDefaultScreenerCapabilities]);
                // Defaut screener capabilities
            }

            // Error during address creation
            AddressDTO addressDTO = userProfileDTO.Address;
            if (addressDTO != null && _commonService.CreateAddress(ref addressDTO) != ErrorCode.NO_ERROR)
            {
                LogManager.Instance.Error(
                    string.Format("CreateAddress at: {0}. Name: {1}.",
                        MethodBase.GetCurrentMethod().Name,
                        addressDTO.Street));
                return ErrorCode.UNKNOWN_ERROR;
            }

            ContactInfoDTO contactInfoDTO = userProfileDTO.ContactInfo;
            if (_commonService.CreateContactInfo(ref contactInfoDTO) != ErrorCode.NO_ERROR)
            {
                if (addressDTO != null)
                    _commonService.DeleteAddress(addressDTO.AddressId);
                LogManager.Instance.Error(
                    string.Format("CreateContactInfo at: {0}. ID: {1}",
                        MethodBase.GetCurrentMethod().Name,
                        contactInfoDTO));
                return ErrorCode.UNKNOWN_ERROR;
            }

            ContactPersonDTO emerContactPersonDTO = userProfileDTO.ContactPerson;
            if (emerContactPersonDTO != null &&
                _commonService.CreateContactPerson(ref emerContactPersonDTO) != ErrorCode.NO_ERROR)
            {
                if (contactInfoDTO != null)
                    _commonService.DeleteContactInfo(contactInfoDTO.ContactInfoId);
                if (addressDTO != null)
                    _commonService.DeleteAddress(addressDTO.AddressId);

                LogManager.Instance.Error(
                    string.Format("CreateContactPerson at: {0}. ID: {1}",
                        MethodBase.GetCurrentMethod().Name,
                        emerContactPersonDTO));

                return ErrorCode.UNKNOWN_ERROR;
            }


            // Client company used only for client profile
            if (userProfileDTO.ClientCompanyForClientUserProfile != null)
                clientCompanyId = userProfileDTO.ClientCompanyForClientUserProfile.ClientCompanyId;

            // Address not used with client profile
            if (addressDTO != null)
                addressId = addressDTO.AddressId;

            // Emergency contact not used with client profile
            if (emerContactPersonDTO != null)
                contactPersonId = emerContactPersonDTO.ContactPersonId;


            if (createMembership)
            {
                WebMatrix.WebData.WebSecurity.CreateUserAndAccount(userProfileDTO.UserName, password,
                    new
                    {
                        contactInfoDTO.ContactInfoId,
                        AddressId = addressId,
                        ContactPersonId = contactPersonId,
                        userProfileDTO.FullName,
                        userProfileDTO.Remarks,
                        ClientCompanyId = clientCompanyId,
                        UserIsDeactivated = false,
                        UserProfileTenantId = userProfileDTO.TenantId,
                        userProfileDTO.ScreenerCategory,
                        userProfileDTO.ScreenerCapabilities
                    });
            }
            else
            {
                ClientCompany clientCompany =
                    _uow.ClientCompanyRepository.Exist(u => u.ClientCompanyId == clientCompanyId)
                        ? _uow.ClientCompanyRepository.Single(u => u.ClientCompanyId == clientCompanyId)
                        : null;


                // This is used only for testing. User profile are create in memory
                var userProfileDo = new webpages_UserProfile
                {
                    UserName = userProfileDTO.UserName,
                    FullName = userProfileDTO.FullName,
                    Remarks = userProfileDTO.Remarks,
                    UserPhoto = userProfileDTO.UserPhoto,
                    ScreenerCategory = userProfileDTO.ScreenerCategory,
                    ClientCompanyForClientUserProfile = clientCompany,
                    UserIsDeactivated = false,
                    ScreenerCapabilities = userProfileDTO.ScreenerCapabilities
                };
                if (clientCompany != null)
                    clientCompany.ClientUserProfiles.Add(userProfileDo);
                _uow.UserProfileRepository.Add(userProfileDo);
            }

            foreach (string aRole in roles)
            {
                if (AddUserToRole(userProfileDTO.UserName, aRole, false) != ErrorCode.NO_ERROR)
                    return ErrorCode.UNKNOWN_ERROR;
            }

            // Setup account permission
            GrantPermission(userProfileDTO.UserName);
            _uow.Commit();

            userProfileDTO.UserId = GetUserProfilebyName(userProfileDTO.UserName).UserId;
            return ErrorCode.NO_ERROR;
        }


        public virtual ErrorCode EditUserProfile(ref UserProfileDTO userProfileDTO, List<string> roles,
            bool commit = true)
        {
            int userId = userProfileDTO.UserId;
            if (!_uow.UserProfileRepository.Exist(u => u.UserId == userId))
            {
                return ErrorCode.ACCOUNT_USERID_NOT_FOUND;
            }

            if (roles == null || roles.Count == 0)
                return ErrorCode.ROLE_MISSING;

            // Screener roles should contain screener category
            if (roles.Contains(webpages_Roles.kScreenerRole) && String.IsNullOrEmpty(userProfileDTO.ScreenerCategory))
                return ErrorCode.ACCOUNT_SCREENER_CATEGORY_MISSING;

            webpages_UserProfile user = _uow.UserProfileRepository.Single(u => u.UserId == userId);

            user.ContactInfo.HomePhoneNumber = userProfileDTO.ContactInfo.HomePhoneNumber;
            user.ContactInfo.WorkPhoneNumber = userProfileDTO.ContactInfo.WorkPhoneNumber;
            user.ContactInfo.MobilePhoneNumber = userProfileDTO.ContactInfo.MobilePhoneNumber;
            user.ContactInfo.Position = userProfileDTO.ContactInfo.Position;


            // Edit contact person only for user profile (not for client profile)
            if (user.ContactPerson != null && userProfileDTO.ContactPerson != null)
            {
                user.ContactPerson.ContactPersonName = userProfileDTO.ContactPerson.ContactPersonName;
                user.ContactPerson.ContactInfo.MobilePhoneNumber =
                    userProfileDTO.ContactPerson.ContactInfo.MobilePhoneNumber;
                user.ContactPerson.ContactInfo.WorkPhoneNumber =
                    userProfileDTO.ContactPerson.ContactInfo.WorkPhoneNumber;
            }

            // Edit address only for user profile (not for client profile)
            if (user.Address != null)
            {
                user.Address.Street = userProfileDTO.Address.Street;
                user.Address.PostalCode = userProfileDTO.Address.PostalCode;
                int locationId = userProfileDTO.Address.Location.LocationId;
                Location location = _uow.LocationRepository.Single(l => l.LocationId == locationId);
                user.Address.Location = location;
            }

            user.FullName = userProfileDTO.FullName;
            user.Remarks = userProfileDTO.Remarks;
            user.ScreenerCategory = userProfileDTO.ScreenerCategory;
            user.webpages_Roles.Clear();
            if (roles != null)
            {
                foreach (string aRole in roles)
                {
                    webpages_Roles role =
                        _uow.RoleRepository.Single(
                            r => string.Compare(r.RoleName, aRole, StringComparison.OrdinalIgnoreCase) == 0);
                    user.webpages_Roles.Add(role);
                }
            }

            if (commit)
                _uow.Commit();

            return ErrorCode.NO_ERROR;
        }

        /// <summary>
        ///     Edit personal user profile
        /// </summary>
        /// <param name="userProfileDTO"></param>
        /// <returns></returns>
        public virtual ErrorCode EditPersonalProfile(ref UserProfileDTO userProfileDTO)
        {
            int userId = userProfileDTO.UserId;
            if (!_uow.UserProfileRepository.Exist(u => u.UserId == userId))
            {
                return ErrorCode.ACCOUNT_USERID_NOT_FOUND;
            }

            webpages_UserProfile user = _uow.UserProfileRepository.Single(u => u.UserId == userId);
            user.UserPhoto = userProfileDTO.UserPhoto;
            user.UserPhotoContentType = userProfileDTO.UserPhotoContentType;

            _uow.Commit();
            return ErrorCode.NO_ERROR;
        }


        /// <summary>
        ///     Delete user profile in the database
        /// </summary>
        /// <param name="userProfileDTO"></param>
        /// <param name="commit"></param>
        /// <returns></returns>
        public virtual ErrorCode DeleteUserProfile(UserProfileDTO userProfileDTO, bool commit = true)
        {
            if (!_uow.UserProfileRepository.Exist(u => u.UserId == userProfileDTO.UserId))
            {
                return ErrorCode.ACCOUNT_USERID_NOT_FOUND;
            }

            webpages_UserProfile userProfile = _uow.UserProfileRepository.Single(u => u.UserId == userProfileDTO.UserId);

            // Delete all the role of the user
            List<RolesDTO> roles = GetRolesByUserProfile(userProfile.UserId);
            foreach (RolesDTO role in roles)
            {
                DeleteUserFromRole(userProfile.UserName, role.RoleName, false);
            }

            // Delete all the leave of the user
            IEnumerable<UserLeaveDTO> leaves = GeAllUserLeavesByUserId(userProfile.UserId);
            foreach (UserLeaveDTO leave in leaves)
            {
                DeleteUserLeave(userProfileDTO, leave.UserLeaveId);
            }

            // Delete all the client companies
            userProfile.ClientCompaniesForAM.Clear();

            // Delete addressif it exists 
            if (userProfile.Address != null)
                _commonService.DeleteAddress(userProfile.Address.AddressId, false);

            // Delete contact info
            if (userProfile.ContactInfo != null)
                _commonService.DeleteContactInfo(userProfile.ContactInfo.ContactInfoId, false);

            // Delete contact person if it exists
            if (userProfile.ContactPerson != null)
                _commonService.DeleteContactPerson(userProfile.ContactPerson.ContactPersonId, false);

            // Delete user from user profile table
            _uow.UserProfileRepository.Delete(userProfile);

            if (commit)
                _uow.Commit();

            // Delete user only from membership table
            var membership = (SimpleMembershipProvider) Membership.Provider;
            membership.DeleteAccount(userProfile.UserName);

            return ErrorCode.NO_ERROR;
        }

        private void GrantPermission(string userName)
        {
            IList<CVScreeningCore.Models.Permission> permissions = new List<CVScreeningCore.Models.Permission>();
            webpages_UserProfile userProfile = _uow.UserProfileRepository.First(u => u.UserName == userName);

            if (userProfile.IsClient())
            {
                userProfile.GrantPermissionForClient(userProfile.ClientCompanyForClientUserProfile);
            }
        }

        #endregion

        #region Client profile CRUD operations

        public IEnumerable<UserProfileDTO> GetAllUserScreeners()
        {
            IEnumerable<webpages_UserProfile> screeners = _uow.UserProfileRepository.GetAll()
                .Where(e => e.webpages_Roles.Any(f => f.RoleName.Equals(webpages_Roles.kScreenerRole)));
            return screeners.Select(Mapper.Map<webpages_UserProfile, UserProfileDTO>);
        }

        /// <summary>
        ///     Create client profile account
        /// </summary>
        /// <param name="userProfileDTO"></param>
        /// <param name="clientCompanyDTO"></param>
        /// <param name="password"></param>
        /// <param name="createMembership"></param>
        /// <returns></returns>
        public virtual ErrorCode CreateClientProfile(
            ref UserProfileDTO userProfileDTO, ClientCompanyDTO clientCompanyDTO, string password,
            bool createMembership = true)
        {
            if (!_uow.ClientCompanyRepository.Exist(u => u.ClientCompanyId == clientCompanyDTO.ClientCompanyId))
            {
                return ErrorCode.CLIENT_COMPANY_NOT_FOUND;
            }

            userProfileDTO.ClientCompanyForClientUserProfile = clientCompanyDTO;
            return CreateUserProfile(ref userProfileDTO, new List<string> {"Client"}, password, createMembership);
        }


        /// <summary>
        ///     Edit client profile account
        /// </summary>
        /// <param name="userProfileDTO"></param>
        /// <param name="roles"></param>
        /// <param name="commit"></param>
        /// <returns></returns>
        public virtual ErrorCode EditClientProfile(ref UserProfileDTO userProfileDTO, bool commit = true)
        {
            return EditUserProfile(ref userProfileDTO, new List<string> {"Client"}, commit);
        }

        /// <summary>
        ///     Deactivate client profile account
        /// </summary>
        /// <param name="name"></param>
        /// <param name="commit"></param>
        /// <returns></returns>
        public virtual ErrorCode DeactivateClientProfileByName(string name, bool commit = true)
        {
            return DeactivateUserProfileByName(name, commit);
        }

        public virtual ErrorCode DeactivateClientProfileById(int id, bool commit = true)
        {
            string name = GetUserProfileById(id).UserName;
            return DeactivateUserProfileByName(name, commit);
        }

        /// <summary>
        ///     Reactivate client profile account
        /// </summary>
        /// <param name="name"></param>
        /// <param name="commit"></param>
        /// <returns></returns>
        public virtual ErrorCode ReactivateClientProfileByName(string name, bool commit = true)
        {
            return ReactivateUserProfileByName(name, commit);
        }

        public virtual ErrorCode ReactivateClientProfileById(int id, bool commit = true)
        {
            string name = GetUserProfileById(id).UserName;
            return ReactivateUserProfileByName(name, commit);
        }


        /// <summary>
        ///     Delete client profile account
        /// </summary>
        /// <param name="userProfileDTO"></param>
        /// <param name="commit"></param>
        /// <returns></returns>
        public virtual ErrorCode DeleteClientProfile(UserProfileDTO userProfileDTO, bool commit = true)
        {
            return DeleteUserProfile(userProfileDTO, commit);
        }

        #endregion
        
        #region Getter for client profile

        public virtual List<UserProfileDTO> GetAllClientProfiles()
        {
            List<webpages_UserProfile> allProfiles = _uow.UserProfileRepository.GetAll().ToList();
            IEnumerable<webpages_UserProfile> userProfiles =
                allProfiles.Where(u => u.webpages_Roles.Any(r => r.RoleName == "Client"));
            return userProfiles.Select(Mapper.Map<UserProfile, UserProfileDTO>).ToList();
        }

        /// <summary>
        ///     Get client profile by name
        /// </summary>
        /// <param name="username"></param>
        /// <returns></returns>
        public virtual UserProfileDTO GetClientProfilebyName(string username)
        {
            return GetUserProfilebyName(username);
        }


        /// <summary>
        ///     Get client profile by id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public virtual UserProfileDTO GetClientProfileById(int id)
        {
            return GetUserProfileById(id);
        }

        #endregion

        #region Roles

        /// <summary>
        ///     Create a role
        /// </summary>
        /// <param name="role">Role name to create</param>
        /// <param name="commit"></param>
        /// <returns></returns>
        public virtual ErrorCode CreateRole(string role, bool commit = true)
        {
            if (_uow.RoleRepository.Exist(r => r.RoleName == role))
                return ErrorCode.ROLE_ALREADY_EXISTS;

            var roleDo = new webpages_Roles
            {
                RoleName = role
            };

            _uow.RoleRepository.Add(roleDo);
            
            if (commit)
                _uow.Commit();

            return ErrorCode.NO_ERROR;
        }


        /// <summary>
        ///     Delete a role
        /// </summary>
        /// <param name="role">Role name to delete</param>
        /// <param name="commit"></param>
        /// <returns></returns>
        public virtual ErrorCode DeleteRole(string role, bool commit = true)
        {
            if (!_uow.RoleRepository.Exist(r => r.RoleName == role))
                return ErrorCode.ROLE_NOT_FOUND;

            webpages_Roles roleDo = _uow.RoleRepository.Single(r => r.RoleName == role);

            // Remove link to all the user that are associated to this role
            List<UserProfileDTO> userProfilesDTO = GetUserProfilesByRoles(role);
            foreach (UserProfileDTO userProfile in userProfilesDTO)
            {
                int id = userProfile.UserId;
                webpages_UserProfile userProfileDo = _uow.UserProfileRepository.Single(r => r.UserId == id);
                userProfileDo.webpages_Roles.Remove(roleDo);
            }

            _uow.RoleRepository.Delete(roleDo);

            if (commit) 
                _uow.Commit();

            return ErrorCode.NO_ERROR;
        }

        /// <summary>
        ///     Retrieve all the roles existing in the applications
        /// </summary>
        /// <returns></returns>
        public virtual List<RolesDTO> GetAllRoles(bool includeClientRole = true)
        {
            IEnumerable<webpages_Roles> roles = includeClientRole
                ? _uow.RoleRepository.GetAll()
                : _uow.RoleRepository.Find(r => r.RoleName != "Client");

            Mapper.CreateMap<Roles, RolesDTO>();
            return roles.Select(Mapper.Map<Roles, RolesDTO>).ToList();
        }

        /// <summary>
        ///     Get role by Id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public virtual RolesDTO GetAllRoleById(int id)
        {
            if (!_uow.RoleRepository.Exist(r => r.RoleId == id))
                return null;

            webpages_Roles role = _uow.RoleRepository.Single(r => r.RoleId == id);

            return Mapper.Map<Roles, RolesDTO>(role);
        }


        /// <summary>
        ///     Retrieve all the user profile that belongs to a role
        /// </summary>
        /// <param name="roleName"></param>
        /// <returns></returns>
        public virtual List<UserProfileDTO> GetUserProfilesByRoles(string roleName)
        {
            if (!_uow.RoleRepository.Exist(r => r.RoleName == roleName))
                return null;

            webpages_Roles role =
                _uow.RoleRepository.Single(
                    r => string.Compare(r.RoleName, roleName, StringComparison.OrdinalIgnoreCase) == 0);

            IEnumerable<webpages_UserProfile> userProfiles = _uow.UserProfileRepository.GetAll()
                .Where(u => u.webpages_Roles.Contains(role));

            Mapper.CreateMap<UserProfile, UserProfileDTO>();
            return userProfiles.Select(Mapper.Map<UserProfile, UserProfileDTO>).ToList();
        }

        /// <summary>
        ///     Retrieve all the roles that belongs to an user
        /// </summary>
        /// <param name="userProfileId"></param>
        /// <returns></returns>
        public virtual List<RolesDTO> GetRolesByUserProfile(int userProfileId)
        {
            if (GetUserProfileById(userProfileId) == null)
                return null;

            webpages_UserProfile userProfile =
                _uow.UserProfileRepository.Single(u => u.UserId == userProfileId);

            return userProfile.webpages_Roles.Select(Mapper.Map<Roles, RolesDTO>).ToList();
        }

        /// <summary>
        ///     Retrieve all the roles that belongs to an user and returns them as string
        /// </summary>
        /// <param name="userProfileId"></param>
        /// <returns></returns>
        public virtual string GetRolesByUserProfileAsString(int userProfileId)
        {
            if (GetUserProfileById(userProfileId) == null)
                return null;

            string aStr = "";
            bool aFirst = true;

            webpages_UserProfile userProfile =
                _uow.UserProfileRepository.Single(u => u.UserId == userProfileId);
            IEnumerable<webpages_Roles> roles = _uow.RoleRepository.GetAll()
                .Where(r => r.webpages_UserProfile.Contains(userProfile));

            foreach (webpages_Roles role in roles)
            {
                if (!aFirst)
                    aStr += ", " + role.RoleName;
                else
                {
                    aStr += role.RoleName;
                    aFirst = false;
                }
            }
            return aStr;
        }

        /// <summary>
        ///     Check whether an username account belongs to a role
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="role"></param>
        /// <returns></returns>
        public virtual bool? IsUserInRole(string userName, string role)
        {
            if (GetUserProfilebyName(userName) == null)
                return null;

            if (!_uow.RoleRepository.Exist(r => r.RoleName == role))
                return null;

            webpages_UserProfile userProfile =
                _uow.UserProfileRepository.Single(u => u.UserName == userName);
            List<RolesDTO> roles = GetRolesByUserProfile(userProfile.UserId);

            return roles.Any(dto => dto.RoleName == role);
        }

        /// <summary>
        ///     Add user to a role using SimpleMembership
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="role"></param>
        /// <param name="commit"></param>
        /// ///
        public virtual ErrorCode AddUserToRole(string userName, string role, bool commit = true)
        {
            if (GetUserProfilebyName(userName) == null)
                return ErrorCode.ACCOUNT_USERNAME_NOT_FOUND;

            if (!_uow.RoleRepository.Exist(r => r.RoleName == role))
                return ErrorCode.ROLE_NOT_FOUND;

            if (IsUserInRole(userName, role) != null && IsUserInRole(userName, role) == true)
                return ErrorCode.ROLE_ALREADY_BELONGS_TO_USER;

            webpages_UserProfile userProfile = _uow.UserProfileRepository.Single(
                u => string.Compare(u.UserName, userName, StringComparison.OrdinalIgnoreCase) == 0);

            webpages_Roles roleDomain = _uow.RoleRepository.Single(r => r.RoleName == role);
            userProfile.webpages_Roles.Add(roleDomain);

            if (commit)
                _uow.Commit();


            return ErrorCode.NO_ERROR;
        }


        /// <summary>
        ///     Remove user to a role
        /// </summary>
        /// <param name="userName">Username account</param>
        /// <param name="role">Role that needs to be removed </param>
        /// <returns></returns>
        public virtual ErrorCode DeleteUserFromRole(string userName, string role, bool commit = true)
        {
            if (GetUserProfilebyName(userName) == null)
                return ErrorCode.ACCOUNT_USERNAME_NOT_FOUND;

            if (!_uow.RoleRepository.Exist(r => r.RoleName == role))
                return ErrorCode.ROLE_NOT_FOUND;

            if (IsUserInRole(userName, role) != null && IsUserInRole(userName, role) == false)
                return ErrorCode.ROLE_NOT_BELONGS_TO_USER;

            webpages_UserProfile userProfile = _uow.UserProfileRepository.Single(
                u => string.Compare(u.UserName, userName, StringComparison.OrdinalIgnoreCase) == 0);

            webpages_Roles roleDomain = _uow.RoleRepository.Single(r => r.RoleName == role);
            userProfile.webpages_Roles.Remove(roleDomain);

            if (commit)
                _uow.Commit();

            return ErrorCode.NO_ERROR;
        }

        #endregion

        #region Leave

        /// Leave management
        /// CRUD method
        public virtual IEnumerable<UserLeaveDTO> GeAllUserLeavesByUserId(int userId)
        {
            if (!_uow.UserProfileRepository.Exist(u => u.UserId == userId))
            {
                return null;
            }

            IEnumerable<UserLeave> userLeaves =
                _uow.UserLeaveRepository.GetAll().Where(ul => ul.webpages_UserProfile.UserId == userId);
            userLeaves = userLeaves.OrderBy(u => u.UserLeaveStartDate);
            return userLeaves.Select(Mapper.Map<UserLeave, UserLeaveDTO>).ToList();
        }

        /// <summary>
        ///     Service - Create a new leave and add it to user profile
        /// </summary>
        /// <param name="userProfileDTO"></param>
        /// <param name="userLeaveDTO"></param>
        /// <param name="commit"></param>
        /// <returns></returns>
        public virtual ErrorCode CreateLeave(UserProfileDTO userProfileDTO, ref UserLeaveDTO userLeaveDTO,
            bool commit = true)
        {
            int userId = userProfileDTO.UserId;
            if (!_uow.UserProfileRepository.Exist(u => u.UserId == userId))
            {
                return ErrorCode.ACCOUNT_USERID_NOT_FOUND;
            }

            webpages_UserProfile userProfile = _uow.UserProfileRepository.Single(u => u.UserId == userId);
            var userLeave = new UserLeave
            {
                UserLeaveName = userLeaveDTO.UserLeaveName,
                UserLeaveRemarks = userLeaveDTO.UserLeaveRemarks,
                UserLeaveStartDate = userLeaveDTO.UserLeaveStartDate.Date,
                UserLeaveEndDate = userLeaveDTO.UserLeaveEndDate.Date
            };


            IEnumerable<UserLeave> userLeaveByUser = _uow.UserLeaveRepository.Find(
                u => u.webpages_UserProfile.UserId == userProfile.UserId);

            DateTime startDate = userLeaveDTO.UserLeaveStartDate;
            DateTime endDate = userLeaveDTO.UserLeaveEndDate;
            IEnumerable<UserLeave> userLeaveOverlap =
                userLeaveByUser.Where(
                    u => u.UserLeaveStartDate <= endDate
                         &&
                         u.UserLeaveEndDate >= startDate);

            if (userLeaveOverlap.Any())
            {
                return ErrorCode.USER_LEAVE_DATE_OVERLAPPING;
            }

            userLeave.webpages_UserProfile = userProfile;
            userProfile.UserLeave.Add(_uow.UserLeaveRepository.Add(userLeave));

            if (!commit)
                return ErrorCode.NO_ERROR;

            _uow.Commit();
            userLeaveDTO.UserLeaveId = userLeave.UserLeaveId;
            return ErrorCode.NO_ERROR;
        }

        public virtual ErrorCode EditLeave(UserLeaveDTO userLeaveDTO, bool commit = true)
        {
            if (!_uow.UserLeaveRepository.Exist(u => u.UserLeaveId == userLeaveDTO.UserLeaveId))
            {
                return ErrorCode.USER_LEAVE_NOT_FOUND;
            }

            UserLeave userLeave = _uow.UserLeaveRepository.Single(u => u.UserLeaveId == userLeaveDTO.UserLeaveId);

            IEnumerable<UserLeave> userLeaveByUser = _uow.UserLeaveRepository.Find(
                u => u.webpages_UserProfile.UserId == userLeave.webpages_UserProfile.UserId);

            IEnumerable<UserLeave> userLeaveOverlap =
                userLeaveByUser.Where(
                    u => u.UserLeaveStartDate <= userLeaveDTO.UserLeaveEndDate
                         &&
                         u.UserLeaveEndDate >= userLeaveDTO.UserLeaveStartDate
                         &&
                         u.UserLeaveId != userLeave.UserLeaveId
                    );

            if (userLeaveOverlap.Any())
            {
                return ErrorCode.USER_LEAVE_DATE_OVERLAPPING;
            }


            userLeave.UserLeaveName = userLeaveDTO.UserLeaveName;
            userLeave.UserLeaveRemarks = userLeaveDTO.UserLeaveRemarks;
            userLeave.UserLeaveStartDate = userLeaveDTO.UserLeaveStartDate.Date;
            userLeave.UserLeaveEndDate = userLeaveDTO.UserLeaveEndDate.Date;

            if (commit)
                _uow.Commit();

            return ErrorCode.NO_ERROR;
        }

        /// <summary>
        ///     Get user leave id id
        /// </summary>
        /// <param name="userLeaveId"></param>
        /// <returns></returns>
        public virtual UserLeaveDTO GetUserLeave(int userLeaveId)
        {
            if (!_uow.UserLeaveRepository.Exist(u => u.UserLeaveId == userLeaveId))
            {
                return null;
            }

            UserLeave userLeave = _uow.UserLeaveRepository.Single(u => u.UserLeaveId == userLeaveId);
            return Mapper.Map<UserLeave, UserLeaveDTO>(userLeave);
        }

        /// <summary>
        ///     Delete leave of a specific user
        /// </summary>
        /// <param name="userProfileDTO"></param>
        /// <param name="userLeaveId"></param>
        /// <param name="commit"></param>
        public virtual ErrorCode DeleteUserLeave(UserProfileDTO userProfileDTO, int userLeaveId, bool commit = true)
        {
            if (!_uow.UserProfileRepository.Exist(u => u.UserId == userProfileDTO.UserId))
            {
                return ErrorCode.ACCOUNT_USERID_NOT_FOUND;
            }

            if (!_uow.UserLeaveRepository.Exist(u => u.UserLeaveId == userLeaveId))
            {
                return ErrorCode.USER_LEAVE_NOT_FOUND;
            }

            webpages_UserProfile userProfile = _uow.UserProfileRepository.Single(u => u.UserId == userProfileDTO.UserId);
            UserLeave userLeave = _uow.UserLeaveRepository.Single(u => u.UserLeaveId == userLeaveId);

            userProfile.UserLeave.Remove(userLeave);
            _uow.UserLeaveRepository.Delete(userLeave);

            if (commit) _uow.Commit();

            return ErrorCode.NO_ERROR;
        }

        /// <summary>
        ///     Retrieve the humber of days for the current to its next leave
        /// </summary>
        /// <param name="userProfileDTO"></param>
        /// <returns></returns>
        public int RetrieveNumberOfWorkingDaysToNextLeave(UserProfileDTO userProfileDTO)
        {
            Contract.Assume(userProfileDTO.UserId > 0, "User profile not found");

            webpages_UserProfile userProfile = _uow.UserProfileRepository.Single(u => u.UserId == userProfileDTO.UserId);

            // Retrieve the first user leave available for this user

            IEnumerable<UserLeave> currentUserLeaves =
                _uow.UserLeaveRepository.Find(u => u.webpages_UserProfile.UserId == userProfile.UserId).ToList();

            currentUserLeaves = currentUserLeaves.Where(u => u.UserLeaveStartDate <= _systemTimeService.GetCurrentDateTime()
                                                   && u.UserLeaveEndDate >= _systemTimeService.GetCurrentDateTime());

            if (currentUserLeaves.Any())
                // Someone that is now in leave is not available, even if he comes back tomorrow
            {
                return 0;
            }

            IEnumerable<UserLeave> userLeaves =
                _uow.UserLeaveRepository.Find(u => u.webpages_UserProfile.UserId == userProfile.UserId).ToList();
            userLeaves = userLeaves.Where(u => u.UserLeaveStartDate > _systemTimeService.GetCurrentDateTime());

            if (!userLeaves.Any()) // No user leave for user, no leave during the year
            {
                return 365;
            }

            UserLeave nextUserLeave = userLeaves.OrderBy(u => u.UserLeaveStartDate).First();

            // Retrieve difference in working days between tomorrow and next user leave. 
            // The availibility weight is the same if the screener is in holiday for 1 day or 10 days. Only the starting date is important
            // Public holidays are excluded from the calculation
            return DateHelper.GetWorkingDaysDifference(_systemTimeService.GetCurrentDateTime().AddDays(1),
                nextUserLeave.UserLeaveStartDate, new List<PublicHolidayDTO>());
        }

        #endregion
    }
}