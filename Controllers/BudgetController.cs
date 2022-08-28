using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Azure;
using Microsoft.Graph;
using PersonalFinanceFrontEnd.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;

namespace PersonalFinanceFrontEnd.Controllers
{
    public partial class PersonalFinanceController
    {
        //BUDGET Intermediate view        
        public ActionResult Budget(double stimated_total, DateTime Future_Date)
        {
            string User_OID = GetUserData().Result; //Fetch User Data
            List <Expiration> ExpirationToShow = new ();
            List<Expiration> TempExpOut = new();
            List<Expiration> TempExpIn = new();
            List<Expiration> ExpDone = new();
            List<KnownMovement> KnownMovements = GetAllItems<KnownMovement>("KnownMovements", User_OID).ToList();
            IEnumerable<Transaction> transactions = GetAllItems<Transaction>("Transactions", User_OID);
            transactions = transactions.OrderBy(x => x.TrsDateTime).Where(x => x.TrsDateTime.Month == DateTime.Now.Month);
            IEnumerable<Balance> Balances = GetAllItems<Balance>("Balances", User_OID);
            if (stimated_total == 0)
            {
                Expirations Expirations = (Expirations)GetAllItemsMain<Expirations>("Expirations", User_OID, DateTime.Now.Year.ToString());
                List<Expiration> MonthExpirations = Expirations.ExpirationList.OrderBy(x => x.ExpDateTime.Month).Where(x => x.ExpDateTime.Month == DateTime.Now.Month).ToList();
                
                bool found = false;

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
                foreach (var item in transactions)
                {
                    if (item.TrsDateTimeExp != null)
                    {
                        MonthExpirations.Add(new Expiration() { ExpTitle = item.TrsCode, ExpValue = item.TrsValue, ColorLabel = item.ExpColorLabel });
                    }
                }
                ExpirationToShow = MonthExpirations;    
            }
            if (stimated_total != 0)
            {
                List<Expiration> Expirations = FilterExpList(User_OID, Future_Date);
                foreach (var km in KnownMovements)
                {
                    Expirations.Add(new Expiration() { ExpTitle = km.KMTitle, ExpValue = km.KMValue, ColorLabel = "orange" });
                }
                ExpirationToShow = Expirations;
                ViewBag.MonthCount = ((Future_Date - DateTime.Now).TotalDays) / 30;
                
            }
         

            foreach (var item in ExpirationToShow)
            { 
                foreach (var tr in transactions)
                {
                    if(item.ExpTitle == tr.TrsCode)
                    {
                        ExpDone.Add(item);
                    }
                }
                if(item.ExpValue < 0)
                {
                    TempExpOut.Add(item);
                }
                if (item.ExpValue >= 0)
                {
                    if (item.ExpTitle.StartsWith("DEB")) { 
                        item.ExpValue = -item.ExpValue;
                        TempExpOut.Add(item); 
                    }
                    else TempExpIn.Add(item);
                }
            }
            double TotIn = TempExpIn.Sum(x => x.ExpValue);
            double TotOut = TempExpOut.Sum(x => x.ExpValue);
            double ActBalance = Balances.Last().ActBalance;
            ViewBag.In = TempExpIn;
            ViewBag.TotIn = TotIn;
            ViewBag.Out = TempExpOut;
            ViewBag.TotOut = TotOut;
            ViewBag.ExpDone = ExpDone;
            ViewBag.ActualFlux = TotIn + TotOut;
            ViewBag.stimated_total = stimated_total;
            ViewBag.Future_Date = Future_Date;            
            ViewBag.ActualBalance = ActBalance;


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
            string User_OID = GetUserData().Result;

            double TotalInOut = 0;


            IEnumerable<Balance> Balances = GetAllItems<Balance>("Balances", User_OID);
            double stimated_total = Balances.Last().ActBalance + bc.Corrective_Item_0 + bc.Corrective_Item_1 + bc.Corrective_Item_2 + bc.Corrective_Item_3;


            List<Expiration> Expirations = FilterExpList(User_OID, bc.Future_Date);
            List<KnownMovement> KnownMovements = GetAllItems<KnownMovement>("KnownMovements", GetUserData().Result).ToList();
            //contare mesi tra date
            //((date1.Year - date2.Year) * 12) + date1.Month - date2.Month
            //    (EndDate - StartDate).TotalDays
            //Se >15 del mese aggiungi uno altrimenti togli
            //cicla per n volte quanti sono i mesi
            double totalKM = 0;
            foreach (var km in KnownMovements)
            {
                totalKM += km.KMValue;
                //Expirations.Add(new Expiration() { ExpTitle = km.KMTitle, ExpValue = km.KMValue });

            }
            double finalKMTotal = (totalKM / 30) * ((bc.Future_Date - DateTime.Now).TotalDays);
            foreach (var item in Expirations)
            {
                if (item.ExpTitle.StartsWith("DEB")) item.ExpValue = -item.ExpValue;
                TotalInOut += item.ExpValue;
            }

            //double DayStimatedInOut = TotalInOut / ((bc.Future_Date - DateTime.Now).TotalDays);

            stimated_total = stimated_total + finalKMTotal + TotalInOut;



            return RedirectToAction("Budget", new { stimated_total = stimated_total, future_date = bc.Future_Date }) ;

        }

        public List<Expiration> FilterExpList(string User_OID, DateTime Future_Date)
        {

            IEnumerable<Expiration> ExpirationList = GetAllExp<Expiration>("Expirations", User_OID);

            List<Expiration> Expirations = ExpirationList.OrderBy(x => x.ExpDateTime).Where(x => x.ExpDateTime <= Future_Date).ToList();
            List<Expiration> ExpirationsToRemove = new();

            foreach (var item in Expirations)
            {
                if (item.ColorLabel == "orange") ExpirationsToRemove.Add(item);
            }
            foreach (var item in ExpirationsToRemove)
            {
                Expirations.Remove(item);
            }
            return Expirations;
        }

        public IEnumerable<T> GetAllExp<T>(string controller, string User_OID)
        {
            IEnumerable<T> detections = null;
            string path = "api/" + controller + "/GetAllExp" + "?User_OID=" + User_OID;
            using (HttpClient client = new())
            {
                client.BaseAddress = new Uri("https://personalfinanceappapi.azurewebsites.net/");
                client.GetAsync(path).Wait();
                client.GetAsync(path).Result.Content.ReadAsAsync<List<T>>().Wait();
                detections = client.GetAsync(path).Result.Content.ReadAsAsync<List<T>>().Result;
            }
            return detections;
        }
    }
}
