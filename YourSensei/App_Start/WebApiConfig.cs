using System.Web.Http.Cors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using Newtonsoft.Json.Serialization;
using Unity;
using YourSensei.Service;
using Unity.Lifetime;
using Microsoft.Owin.Security.OAuth;

namespace YourSensei
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            // Enable CORS for the Angular App
            var cors = new EnableCorsAttribute( "*", "*", "*" );
            config.EnableCors(cors);


            var container = new UnityContainer();
            container.RegisterType<IAuthenticationService, AuthenticationService>(new HierarchicalLifetimeManager());
            container.RegisterType<ITrainingEventService, TrainingEventService>(new HierarchicalLifetimeManager());
            container.RegisterType<ICompanySettingService, CompanySettingService>(new HierarchicalLifetimeManager());
            container.RegisterType<IEmployeeService, EmployeeService>(new HierarchicalLifetimeManager());
            container.RegisterType<ILibraryService, LibraryService>(new HierarchicalLifetimeManager());
            container.RegisterType<IMentorService, MentorService>(new HierarchicalLifetimeManager());
            container.RegisterType<ICreditLogService, CreditLogService>(new HierarchicalLifetimeManager());
            container.RegisterType<ICompanyDetailService, CompanyDetailService>(new HierarchicalLifetimeManager());
            container.RegisterType<IQuizService, QuizService>(new HierarchicalLifetimeManager());
            container.RegisterType<IInitialAssessmentService, InitialAssessmentService>(new HierarchicalLifetimeManager());
            container.RegisterType<IDashboardService, DashboardService>(new HierarchicalLifetimeManager());
            config.DependencyResolver = new UnityResolver(container);


            // Set JSON formatter as default one and remove XmlFormatter
            var jsonFormatter = config.Formatters.JsonFormatter;
            jsonFormatter.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
            config.Formatters.Remove(config.Formatters.XmlFormatter);
            jsonFormatter.SerializerSettings.DateTimeZoneHandling = Newtonsoft.Json.DateTimeZoneHandling.Utc;

            // Web API configuration and services
            config.SuppressDefaultHostAuthentication();
            config.Filters.Add(new HostAuthenticationFilter(OAuthDefaults.AuthenticationType));

            // Web API routes
            config.MapHttpAttributeRoutes();

            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{action}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );
        }
    }
}
