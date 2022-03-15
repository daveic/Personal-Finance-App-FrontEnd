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
using System.Net.Http.Json;

namespace PersonalFinanceFrontEnd.Controllers
{
    public partial class PersonalFinanceController
    {
        //DEBITS Intermediate view
        [AuthorizeForScopes(ScopeKeySection = "DownstreamApi:Scopes")]
        public ActionResult Debits()
        {
            Debits debits = new()
            {
                DebitList = GetAllItemsN<Debit>("Debits", GetUserData().Result)
            };            
            ViewBag.state = (int)(TempData.ContainsKey("sendFlagDeb") ? TempData["sendFlagDeb"] : 0);
            return View(debits);
        }
        public ActionResult Debit_Details(int id)
        {
            Debit Debit = GetItemIDN<Debit>("Debits", id, GetUserData().Result);
            Debit.input_value = Debit.DebValue.ToString();
            Debit.input_value_remain = Debit.RemainToPay.ToString();
            return PartialView(Debit);
        }
        public IActionResult Debit_Add()
        {
            Debit model = new()
            {
                DebDateTime = DateTime.MinValue
            };
            return View(model);
        }
        [HttpPost]
        public ActionResult Debit_Add(Debit d, int i)
        {
            if (i != 1)
            {
                d.input_value_remain = d.input_value_remain.Replace(".", ",");
                d.RemainToPay = Convert.ToDouble(d.input_value_remain);
                d.input_value = d.input_value.Replace(".", ",");
                d.DebValue = Convert.ToDouble(d.input_value);
            }
            d.Usr_OID = GetUserData().Result;
            //if (d.DebDateTime == DateTime.MinValue)
            //{
            //    d.DebDateTime = d.DebInsDate.AddMonths(Convert.ToInt32((d.RtNum * d.Multiplier)));
            //}

            //for (int k = 0; k < d.RtNum; k++)
            //{
            //    Expiration exp = new Expiration();
            //    exp.Usr_OID = d.Usr_OID;
            //    exp.ExpTitle = d.DebTitle;
            //    exp.ExpDescription = d.DebTitle + "rata: " + (k + 1);
            //    if (d.RtFreq == "Mesi")
            //    {
            //        exp.ExpDateTime = d.DebInsDate.AddMonths(k * d.Multiplier);
            //    }
            //    if (d.RtFreq == "Anni")
            //    {
            //        exp.ExpDateTime = d.DebInsDate.AddYears(k * d.Multiplier);
            //    }
            //    exp.ColorLabel = "red";
            //    exp.ExpValue = d.DebValue / d.RtNum;
            //    AddItem<Expiration>("Expiration", exp);
            //}
            //IEnumerable<Expiration> Expirations = GetAllItems<Expiration>("PersonalFinanceAPI", nameof(Expirations), d.Usr_OID);
            //d.Exp_ID = Expirations.Last().ID - Convert.ToInt32(d.RtNum) + 1;
            int result = AddItemN<Debit>("Debits", d); //????
            if (result == 0)
            {
                TempData["sendFlagDeb"] = 3;

                return RedirectToAction(nameof(Debits));
            }
            return View();
        }
        public ActionResult Debit_Edit(int id)
        {
            return Debit_Details(id);
        }
        [HttpPost]
        public ActionResult Debit_Edit(Debit d, int i, bool fromTransaction)
        {
            if (i != 1)
            {
                d.input_value = d.input_value.Replace(".", ",");
                d.DebValue = Convert.ToDouble(d.input_value);
                d.input_value_remain = d.input_value_remain.Replace(".", ",");
                d.RemainToPay = Convert.ToDouble(d.input_value_remain);
            }
            d.Usr_OID = GetUserData().Result;
            int result = EditItemID<Debit>(nameof(Debit), d);
            if (result == 0)
            {
                TempData["sendFlagDeb"] = 2;
                return RedirectToAction(nameof(Debits));
            }
            return View();
        }
        [HttpPost]
        public ActionResult Debit_Exp_Update(Debit d, bool fromTransaction)
        {
            Debit_Exp dexp = new()
            {
                Debit = d,
                FromTransaction = fromTransaction
            };
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri("https://personalfinanceappapi.azurewebsites.net/api/Debits/");
                var postTask = client.PutAsJsonAsync<Debit_Exp>("UpdateExpOnDebit", dexp);
                postTask.Wait();
                var result = postTask.Result;
            }
            return RedirectToAction(nameof(KnownMovements));
        }
        public ActionResult Debit_Delete(int id)
        {
            return Debit_Details(id);
        }
        [HttpPost]
        public ActionResult Debit_Delete(Debit d)
        {
            Debit deb = GetItemIDN<Debit>("Debits", d.ID, GetUserData().Result);
            for (int i = 0; i <= (deb.RtNum - deb.RtPaid); i++)
            {
                int res = DeleteItemN("Expirations", (deb.Exp_ID + i), GetUserData().Result);
            }
            int result = DeleteItemN("Debits", d.ID, GetUserData().Result);
            if (result == 0)
            {
                TempData["sendFlagDeb"] = 1;
                return RedirectToAction(nameof(Debits));
            }
            return View();
        }

    }
}
