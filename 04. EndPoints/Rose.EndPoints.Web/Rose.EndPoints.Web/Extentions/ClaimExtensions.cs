﻿using System.Security.Claims;

namespace Rose.EndPoints.Web.Extentions
{
    public static class ClaimExtensions
    {
        public static string GetClaim(this ClaimsPrincipal userClaimsPrincipal, string claimType)
        {
            return userClaimsPrincipal.Claims.FirstOrDefault((Claim x) => x.Type == claimType)?.Value;
        }
    }
}