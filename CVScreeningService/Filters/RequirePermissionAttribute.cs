using CVScreeningService.Interceptor;
using Ninject;
using Ninject.Extensions.Interception;
using Ninject.Extensions.Interception.Attributes;
using Ninject.Extensions.Interception.Request;

namespace CVScreeningService.Filters
{

    public class RequirePermissionAttribute : InterceptAttribute
    {
        /// <summary>
        /// Type of object to check permission  
        /// </summary>
        
        readonly string _permissionName;
        
        public RequirePermissionAttribute(string permissionName)
		{
			// Set the object type
            this._permissionName = permissionName;
		}

        public override IInterceptor CreateInterceptor(IProxyRequest request)
        {
            var param = new Ninject.Parameters.ConstructorArgument("permissionName", _permissionName);
            return request.Kernel.Get<RequirePermissionInterceptor>(param, param);
        }
    }

}
