using System;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using System.Web.SessionState;
using MTGBotWebsite.App_Start;

namespace MTGBotWebsite
{
    // Note: For instructions on enabling IIS6 or IIS7 classic mode, 
    // visit http://go.microsoft.com/?LinkId=9394801

    public class MvcApplication : System.Web.HttpApplication
    {
        private static RouteBase _hubRoute;

        protected void Application_Start()
        {
            _hubRoute = RouteTable.Routes.MapHubs();
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
            var routeData = _hubRoute.GetRouteData(new HttpContextWrapper(context));

            // If the routeData isn't null then it's a SignalR request
            return routeData != null;
        }
    }

    public class CustomAuthorize : System.Web.Mvc.AuthorizeAttribute
    {
        protected override void HandleUnauthorizedRequest(AuthorizationContext filterContext)
        {
            /*if (!filterContext.HttpContext.User.Identity.IsAuthenticated)
            {
                base.HandleUnauthorizedRequest(filterContext);
            }
            else
            {
                filterContext.Result = new RedirectToRouteResult(new
                RouteValueDictionary(new { controller = "Error", action = "AccessDenied" }));
            }*/
            throw new Exception("Testing");
        }
    }
}