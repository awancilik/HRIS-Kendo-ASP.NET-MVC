using System;
using System.Linq;
using CVScreeningCore.Exception;
using CVScreeningService.Filters;
using CVScreeningService.Services.Permission;
using CVScreeningService.Services.UserManagement;
using Nalysa.Common.Log;
using Ninject.Extensions.Interception;

namespace CVScreeningService.Interceptor
{
    public class RequirePermissionInterceptor : IInterceptor
    {
        private readonly IPermissionService _permissionService;
        private readonly IUserManagementService _userManagementService;

        /// <summary>
        /// Permission(s) to check, separate by comma
        /// </summary>
        readonly string _permissionName;

        /// <summary>
        /// List of permission to check
        /// </summary>
        private string[] _permissionNames;

        public RequirePermissionInterceptor(string permissionName, 
            IPermissionService permissionService, 
            IUserManagementService userManagementService)
		{
            // Set the permission name
            this._permissionName = permissionName;
            this._permissionNames = permissionName.Split(',');
            this._permissionService = permissionService;
            this._userManagementService = userManagementService;
		}

        public void Intercept(IInvocation invocation)
        {
            int? objectId = null;
            var property = invocation.Request.Arguments.First().GetType().GetProperties().FirstOrDefault(
                p => Attribute.IsDefined(p, typeof(ObjectIdAttribute)));

            if (property != null)
                objectId = property.GetValue(invocation.Request.Arguments.First(), null) as int?;
            else
                objectId = invocation.Request.Arguments.First() as int?;


            // Access to object denied
            LogManager.Instance.Info(string.Format("Checking permission for method: {0} ...", invocation.Request.Method));
            // Loop on all the list to permission, access is allowed if at least one permission is granted
            if (!_permissionNames.Any(p => _permissionService.IsGranted(p, objectId)))
            {
                LogManager.Instance.Info(string.Format("Access to object DENIED"));
                throw new ExceptionNotAuthorized();
            }
            LogManager.Instance.Debug(string.Format("Access to object ALLOWED"));
            invocation.Proceed();

        }
    }
}