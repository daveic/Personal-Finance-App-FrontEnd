using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PersonalFinanceFrontEnd.Controllers;
using PersonalFinanceFrontEnd.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System;
using System.Net.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Graph;
using Microsoft.Identity.Web;
using Microsoft.Extensions.Configuration;
using System.IO;
using System.Security.Claims;

namespace PersonalFinanceFrontEnd.Controllers
{
    public class GetUserDataController : Controller
    {
        
        private readonly ILogger<GetUserDataController> _logger;

        private readonly GraphServiceClient _graphServiceClient;

        private readonly MicrosoftIdentityConsentAndConditionalAccessHandler _consentHandler;

        private string[] _graphScopes;



        public GetUserDataController(ILogger<GetUserDataController> logger,
                            IConfiguration configuration,
                            GraphServiceClient graphServiceClient,
                            MicrosoftIdentityConsentAndConditionalAccessHandler consentHandler) 
        {
            _logger = logger;
            _graphServiceClient = graphServiceClient;
            this._consentHandler = consentHandler;
            _graphScopes = configuration.GetValue<string>("DownstreamApi:Scopes")?.Split(' ');


        }
  
        public async Task<string> GetUserID()
        {
            User currentUser = null;
            currentUser = await _graphServiceClient.Me.Request().GetAsync();
            // Get user photo
            using (var photoStream = await _graphServiceClient.Me.Photo.Content.Request().GetAsync())
            {
                byte[] photoByte = ((MemoryStream)photoStream).ToArray();
                ViewData["Photo"] = Convert.ToBase64String(photoByte);
            }
            //ViewData["Me"] = currentUser;
            ViewBag.Name = currentUser.GivenName;
            ViewBag.Email = currentUser.UserPrincipalName;
            ViewBag.id = currentUser;
            ClaimsPrincipal LoggedUser = this.User;
            return LoggedUser.FindFirst(ClaimTypes.NameIdentifier).Value;
        }

    }
}