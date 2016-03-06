using System;
using System.Linq;
using System.Text;
using CVScreeningCore.Error;
using Nalysa.Common.Log;
using Newtonsoft.Json;
using Ninject.Extensions.Interception;
using Newtonsoft.Json.Linq;

namespace CVScreeningService.Interceptor
{
    public class ExceptionInterceptor : IInterceptor
    {
        public ExceptionInterceptor()
        {
        }

        /// <summary>
        /// Flat exception to display readable information
        /// </summary>
        /// <param name="exception"></param>
        /// <returns></returns>
        public static string FlattenException(Exception exception)
        {
            var stringBuilder = new StringBuilder();
            while (exception != null)
            {
                stringBuilder.AppendLine(exception.Message);
                stringBuilder.AppendLine(exception.StackTrace);
                exception = exception.InnerException;
            }
            return stringBuilder.ToString();
        }

        public static string DisplayObjectProperties(object obj)
        {
            if (obj.GetType().IsPrimitive)
                return obj.ToString();

            return JObject.FromObject(obj, new JsonSerializer()
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            }).ToString();

        }

        public static void SetReturnValue(ref IInvocation invocation)
        {   
            // Set return value. All the service getter returns null if an error appears and all other
            // service method
            if (invocation.Request.Method.ReturnType == typeof(ErrorCode))
            {
                invocation.ReturnValue = ErrorCode.UNKNOWN_ERROR;
            }
            else
            {
                invocation.ReturnValue = null;
            }
        }

        public void Intercept(IInvocation invocation)
        {
            try
            {
                invocation.Proceed();
            }
            catch (Exception ex)
            {
                string arguments = 
                    invocation.Request.Arguments.Aggregate(Environment.NewLine, (current, argument) => string.Format("{0}Argument {1}: {2}\n", current, argument.GetType(), DisplayObjectProperties(argument)));
                LogManager.Instance.Error(
                    string.Format("Service exception thrown: Method: {0}:\n{1}", 
                    invocation.Request.Method.Name, arguments));
                LogManager.Instance.Error(
                    string.Format("Stack exception: {0}", FlattenException(ex)));

                SetReturnValue(ref invocation);
            }
        }
    }
}