using System;
using Microsoft.AspNetCore.Mvc;
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
            ViewBag.state = (int)(TempData.ContainsKey("sendFlagDeb") ? TempData["sendFlagDeb"] : 0);
            return View(debits);
        }
        public ActionResult Debit_Details(int id)
        {
            Debit Debit = GetItemIDN<Debit>("Debits", id, GetUserData().Result);
            Debit.Input_value = Debit.DebValue.ToString();
            Debit.Input_value_remain = Debit.RemainToPay.ToString();
            return PartialView(Debit);
        }
        public IActionResult Debit_Add()
        {
            return View(new Debit());
        }
        [HttpPost]
        public ActionResult Debit_Add(Debit d, int i)
        {
            if (i != 1)
            {
                d.Input_value = d.Input_value.Replace(".", ",");
                d.DebValue = Convert.ToDouble(d.Input_value);
                d.RemainToPay = d.DebValue;
            }
            d.DebInsDate = DateTime.Now;
            d.RtNum = 1;
            d.RtPaid = 0;
            d.RtFreq = "Mesi";
            d.Multiplier = 0;
            d.Usr_OID = GetUserData().Result;
            int result = AddItemN<Debit>("Debits", d);
            if (result == 0)
            {
                TempData["sendFlagDeb"] = 3;

                return RedirectToAction(nameof(Debits));
            }
            return View();
        }
        public IActionResult Debit_Add_Part()
        {
            return View(new Debit());
        }
        [HttpPost]
        public ActionResult Debit_Add_Part(Debit d, int i)
        {
            if (i != 1)
            {
                d.Input_value = d.Input_value.Replace(".", ",");
                d.DebValue = Convert.ToDouble(d.Input_value);
                d.RemainToPay = d.DebValue;
            }
            d.DebInsDate = DateTime.Now;
            d.DebDateTime = DateTime.MinValue;
            d.RtPaid = 0;
            d.Usr_OID = GetUserData().Result;

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
        public ActionResult Debit_Edit(Debit d, int i/*, bool fromTransaction*/)
        {
            if (i != 1)
            {
                if(d.Input_value != null)
                {
                    d.Input_value = d.Input_value.Replace(".", ",");
                    d.DebValue = Convert.ToDouble(d.Input_value);
                }
                if(d.Multiplier != 0) //Sto modificando un debito a rate
                {
                    d.Input_value_remain = d.Input_value_remain.Replace(".", ",");
                    d.RemainToPay = Convert.ToDouble(d.Input_value_remain);
                }
            }
            d.Usr_OID = GetUserData().Result;
            int result = EditItemIDN<Debit>("Debits", d);
            if (result == 0)
            {
                TempData["sendFlagDeb"] = 2;
                return RedirectToAction(nameof(Debits));
            }
            return View();
        }

        //[HttpPost]
        //public ActionResult Debit_Exp_Update(Debit d, bool fromTransaction)
        //{
        //    Debit_Exp dexp = new()
        //    {
        //        Debit = d,
        //        FromTransaction = fromTransaction
        //    };
        //    using (var client = new HttpClient())
        //    {
        //        client.BaseAddress = new Uri("https://personalfinanceappapi.azurewebsites.net/api/Debits/");
        //        var postTask = client.PutAsJsonAsync<Debit_Exp>("UpdateExpOnDebit", dexp);
        //        postTask.Wait();
        //        var result = postTask.Result;
        //    }
        //    return RedirectToAction(nameof(KnownMovements));
        //}
        public ActionResult Debit_Delete(int id)
        {
            return Debit_Details(id);
        }
        [HttpPost]
        public ActionResult Debit_Delete(Debit d)
        {
            //Debit deb = GetItemIDN<Debit>("Debits", d.ID, GetUserData().Result);
            //for (int i = 0; i <= (deb.RtNum - deb.RtPaid); i++)
            //{
            //    int res = DeleteItemN("Expirations", (deb.Exp_ID + i), GetUserData().Result);
            //}
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
