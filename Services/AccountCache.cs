﻿using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Identity.Client;
using Microsoft.Identity.Web;
using System.Threading.Tasks;

namespace PersonalFinanceFrontEnd.Controllers;
internal class RejectSessionCookieWhenAccountNotInCacheEvents : CookieAuthenticationEvents
{    
    
    
    
    
   




    public async override Task ValidatePrincipal(CookieValidatePrincipalContext context)
    {
        try
        {
            var tokenAcquisition = context.HttpContext.RequestServices.GetRequiredService<ITokenAcquisition>();
            string token = await tokenAcquisition.GetAccessTokenForUserAsync(
                scopes: new[] { "profile" },
                user: context.Principal);
           


        }
        catch (MicrosoftIdentityWebChallengeUserException ex) when (AccountDoesNotExitInTokenCache(ex))
        {
            context.RejectPrincipal();
        }
    }
    /// <summary>
    /// Is the exception thrown because there is no account in the token cache?
    /// </summary>
    /// <param name="ex">Exception thrown by <see cref="ITokenAcquisition"/>.GetTokenForXX methods.</param>
    /// <returns>A boolean telling if the exception was about not having an account in the cache</returns>
    private static bool AccountDoesNotExitInTokenCache(MicrosoftIdentityWebChallengeUserException ex)
    {
        return ex.InnerException is MsalUiRequiredException && (ex.InnerException as MsalUiRequiredException).ErrorCode == "user_null";
    }

























    


}