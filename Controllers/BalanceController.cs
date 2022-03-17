using Microsoft.AspNetCore.Mvc;
using PersonalFinanceFrontEnd.Models;
using System;
using System.Collections.Generic;
using System.Net.Http;

namespace PersonalFinanceFrontEnd.Controllers
{
    public partial class PersonalFinanceController
    {
        //FAST UPDATE LOGIC
        //In caso di aggiunta di nuova banca/ticket, occorre solo aggiungere l'immagine sotto images con nome in minuscolo e "-" al posto degli spazi (in .jpeg)
        public ActionResult Fast_Update()
        {
            string User_OID = GetUserData().Result;
            ViewModel model = new();
            model.Banks = GetAllItemsN<Bank>("Banks", User_OID);
            model.Tickets = GetAllItemsN<Ticket>("Tickets", User_OID);
            List<Bank> BankList = new();
            List<Ticket> TicketList = new();
            foreach (var item in model.Banks)
            {
                item.input_value = item.BankValue.ToString();
                BankList.Add(item);
            }
            model.BankList = BankList;
            foreach (var item in model.Tickets)
            {
                item.input_value = item.NumTicket.ToString();
                TicketList.Add(item);
            }
            model.TicketList = TicketList;
            return PartialView(model);
        }
        [HttpPost]
        public ActionResult Fast_Update(ViewModel model)
        {
            string User_OID = GetUserData().Result;
            List<Bank> BankList = model.BankList;
            List<Ticket> TicketList = model.TicketList;
            IEnumerable<Bank> Banks = GetAllItemsN<Bank>("Banks", User_OID);
            IEnumerable<Ticket> Tickets = GetAllItemsN<Ticket>("Tickets", User_OID);
            foreach (var item in BankList)
            {
                foreach (var bank in Banks)
                {
                    if (item.ID == bank.ID)
                    {
                        item.input_value = item.input_value.Replace(".", ",");
                        bank.BankValue = Convert.ToDouble(item.input_value);
                        int result = EditItemIDN<Bank>("Banks", bank);
                    }
                }
            }
            foreach (var itemt in TicketList)
            {
                foreach (var ticket in Tickets)
                {
                    if (itemt.ID == ticket.ID)
                    {
                        ticket.NumTicket = itemt.input_value;
                        int result = EditItemIDN<Ticket>("Tickets", ticket);
                    }
                }
            }
            Balance_Update(User_OID);
            return RedirectToAction(nameof(Index));
        }
        public int Balance_Update(string User_OID)
        {
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri("https://personalfinanceappapi.azurewebsites.net/api/Balances/");
                client.PostAsJsonAsync<string>("Update", User_OID).Wait();
                var result = client.PostAsJsonAsync<string>("Update", User_OID).Result;
            }
            return 1;
        }
    }
}
