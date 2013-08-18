using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using FluentNHibernate.Cfg;
using FluentNHibernate.Cfg.Db;
using NHibernate;
using NHibernate.Tool.hbm2ddl;

namespace MTGO.BuildSchema
{
    class Program
    {
        static void Main(string[] args)
        {
            var session = BuildSessionFactory();
            Console.WriteLine("Finished! (For Styles)");
            Console.ReadKey();
        }

        public static ISessionFactory BuildSessionFactory()
        {
            return GetConfiguration().BuildSessionFactory();
        }

        public static FluentConfiguration GetConfiguration()
        {
            return Fluently.Configure().Database(
                MySQLConfiguration.Standard
                                  .ConnectionString(x => x.FromConnectionStringWithKey("Main")))
                            .Mappings(x => x.FluentMappings.AddFromAssembly(Assembly.Load("MTGO.Common")))
                            .ExposeConfiguration(configuration => new SchemaUpdate(configuration).Execute(true, true));
        }
    }
}
