using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PersonalFinanceFrontEnd.Controllers;
using PersonalFinanceFrontEnd.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System;
using System.Net.Http;

namespace PersonalFinanceFrontEnd.ViewComponents
{
    public class PriorityListViewComponent : ViewComponent
    {
        private IEnumerable<T> GetAllItems<T>(string controller, string type, string User_OID)
        {
            IEnumerable<T> detections = null;
            string path = "api/" + controller + "/GetAll" + type + "?User_OID=" + User_OID;
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri("https://personalfinanceappapi.azurewebsites.net/");
                var responseTask = client.GetAsync(path);
                responseTask.Wait();
                var result = responseTask.Result;
                var readTask = result.Content.ReadAsAsync<List<T>>();
                readTask.Wait();
                detections = readTask.Result.ToList();
            }
            return (detections);
        }

        public async Task<IViewComponentResult> InvokeAsync(string User_OID)
        {
            IEnumerable<Expiration> items = GetAllItems<Expiration>("PersonalFinanceAPI", "Expirations", User_OID);
                items= items.OrderBy(x => x.ExpDateTime.Month).Take(5).ToList(); //Fetch imminent expirations

            return await Task.FromResult((IViewComponentResult)View(items));
        }

    }
}