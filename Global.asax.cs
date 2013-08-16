using System;
using System.Collections.Generic;
using System.Globalization;
using System.Security.Claims;
using System.Security.Principal;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using System.Web.Security;
using System.Web.SessionState;
using MTGBotWebsite.App_Start;
using MTGBotWebsite.Infastructure;
using MTGOLibrary.Models;
using Microsoft.AspNet.SignalR;

namespace MTGBotWebsite
{
    // Note: For instructions on enabling IIS6 or IIS7 classic mode, 
    // visit http://go.microsoft.com/?LinkId=9394801

    public class MvcApplication : HttpApplication
    {
        public static RouteBase HubRoute;

        protected void Application_Start()
        {
            var hubConfiguration = new HubConfiguration {EnableDetailedErrors = true};
            HubRoute = RouteTable.Routes.MapHubs(hubConfiguration);
            AreaRegistration.RegisterAllAreas();

            //Database.SetInitializer(new DropCreateMySqlDatabaseIfModelChanges<MainDbContext>());
            WebApiConfig.Register(GlobalConfiguration.Configuration);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
            AuthConfig.RegisterAuth();
        }

        protected void Application_BeginRequest(object sender, EventArgs e)
        {
            if (IsSignalRRequest(Context))
            {
                // Turn readonly sessions on for SignalR
                Context.SetSessionStateBehavior(SessionStateBehavior.ReadOnly);
            }
        }

        private static bool IsSignalRRequest(HttpContext context)
        {
            var routeData = HubRoute.GetRouteData(new HttpContextWrapper(context));

            // If the routeData isn't null then it's a SignalR request
            return routeData != null;
        }

        protected void Application_AuthenticateRequest(object sender, EventArgs e)
        {
            TwitchAuthorization.TryAuth();
        }

        protected void Application_AuthorizeRequest(object sender, EventArgs e)
        {
            //if (!Context.User.IsAuthenticated())
            //    TwitchAuthorization.RedirectToTwitch();
        }
    }
}