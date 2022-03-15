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
        public ActionResult Transaction_Details(int id)
        {
            return PartialView(GetItemIDN<Transaction>("Transactions", id, GetUserData().Result));
        }
        public IActionResult Transaction_Add()
        {
            return View(new Transaction());
        }
        [HttpPost]
        public ActionResult Transaction_Add(Transaction t)
        {
            t.input_value = t.input_value.Replace(".", ",");
            t.TrsValue = Convert.ToDouble(t.input_value);
            if (t.Type == false) t.TrsValue = -t.TrsValue;
            if (t.NewTrsCode != null) t.TrsCode = t.NewTrsCode;
            t.Usr_OID = GetUserData().Result;
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri("https://personalfinanceappapi.azurewebsites.net/api/Transactions/");
                var postTask = client.PostAsJsonAsync<Transaction>("Add", t);
                postTask.Wait();
                var result = postTask.Result;
                if (result.IsSuccessStatusCode)
                {
                    TempData["sendFlagTr"] = 3;
                    Transaction_Balance_Update(t.Usr_OID);
                    Transaction_Credit_Debit_Update(t);
                    return RedirectToAction(nameof(Transactions));
                }
            }
            return View();
        }
        public ActionResult Transaction_Delete(int id)
        {
            return Transaction_Details(id);
        }
        [HttpPost]
        public ActionResult Transaction_Delete(Transaction t)
        {
            int result = DeleteItemN("Transactions", t.ID, GetUserData().Result);
            if (result == 0)
            {
                TempData["sendFlagTr"] = 1;
                Transaction_Balance_Update(GetUserData().Result);
                return RedirectToAction(nameof(Transactions));
            }
            return View();
        }
    }
}
