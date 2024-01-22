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
//using Microsoft.Graph.Models;
using GraphServiceClient = Microsoft.Graph.GraphServiceClient;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Identity.Abstractions;
using System.Text.Json;
using System.Net.Http.Json;
//using GraphBetaServiceClient = Microsoft.Graph.GraphBetaServiceClient;




namespace PersonalFinanceFrontEnd.Controllers
{
    //[AuthorizeForScopes(ScopeKeySection = "DownstreamApi:Scopes")]
    public partial class PersonalFinanceController
    {

        private readonly ILogger<PersonalFinanceController> _logger;

        private readonly GraphServiceClient _graphServiceClient;

        private readonly MicrosoftIdentityConsentAndConditionalAccessHandler _consentHandler;

        private readonly string[] _graphScopes;

        private readonly INotyfService _notyf;

        private readonly IDownstreamApi _downstreamWebApi;

        public PersonalFinanceController(ILogger<PersonalFinanceController> logger,
                            IConfiguration configuration,
                            GraphServiceClient graphServiceClient,
                            MicrosoftIdentityConsentAndConditionalAccessHandler consentHandler, INotyfService notyf, IDownstreamApi downstreamWebApi) 
        {
            _logger = logger;
            _graphServiceClient = graphServiceClient;
            this._consentHandler = consentHandler;
            _graphScopes = configuration.GetValue<string>("DownstreamApi:Scopes")?.Split(' ');
            _notyf = notyf;
            _downstreamWebApi = downstreamWebApi;
        }



        


    public async Task OnGet()
    {
        using var response = await _downstreamWebApi.CallApiForUserAsync("DownstreamApi").ConfigureAwait(false);
        if (response.StatusCode == System.Net.HttpStatusCode.OK)
        {
            var apiResult = await response.Content.ReadFromJsonAsync<JsonDocument>().ConfigureAwait(false);
            ViewData["ApiResult"] = JsonSerializer.Serialize(apiResult, new JsonSerializerOptions { WriteIndented = true });
        }
        else
        {
            var error = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            throw new HttpRequestException($"Invalid status code in the HttpResponseMessage: {response.StatusCode}: {error}");
        }
    }
    

    public async Task<string> GetUserData() 
        {
            //var currentUser = await _graphServiceClient.Me.GetAsync();


            ViewData["Photo"] = "photo";

            //ViewBag.Name = currentUser.GivenName;
            //ViewBag.Email = currentUser.UserPrincipalName;
            //ViewBag.id = currentUser;
            //ClaimsPrincipal LoggedUser = this.User;
            //ViewData["Me"] = LoggedUser.FindFirst(ClaimTypes.NameIdentifier).Value;
            //return LoggedUser.FindFirst(ClaimTypes.NameIdentifier).Value;
            return "AAAAAAAAAAAAAAAAAAAAANgDJxkCf2VKqG87lDnSoGg";
        }
    }
}