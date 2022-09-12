using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Azure;
using Microsoft.Graph;
using Newtonsoft.Json;
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
            IEnumerable<Transaction> AllTransactions = GetAllItems<Transaction>("Transactions", User_OID);
            IEnumerable<Transaction> transactions = AllTransactions.OrderBy(x => x.TrsDateTime).Where(x => x.TrsDateTime.Month == DateTime.Now.Month);
            AllTransactions = AllTransactions.Where(x => x.TrsDateTime >= DateTime.Now.AddMonths(-5));
            IEnumerable<Balance> Balances = GetAllItems<Balance>("Balances", User_OID);
            double MonthFlux = 0;

            if(Future_Date <= DateTime.UtcNow.Date && Future_Date != new DateTime())
            {
                _notyf.Error("Selezionare una data successiva a quella attuale");
                return RedirectToAction(nameof(Budget));
            }
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
                    if (found is false) MonthExpirations.Add(new Expiration() { ExpTitle = km.KMTitle, ExpValue = km.KMValue, ColorLabel = "orange" });
                    found = false;
                }
                foreach (var item in transactions)
                {
                    if (item.TrsTitle.StartsWith("CRE ") || item.TrsTitle.StartsWith("DEB ") || item.TrsTitle.StartsWith("MVF ") || item.TrsTitle.StartsWith("SCD "))
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

            foreach (var trs in transactions) MonthFlux += trs.TrsValue;
            foreach (var item in ExpirationToShow)
            { 
                foreach (var tr in transactions)
                {                    
                    if (item.ExpTitle == tr.TrsCode)
                    {
                        ExpDone.Add(item);
                    }
                }
                if (item.ExpTitle.StartsWith("MVF") && stimated_total != 0)
                {
                    item.ExpValue = item.ExpValue * ViewBag.MonthCount;
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
            double ExpDonePaid = ExpDone.Sum(x => x.ExpValue);
            ViewBag.In = TempExpIn;
            ViewBag.TotIn = TotIn;
            ViewBag.Out = TempExpOut;
            ViewBag.TotOut = TotOut;
            ViewBag.ExpDone = ExpDone;
            ViewBag.ExpDoneRemain = TotIn + TotOut - ExpDonePaid;
            ViewBag.ActualFlux = TotIn + TotOut;
            ViewBag.stimated_total = stimated_total;
            ViewBag.Future_Date = Future_Date;            
            ViewBag.ActualBalance = ActBalance;
            ViewBag.MonthFlux = MonthFlux;
            ViewBag.DiffExEff = TotIn + TotOut - MonthFlux; //Differenza tra flusso previsto e effettivo
            ViewBag.PercDiff = ( 100 * MonthFlux) / (TotIn + TotOut); //Scarto percentuale tra flusso previsto ed effettivo

            List<string> BarMonths = new();
            List<double> BarValues = new();
            for (int i=0; i<=4; i++)
            {
                int actMonth = (DateTime.UtcNow.Month - i);
                double tot = 0;
                foreach(var trs in AllTransactions)
                {

                    if(trs.TrsDateTime.Month == actMonth)
                    {
                        tot += trs.TrsValue;
                    }
                }
                BarMonths.Add(MonthConverter(actMonth));
                BarValues.Add(tot);

            }
            string jsonBarMonths = JsonConvert.SerializeObject(Enumerable.Reverse(BarMonths));
            string jsonBarValues = JsonConvert.SerializeObject(Enumerable.Reverse(BarValues));
            ViewBag.BarMonths = jsonBarMonths;
            ViewBag.BarValues = jsonBarValues;
            ViewModel viewModel = new();
            //{
            //    Banks = GetAllItems<Bank>("Banks", User_OID),
            //    Bank = new Bank(),
            //    Deposits = GetAllItems<Deposit>("Deposits", User_OID),
            //    Deposit = new Deposit(),
            //    Tickets = GetAllItems<Ticket>("Tickets", User_OID),
            //    Ticket = new Ticket()
            //};
            //viewModel.Contanti = viewModel.Banks.First();
            //viewModel.Budget_Calc = new Budget_Calc();
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

        private static string MonthConverter(int monthNum)
        {
            string ConvertedMonth = "";
            switch (monthNum)
            {
                case 1:
                    ConvertedMonth = "Gennaio";
                    break;
                case 2:
                    ConvertedMonth = "Febbraio";
                    break;
                case 3:
                    ConvertedMonth = "Marzo";
                    break;
                case 4:
                    ConvertedMonth = "Aprile";
                    break;
                case 5:
                    ConvertedMonth = "Maggio";
                    break;
                case 6:
                    ConvertedMonth = "Giugno";
                    break;
                case 7:
                    ConvertedMonth = "Luglio";
                    break;
                case 8:
                    ConvertedMonth = "Agosto";
                    break;
                case 9:
                    ConvertedMonth = "Settembre";
                    break;
                case 10:
                    ConvertedMonth = "Ottobre";
                    break;
                case 11:
                    ConvertedMonth = "Novembre";
                    break;
                case 12:
                    ConvertedMonth = "Dicembre";
                    break;
            }
            return ConvertedMonth;
        }
    }
}
