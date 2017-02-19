namespace DevOpsWithVsts.Web
{
    using Autofac;
    using Autofac.Integration.Mvc;
    using DevOpsWithVsts.Web.Aad;
    using DevOpsWithVsts.Web.Authentication;
    using DevOpsWithVsts.Web.FeatureFlag;
    using DevOpsWithVsts.Web.Todo;
    using Microsoft.ApplicationInsights.Extensibility;
    using Owin;
    using System.Configuration;
    using System.Web.Mvc;

    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            TelemetryConfiguration.Active.InstrumentationKey =
                ConfigurationManager.AppSettings["AppInsight:InstrumentationKey"];

            var builder = new ContainerBuilder();
            builder.RegisterControllers(typeof(Startup).Assembly);
            builder.RegisterType<ClaimsPrincipalService>()
                .As<IClaimsPrincipalService>();
            builder.RegisterType<AadClient>()
                .As<IAadClient>();
            builder.RegisterType<FeatureFlagManager>()
                .As<IFeatureFlagManager>()
                .SingleInstance();
            builder.RegisterType<TodoStorage>()
                .As<ITodoStorage>()
                .WithParameter("storageConnectionString", ConfigurationManager.AppSettings["StorageConnectionString"]);

            var container = builder.Build();
            DependencyResolver.SetResolver(new AutofacDependencyResolver(container));

            ConfigureAuth(app);
        }
    }
}
