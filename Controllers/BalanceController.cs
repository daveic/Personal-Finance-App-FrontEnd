using Microsoft.AspNetCore.Mvc;
using PersonalFinanceFrontEnd.Models;
using System;
using System.Collections.Generic;

namespace PersonalFinanceFrontEnd.Controllers
{
    public partial class PersonalFinanceController
    {
        //FAST UPDATE LOGIC
        //Vengono presentati tutti i conti coi relativi saldi e l'utente ha la possibilità di modificare il saldo di ognuno, aggiornando poi il Balance totale
        public ActionResult Fast_Update()
        {
            string User_OID = GetUserData().Result;
            ViewModel model = new()
            {
                Banks = GetAllItems<Bank>("Banks", User_OID),
                Tickets = GetAllItems<Ticket>("Tickets", User_OID)
            };
            List<Bank> BankList = new();
            List<Ticket> TicketList = new();
            foreach (var item in model.Banks)
            {
                item.Input_value = item.BankValue.ToString();
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
            IEnumerable<Bank> Banks = GetAllItems<Bank>("Banks", User_OID);
            IEnumerable<Ticket> Tickets = GetAllItems<Ticket>("Tickets", User_OID);
            foreach (var item in BankList)
            {
                foreach (var bank in Banks)
                {
                    if (item.ID == bank.ID)
                    {
                        item.Input_value = item.Input_value.Replace(",", ".");
                        bank.BankValue = Convert.ToDouble(item.Input_value);
                        int result = EditItemIDN<Bank>("Banks", bank);
                    }
                }
            }
            if(TicketList != null)
            {
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
            }
            BalanceUpdate(User_OID , 1);
            return RedirectToAction(nameof(Index));
        }
        [HttpPost]
        public int BalanceUpdate(string User_OID, int fromFU)
        {
            Balance b = new()
            {
                Usr_OID = User_OID,
                FromFU = fromFU,
                BalDateTime = DateTime.UtcNow
            };
            int result = AddItemN<Balance>("Balances", b);
            return result;
        }
    }
}