﻿using System.Security.Claims;
using System.Security.Principal;

namespace MTGO.Web.Infastructure
{
    public class AuthorizeClaim : AuthorizeAttribute
    {
        private readonly string _claimType;
        public AuthorizeClaim(string claimType)
        {
            _claimType = claimType;
        }

        protected override bool UserAuthorized(IPrincipal user)
        {
            var claimsPrincipal = user as ClaimsPrincipal;

            return claimsPrincipal != null && claimsPrincipal.HasClaim(_claimType);
        }
    }
}