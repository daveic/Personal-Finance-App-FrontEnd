using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Azure;
using PersonalFinanceFrontEnd.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PersonalFinanceFrontEnd.Controllers
{
    public partial class PersonalFinanceController
    {
        //BUDGET Intermediate view        
        public ActionResult Budget()
        {
            string User_OID = GetUserData().Result; //Fetch User Data
            //List<Expiration> Expirations = GetAllItems<Expiration>("Expirations", User_OID).OrderBy(x => x.ExpDateTime.Month).Where(x => x.ExpDateTime.Month == DateTime.Now.Month).ToList();
            Expirations Expirations = (Expirations)GetAllItemsMain<Expirations>("Expirations", User_OID, DateTime.Now.Year.ToString());
            List<Expiration> MonthExpirations = Expirations.ExpirationList.OrderBy(x => x.ExpDateTime.Month).Where(x => x.ExpDateTime.Month == DateTime.Now.Month).ToList();
            //List<Expiration> Expirations = GetAllItems<Expiration>("Expirations", User_OID).OrderBy(x => x.ExpDateTime.Month).Where(x => x.ExpDateTime.Month == DateTime.Now.Month).ToList(); //Fetch imminent expirations
            List<KnownMovement> KnownMovements = GetAllItems<KnownMovement>("KnownMovements", User_OID).ToList();
            bool found = false;
            List<Expiration> TempExpOut = new();
            List<Expiration> TempExpIn = new();
            foreach (var km in KnownMovements)
            {
                foreach (var exp in MonthExpirations)
                {
                    if (km.KMTitle == exp.ExpTitle && km.KMValue == exp.ExpValue)
                    {
                        found = true;
                    }
                }
                if (found is false) MonthExpirations.Add(new Expiration() { ExpTitle = km.KMTitle, ExpValue = km.KMValue });
                found = false;
            }
            foreach (var item in MonthExpirations)
            { 
                if(item.ExpValue < 0)
                {
                    TempExpOut.Add(item);
                }
                if (item.ExpValue >= 0)
                {
                    if(item.ExpTitle.StartsWith("DEB") ) TempExpOut.Add(item);
                    else TempExpIn.Add(item);
                }
            }

            ViewBag.In = TempExpIn;
            ViewBag.Out = TempExpOut;

            ViewModel viewModel = new()
            {
                Banks = GetAllItems<Bank>("Banks", User_OID),
                Bank = new Bank(),
                Deposits = GetAllItems<Deposit>("Deposits", User_OID),
                Deposit = new Deposit(),
                Tickets = GetAllItems<Ticket>("Tickets", User_OID),
                Ticket = new Ticket()
            };
            viewModel.Contanti = viewModel.Banks.First();
            viewModel.Budget_Calc = new Budget_Calc();
            return View(viewModel);
        }
        public IActionResult Budget_Calc()
        {
            return View(new Budget_Calc());
        }
        [HttpPost]
        public ActionResult Budget_Calc(Budget_Calc bc)
        {





            IEnumerable<Balance> Balances = GetAllItems<Balance>("Balances", GetUserData().Result);
            double stimated_total = Balances.Last().ActBalance + bc.Corrective_Item_0 + bc.Corrective_Item_1 + bc.Corrective_Item_2 + bc.Corrective_Item_3;

            List<Expiration> Expirations = GetAllItems<Expiration>("Expirations", GetUserData().Result).OrderBy(x => x.ExpDateTime).Where(x => x.ExpDateTime <= bc.Future_Date).ToList();
            foreach (var item in Expirations)
            {
                if (item.ColorLabel == "orange") Expirations.Remove(item);
            }
            List<KnownMovement> KnownMovements = GetAllItems<KnownMovement>("KnownMovements", GetUserData().Result).ToList();
            //contare mesi tra date
            //Se >15 del mese aggiungi uno altrimenti togli
            //cicla per n volte quanti sono i mesi
            foreach (var km in KnownMovements)
            {
                Expirations.Add(new Expiration() { ExpTitle = km.KMTitle, ExpValue = km.KMValue });

            }
            foreach (var item in Expirations)
            {
                stimated_total += item.ExpValue;
            }

            //e.input_value = e.input_value.Replace(".", ",");
            //e.ExpValue = Convert.ToDouble(e.input_value);


            return RedirectToAction(nameof(Budget));

        }
    }
}
