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
        //WALLET Intermediate view
        [AuthorizeForScopes(ScopeKeySection = "DownstreamApi:Scopes")]
        public ActionResult Wallet()
        {
            string User_OID = GetUserData().Result; //Fetch User Data
            ViewModel viewModel = new ViewModel();
            viewModel.Banks = GetAllItems<Bank>("PersonalFinanceAPI", "Banks", User_OID);
            viewModel.Bank = new Bank();
            viewModel.Deposits = GetAllItems<Deposit>("PersonalFinanceAPI", "Deposits", User_OID);
            viewModel.Deposit = new Deposit();
            viewModel.Tickets = GetAllItems<Ticket>("PersonalFinanceAPI", "Tickets", User_OID); ;
            viewModel.Ticket = new Ticket();
            viewModel.Contanti = viewModel.Banks.First();
            return View(viewModel);
        }

    }
}
