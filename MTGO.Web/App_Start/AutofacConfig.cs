using System.Reflection;
using System.Web.Http;
using System.Web.Mvc;
using Autofac;
using Autofac.Integration.Mvc;
using MTGO.Common.Helpers;
using NHibernate;
using AutofacDependencyResolver = SignalR.Autofac.AutofacDependencyResolver;

namespace MTGO.Web.App_Start
{
    public static class AutofacConfig
    {
        public static void Init()
        {
            var builder = new ContainerBuilder();

            builder.RegisterControllers(Assembly.GetExecutingAssembly());

            builder.Register(c => ConnectionHelper.BuildSessionFactory()).As<ISessionFactory>().SingleInstance();
            builder.Register(c => c.Resolve<ISessionFactory>().OpenSession()).InstancePerLifetimeScope();

            builder.RegisterFilterProvider();

            var container = builder.Build();
            DependencyResolver.SetResolver(new AutofacDependencyResolver(container));
        }
    }
}