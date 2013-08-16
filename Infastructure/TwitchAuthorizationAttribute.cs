using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Security.Principal;
using System.Text;
using System.Web;
using System.Web.Helpers;
using System.Web.Mvc;
using System.Web.Security;
using MTGOLibrary.Models;
using log4net;

namespace MTGBotWebsite.Infastructure
{
    public class TwitchAuthorizeAttribute : AuthorizeAttribute
    {
        public bool RequireAuthentication { get; internal set; }

        /// <summary>
        /// For logging with Log4net.
        /// </summary>
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public TwitchAuthorizeAttribute(bool requireAuthentication = true)
        {
            RequireAuthentication = requireAuthentication;
        }

        /*protected override void HandleUnauthorizedRequest(System.Web.Http.Controllers.HttpActionContext actionContext)
        {
            Log.Debug("HandleUnauthorizedRequest();");
            var challengeMessage = new System.Net.Http.HttpResponseMessage(System.Net.HttpStatusCode.Unauthorized);
            challengeMessage.Headers.Add("WWW-Authenticate", "Basic");
            throw new HttpResponseException(challengeMessage);
            //throw new HttpResponseException();
        }*/

        public override void OnAuthorization(AuthorizationContext filterContext)
        {
            Log.DebugFormat("OnAuthorization()");
            if (Authenticate(filterContext) || !RequireAuthentication)
            {
                return;
            }
            else
            {
                HandleUnauthorizedRequest(filterContext);
            }
            //base.OnAuthorization(filterContext);
        }

        protected override void HandleUnauthorizedRequest(AuthorizationContext filterContext)
        {
            TwitchAuthorization.RedirectToTwitch();

            //base.HandleUnauthorizedRequest(filterContext);
        }

        private bool Authenticate(AuthorizationContext filterContext)
        {
            Log.DebugFormat("Authenticate(); ActionContext: {0}", filterContext);

            /*HttpCookie authCookie = Request.Cookies[FormsAuthentication.FormsCookieName];
            if (authCookie != null)
            {
                FormsAuthenticationTicket ticket = FormsAuthentication.Decrypt(authCookie.Value);
                UserIdentity identity = new UserIdentity(ticket.Name);
                UserPrincipal principal = new UserPrincipal(identity);
                HttpContext.Current.User = principal;
            }*/

            return TwitchAuthorization.TryAuth();

            /*if (RequireSsl && !HttpContext.Current.Request.IsSecureConnection && !HttpContext.Current.Request.IsLocal)
            {
                Log.Error("Failed to login: SSL:" + HttpContext.Current.Request.IsSecureConnection);
                return false;
            }

            if (!HttpContext.Current.Request.Headers.AllKeys.Contains("Authorization")) return false;

            string authHeader = HttpContext.Current.Request.Headers["Authorization"];

            IPrincipal principal;
            if (TryGetPrincipal(authHeader, out principal))
            {
                HttpContext.Current.User = principal;
                return true;
            }
            return false;*/
        }

        /*private bool TryGetPrincipal(string authHeader, out IPrincipal principal)
        {
            var creds = ParseAuthHeader(authHeader);
            if (creds != null)
            {
                if (TryGetPrincipal(creds[0], creds[1], out principal)) return true;
            }

            principal = null;
            return false;
        }

        private bool TryGetPrincipal(string username, string password, out IPrincipal principal)
        {
            // this is the method that does the authentication 

            //users often add a copy/paste space at the end of the username
            username = username.Trim();
            password = password.Trim();

            //TODO
            //Replace this with your own Authentication Code
            //Person person = AccountManagement.ApiLogin(username, password);

            if (person != null)
            {
                // once the user is verified, assign it to an IPrincipal with the identity name and applicable roles
                principal = new GenericPrincipal(new GenericIdentity(username), System.Web.Security.Roles.GetRolesForUser(username));
                return true;
            }
            else
            {
                if (!String.IsNullOrWhiteSpace(username))
                {
                    Log.Error("Failed to login: username=" + username + "; password=" + password);
                }
                principal = null;
                return false;
            }
        }*/
    }
}