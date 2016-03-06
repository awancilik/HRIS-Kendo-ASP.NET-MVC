using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using CVScreeningCore.Models;
using CVScreeningDAL.UnitOfWork;
using CVScreeningService.Filters;
using Nalysa.Common.Log;
using WebMatrix.WebData;


namespace CVScreeningService.Services.Permission
{
    [Logging(Order = 1), ExceptionHandling(Order = 2)]
    public class PermissionService : IPermissionService
    {
        private readonly IUnitOfWork _uow;
        private string _currentUserName;

        public PermissionService(IUnitOfWork uow)
        {
            _uow = uow;
            _currentUserName = WebSecurity.CurrentUserName;
        }

        public PermissionService(IUnitOfWork uow, string userName)
        {
            _uow = uow;
            _currentUserName = userName;
        }

        public virtual void SwitchCurrentUser(string userName, int userId)
        {
            _currentUserName = userName;
        }

        /// <summary>
        /// Check if user is granted to access to an object
        /// </summary>
        /// <param name="permissionName">Permission name</param>
        /// <param name="objectId">Object id</param>
        /// <returns></returns>
        public virtual bool IsGranted(string permissionName, int? objectId)
        {
            LogManager.Instance.Info(string.Format("Permission info: user:{0}, permission:{1}, object id:{2} ...",
                 _currentUserName, permissionName, objectId));

            if (!_uow.UserProfileRepository.Exist(u => u.UserName == _currentUserName))
            {
                return false;
            }

            var userProfile = _uow.UserProfileRepository.Single(u => u.UserName == _currentUserName);
            return CheckPermission(userProfile, permissionName, objectId);
        }

        /// <summary>
        /// Check if user is granted to access to an object
        /// </summary>
        /// <param name="userProfile"></param>
        /// <param name="permissionName"></param>
        /// <param name="objectId"></param>
        /// <returns></returns>
        private bool CheckPermission(
            webpages_UserProfile userProfile, string permissionName, int? objectId)
        {
            // When role is granted, object id is null
            // When user is granted, object id is set
            IEnumerable<CVScreeningCore.Models.Permission> permissions = null;
            var roles = userProfile.webpages_Roles;
            var rolesAsArray = roles.Select(r => r.RoleId).ToArray();

            if (userProfile.IsAdministrator())
                return true;

            switch (permissionName)
            {
                // Permission on screening
                case CVScreeningCore.Models.Permission.kScreeningManagePermission:
                case CVScreeningCore.Models.Permission.kScreeningViewPermission:
                case CVScreeningCore.Models.Permission.kScreeningDeactivatePermission:
                case CVScreeningCore.Models.Permission.kReportUploadPermission:
                    permissions = _uow.PermissionRepository.Find(p => 
                        p.PermissionName == permissionName &&
                        ((p.Screening != null && p.Screening.ScreeningId == objectId && p.UserProfile != null && p.UserProfile.UserId == userProfile.UserId) ||
                            (p.Screening == null && p.Roles!= null && rolesAsArray.Contains(p.Roles.RoleId))));

                    break;

                // Permission on atomic check
                case CVScreeningCore.Models.Permission.kAtomicCheckManagePermission:
                case CVScreeningCore.Models.Permission.kAtomicCheckViewPermission:
                case CVScreeningCore.Models.Permission.kAtomicCheckAssignPermission:
                    permissions = _uow.PermissionRepository.Find(p =>
                        p.PermissionName == permissionName &&
                        ((p.AtomicCheck != null && p.AtomicCheck.AtomicCheckId == objectId && p.UserProfile != null && p.UserProfile.UserId == userProfile.UserId) ||
                            (p.AtomicCheck == null && p.Roles != null && rolesAsArray.Contains(p.Roles.RoleId))));
                    break;

                // Permission on report
                case CVScreeningCore.Models.Permission.kReportManagePermission:
                case CVScreeningCore.Models.Permission.kReportViewPermission:
                    permissions = _uow.PermissionRepository.Find(p =>
                        p.PermissionName == permissionName &&
                        ((p.ScreeningReport != null && p.ScreeningReport.ScreeningReportId == objectId && p.UserProfile != null && p.UserProfile.UserId == userProfile.UserId) ||
                            (p.ScreeningReport == null && p.Roles != null && rolesAsArray.Contains(p.Roles.RoleId))));
                    break;

                // Permission on discussion
                case CVScreeningCore.Models.Permission.kExternalScreeningDiscussionManagePermission:
                case CVScreeningCore.Models.Permission.kInternalScreeningDiscussionManagePermission:
                case CVScreeningCore.Models.Permission.kInternalAtomicCheckDiscussionManagePermission:
                    permissions = _uow.PermissionRepository.Find(p =>
                        p.PermissionName == permissionName &&
                        ((p.Discussion != null && p.Discussion.DiscussionId == objectId && p.UserProfile != null && p.UserProfile.UserId == userProfile.UserId) ||
                            (p.Discussion == null && p.Roles != null && rolesAsArray.Contains(p.Roles.RoleId))));
                    break;

                // Permission on contract
                case CVScreeningCore.Models.Permission.kContractViewPermission:
                    permissions = _uow.PermissionRepository.Find(p =>
                        p.PermissionName == permissionName &&
                        ((p.Contract != null && p.Contract.ContractId == objectId && p.UserProfile != null && p.UserProfile.UserId == userProfile.UserId) ||
                            (p.Contract == null && p.Roles != null && rolesAsArray.Contains(p.Roles.RoleId))));
                    break;

                // Permission on screening level
                case CVScreeningCore.Models.Permission.kScreeningLevelViewPermission:
                    permissions = _uow.PermissionRepository.Find(p =>
                        p.PermissionName == permissionName &&
                        ((p.ScreeningLevel != null && p.ScreeningLevel.ScreeningLevelId == objectId && p.UserProfile != null && p.UserProfile.UserId == userProfile.UserId) ||
                            (p.ScreeningLevel == null && p.Roles != null && rolesAsArray.Contains(p.Roles.RoleId))));
                    break;

                // Permission on screening level version
                case CVScreeningCore.Models.Permission.kScreeningCreatePermission:
                case CVScreeningCore.Models.Permission.kScreeningLevelVersionViewPermission:
                    permissions = _uow.PermissionRepository.Find(p =>
                        p.PermissionName == permissionName &&
                        ((p.ScreeningLevelVersion != null && p.ScreeningLevelVersion.ScreeningLevelVersionId == objectId && p.UserProfile != null && p.UserProfile.UserId == userProfile.UserId) ||
                            (p.ScreeningLevelVersion == null && p.Roles != null && rolesAsArray.Contains(p.Roles.RoleId))));
                    break;
            }
            
            if (permissions != null && permissions.Any())
                return permissions.Any(p => p.PermissionIsGranted);
            return false;
        }

        public bool HasAccessFromRoles(string permissionName)
        {
            var userProfile = _uow.UserProfileRepository.Single(u => u.UserName == _currentUserName);
            if (userProfile.IsAdministrator())
                return true;

            var roles = userProfile.webpages_Roles;
            var rolesAsArray = roles.Select(r => r.RoleId).ToArray();

            return _uow.PermissionRepository.Exist(p => p.PermissionName == permissionName &&
                            (p.Roles != null && rolesAsArray.Contains(p.Roles.RoleId)));
        }


        /// <summary>
        /// Generate Linq to Entities query for screening
        /// </summary>
        /// <param name="permissionName"></param>
        /// <returns></returns>
        public Expression<Func<CVScreeningCore.Models.Screening, bool>> GenerateScreeningPermissionQuery(
            string permissionName)
        {
            var userProfile = _uow.UserProfileRepository.Single(u => u.UserName == _currentUserName);

            switch (permissionName)
            {
                    // Permission on screening
                case CVScreeningCore.Models.Permission.kScreeningManagePermission:
                case CVScreeningCore.Models.Permission.kScreeningViewPermission:
                case CVScreeningCore.Models.Permission.kScreeningDeactivatePermission:
                case CVScreeningCore.Models.Permission.kReportUploadPermission:
                    return s => s.Permission.Any(
                            p => p.PermissionName == permissionName &&
                                 ((p.Screening != null && p.Screening == s && p.UserProfile != null
                                   && p.UserProfile.UserId == userProfile.UserId)));
            }
            throw new ApplicationException("Bad parameter for GenerateScreeningPermissionQuery");
        }

        /// <summary>
        /// Generate Linq to Entities query for atomic check
        /// </summary>
        /// <param name="permissionName"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        public Expression<Func<AtomicCheck, bool>> GenerateAtomicCheckPermissionQuery(string permissionName)
        {
            var userProfile = _uow.UserProfileRepository.Single(u => u.UserName == _currentUserName);

            switch (permissionName)
            {
                // Permission on atomic check
                case CVScreeningCore.Models.Permission.kAtomicCheckManagePermission:
                case CVScreeningCore.Models.Permission.kAtomicCheckViewPermission:
                case CVScreeningCore.Models.Permission.kAtomicCheckAssignPermission:
                    return a => a.Permission.Any(
                            p => p.PermissionName == permissionName &&
                                 ((p.AtomicCheck != null && p.AtomicCheck == a && p.UserProfile != null
                                   && p.UserProfile.UserId == userProfile.UserId)));
            }

            throw new ApplicationException("Bad parameter for GenerateAtomicCheckPermissionQuery");
        }







    }
}