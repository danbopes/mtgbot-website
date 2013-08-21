using System.Reflection;
using FluentNHibernate.Cfg;
using FluentNHibernate.Cfg.Db;
using NHibernate;
using NHibernate.Tool.hbm2ddl;

namespace MTGO.Common.Helpers
{
    public class ConnectionHelper
    {
        public static ISessionFactory BuildSessionFactory(bool updateSchema = false)
        {
            return GetConfiguration(updateSchema).BuildSessionFactory();
        }

        public static FluentConfiguration GetConfiguration(bool updateSchema)
        {
            return Fluently.Configure().Database(
                MySQLConfiguration.Standard
                                  .ConnectionString(x => x.FromConnectionStringWithKey("Main")))
                            .Mappings(x => x.FluentMappings.AddFromAssembly(Assembly.Load("MTGO.Common")))
                            .ExposeConfiguration(configuration => new SchemaUpdate(configuration).Execute(updateSchema, true));
        }
    }
}
