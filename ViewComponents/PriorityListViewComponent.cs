using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PersonalFinanceFrontEnd.Controllers;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System;
using System.Net.Http;
using PersonalFinanceFrontEnd.Models;

namespace PersonalFinanceFrontEnd.ViewComponents
{
    public class PriorityListViewComponent : ViewComponent
    {
        public IEnumerable<T> GetFirst<T>(string controller, string User_OID)
        {
            IEnumerable<T> detections = null;
            string path = "api/" + controller + "/GetFirst" + "?User_OID=" + User_OID;
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri("https://personalfinanceappapi.azurewebsites.net/");
                var responseTask = client.GetAsync(path);
                responseTask.Wait();
                var result = responseTask.Result;
                var readTask = result.Content.ReadAsAsync<List<T>>();
                readTask.Wait();
                detections = readTask.Result;
            }
            return (detections);
        }

        public async Task<IViewComponentResult> InvokeAsync(string User_OID)
        {
            return await Task.FromResult((IViewComponentResult)View(GetFirst<Expiration>("Expirations", User_OID)));
        }
    }
}