using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Web;
using PersonalFinanceFrontEnd.Models;

namespace PersonalFinanceFrontEnd.Controllers
{
    public partial class PersonalFinanceController
    {
        public ActionResult Credit_Details(int id)
        {
            Credit Credit = GetItemIDN<Credit>("Credits", id, GetUserData().Result);
            Credit.input_value = Credit.CredValue.ToString();
            return PartialView(Credit);
        }
        public IActionResult Credit_Add()
        {
            return View(new Credit());
        }
        [HttpPost]
        public ActionResult Credit_Add(Credit c, int i)
        {
            if (i != 1)
            {
                c.input_value = c.input_value.Replace(".", ",");
                c.CredValue = Convert.ToDouble(c.input_value);
                c.Usr_OID = GetUserData().Result;
            }
            int result = AddItemN<Credit>("Credits", c);
            if (result == 0)
            {
                TempData["sendFlagCred"] = 3;
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
                c.input_value = c.input_value.Replace(".", ",");
                c.CredValue = Convert.ToDouble(c.input_value);
            }
            c.Usr_OID = GetUserData().Result;
            int result = EditItemIDN<Credit>("Credits", c);
            if (result == 0)
            {
                TempData["sendFlagCred"] = 2;
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
                TempData["sendFlagCred"] = 1;
                return RedirectToAction(nameof(Credits));
            }
            return View();
        }
        [Route("PersonalFinance/Credits")]
        [AuthorizeForScopes(ScopeKeySection = "DownstreamApi:Scopes")]
        public ActionResult Credits()
        {
            string User_OID = GetUserData().Result; //Fetch User Data
            Credits Credits = new()
            {
                CreditList = GetAllItemsN<Credit>("Credits", User_OID)
            };
            ViewBag.state = (int)(TempData.ContainsKey("sendFlagCred") ? TempData["sendFlagCred"] : 0);
            return View(Credits);
        }
    }
}
