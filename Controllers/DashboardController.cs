using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Microsoft.Identity.Web;
using PersonalFinanceFrontEnd.Models;
using Microsoft.AspNetCore.Authorization;
using System.Net.Http;
using AspNetCoreHero.ToastNotification.Abstractions;
using Microsoft.Extensions.Logging;
using Microsoft.Graph;

namespace PersonalFinanceFrontEnd.Controllers
{
    public partial class PersonalFinanceController : Controller
    {
        [Authorize]
        [AuthorizeForScopes(ScopeKeySection = "DownstreamApi:Scopes")]
        public ActionResult Index(string selectedYear, string selectedMonth, string selectedYearTr, string selectedMonthTr)
        {
            
            DashAPIOut DOut = new();
            string path = "api/Dashboard/Main" + "?sY=" + selectedYear + "&sM=" + selectedMonth + "&sYT=" + selectedYearTr + "&sMT=" + selectedMonthTr + "&User_OID=" + GetUserData().Result;
            using (HttpClient client = new())
            {
                client.BaseAddress = new Uri("https://personalfinanceappapi.azurewebsites.net/");
                client.GetAsync(path).Wait();
                client.GetAsync(path).Result.Content.ReadAsAsync<DashAPIOut>().Wait();
                DOut = client.GetAsync(path).Result.Content.ReadAsAsync<DashAPIOut>().Result;
            }
            GetDonutData(DOut.TransactionsIn, 1);
            GetDonutData(DOut.TransactionsOut, 0);
            TempData["Codes"] = DOut.Codes;
            TransactionDetailsEdit detection = new();
            path = "api/Transactions/DetailsEdit?User_OID=" + GetUserData().Result;
            using (HttpClient client = new())
            {
                client.BaseAddress = new Uri("https://personalfinanceappapi.azurewebsites.net/");
                client.GetAsync(path).Wait();
                client.GetAsync(path).Result.Content.ReadAsAsync<TransactionDetailsEdit>().Wait();
                detection = client.GetAsync(path).Result.Content.ReadAsAsync<TransactionDetailsEdit>().Result;
            }
            ViewBag.DebitListRat = detection.DebitsRat;
            ViewBag.DebitList = detection.DebitsMono;
            ViewBag.CreditList = detection.CreditsMono;
            ViewBag.MonthExpirations = detection.MonthExpirations;

            var Transactions = (DOut.TransactionsIn ?? Enumerable.Empty<Transaction>()).Concat(DOut.TransactionsOut ?? Enumerable.Empty<Transaction>());
            var monthExpNotDone = detection.MonthExpirations.Where(p => !Transactions.Any(p2 => p2.TrsCode == p.ExpTitle));
            ViewBag.MonthExpirations = monthExpNotDone;
            return View(DOut);
        }

        private void GetDonutData(IEnumerable<Transaction> Transactions, int type)
        {
            var UniqueCodes = Transactions.GroupBy(x => x.TrsCode)
                                            .OrderBy(x => x.Key)
                                            .Select(x => new { x.Key })
                                            .ToList();
            List<string> strCodes = new();
            foreach (var item in UniqueCodes) strCodes.Add(item.Key);
            string jsonCodes = JsonConvert.SerializeObject(strCodes);
            if (type == 0) { ViewBag.CodesOut = jsonCodes; ViewBag.CodesOutV = strCodes; }
            if (type == 1) { ViewBag.CodesIn = jsonCodes; ViewBag.CodesInV = strCodes; }

            int i = 0;
            double totalCountIn = 0;
            double totalCountOut = 0;

            double[] count = new double[strCodes.Count];
            foreach (var item in strCodes)
            {
                var CodeValues = Transactions.Where(x => x.TrsCode == item);
                if (type == 0) foreach (var value in CodeValues) { count[i] -= value.TrsValue; totalCountOut += value.TrsValue; }
                else foreach (var value in CodeValues) { count[i] += value.TrsValue; totalCountIn += value.TrsValue; }
                i++;
            }

            string jsonCodeValues = JsonConvert.SerializeObject(count);
            if (type == 0) { ViewBag.CodeValuesOut = jsonCodeValues; ViewBag.CodeValuesOutV = count; ViewBag.TotCountOut = totalCountOut; }
            if (type == 1) { ViewBag.CodeValuesIn = jsonCodeValues; ViewBag.CodeValuesInV = count; ViewBag.TotCountIn = totalCountIn; }
        }

        
  
        //private static string MonthConverter(int monthNum)
        //{
        //    string ConvertedMonth = "";
        //    switch (monthNum)
        //    {
        //        case 1:
        //            ConvertedMonth = "Gennaio";
        //            break;
        //        case 2:
        //            ConvertedMonth = "Febbraio";
        //            break;
        //        case 3:
        //            ConvertedMonth = "Marzo";
        //            break;
        //        case 4:
        //            ConvertedMonth = "Aprile";
        //            break;
        //        case 5:
        //            ConvertedMonth = "Maggio";
        //            break;
        //        case 6:
        //            ConvertedMonth = "Giugno";
        //            break;
        //        case 7:
        //            ConvertedMonth = "Luglio";
        //            break;
        //        case 8:
        //            ConvertedMonth = "Agosto";
        //            break;
        //        case 9:
        //            ConvertedMonth = "Settembre";
        //            break;
        //        case 10:
        //            ConvertedMonth = "Ottobre";
        //            break;
        //        case 11:
        //            ConvertedMonth = "Novembre";
        //            break;
        //        case 12:
        //            ConvertedMonth = "Dicembre";
        //            break;
        //    }
        //    return ConvertedMonth;
        //}
    }
}