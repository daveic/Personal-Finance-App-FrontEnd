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
        public T GetWallet<T>(string controller, string User_OID)
        {
            T detections = default;
            //T detections = null;
            string path = "api/" + controller + "/All" + "?User_OID=" + User_OID;
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
        //WALLET Intermediate view
        [AuthorizeForScopes(ScopeKeySection = "DownstreamApi:Scopes")]
        public ActionResult Wallet()
        {
            string User_OID = GetUserData().Result; //Fetch User Data
            var wallet = GetWallet<Wallet>("Wallet", User_OID);
            //ViewModel viewModel = new ViewModel();
            //viewModel.Banks = GetAllItems<Bank>("PersonalFinanceAPI", "Banks", User_OID);
            //viewModel.Bank = new Bank();
            //viewModel.Deposits = GetAllItems<Deposit>("PersonalFinanceAPI", "Deposits", User_OID);
            //viewModel.Deposit = new Deposit();
            //viewModel.Tickets = GetAllItems<Ticket>("PersonalFinanceAPI", "Tickets", User_OID); ;
            //viewModel.Ticket = new Ticket();
            //viewModel.Contanti = viewModel.Banks.First();
            return View(wallet);
        }
        public ActionResult Bank_Details(int id)
        {
            Bank Bank = GetItemIDN<Bank>("Banks", id, GetUserData().Result);
            Bank.input_value = Bank.BankValue.ToString();
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
        public IActionResult Bank_Add()
        {
            return View(new Bank());
        }
        [HttpPost]
        public ActionResult Bank_Add(Bank b)
        {
            b.input_value = b.input_value.Replace(".", ",");
            b.BankValue = Convert.ToDouble(b.input_value);
            b.Usr_OID = GetUserData().Result;
            int result = AddItemN<Bank>("Banks", b);
            if (result == 0)
            {
                TempData["sendFlagB"] = 3;
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
                TempData["sendFlagD"] = 3;
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
                TempData["sendFlagT"] = 3;
                //Balance_Update(t.Usr_OID);
                return RedirectToAction(nameof(Wallet));
            }
            return View();
        }
        public ActionResult Bank_Edit(int id)
        {
            return Bank_Details(id);
        }
        [HttpPost]
        public ActionResult Bank_Edit(Bank b)
        {
            b.input_value = b.input_value.Replace(".", ",");
            b.BankValue = Convert.ToDouble(b.input_value);           
            b.Usr_OID = GetUserData().Result;
            int result = EditItemIDN<Bank>("Banks", b);
            if (result == 0)
            {
                TempData["sendFlagB"] = 2;
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
                TempData["sendFlagD"] = 2;
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
                TempData["sendFlagT"] = 2;
                Balance_Update(t.Usr_OID);
                return RedirectToAction(nameof(Wallet));
            }
            return View();
        }
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
                TempData["sendFlagB"] = 1;
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
                TempData["sendFlagD"] = 1;
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
                TempData["sendFlagT"] = 1;
                Balance_Update(GetUserData().Result);
                return RedirectToAction(nameof(Wallet));
            }
            return View();
        }

    }
}
