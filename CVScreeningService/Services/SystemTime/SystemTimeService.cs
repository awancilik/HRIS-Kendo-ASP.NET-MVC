using System;
using CVScreeningService.Filters;

namespace CVScreeningService.Services.SystemTime
{
    [Logging(Order = 1), ExceptionHandling(Order = 2)]
    public class SystemTimeService : ISystemTimeService
    {
        public SystemTimeService()
        {

        }

        public virtual DateTime GetCurrentDateTime()
        {
            return DateTime.Now;
        }

    }
}