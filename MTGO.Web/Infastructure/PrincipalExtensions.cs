using System;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;

namespace MTGO.Web.Infastructure
{
    public static class PrincipalExtensions
    {
        public static bool IsAuthenticated(this IPrincipal principal)
        {
            var userId = GetUserId(principal);

            return userId != null && userId > 0;
        }

        public static int? GetUserId(this IPrincipal principal)
        {
            if (principal == null)
            {
                return null;
            }

            var claimsPrincipal = principal as ClaimsPrincipal;

            if (claimsPrincipal != null)
            {
                return (claimsPrincipal.Identities.Where(
                    identity => identity.AuthenticationType == Constants.MtgbotAuthType)
                                       .Select(identity => identity.FindFirst(MtgbotClaimTypes.Identifier))
                                       .Where(idClaim => idClaim != null)
                                       .Select(idClaim => Convert.ToInt32(idClaim.Value))).FirstOrDefault();
            }

            return null;
        }

        public static bool HasClaim(this ClaimsPrincipal principal, string type)
        {
            return !String.IsNullOrEmpty(principal.GetClaimValue(type));
        }

        public static string GetClaimValue(this ClaimsPrincipal principal, string type)
        {
            Claim claim = principal.FindFirst(type);

            return claim != null ? claim.Value : null;
        }

        public static string GetIdentityProvider(this ClaimsPrincipal principal)
        {
            return principal.GetClaimValue(ClaimTypes.AuthenticationMethod) ??
                   principal.GetClaimValue(AcsClaimTypes.IdentityProvider);
        }

        public static bool HasRequiredClaims(this ClaimsPrincipal principal)
        {
            return principal.HasClaim(ClaimTypes.NameIdentifier) &&
                   principal.HasClaim(ClaimTypes.Name) &&
                   !String.IsNullOrEmpty(principal.GetIdentityProvider());
        }

        public static bool HasPartialIdentity(this ClaimsPrincipal principal)
        {
            return !String.IsNullOrEmpty(principal.GetClaimValue(MtgbotClaimTypes.PartialIdentity));
        }
    }
}