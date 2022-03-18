using System;
using System.Net.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Web;
using PersonalFinanceFrontEnd.Models;

namespace PersonalFinanceFrontEnd.Controllers
{
    public partial class PersonalFinanceController
    {
        //CRUD Service: HTTP GET-ALL Method
        public T GetWallet<T>(string controller, string User_OID)
        {
            T detections = default;
            string path = "api/" + controller + "/All" + "?User_OID=" + User_OID;
            using (HttpClient client = new())
            {
                client.BaseAddress = new Uri("https://personalfinanceappapi.azurewebsites.net/");
                client.GetAsync(path).Wait();
                client.GetAsync(path).Result.Content.ReadAsAsync<T>().Wait();
                detections = client.GetAsync(path).Result.Content.ReadAsAsync<T>().Result;
            }
            return detections;
        }

        //WALLET Main View
        [AuthorizeForScopes(ScopeKeySection = "DownstreamApi:Scopes")]
        public ActionResult Wallet()
        {
            return View(GetWallet<Wallet>("Wallet", GetUserData().Result));
        }

        //DETAILS Methods
        public ActionResult Bank_Details(int id)
        {
            Bank Bank = GetItemIDN<Bank>("Banks", id, GetUserData().Result);
            Bank.Input_value = Bank.BankValue.ToString();
            return PartialView(Bank);
        }
        public ActionResult Deposit_Details(int id)
        {
            Deposit Deposit = GetItemIDN<Deposit>("Deposits", id, GetUserData().Result);
            Deposit.input_value = Deposit.DepValue.ToString();
            return PartialView(Deposit);
        }
        public ActionResult Ticket_Details(int id)
        {
            Ticket Ticket = GetItemIDN<Ticket>("Tickets", id, GetUserData().Result);
            Ticket.input_value = Ticket.TicketValue.ToString();
            return PartialView(Ticket);
        }

        //ADD Methods
        public IActionResult Bank_Add()
        {
            return View(new Bank());
        }
        [HttpPost]
        public ActionResult Bank_Add(Bank b)
        {
            b.Input_value = b.Input_value.Replace(".", ",");
            b.BankValue = Convert.ToDouble(b.Input_value);
            b.Usr_OID = GetUserData().Result;
            int result = AddItemN<Bank>("Banks", b);
            if (result == 0)
            {
                Balance_Update(b.Usr_OID);
                return RedirectToAction(nameof(Wallet));
            }
            return View();
        }
        public IActionResult Deposit_Add()
        {
            return View(new Deposit());
        }
        [HttpPost]
        public ActionResult Deposit_Add(Deposit d)
        {
            d.input_value = d.input_value.Replace(".", ",");
            d.DepValue = Convert.ToDouble(d.input_value);
            d.Usr_OID = GetUserData().Result;
            int result = AddItemN<Deposit>("Deposits", d);
            if (result == 0)
            {
                return RedirectToAction(nameof(Wallet));
            }
            return View();
        }
        public IActionResult Ticket_Add()
        {
            return View(new Ticket());
        }
        [HttpPost]
        public ActionResult Ticket_Add(Ticket t)
        {
            t.input_value = t.input_value.Replace(".", ",");
            t.TicketValue = Convert.ToDouble(t.input_value);
            t.Usr_OID = GetUserData().Result;
            int result = AddItemN<Ticket>("Tickets", t);
            if (result == 0)
            {
                Balance_Update(t.Usr_OID);
                return RedirectToAction(nameof(Wallet));
            }
            return View();
        }

        //EDIT Methods
        public ActionResult Bank_Edit(int id)
        {
            return Bank_Details(id);
        }
        [HttpPost]
        public ActionResult Bank_Edit(Bank b)
        {
            b.Input_value = b.Input_value.Replace(".", ",");
            b.BankValue = Convert.ToDouble(b.Input_value);           
            b.Usr_OID = GetUserData().Result;
            int result = EditItemIDN<Bank>("Banks", b);
            if (result == 0)
            {
                Balance_Update(b.Usr_OID);
                return RedirectToAction(nameof(Wallet));
            }
            return View();
        }
        public ActionResult Deposit_Edit(int id)
        {
            return Deposit_Details(id);
        }
        [HttpPost]
        public ActionResult Deposit_Edit(Deposit d)
        {
            d.input_value = d.input_value.Replace(".", ",");
            d.DepValue = Convert.ToDouble(d.input_value);
            d.Usr_OID = GetUserData().Result;
            int result = EditItemIDN<Deposit>("Deposits", d);
            if (result == 0)
            {
                return RedirectToAction(nameof(Wallet));
            }
            return View();
        }
        public ActionResult Ticket_Edit(int id)
        {
            return Ticket_Details(id);
        }
        [HttpPost]
        public ActionResult Ticket_Edit(Ticket t)
        {
            t.input_value = t.input_value.Replace(".", ",");
            t.TicketValue = Convert.ToDouble(t.input_value);
            t.Usr_OID = GetUserData().Result;
            int result = EditItemIDN<Ticket>("Tickets", t);
            if (result == 0)
            {
                Balance_Update(t.Usr_OID);
                return RedirectToAction(nameof(Wallet));
            }
            return View();
        }

        //DELETE Methods
        public ActionResult Bank_Delete(int id)
        {
            return Bank_Details(id);
        }
        [HttpPost]
        public ActionResult Bank_Delete(Bank b)
        {
            int result = DeleteItemN("Banks", b.ID, GetUserData().Result);
            if (result == 0)
            {
                Balance_Update(GetUserData().Result);
                return RedirectToAction(nameof(Wallet));
            }
            return View();
        }
        public ActionResult Deposit_Delete(int id)
        {
            return Deposit_Details(id);
        }
        [HttpPost]
        public ActionResult Deposit_Delete(Deposit d)
        {
            int result = DeleteItemN("Deposits", d.ID, GetUserData().Result);
            if (result == 0)
            {
                return RedirectToAction(nameof(Wallet));
            }
            return View();
        }
        public ActionResult Ticket_Delete(int id)
        {
            return Ticket_Details(id);
        }
        [HttpPost]
        public ActionResult Ticket_Delete(Ticket t)
        {
            int result = DeleteItemN("Tickets", t.ID, GetUserData().Result);
            if (result == 0)
            {
                Balance_Update(GetUserData().Result);
                return RedirectToAction(nameof(Wallet));
            }
            return View();
        }
    }
}
