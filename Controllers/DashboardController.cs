using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;
using Microsoft.Identity.Web;
using PersonalFinanceFrontEnd.Models;
using Microsoft.AspNetCore.Authorization;

namespace PersonalFinanceFrontEnd.Controllers
{
    public partial class PersonalFinanceController : Controller
    {

        [Authorize]
        [AuthorizeForScopes(ScopeKeySection = "DownstreamApi:Scopes")]
        public ActionResult Index(string selectedYear, string selectedMonth, string selectedYearTr, string selectedMonthTr)
        {

            string User_OID = GetUserData().Result;
          

            ViewModel viewModel = new();
            IEnumerable<Transaction> Transactions = GetAllItemsN<Transaction>("Transactions", User_OID);
            IEnumerable<Credit> Credits = GetAllItemsN<Credit>("Credits", User_OID);
            IEnumerable<Debit> Debits = GetAllItemsN<Debit>("Debits", User_OID);
            IEnumerable<Bank> Banks = GetAllItemsN<Bank>("Banks", User_OID);
            IEnumerable<Deposit> Deposits = GetAllItemsN<Deposit>("Deposits", User_OID);
            IEnumerable<Ticket> Tickets = GetAllItemsN<Ticket>("Tickets", User_OID);
            IEnumerable<Balance> Balances = GetAllItemsN<Balance>("Balances", User_OID);

            if (!Banks.Any())
            {
                Bank b = new() { Usr_OID = User_OID, BankName = "Contanti", Iban = null, ID = 0, BankValue = 0, BankNote = "Totale contanti" };
                int result = AddItemN<Bank>("Banks", b);
            }
            double TransactionSum = 0;
            foreach (var item in Transactions)
            {
                TransactionSum += item.TrsValue;
            }
            double CreditSum = 0;
            foreach (var item in Credits)
            {
                CreditSum += item.CredValue;
            }
            double DebitSum = 0;
            foreach (var item in Debits)
            {
                DebitSum += item.DebValue;
            }
            // Totale saldo + crediti - debiti
            double TotWithDebits = TransactionSum + CreditSum - DebitSum;
            // Totale saldo + crediti
            double TotNoDebits = TransactionSum + CreditSum;


            //############################################################################################################################
            //FILTRI ANNO E MESE PER GRAFICO SALDO
            //############################################################################################################################
            //Trovo gli anni "unici"
            var UniqueYear = Balances.GroupBy(x => x.BalDateTime.Year)
                                    .OrderBy(x => x.Key)
                                    .Select(x => new { Year = x.Key })
                                    .ToList();
            //Creo la lista di anni "unici" per il dropdown filter del grafico saldo
            List<SelectListItem> itemlistYear = new();
            foreach (var year in UniqueYear)
            {
                SelectListItem subitem = new() { Text = year.Year.ToString(), Value = year.Year.ToString() };
                itemlistYear.Add(subitem);
            }
            //Passo alla view la lista
            ViewBag.ItemList = itemlistYear;
            //Se al caricamento della pagina ho selezionato un anno (not empty), salvo in Balances i saldi di quell'anno
            if (!String.IsNullOrEmpty(selectedYear)) Balances = Balances.AsQueryable().Where(x => x.BalDateTime.Year.ToString() == selectedYear);
            //Trovo i mesi "unici"
            var UniqueMonth = Balances.GroupBy(x => x.BalDateTime.Month)
                        .OrderBy(x => x.Key)
                        .Select(x => new { Month = x.Key })
                        .ToList();
            //Creo la lista di mesi "unici" per il dropdown filter del grafico saldo
            List<SelectListItem> itemlistMonth = new();
            foreach (var month in UniqueMonth)
            {
                SelectListItem subitem = new() { Text = MonthConverter(month.Month), Value = MonthConverter(month.Month) };
                itemlistMonth.Add(subitem);
            }
            //Passo alla view la lista
            ViewBag.ItemListMonth = itemlistMonth;
            //Se al caricamento della pagina ho selezionato un mese (not empty), salvo in Balances i saldi di quel mese
            if (!String.IsNullOrEmpty(selectedMonth)) Balances = Balances.AsQueryable().Where(x => MonthConverter(x.BalDateTime.Month) == selectedMonth);
            //############################################################################################################################


            //############################################################################################################################
            //FILTRI ANNO E MESE PER GRAFICI TRANSAZIONI
            //############################################################################################################################
            //Trovo gli anni "unici"
            var UniqueYearTr = Transactions.GroupBy(x => x.TrsDateTime.Year)
                                    .OrderBy(x => x.Key)
                                    .Select(x => new { Year = x.Key })
                                    .ToList();
            //Creo la lista di anni "unici" per il dropdown filter del grafico saldo
            List<SelectListItem> itemlistYearTr = new();
            foreach (var year in UniqueYearTr)
            {
                SelectListItem subitem = new() { Text = year.Year.ToString(), Value = year.Year.ToString() };
                itemlistYearTr.Add(subitem);
            }
            //Passo alla view la lista
            ViewBag.ItemListTr = itemlistYearTr;
            //Se al caricamento della pagina ho selezionato un anno (not empty), salvo in Balances i saldi di quell'anno
            if (!String.IsNullOrEmpty(selectedYearTr)) Transactions = Transactions.AsQueryable().Where(x => x.TrsDateTime.Year.ToString() == selectedYearTr);
            //Trovo i mesi "unici"
            var UniqueMonthTr = Transactions.GroupBy(x => x.TrsDateTime.Month)
                        .OrderBy(x => x.Key)
                        .Select(x => new { Month = x.Key })
                        .ToList();
            //Creo la lista di mesi "unici" per il dropdown filter del grafico saldo
            List<SelectListItem> itemlistMonthTr = new();
            foreach (var month in UniqueMonthTr)
            {
                SelectListItem subitem = new() { Text = MonthConverter(month.Month), Value = MonthConverter(month.Month) };
                itemlistMonthTr.Add(subitem);
            }
            //Passo alla view la lista
            ViewBag.ItemListMonthTr = itemlistMonthTr;
            //Se al caricamento della pagina ho selezionato un mese (not empty), salvo in Balances i saldi di quel mese
            if (!String.IsNullOrEmpty(selectedMonthTr)) Transactions = Transactions.AsQueryable().Where(x => MonthConverter(x.TrsDateTime.Month) == selectedMonthTr);
            //############################################################################################################################


            var TransactionsIn = Transactions.Where(x => x.TrsValue >= 0);
            var TransactionsOut = Transactions.Where(x => x.TrsValue < 0);


            //############################################################################################################################
            //Rimuovo l'orario dal DateTime e salvo come json

            string json = JsonConvert.SerializeObject(Balances);
            //Passo alla view la lista aggiornata e convertita
            ViewBag.Balances = json;






            var UniqueCodes = Transactions.GroupBy(x => x.TrsCode)
                               .Select(x => x.First())
                               .ToList();
            List<SelectListItem> Codes = new();
            foreach (var item in UniqueCodes)
            {
                SelectListItem code = new();
                code.Value = item.TrsCode;
                code.Text = item.TrsCode;
                Codes.Add(code);
            }
            bool isPresent = false;
            foreach (var credit in Credits)
            {
                foreach (var item in Codes)
                {
                    if (credit.CredCode == item.Value) isPresent = true;
                }
                if (isPresent is true)
                {
                    SelectListItem code = new()
                    {
                        Value = credit.CredCode,
                        Text = credit.CredCode
                    };
                    Codes.Add(code);
                }
                isPresent = false;
            }
            foreach (var debit in Debits)
            {
                foreach (var item in Codes)
                {
                    if (debit.DebCode == item.Value) isPresent = true;
                }
                if (isPresent is false)
                {
                    SelectListItem code = new()
                    {
                        Value = debit.DebCode,
                        Text = debit.DebCode
                    };
                    Codes.Add(code);
                }
                isPresent = false;
            }
            TempData["Codes"] = Codes;
            viewModel.Transaction = new Transaction();

            GetDonutData(TransactionsIn, 1);
            GetDonutData(TransactionsOut, 0);

            string jsonTrans = JsonConvert.SerializeObject(Transactions);
            ViewBag.Transactions = jsonTrans;

            //Passaggio di dati alla vista con ViewModel
            viewModel.TransactionSum = TransactionSum;
            viewModel.CreditSum = CreditSum;
            viewModel.DebitSum = DebitSum;
            viewModel.TotWithDebits = TotWithDebits;
            viewModel.TotNoDebits = TotNoDebits;
            viewModel.Banks = Banks;

            return View(viewModel);
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