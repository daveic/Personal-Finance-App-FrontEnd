using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Web;
using PersonalFinanceFrontEnd.Models;

namespace PersonalFinanceFrontEnd.Controllers
{
    public partial class PersonalFinanceController
    {
        //DEBITS Intermediate view
        [AuthorizeForScopes(ScopeKeySection = "DownstreamApi:Scopes")]
        public ActionResult Expirations(string selectedYear)
        {
            string User_OID = GetUserData().Result; //Fetch User Data
            Expirations Expirations = (Expirations)GetAllItemsMain<Expirations>("Expirations", User_OID, selectedYear);

            //Lista di anni "unici" per il dropdown filter del grafico saldo
            ViewBag.ItemList = Expirations.ItemlistYear;
            //Lista di mesi "unici" per il dropdown filter del grafico saldo
            ViewBag.UniqueMonthNames = Expirations.UniqueMonthNames;

            return View(Expirations);
        }
        public T GetAllItemsMain<T>(string controller, string User_OID, string selectedYear)
        {
            T detections = default;
            string path = "api/" + controller + "/All" + "?User_OID=" + User_OID + "&selectedYear=" + selectedYear;
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri("https://personalfinanceappapi.azurewebsites.net/");
                var responseTask = client.GetAsync(path);
                responseTask.Wait();
                var result = responseTask.Result;
                var readTask = result.Content.ReadAsAsync<T>();
                readTask.Wait();
                detections = readTask.Result;
            }
            
            return (detections);
        }
        public IActionResult Expiration_Add()
        {
            return View(new Expiration());
        }
        [HttpPost]
        public ActionResult Expiration_Add(Expiration e)
        {
            e.Input_value = e.Input_value.Replace(".", ",");
            e.ExpValue = Convert.ToDouble(e.Input_value);
            e.Usr_OID = GetUserData().Result;
            e.ExpTitle = "SCD " + e.ExpTitle;
            int result = AddItemN<Expiration>("Expirations", e);
            if (result == 0)
            {
                return RedirectToAction(nameof(Expirations));
            }
            return View();
        }
        public ActionResult Expiration_Details(int id)
        {
            Expiration Expiration = GetItemID<Expiration>("Expirations", id, GetUserData().Result);
            Expiration.Input_value = Expiration.ExpValue.ToString();
            return PartialView(Expiration);
        }
        public ActionResult Expiration_Delete(int id)
        {
            return Expiration_Details(id);
        }
        [HttpPost]
        public ActionResult Expiration_Delete(Expiration e)
        {
            int result = DeleteItemN("Expirations", e.ID, GetUserData().Result);
            if (result == 0)
            {
                TempData["sendFlagT"] = 1;
                return RedirectToAction(nameof(Expirations));
            }
            return View();
        }
    }
}
