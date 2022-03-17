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
        [AuthorizeForScopes(ScopeKeySection = "DownstreamApi:Scopes")]
        public ActionResult KnownMovements()
        {
            string User_OID = GetUserData().Result; //Fetch User Data 
            KnownMovements knownMovements = new()
            {
                KnownMovementList = GetAllItemsN<KnownMovement>("KnownMovements", User_OID)
            };
            ViewBag.state = (int)(TempData.ContainsKey("sendFlagKM") ? TempData["sendFlagKM"] : 0);
            ViewBag.ID = User_OID;
            return View(knownMovements);
        }
        public IActionResult KnownMovement_Add()
        {
            return View(new KnownMovement());
        }
        [HttpPost]
        public ActionResult KnownMovement_Add(KnownMovement k)
        {
            k.Usr_OID = GetUserData().Result;
            k.KMValue = Convert.ToDouble(k.Input_value.Replace(".", ","));
            int result = AddItemN<KnownMovement>("KnownMovements", k);
            if (result == 0)
            {
                TempData["sendFlagKM"] = 3;
                return RedirectToAction(nameof(KnownMovements));
            }
            return View();
        }

        public ActionResult KnownMovement_Edit(int id)
        {
            return KnownMovement_Details(id);
        }

        [HttpPost]
        public ActionResult KnownMovement_Edit(KnownMovement k)
        {
            k.KMValue = Convert.ToDouble(k.Input_value.Replace(".", ","));
            k.Usr_OID = GetUserData().Result;
            int result = EditItemIDN<KnownMovement>("KnownMovements", k);
            if (result == 0)
            {
                TempData["sendFlagKM"] = 2;
                return RedirectToAction(nameof(KnownMovements));
            }
            return View();
        }

        [HttpPost]
        public ActionResult KnownMovement_Exp_Update(KnownMovement_Exp KM_Exp)
        {
            KM_Exp.Usr_OID = GetUserData().Result;
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri("https://personalfinanceappapi.azurewebsites.net/api/KnownMovements/");
                var postTask = client.PutAsJsonAsync<KnownMovement_Exp>("UpdateExpOnKnownMovement", KM_Exp);
                postTask.Wait();
                var result = postTask.Result;
            }
            return RedirectToAction(nameof(KnownMovements));
        }
        public ActionResult KnownMovement_Details(int id)
        {
            KnownMovement KnownMovement = GetItemIDN<KnownMovement>("KnownMovements", id, GetUserData().Result);
            KnownMovement.Input_value = KnownMovement.KMValue.ToString();
            return PartialView(KnownMovement);
        }
        public ActionResult KnownMovement_Delete(int id)
        {
            return KnownMovement_Details(id);
        }
        [HttpPost]
        public ActionResult KnownMovement_Delete(KnownMovement k)
        {
            int result = DeleteItemN("KnownMovements", k.ID, GetUserData().Result);
            if (result == 0)
            {
                TempData["sendFlagKM"] = 1;
                return RedirectToAction(nameof(KnownMovements));
            }
            return View();
        }

    }
}
