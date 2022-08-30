using AspNetCoreHero.ToastNotification.Abstractions;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace PersonalFinanceFrontEnd.Controllers
{
    public partial class PersonalFinanceController
    {
        //HTTP GET-ALL Generic method
        public IEnumerable<T> GetAllItems<T>(string controller, string User_OID)
        {
            IEnumerable<T> detections = null;
            string path = "api/" + controller + "/All" + "?User_OID=" + User_OID;
            using (HttpClient client = new())
            {
                client.BaseAddress = new Uri("https://personalfinanceappapi.azurewebsites.net/");
                client.GetAsync(path).Wait();
                client.GetAsync(path).Result.Content.ReadAsAsync<List<T>>().Wait();
                detections = client.GetAsync(path).Result.Content.ReadAsAsync<List<T>>().Result;
            }
            return detections;
        }
        //HTTP GET-BY-ID Generic method
        public T GetItemID<T>(string controller, int id, string User_OID) where T : new()
        {
            T detection = new();
            string path = "api/" + controller + "/Details" + "?id=" + id + "&User_OID=" + User_OID;
            using (HttpClient client = new())
            {
                client.BaseAddress = new Uri("https://personalfinanceappapi.azurewebsites.net/");
                client.GetAsync(path).Wait();
                client.GetAsync(path).Result.Content.ReadAsAsync<T>().Wait();
                detection = client.GetAsync(path).Result.Content.ReadAsAsync<T>().Result;
            }
            return detection;
        }
        //HTTP ADD Generic method
        public int AddItemN<T>(string controller, T obj) where T : new()
        {
            string path = "api/" + controller + "/Add";
            using (HttpClient client = new())
            {
                client.BaseAddress = new Uri("https://personalfinanceappapi.azurewebsites.net/");
                var postTask = client.PostAsJsonAsync(path, obj);
                postTask.Wait();
                var result = postTask.Result;
                if (result.IsSuccessStatusCode)
                {
                    return (0);
                }
            }
            return (1);
        }
        //HTTP EDIT Generic method
        public async Task<int> EditItemIDKAsync<T>(string controller, T obj) where T : new()
        {
            string path = "api/" + controller + "/Update";
            using (HttpClient client = new())
            {
                client.BaseAddress = new Uri("https://personalfinanceappapi.azurewebsites.net/");
                string json = JsonSerializer.Serialize(obj);
                var content = new StringContent(json);
                HttpResponseMessage response = await client.PutAsync(path, content);
                if (response.IsSuccessStatusCode) { return (0); }
            }
            return (1);
        }
        public int EditItemIDN<T>(string controller, T obj) where T : new()
        {
            string path = "api/" + controller + "/Update";
            using (HttpClient client = new())
            {
                client.BaseAddress = new Uri("https://personalfinanceappapi.azurewebsites.net/");
                client.DefaultRequestHeaders.TransferEncodingChunked = false;
                var postTask = client.PutAsJsonAsync<T>(path, obj);
                postTask.Wait();
                var result = postTask.Result;
                if (result.IsSuccessStatusCode) { return (0); }
            }
            return (1);
        }
        //HTTP DELETE Generic method
        public int DeleteItemN(string controller, int id, string User_OID)
        {
            string path = "api/" + controller + "/Delete" + "?id=" + id + "&User_OID=" + User_OID;
            using (HttpClient client = new())
            {
                client.BaseAddress = new Uri("https://personalfinanceappapi.azurewebsites.net/");
                var postTask = client.DeleteAsync(path);
                postTask.Wait();
                var result = postTask.Result; 
                if (result.IsSuccessStatusCode)
                {
                    return (0);
                }
            }
            return (1);
        }
    }
}
