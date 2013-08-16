using System;

namespace MTGO.Web.Infastructure
{
    public static class Constants
    {
        public static readonly string AuthResultCookie = "mtgbot.authResult";
        public static readonly Version MtgbotVersion = typeof(Constants).Assembly.GetName().Version;
        public static readonly string MtgbotAuthType = "Mtgbot";
    }

    public static class MtgbotClaimTypes
    {
        public const string Identifier = "urn:mtgbot:id";
        public const string Admin = "urn:mtgbot:admin";
        public const string PartialIdentity = "urn:mtgbot:partialid";
    }

    public static class AcsClaimTypes
    {
        public static readonly string IdentityProvider = "http://schemas.microsoft.com/accesscontrolservice/2010/07/claims/IdentityProvider";
    }

    public static class ContentTypes
    {
        public const string Html = "text/html";
        public const string Text = "text/plain";
    }
}