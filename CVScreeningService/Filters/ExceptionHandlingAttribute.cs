using CVScreeningService.Interceptor;
using Ninject;
using Ninject.Extensions.Interception;
using Ninject.Extensions.Interception.Attributes;
using Ninject.Extensions.Interception.Request;

namespace CVScreeningService.Filters
{

    public class ExceptionHandlingAttribute : InterceptAttribute
    {
        public override IInterceptor CreateInterceptor(IProxyRequest request)
        {
            return request.Kernel.Get<ExceptionInterceptor>();
        }
    }

}
