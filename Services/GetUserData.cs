using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PersonalFinanceFrontEnd.Controllers;
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
using AspNetCoreHero.ToastNotification.Abstractions;
using Microsoft.Graph.Models;


namespace PersonalFinanceFrontEnd.Controllers
{
    public partial class PersonalFinanceController
    {

        private readonly ILogger<PersonalFinanceController> _logger;

        private readonly GraphServiceClient _graphServiceClient;

        private readonly MicrosoftIdentityConsentAndConditionalAccessHandler _consentHandler;

        private readonly string[] _graphScopes;

        private readonly INotyfService _notyf;


        public PersonalFinanceController(ILogger<PersonalFinanceController> logger,
                            IConfiguration configuration,
                            GraphServiceClient graphServiceClient,
                            MicrosoftIdentityConsentAndConditionalAccessHandler consentHandler, INotyfService notyf) 
        {
            _logger = logger;
            _graphServiceClient = graphServiceClient;
            this._consentHandler = consentHandler;
            _graphScopes = configuration.GetValue<string>("DownstreamApi:Scopes")?.Split(' ');
            _notyf = notyf;
        }

        public async Task<string> GetUserData() 
        {
             User currentUser = await _graphServiceClient.Me.GetAsync();


            ViewData["Photo"] = "photo";

            ViewBag.Name = currentUser.GivenName;
            ViewBag.Email = currentUser.UserPrincipalName;
            ViewBag.id = currentUser;
            ClaimsPrincipal LoggedUser = this.User;
            ViewData["Me"] = LoggedUser.FindFirst(ClaimTypes.NameIdentifier).Value;
            return LoggedUser.FindFirst(ClaimTypes.NameIdentifier).Value;
        }
    }
}