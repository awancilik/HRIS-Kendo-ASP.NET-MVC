using System.Diagnostics;
using Nalysa.Common.Log;
using Ninject.Extensions.Interception;

namespace CVScreeningService.Interceptor
{
    public class LoggingInterceptor : IInterceptor
    {
        public LoggingInterceptor()
        {
        }

        public void Intercept(IInvocation invocation)
        {
            var sw = new Stopwatch();
            sw.Start();
            LogManager.Instance.Debug(string.Format("Entering service method: {0} ...", invocation.Request.Method));
            invocation.Proceed();
            sw.Stop();
            LogManager.Instance.Debug(string.Format("Ending service method: {0} ... Duration in ms: {1}", invocation.Request.Method, sw.ElapsedMilliseconds));
        }
    }
}