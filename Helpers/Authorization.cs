using System;
using System.Collections.Specialized;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using System.Web.Helpers;
using MTGBotWebsite.Models;

namespace MTGBotWebsite.Helpers
{
    public class Authorization
    {
        private static readonly MainDbContext DB = new MainDbContext();
        public static void Authorize()
        {
            if (HttpContext.Current.Session["user_name"] != null)
                return;

            if (HttpContext.Current.Request.Cookies["twitch_auth"] != null)
            {
                try
                {
                    var cookie = HttpContext.Current.Request.Cookies["twitch_auth"].Value;
                    var twitchOauth =
                        DB.TwitchOAuths.FirstOrDefault(
                            o => o.OAuth == cookie);

                    if (twitchOauth != null)
                    {
                        HttpContext.Current.Session.Add("user_name", twitchOauth.Username);
                        return;
                    }

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

                    HttpContext.Current.Session.Add("twitch_oauth", (string) json.access_token);

                    WebRequest req =
                        WebRequest.CreateHttp("https://api.twitch.tv/kraken?oauth_token=" + (string) json.access_token);

                    using (WebResponse res = req.GetResponse())
                    {
                        using (var reader = new StreamReader(res.GetResponseStream()))
                        {
                            response = reader.ReadToEnd();
                        }
                    }

                    var json2 = Json.Decode(response);

                    HttpContext.Current.Session.Add("user_name", (string) json2.token.user_name);

                    DB.TwitchOAuths.Add(new TwitchOAuth
                        {
                            OAuth = (string) json.access_token,
                            Username = (string) json2.token.user_name
                        });
                    DB.SaveChanges();
                    return;
                }
                catch
                {
                }
            }

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