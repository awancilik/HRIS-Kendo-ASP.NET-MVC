using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Configuration;
using Nalysa.Common.Log;
using Ninject;
using Quartz;
using Quartz.Impl;

namespace CVScreeningWeb.Job
{
    public class SchedulerJob
    {
        public static void RegisterScheduler(IKernel kernel)
        {
            try
            {
                var scheduler = new StdSchedulerFactory().GetScheduler();
                scheduler.JobFactory = new JobFactory(kernel);
                scheduler.Start();

                var intervalInMinutes = int.Parse(WebConfigurationManager.AppSettings["IntervalInMinutes"]);

                IJobDetail dispatchingJob = JobBuilder.Create<DispatchingJob>().WithIdentity("DispatchingJob").Build();
                ITrigger triggerDispatching = TriggerBuilder.Create()
                    .WithIdentity("triggerDispatching")
                    .WithDailyTimeIntervalSchedule(
                            x => x.StartingDailyAt(TimeOfDay.HourAndMinuteOfDay(6, 0))
                                     .EndingDailyAt(TimeOfDay.HourAndMinuteOfDay(22, 0))
                                     .WithIntervalInMinutes(intervalInMinutes)).StartNow()
                    .Build();

                scheduler.ScheduleJob(dispatchingJob, triggerDispatching);
            }
            catch (Exception ex)
            {
                LogManager.Instance.Info(string.Format("Exception caught when register job and scheduler at {0}", DateTime.Now));
            }
        }

    }
}