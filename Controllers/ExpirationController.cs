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
            //ViewBag.Expirations = Expirations.Where(x => x.ExpDateTime.Year.ToString() == DateTime.Now.Year.ToString()).OrderBy(item => item.ExpDateTime.Month).Take(5).ToList(); //Fetch imminent expirations
            ////Trovo gli anni "unici"
            //var UniqueYear = Expirations.GroupBy(item => item.ExpDateTime.Year)
            //        .Select(group => group.First())
            //        .Select(item => item.ExpDateTime.Year)
            //        .ToList();
            ////Creo la lista di anni "unici" per il dropdown filter del grafico saldo
            //List<SelectListItem> itemlistYear = new();
            //foreach (var year in UniqueYear.Skip(1)) itemlistYear.Add(new SelectListItem() { Text = year.ToString(), Value = year.ToString() });
            //Passo alla view la lista
            ViewBag.ItemList = Expirations.ItemlistYear;
            //Se al caricamento della pagina ho selezionato un anno (not empty), salvo in Balances i saldi di quell'anno
            //if (!String.IsNullOrEmpty(selectedYear)) Expirations.ExpirationList = Expirations.ExpirationList.AsQueryable().Where(x => x.ExpDateTime.Year.ToString() == selectedYear).OrderBy(item => item.ExpDateTime.Month);
            //else Expirations.ExpirationList = Expirations.ExpirationList.AsQueryable().Where(x => x.ExpDateTime.Year.ToString() == DateTime.Now.Year.ToString()).OrderBy(item => item.ExpDateTime.Month);


            //ViewModel viewModel = new ViewModel();
            //viewModel.Expiration = new Expiration();




            /*var UniqueMonth = Expirations.GroupBy(item => item.ExpDateTime.Month)
                                            .Select(group => group.First())
                                            .Select(item => item.ExpDateTime.Month)
                                            .ToList();*/
            //List<string> UniqueMonthNames = new();



            //List<ExpMonth> expMonth = new();
            //foreach (var month in Expirations.UniqueMonth)
            //{
            //    UniqueMonthNames.Add(MonthConverter(month));
            //    var singleMonthExp = Expirations.ExpirationList.AsQueryable().Where(x => x.ExpDateTime.Month.ToString() == month.ToString());
            //    foreach (var exp in singleMonthExp)
            //    {
            //        ExpMonth item = new();
            //        item.Month = MonthConverter(month);
            //        item.ExpItem = exp;
            //        expMonth.Add(item);
            //    }
            //}

            //ViewBag.UniqueMonth = Expirations.UniqueMonth;
            ViewBag.UniqueMonthNames = Expirations.UniqueMonthNames;
            //viewModel.ExpirationList = expMonth;
            //Expirations.ExpMonth = expMonth;


            return View(Expirations);
        }
        public T GetAllItemsMain<T>(string controller, string User_OID, string selectedYear)
        {
            T detections = default;
            //T detections = null;
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
