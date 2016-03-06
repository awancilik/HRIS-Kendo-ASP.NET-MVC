using System;
using System.Linq;
using System.Web;
using System.Web.Http;
using CVScreeningCore.Error;
using CVScreeningDAL.UnitOfWork;
using CVScreeningService.DTO.LookUpDatabase;
using CVScreeningService.Interceptor;
using CVScreeningService.Services.Client;
using CVScreeningService.Services.Common;
using CVScreeningService.Services.DAL;
using CVScreeningService.Services.Discussion;
using CVScreeningService.Services.DispatchingManagement;
using CVScreeningService.Services.ErrorHandling;
using CVScreeningService.Services.File;
using CVScreeningService.Services.LookUpDatabase;
using CVScreeningService.Services.Notification;
using CVScreeningService.Services.Permission;
using CVScreeningService.Services.Reporting;
using CVScreeningService.Services.Screening;
using CVScreeningService.Services.Settings;
using CVScreeningService.Services.SystemTime;
using CVScreeningService.Services.UserManagement;
using CVScreeningWeb;
using CVScreeningWeb.Controllers;
using CVScreeningWeb.Helpers;
using CVScreeningWeb.Job;
using Microsoft.Web.Infrastructure.DynamicModuleHelper;
using Nalysa.Common.Log;
using Ninject;
using Ninject.Activation;
using Ninject.Extensions.Interception.Infrastructure.Language;
using Ninject.Extensions.NamedScope;
using Ninject.Parameters;
using Ninject.Planning.Targets;
using Ninject.Web.Common;
using Ninject.Extensions.Conventions;
using Ninject.Web.Mvc;
using Quartz;
using WebActivator;
using WebMatrix.WebData;

[assembly: WebActivator.PreApplicationStartMethod(typeof (NinjectWebCommon), "Start")]
[assembly: ApplicationShutdownMethod(typeof (NinjectWebCommon), "Stop")]

namespace CVScreeningWeb
{
    
    public class NonRequestScopedParameter : IParameter
    {
        public bool Equals(IParameter other)
        {
            if (other == null)
            {
                return false;
            }

            return other is NonRequestScopedParameter;
        }

        public object GetValue(IContext context, ITarget target)
        {
            throw new NotSupportedException("this parameter does not provide a value");
        }

        public string Name
        {
            get { return typeof(NonRequestScopedParameter).Name; }
        }

        // this is very important
        public bool ShouldInherit
        {
            get { return true; }
        }
    }


    public static class NinjectWebCommon
    {
        private const bool TestingMode = false;

        private static readonly Bootstrapper Bootstrapper = new Bootstrapper();

        /// <summary>
        ///     Starts the application
        /// </summary>
        public static void Start()
        {
            DynamicModuleUtility.RegisterModule(typeof (OnePerRequestHttpModule));
            DynamicModuleUtility.RegisterModule(typeof (NinjectHttpModule));
            Bootstrapper.Initialize(CreateKernel);

            SchedulerJob.RegisterScheduler(Bootstrapper.Kernel);


        }

        /// <summary>
        ///     Stops the application.
        /// </summary>
        public static void Stop()
        {
            Bootstrapper.ShutDown();
        }

        /// <summary>
        ///     Creates the kernel that will manage your application.
        /// </summary>
        /// <returns>The created kernel.</returns>
        private static IKernel CreateKernel()
        {
            var kernel = new StandardKernel();
            kernel.Bind<Func<IKernel>>().ToMethod(ctx => () => new Bootstrapper().Kernel);
            kernel.Bind<IHttpModule>().To<HttpApplicationInitializationHttpModule>();

            RegisterServices(kernel);
            return kernel;
        }




        /// <summary>
        /// Get construct argument from context
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        private static object GetConstructorArgFromContextAndSession(IContext context)
        {
            try
            {
                var httpContext = context.Kernel.Get<HttpContextBase>();
                if (httpContext != null && httpContext.Request != null)
                {
                    return TenantHelper.GetTenantId(httpContext.Request.Url.Host);
                }
                // By default, binding using Tenant 2
                return (Byte)1;
            }
            catch (Exception)
            {
                return (Byte)1;
            }
        }

        /// <summary>
        ///     Load your modules or register your services here!
        /// </summary>
        /// <param name="kernel">The kernel.</param>
        private static void RegisterServices(IKernel kernel)
        {
            LogManager.Instance.Info("Register NInject services");

            /*
             * CORE Stuffs
             */
            kernel.Bind<IErrorFactory>().To<ResourceErrorFactory>().InRequestScope();
            kernel.Bind<ISystemTimeService>().To<SystemTimeService>().InRequestScope();
            kernel.Bind<IWebSecurity>().To<CVScreeningService.Services.UserManagement.WebSecurity>().InRequestScope();

            /*
             * DAL Stuffs
             */
            // Dataaccess implementation using database and entity framework

// ReSharper disable once ConditionIsAlwaysTrueOrFalse
            if (TestingMode)
            {
// ReSharper disable once CSharpWarnings::CS0162
// ReSharper disable HeuristicUnreachableCode
                kernel.Bind<IUnitOfWork>().To<InMemoryUnitOfWork>().InSingletonScope();
// ReSharper restore HeuristicUnreachableCode
// ReSharper disable once RedundantIfElseBlock
            }
            else
            {
                // Default binding by request scope and binding by call scope for job
                kernel.Bind<IUnitOfWork>().To<EfUnitOfWork>().InRequestScope().WithConstructorArgument("tenantId", GetConstructorArgFromContextAndSession);
                kernel.Bind<IUnitOfWork>().To<EfUnitOfWork>().When(x => x.Parameters.OfType<NonRequestScopedParameter>().Any()).InCallScope().WithConstructorArgument("tenantId", (Byte)1);
            }

            // Dataaccess implementation using memory repository
            kernel.Bind<IQualificationPlaceFactory>().To<QualificationPlaceFactory>().InRequestScope();


            /*
             * Service Stuffs
             */
            kernel.Bind<IUserManagementService>().To<UserManagementService>().InRequestScope();
            kernel.Bind<ICommonService>().To<CommonService>().InRequestScope();
            kernel.Bind<ISettingsService>().To<SettingsService>().InRequestScope();
            kernel.Bind<IScreeningService>().To<ScreeningService>().InRequestScope();
            kernel.Bind<IDiscussionService>().To<DiscussionService>().InRequestScope();
            kernel.Bind<IQualificationService>().To<QualificationService>().InRequestScope();
            kernel.Bind<IPermissionService>().To<PermissionService>().InRequestScope();
            kernel.Bind<INotificationService>().To<NotificationService>().InRequestScope();

            kernel.Bind<IErrorMessageFactoryService>().To<ErrorMessageFactoryService>().InRequestScope();
            kernel.Bind<IUowService>().To<UowService>().InRequestScope();

            /*
             * File
             */
            kernel.Bind<IFileService>().To<FileService>().InRequestScope();
            kernel.Bind<IPDFConverter>().To<PDFConverter>().InRequestScope();

            /*
             * Client Management
             */
            kernel.Bind<IClientService>().To<ClientService>().InRequestScope();

            /*
             * Look Up Database Stuffs
             */
            kernel.Bind<ILookUpDatabaseService<QualificationPlaceDTO>>()
                .To<QualificationPlaceService>()
                .WhenInjectedInto((typeof (ContactController)))
                .InRequestScope();
            kernel.Bind<ILookUpDatabaseService<QualificationPlaceDTO>>()
               .To<QualificationPlaceService>()
               .WhenInjectedInto((typeof(QualificationPlaceController)))
               .InRequestScope();
            kernel.Bind<ILookUpDatabaseService<QualificationPlaceDTO>>()
               .To<QualificationPlaceService>()
               .WhenInjectedInto((typeof(DispatchingController)))
               .InRequestScope();

            kernel.Bind<ILookUpDatabaseService<PoliceDTO>>()
                .To<PoliceLookUpDatabaseService>()
                .WhenInjectedInto((typeof (PoliceController)))
                .InRequestScope();
            kernel.Bind<ILookUpDatabaseService<CourtDTO>>()
                .To<CourtLookUpDatabaseService>()
                .WhenInjectedInto((typeof (CourtController)))
                .InRequestScope();
            kernel.Bind<ILookUpDatabaseService<HighSchoolDTO>>()
                .To<HighSchoolLookUpDatabaseService>()
                .WhenInjectedInto((typeof (HighSchoolController)))
                .InRequestScope();
            kernel.Bind<ILookUpDatabaseService<CompanyDTO>>()
                .To<CompanyLookUpDatabaseService>()
                .WhenInjectedInto((typeof (CompanyController)))
                .InRequestScope();
            kernel.Bind<ILookUpDatabaseService<DrivingLicenseOfficeDTO>>()
                .To<DrivingLicenseOfficeLookUpDatabaseService>()
                .WhenInjectedInto((typeof (DrivingLicenseOfficeController)))
                .InRequestScope();
            kernel.Bind<ILookUpDatabaseService<FacultyDTO>>()
                .To<FacultyLookUpDatabaseService>()
                .WhenInjectedInto((typeof (UniversityController)))
                .InRequestScope();
            kernel.Bind<ILookUpDatabaseService<ImmigrationOfficeDTO>>()
                .To<ImmigrationOfficeLookUpDatabaseService>()
                .WhenInjectedInto((typeof (ImmigrationOfficeController)))
                .InRequestScope();
            kernel.Bind<ILookUpDatabaseService<CertificationPlaceDTO>>()
                .To<CertificationPlaceLookUpDatabaseService>()
                .WhenInjectedInto((typeof (CertificationPlaceController)))
                .InRequestScope();
            kernel.Bind<ILookUpDatabaseService<CertificationPlaceDTO>>()
                .To<CertificationPlaceLookUpDatabaseService>()
                .WhenInjectedInto((typeof (ProfessionalQualificationController)))
                .InRequestScope();
            kernel.Bind<IProfessionalQualificationService>().To<ProfessionalQualificationService>().InRequestScope();
            kernel.Bind<IUniversityLookUpDatabaseService>().To<UniversityLookUpDatabaseService>().InRequestScope();
            kernel.Bind<ILookUpDatabaseService<PopulationOfficeDTO>>()
                .To<PopulationOfficeLookUpDatabaseService>()
                .WhenInjectedInto((typeof(PopulationOfficeController)))
                .InRequestScope();

            /*
             * Request scope by default for dispatching management service binding
             * Call scope when binding done in job
             */
            kernel.Bind<IDispatchingManagementService>().To<DispatchingManagementService>().InRequestScope();

            /*
             * Nalysa.Common Stuffs
             */
            kernel.Bind<ILogger>().To<Logger>().InRequestScope();
            kernel.Bind<IJob>().To<DispatchingJob>();


        }
    }
}