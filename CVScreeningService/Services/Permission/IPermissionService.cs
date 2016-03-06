using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using CVScreeningCore.Models;

namespace CVScreeningService.Services.Permission
{
    public interface IPermissionService
    {

        /// <summary>
        /// Switch current user to check permission for
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="userName"></param>
        void SwitchCurrentUser(string userName, int userId);

        /// <summary>
        /// Check if user is granted to access to an object
        /// </summary>
        /// <param name="permissionName">Permission name</param>
        /// <param name="objectId">Object id</param>
        /// <returns></returns>
        bool IsGranted(string permissionName, int? objectId);


        /// <summary>
        /// Check if user has access using role
        /// </summary>
        /// <param name="permissionName"></param>
        /// <returns></returns>
        bool HasAccessFromRoles(string permissionName);


        /// <summary>
        /// Generate Linq to Entities query for screening permission
        /// </summary>
        /// <param name="permissionName"></param>
        /// <returns></returns>
        Expression<Func<CVScreeningCore.Models.Screening, bool>> GenerateScreeningPermissionQuery(
            string permissionName);

        /// <summary>
        /// Generate Linq to Entities query for atomic check permission
        /// </summary>
        /// <param name="permissionName"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        Expression<Func<AtomicCheck, bool>> GenerateAtomicCheckPermissionQuery(
            string permissionName);
    }
}