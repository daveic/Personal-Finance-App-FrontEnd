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
        //private IEnumerable<T> GetAllItems<T>(string controller, string type, string User_OID)
        //{
        //    IEnumerable<T> detections = null;
        //    string path = "api/" + controller + "/GetAll" + type + "?User_OID=" + User_OID;
        //    using (var client = new HttpClient())
        //    {
        //        client.BaseAddress = new Uri("https://personalfinanceappapi.azurewebsites.net/");
        //        var responseTask = client.GetAsync(path);
        //        responseTask.Wait();
        //        var result = responseTask.Result;
        //        var readTask = result.Content.ReadAsAsync<List<T>>();
        //        readTask.Wait();
        //        detections = readTask.Result.ToList();
        //    }
        //    return (detections);
        //}

        //public async Task<IViewComponentResult> InvokeAsync(string User_OID)
        //{
        //    IEnumerable<Expiration> items = GetAllItems<Expiration>("PersonalFinanceAPI", "Expirations", User_OID);
        //        items= items.OrderBy(x => x.ExpDateTime.Month).Take(5).ToList(); //Fetch imminent expirations

        //    return await Task.FromResult((IViewComponentResult)View(items));
        //}
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