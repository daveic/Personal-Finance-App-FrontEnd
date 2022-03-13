using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Graph;
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
            Expirations Expirations = (Expirations)GetAllItemsN<Expirations>("Expirations", User_OID);
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
            if (!String.IsNullOrEmpty(selectedYear)) Expirations.ExpirationList = Expirations.ExpirationList.AsQueryable().Where(x => x.ExpDateTime.Year.ToString() == selectedYear).OrderBy(item => item.ExpDateTime.Month);
            else Expirations.ExpirationList = Expirations.ExpirationList.AsQueryable().Where(x => x.ExpDateTime.Year.ToString() == DateTime.Now.Year.ToString()).OrderBy(item => item.ExpDateTime.Month);


            //ViewModel viewModel = new ViewModel();
            //viewModel.Expiration = new Expiration();




            /*var UniqueMonth = Expirations.GroupBy(item => item.ExpDateTime.Month)
                                            .Select(group => group.First())
                                            .Select(item => item.ExpDateTime.Month)
                                            .ToList();*/
            List<string> UniqueMonthNames = new List<string>();



            List<ExpMonth> expMonth = new List<ExpMonth>();
            foreach (var month in Expirations.UniqueMonth)
            {
                UniqueMonthNames.Add(MonthConverter(month));
                var singleMonthExp = Expirations.ExpirationList.AsQueryable().Where(x => x.ExpDateTime.Month.ToString() == month.ToString());
                foreach (var exp in singleMonthExp)
                {
                    ExpMonth item = new ExpMonth();
                    item.Month = MonthConverter(month);
                    item.ExpItem = exp;
                    expMonth.Add(item);
                }
            }

            //ViewBag.UniqueMonth = Expirations.UniqueMonth;
            ViewBag.UniqueMonthNames = UniqueMonthNames;
            //viewModel.ExpirationList = expMonth;
            Expirations.ExpMonth = expMonth;


            return View(Expirations);
        }
        public IActionResult Expiration_Add()
        {
            return View(new Expiration());
        }
        [HttpPost]
        public ActionResult Expiration_Add(Expiration e)
        {
            e.input_value = e.input_value.Replace(".", ",");
            e.ExpValue = Convert.ToDouble(e.input_value);
            e.Usr_OID = GetUserData().Result;
            int result = AddItemN<Expiration>("Expirations", e);
            if (result == 0)
            {
                return RedirectToAction(nameof(Expirations));
            }
            return View();
        }


    }
}
