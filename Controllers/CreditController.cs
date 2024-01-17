using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.VisualBasic.Syntax;
using Microsoft.Identity.Web;
using PersonalFinanceFrontEnd.Models;

namespace PersonalFinanceFrontEnd.Controllers
{
    public partial class PersonalFinanceController
    {
        [Route("PersonalFinance/Credits")]
        [AuthorizeForScopes(ScopeKeySection = "DownstreamApi:Scopes")]
        public ActionResult Credits()
        {
            string User_OID = GetUserData().Result; //Fetch User Data
            Credits Credits = new()
            {
                CreditList = GetAllItems<Credit>("Credits", User_OID)
            };
            //ViewBag.state = (int)(TempData.ContainsKey("sendFlagCred") ? TempData["sendFlagCred"] : 0);
            return View(Credits);
        }
        public ActionResult Credit_Details(int id)
        {
            Credit Credit = GetItemID<Credit>("Credits", id, GetUserData().Result);
            Credit.Input_value = Credit.CredValue.ToString();
            return PartialView(Credit);
        }
        public IActionResult Credit_Add()
        {
            return View(new Credit());
        }
        [HttpPost]
        public ActionResult Credit_Add(Credit c, int i)
        {
            if(c.PrevDateTime <= c.CredDateTime)
            {
                _notyf.Error("La data di restituzione deve essere successiva a quella di inserimento.");
                return RedirectToAction(nameof(Credits));
            }
            
            if(CheckNameExist("CRE " + c.CredTitle, "Credits"))
            {
                _notyf.Error("Il codice inserito è già presente. Scegliere un nome diverso");
                return RedirectToAction(nameof(Credits));
            }
            
            if (i != 1)
            {
                c.Input_value = c.Input_value.Replace(",", ".");
                c.CredValue = Convert.ToDouble(c.Input_value);
                c.Usr_OID = GetUserData().Result;
            }

            int result = AddItemN<Credit>("Credits", c);
            if (result == 0)
            {
                _notyf.Success("Credito inserito correttamente.");
                return RedirectToAction(nameof(Credits));
            }
            return View();
        }
        public ActionResult Credit_Edit(int id)
        {
            return Credit_Details(id);
        }
        [HttpPost]
        public ActionResult Credit_Edit(Credit c, int i)
        {
            if (i != 1)
            {
                c.Input_value = c.Input_value.Replace(",", ".");
                c.CredValue = Convert.ToDouble(c.Input_value);
            }
            c.Usr_OID = GetUserData().Result;
            int result = EditItemIDN<Credit>("Credits", c);
            if (result == 0)
            {
                _notyf.Success("Credito modificato correttamente.");
                return RedirectToAction(nameof(Credits));
            }
            return View();
        }
        public ActionResult Credit_Delete(int id)
        {
            return Credit_Details(id);
        }
        [HttpPost]
        public ActionResult Credit_Delete(Credit c)
        {
            int result = DeleteItemN("Credits", c.ID, GetUserData().Result);
            if (result == 0)
            {
                _notyf.Warning("Credito rimosso correttamente.");
                return RedirectToAction(nameof(Credits));
            }
            return View();
        }        
    }
}
