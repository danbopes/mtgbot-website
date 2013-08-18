using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Data.Entity.Validation;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Text;
using System.Web;
using System.Web.Helpers;
using System.Web.Security;
using MTGO.Common.Entities;
using log4net;

namespace MTGO.Web.Infastructure
{
    public class TwitchAuthorization
    {
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public static bool TryAuth()
        {
            HttpCookie authCookie = HttpContext.Current.Request.Cookies[FormsAuthentication.FormsCookieName];

            if (authCookie != null)
            {
                var ticket = FormsAuthentication.Decrypt(authCookie.Value);

                if (ticket != null)
                {
                    //Log.DebugFormat("Ticket: {0}", ticket.);
                    //var identity = new UserIdentity()
                    using (var db = new MainDbContext())
                    {
                        var user = db.Users.Find(Convert.ToInt32(ticket.Name));

                        if (user != null)
                        {
                            var claims = new List<Claim>
                            {
                                new Claim(MtgbotClaimTypes.Identifier, user.Id.ToString(CultureInfo.InvariantCulture)),
                                new Claim(ClaimTypes.Name, user.TwitchUsername)
                            };

                            if (user.Admin)
                                claims.Add(new Claim(MtgbotClaimTypes.Admin, "True"));

                            HttpContext.Current.User = new ClaimsPrincipal(new ClaimsIdentity(claims, Constants.MtgbotAuthType));

                            return true;
                        }
                    }
                }
            }

            if (HttpContext.Current.Request.Cookies["twitch_auth"] != null)
            {
                try
                {
                    Log.Debug("Making request to twitch");
                    var cookie = HttpContext.Current.Request.Cookies["twitch_auth"].Value;

                    string response;
                    using (var wb = new WebClient())
                    {
                        var data = new NameValueCollection();
                        data["client_id"] = ConfigurationManager.AppSettings["TwitchClientKey"];
                        data["client_secret"] = ConfigurationManager.AppSettings["TwitchSecretKey"];
                        data["grant_type"] = "authorization_code";
                        data["redirect_uri"] = ConfigurationManager.AppSettings["TwitchRedirect"];
                        data["code"] = cookie;

                        response =
                            Encoding.ASCII.GetString(wb.UploadValues("https://api.twitch.tv/kraken/oauth2/token", "POST",
                                                                     data));
                    }

                    var json = Json.Decode(response);

                    //HttpContext.Current.Session.Add("twitch_oauth", (string)json.access_token);

                    WebRequest req =
                        WebRequest.CreateHttp("https://api.twitch.tv/kraken?oauth_token=" + (string)json.access_token);

                    using (WebResponse res = req.GetResponse())
                    {
                        using (var reader = new StreamReader(res.GetResponseStream()))
                        {
                            response = reader.ReadToEnd();
                        }
                    }

                    var json2 = Json.Decode(response);

                    //HttpContext.Current.Session.Add("user_name", (string)json2.token.user_name);

                    using (var db = new MainDbContext())
                    {
                        var username = (string)json2.token.user_name;

                        var user = db.Users.FirstOrDefault(c => c.TwitchUsername == username);

                        if (user == null)
                        {
                            var ipAddress = HttpContext.Current.Request.UserHostAddress;

                            if (HttpContext.Current.Request.ServerVariables["HTTP_CF_CONNECTING_IP"] != null)
                                ipAddress = HttpContext.Current.Request.ServerVariables["HTTP_CF_CONNECTING_IP"];

                            try
                            {
                                user = new User
                                {
                                    Admin = false,
                                    Created = DateTime.UtcNow,
                                    SignupIpAddress = ipAddress,
                                    TwitchUsername = username
                                };

                                db.Users.Add(user);
                                db.SaveChanges();
                            }
                            catch (DbEntityValidationException ex)
                            {
                                Log.Error(ipAddress);
                                Log.Error("Exception adding user.", ex);
                                Log.ErrorFormat("Entity Validation Errors: {0}", String.Join(",", ex.EntityValidationErrors.Select(v => v.Entry + ": (" + String.Join(",", v.ValidationErrors.Select(e => e.ErrorMessage)) + ")")));
                                throw;
                            }
                            
                        }

                        var claims = new List<Claim>
                            {
                                new Claim(MtgbotClaimTypes.Identifier, user.Id.ToString(CultureInfo.InvariantCulture)),
                                new Claim(MtgbotClaimTypes.Admin, user.Admin.ToString()),
                                new Claim(ClaimTypes.Name, user.TwitchUsername)
                            };

                        HttpContext.Current.User = new ClaimsPrincipal(new ClaimsIdentity(claims, Constants.MtgbotAuthType));
                        FormsAuthentication.SetAuthCookie(user.Id.ToString(CultureInfo.InvariantCulture), true);
                    }

                    //HttpContext.Current.User = 

                    /*DB.TwitchOAuths.Add(new TwitchOAuth
                        {
                            OAuth = (string) json.access_token,
                            Username = (string) json2.token.user_name
                        });
                    DB.SaveChanges();*/
                    return true;
                }
                catch
                {
                    throw;
                }
            }

            return false;
        }

        public static void RedirectToTwitch()
        {
            var route = string.Format("{0}/{1}", HttpContext.Current.Request.RequestContext.RouteData.Values["controller"], HttpContext.Current.Request.RequestContext.RouteData.Values["action"]);

            if (HttpContext.Current.Request.RequestContext.RouteData.Values["id"] != null)
                route += "/" + HttpContext.Current.Request.RequestContext.RouteData.Values["id"];

            HttpContext.Current.Response.Redirect(String.Format(
                "https://api.twitch.tv/kraken/oauth2/authorize?response_type=code&client_id={0}&redirect_uri={1}&scope={2}&state={3}",
                ConfigurationManager.AppSettings["TwitchClientKey"],
                HttpUtility.UrlEncode(ConfigurationManager.AppSettings["TwitchRedirect"]),
                "",
                HttpUtility.UrlEncode(route)
            ));
        }
    }
}