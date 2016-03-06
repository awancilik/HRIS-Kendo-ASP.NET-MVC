using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Microsoft.Practices.ServiceLocation;
using Nalysa.Common.Log;
using Ninject;
using Ninject.Syntax;
using Ninject.Web.Mvc;
using Quartz;
using Quartz.Spi;

namespace CVScreeningWeb.Job
{
    public class JobFactory : IJobFactory
    {
        private readonly IResolutionRoot _resolutionRoot;

        public JobFactory(IResolutionRoot resolutionRoot)
        {
            this._resolutionRoot = resolutionRoot;
        }

        public IJob NewJob(TriggerFiredBundle bundle, IScheduler scheduler)
        {
            try
            {
                return (IJob)_resolutionRoot.Get(
                     bundle.JobDetail.JobType, new NonRequestScopedParameter()); // parameter goes here
            }
            catch (Exception ex)
            {
                LogManager.Instance.Info(string.Format("Exception raised in JobFactory: {0}", ex.ToString()));
                throw;
            }
        }

        public void ReturnJob(IJob job)
        {
        }
    }
}