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
        public ActionResult Debits()
        {
            Debits debits = new()
            {
                DebitList = GetAllItemsN<Debit>("Debits", GetUserData().Result)
            };
            //viewModel.Debits = 
            //List<SelectListItem> Frequency = new List<SelectListItem>();
            /* List<string> Codes = {["Settimana", "Mese", "Anno"]};
            foreach (var item in UniqueCodes)
             {
                 SelectListItem code = new SelectListItem();
                 code.Value = item.TrsCode;
                 code.Text = item.TrsCode;
                 Codes.Add(code);
             }*/
            
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
            Debit model = new Debit();
            model.DebDateTime = DateTime.MinValue;
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
            int result = AddItemN<Debit>("Debits", d);
            if (result == 0)
            {
                TempData["sendFlagDeb"] = 3;

                return RedirectToAction(nameof(Debits));
            }
            return View();
        }
    }
}
