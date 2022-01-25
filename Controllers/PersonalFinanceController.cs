﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Net.Http;
using System.Reflection;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;
using PersonalFinanceFrontEnd.Models;



namespace PersonalFinanceFrontEnd.Controllers
{
    public class PersonalFinanceController : Controller
    {
        [Authorize]
        public ActionResult Index(string selectedYear, string selectedMonth, string selectedYearTr, string selectedMonthTr)
        {
            ClaimsPrincipal currentUser = this.User;
            string User_OID = currentUser.FindFirst(ClaimTypes.NameIdentifier).Value;
            ViewModel viewModel = new ViewModel();
            IEnumerable<Transaction> Transactions = GetAllItems<Transaction>(nameof(Transactions), User_OID);
            IEnumerable<Credit> Credits = GetAllItems<Credit>(nameof(Credits), User_OID);
            IEnumerable<Debit> Debits = GetAllItems<Debit>(nameof(Debits), User_OID);
            IEnumerable<Bank> Banks = GetAllItems<Bank>(nameof(Banks), User_OID);
            IEnumerable<Deposit> Deposits = GetAllItems<Deposit>(nameof(Deposits), User_OID);
            IEnumerable<Ticket> Tickets = GetAllItems<Ticket>(nameof(Tickets), User_OID);
            IEnumerable<Balance> Balances = GetAllItems<Balance>(nameof(Balances), User_OID);

            if (Banks.Count() == 0)
            {
                Bank b = new Bank { Usr_OID = User_OID, BankName = "Contanti", Iban = null, ID = 0, BankValue = 0, BankNote = "Totale contanti" };
                int result = AddItem<Bank>(nameof(Bank), b);
            }
            int TransactionSum = 0;
            foreach (var item in Transactions)
            {
                TransactionSum += item.TrsValue;
            }
            int CreditSum = 0;
            foreach (var item in Credits)
            {
                CreditSum += item.CredValue;
            }
            int DebitSum = 0;
            foreach (var item in Debits)
            {
                DebitSum += item.DebValue;
            }
            // Totale saldo + crediti - debiti
            int TotWithDebits = TransactionSum + CreditSum - DebitSum;
            // Totale saldo + crediti
            int TotNoDebits = TransactionSum + CreditSum;


            //############################################################################################################################
            //FILTRI ANNO E MESE PER GRAFICO SALDO
            //############################################################################################################################
            //Trovo gli anni "unici"
            var UniqueYear = Balances.GroupBy(x => x.BalDateTime.Year)
                                    .OrderBy(x => x.Key)
                                    .Select(x => new { Year = x.Key })
                                    .ToList();
            //Creo la lista di anni "unici" per il dropdown filter del grafico saldo
            List<SelectListItem> itemlistYear = new List<SelectListItem>();
            foreach (var year in UniqueYear)
            {
                SelectListItem subitem = new SelectListItem() { Text = year.Year.ToString(), Value = year.Year.ToString() };
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
            List<SelectListItem> itemlistMonth = new List<SelectListItem>();
            foreach (var month in UniqueMonth)
            {
                SelectListItem subitem = new SelectListItem() { Text = MonthConverter(month.Month), Value = MonthConverter(month.Month) };
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
            List<SelectListItem> itemlistYearTr = new List<SelectListItem>();
            foreach (var year in UniqueYearTr)
            {
                SelectListItem subitem = new SelectListItem() { Text = year.Year.ToString(), Value = year.Year.ToString() };
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
            List<SelectListItem> itemlistMonthTr = new List<SelectListItem>();
            foreach (var month in UniqueMonthTr)
            {
                SelectListItem subitem = new SelectListItem() { Text = MonthConverter(month.Month), Value = MonthConverter(month.Month) };
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
            List<SelectListItem> Codes = new List<SelectListItem>();
            foreach (var item in UniqueCodes)
            {
                SelectListItem code = new SelectListItem();
                code.Value = item.TrsCode;
                code.Text = item.TrsCode;
                Codes.Add(code);
            }
            TempData["Codes"] = Codes;
            viewModel.TransactionExt = new TransactionExt();

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
            List<string> strCodes = new List<string>();
            foreach (var item in UniqueCodes) strCodes.Add(item.Key);
            string jsonCodes = JsonConvert.SerializeObject(strCodes);
            if (type == 0) { ViewBag.CodesOut = jsonCodes; ViewBag.CodesOutV = strCodes; }
            if (type == 1) { ViewBag.CodesIn = jsonCodes; ViewBag.CodesInV = strCodes; }

            int i = 0;
            int totalCountIn = 0;
            int totalCountOut = 0;

            int[] count = new int[strCodes.Count];
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

        private string MonthConverter(int monthNum)
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

        private IEnumerable<T> GetAllItems<T>(string type, string User_OID)
        {
            IEnumerable<T> detections = null;
            string path = "GetAll" + type + "?User_OID=" + User_OID;
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri("https://personalfinanceappapi.azurewebsites.net/api/PersonalFinanceAPI/");
                var responseTask = client.GetAsync(path);
                responseTask.Wait();
                var result = responseTask.Result;
                var readTask = result.Content.ReadAsAsync<List<T>>();
                readTask.Wait();
                detections = readTask.Result;
            }
            return (detections);
        }
        private T GetItemID<T>(string type, int id) where T : new()
        {
            T detection = new T();
            string path = "Get" + type + "Id?id=" + id;
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri("https://personalfinanceappapi.azurewebsites.net/api/PersonalFinanceAPI/");
                var responseTask = client.GetAsync(path);
                responseTask.Wait();
                var result = responseTask.Result;
                var readTask = result.Content.ReadAsAsync<T>();
                readTask.Wait();
                detection = readTask.Result;
            }
            return (detection);
        }
        private int EditItemID<T>(string type, T obj) where T : new()
        {
            string path = "Update" + type;
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri("https://personalfinanceappapi.azurewebsites.net/api/PersonalFinanceAPI/");
                var postTask = client.PutAsJsonAsync<T>(path, obj);
                postTask.Wait();
                var result = postTask.Result;
                if (result.IsSuccessStatusCode)
                {
                    return (0);
                }
            }
            return (1);
        }
        private int AddItem<T>(string type, T obj) where T : new()
        {
            string path = "Add" + type;
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri("https://personalfinanceappapi.azurewebsites.net/api/PersonalFinanceAPI/");
                var postTask = client.PostAsJsonAsync<T>(path, obj);
                postTask.Wait();
                var result = postTask.Result;
                if (result.IsSuccessStatusCode)
                {
                    return (0);
                }
            }
            return (1);
        }
        private int DeleteItem(string type, int id)
        {
            string path = "Delete" + type + "?id=" + id;
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri("https://personalfinanceappapi.azurewebsites.net/api/PersonalFinanceAPI/");
                var postTask = client.DeleteAsync(path);
                postTask.Wait();
                var result = postTask.Result;
                if (result.IsSuccessStatusCode)
                {
                    return (0);
                }
            }
            return (1);
        }


        //GET ALL Methods
        public IEnumerable<Bank> GetBanks(string User_OID)
        {
            IEnumerable<Bank> Banks = GetAllItems<Bank>(nameof(Banks), User_OID);
            return Banks;
        }
        public IEnumerable<Deposit> GetDeposits(string User_OID)
        {
            IEnumerable<Deposit> Deposits = GetAllItems<Deposit>(nameof(Deposits), User_OID);
            return Deposits;
        }
        public IEnumerable<Ticket> GetTickets(string User_OID)
        {
            IEnumerable<Ticket> Tickets = GetAllItems<Ticket>(nameof(Tickets), User_OID);
            return Tickets;
        }
        public IEnumerable<Credit> GetCredits(string User_OID)
        {
            IEnumerable<Credit> Credits = GetAllItems<Credit>(nameof(Credits), User_OID);
            return Credits;
        }
        public IEnumerable<Debit> GetDebits(string User_OID)
        {
            IEnumerable<Debit> Debits = GetAllItems<Debit>(nameof(Debits), User_OID);
            return Debits;
        }

        //DETAILS: Controller methods for detail action - GET-BY-ID
        public ActionResult Credit_Details(int id)
        {
            Credit Credit = GetItemID<Credit>(nameof(Credit), id);
            return PartialView(Credit);
        }
        public ActionResult Debit_Details(int id)
        {
            Debit Debit = GetItemID<Debit>(nameof(Debit), id);
            return PartialView(Debit);
        }
        public ActionResult KnownMovement_Details(int id)
        {
            KnownMovement KnownMovement = GetItemID<KnownMovement>(nameof(KnownMovement), id);
            return PartialView(KnownMovement);
        }
        public ActionResult Bank_Details(int id)
        {
            Bank Bank = GetItemID<Bank>(nameof(Bank), id);
            return PartialView(Bank);
        }
        public ActionResult Deposit_Details(int id)
        {
            Deposit Deposit = GetItemID<Deposit>(nameof(Deposit), id);
            return PartialView(Deposit);
        }
        public ActionResult Ticket_Details(int id)
        {
            Ticket Ticket = GetItemID<Ticket>(nameof(Ticket), id);
            return PartialView(Ticket);
        }
        public ActionResult Transaction_Details(int id)
        {
            Transaction Transaction = GetItemID<Transaction>(nameof(Transaction), id);
            return PartialView(Transaction);
        }
        public ActionResult Transaction_Details_Edit(int id, string User_OID)
        {
            IEnumerable<Transaction> Transactions = GetAllItems<Transaction>(nameof(Transactions), User_OID);
            Transaction t = GetItemID<Transaction>(nameof(Transaction), id);
            var UniqueCodes = Transactions.GroupBy(x => x.TrsCode)
                                          .Select(x => x.First())
                                          .ToList();
            List<SelectListItem> Codes = new List<SelectListItem>();
            foreach (var item in UniqueCodes)
            {
                SelectListItem code = new SelectListItem();
                code.Value = item.TrsCode;
                code.Text = item.TrsCode;
                Codes.Add(code);
            }
            TempData["Codes"] = Codes;
            TransactionExt tr = new TransactionExt() { ID = t.ID, TrsCode = t.TrsCode, TrsTitle = t.TrsTitle, TrsDateTime = t.TrsDateTime, TrsValue = t.TrsValue, TrsNote = t.TrsNote };
            if (tr.TrsValue < 0) tr.Type = false; else tr.Type = true;
       //     if (t.Type == false) t.TrsValue = -Math.Abs(t.TrsValue);
         //   if (t.Type == true) t.TrsValue = Math.Abs(t.TrsValue);
            return PartialView(tr);
        }

        //DELETE: Controller methods for Delete-single-entry action - They send 1 if succeded to let green confirmation popup appear (TempData["sendFlag.."])
        public ActionResult Credit_Delete(int id)
        {
            return Credit_Details(id);
        }
        [HttpPost]
        public ActionResult Credit_Delete(Credit c)
        {
            int result = DeleteItem(nameof(Credit), c.ID);
            if (result == 0)
            {
                TempData["sendFlagCred"] = 1;
                return RedirectToAction(nameof(Credits));
            }
            return View();
        }
        public ActionResult Debit_Delete(int id)
        {
            return Debit_Details(id);
        }
        [HttpPost]
        public ActionResult Debit_Delete(Debit d)
        {
            int result = DeleteItem(nameof(Debit), d.ID);
            if (result == 0)
            {
                TempData["sendFlagDeb"] = 1;
                return RedirectToAction(nameof(Debits));
            }
            return View();
        }
        public ActionResult Transaction_Delete(int id)
        {
            return Transaction_Details(id);
        }
        [HttpPost]
        public ActionResult Transaction_Delete(Transaction t)
        {
            int result = DeleteItem(nameof(Transaction), t.ID);
            if (result == 0)
            {
                TempData["sendFlagTr"] = 1;
                ClaimsPrincipal currentUser = this.User;
                Transaction_Balance_Update(currentUser.FindFirst(ClaimTypes.NameIdentifier).Value);
                return RedirectToAction(nameof(Transactions));
            }
            return View();
        }
        public ActionResult KnownMovement_Delete(int id)
        {
            return KnownMovement_Details(id);
        }
        [HttpPost]
        public ActionResult KnownMovement_Delete(KnownMovement k)
        {
            int result = DeleteItem(nameof(KnownMovement), k.ID);
            if (result == 0)
            {
                TempData["sendFlagKM"] = 1;
                return RedirectToAction(nameof(KnownMovements));
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
            int result = DeleteItem(nameof(Bank), b.ID);
            if (result == 0)
            {
                TempData["sendFlagB"] = 1;
                ClaimsPrincipal currentUser = this.User;
                Balance_Update(currentUser.FindFirst(ClaimTypes.NameIdentifier).Value);
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
            int result = DeleteItem(nameof(Deposit), d.ID);
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
            int result = DeleteItem(nameof(Ticket), t.ID);
            if (result == 0)
            {
                TempData["sendFlagT"] = 1;
                ClaimsPrincipal currentUser = this.User;                
                Balance_Update(currentUser.FindFirst(ClaimTypes.NameIdentifier).Value);
                return RedirectToAction(nameof(Wallet));
            }
            return View();
        }

        //EDIT: Controller methods for Updating/Editing-single-entry action - They send 2 if succeded to let green confirmation popup appear (TempData["sendFlag.."])
        public ActionResult Credit_Edit(int id)
        {
            return Credit_Details(id);
        }
        [HttpPost]
        public ActionResult Credit_Edit(Credit c)
        {
            ClaimsPrincipal currentUser = this.User;
            c.Usr_OID = currentUser.FindFirst(ClaimTypes.NameIdentifier).Value;
            int result = EditItemID<Credit>(nameof(Credit), c);
            if (result == 0)
            {
                TempData["sendFlagCred"] = 2;
                return RedirectToAction(nameof(Credits));
            }
            return View();
        }
        public ActionResult Debit_Edit(int id)
        {
            return Debit_Details(id);
        }
        [HttpPost]
        public ActionResult Debit_Edit(Debit d)
        {
            ClaimsPrincipal currentUser = this.User;
            d.Usr_OID = currentUser.FindFirst(ClaimTypes.NameIdentifier).Value;
            int result = EditItemID<Debit>(nameof(Debit), d);
            if (result == 0)
            {
                TempData["sendFlagDeb"] = 2;
                return RedirectToAction(nameof(Debits));
            }
            return View();
        }
        public ActionResult Transaction_Edit(int id)
        {
            ClaimsPrincipal currentUser = this.User;
            string User_OID = currentUser.FindFirst(ClaimTypes.NameIdentifier).Value;
            return Transaction_Details_Edit(id, User_OID);
        }
        [HttpPost]
        public ActionResult Transaction_Edit(TransactionExt t)
        {
            if (t.Type == false) t.TrsValue = -Math.Abs(t.TrsValue);
            if (t.Type == true) t.TrsValue = Math.Abs(t.TrsValue);
            if (t.NewTrsCode != null) t.TrsCode = t.NewTrsCode;
            Transaction tr = new Transaction() { ID = t.ID, TrsCode = t.TrsCode, TrsTitle = t.TrsTitle, TrsDateTime = t.TrsDateTime, TrsValue = t.TrsValue, TrsNote = t.TrsNote };
            ClaimsPrincipal currentUser = this.User;
            tr.Usr_OID = currentUser.FindFirst(ClaimTypes.NameIdentifier).Value;
            int result = EditItemID<Transaction>(nameof(Transaction), tr);
            if (result == 0)
            {
                TempData["sendFlagTr"] = 2;
                Transaction_Balance_Update(tr.Usr_OID);
                return RedirectToAction(nameof(Transactions));
            }
            return View();
        }
        public ActionResult KnownMovement_Edit(int id)
        {
            return KnownMovement_Details(id);
        }
        [HttpPost]
        public ActionResult KnownMovement_Edit(KnownMovement k)
        {
            ClaimsPrincipal currentUser = this.User;
            k.Usr_OID = currentUser.FindFirst(ClaimTypes.NameIdentifier).Value;
            if (k.KMValue < 0) k.KMType = "Uscita"; else if (k.KMValue >= 0) k.KMType = "Entrata";
            int result = EditItemID<KnownMovement>(nameof(KnownMovement), k);
            if (result == 0)
            {
                TempData["sendFlagKM"] = 2;
                return RedirectToAction(nameof(KnownMovements));
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
            ClaimsPrincipal currentUser = this.User;
            b.Usr_OID = currentUser.FindFirst(ClaimTypes.NameIdentifier).Value;
            int result = EditItemID<Bank>(nameof(Bank), b);
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
            ClaimsPrincipal currentUser = this.User;
            d.Usr_OID = currentUser.FindFirst(ClaimTypes.NameIdentifier).Value;
            int result = EditItemID<Deposit>(nameof(Deposit), d);
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
            ClaimsPrincipal currentUser = this.User;
            t.Usr_OID = currentUser.FindFirst(ClaimTypes.NameIdentifier).Value;
            int result = EditItemID<Ticket>(nameof(Ticket), t);
            if (result == 0)
            {
                TempData["sendFlagT"] = 2;
                Balance_Update(t.Usr_OID);
                return RedirectToAction(nameof(Wallet));
            }
            return View();
        }

        //VIEWS
        //CREDITS DEBITS Intermediate view
        public ActionResult Credits_Debits()
        {
            ClaimsPrincipal currentUser = this.User;
            string User_OID = currentUser.FindFirst(ClaimTypes.NameIdentifier).Value;
            ViewModel viewModel = new ViewModel();
            viewModel.Credits = GetCredits(User_OID);
            viewModel.Credit = new Credit();
            viewModel.Debits = GetDebits(User_OID);
            viewModel.Debit = new Debit();
            return View(viewModel);
        }
        //CREDITS Intermediate view
        public ActionResult Credits()
        {
            ClaimsPrincipal currentUser = this.User;
            string User_OID = currentUser.FindFirst(ClaimTypes.NameIdentifier).Value;
            ViewModel viewModel = new ViewModel();
            viewModel.Credits = GetCredits(User_OID);
            int sendFlag = (int)(TempData.ContainsKey("sendFlagCred") ? TempData["sendFlagCred"] : 0);
            viewModel.state = sendFlag;
            return View(viewModel);
        }
        //DEBITS Intermediate view
        public ActionResult Debits()
        {
            ClaimsPrincipal currentUser = this.User;
            string User_OID = currentUser.FindFirst(ClaimTypes.NameIdentifier).Value;
            ViewModel viewModel = new ViewModel();
            viewModel.Debits = GetDebits(User_OID);
            int sendFlag = (int)(TempData.ContainsKey("sendFlagDeb") ? TempData["sendFlagDeb"] : 0);
            viewModel.state = sendFlag;
            return View(viewModel);
        }
        //WALLET Intermediate view        
        public ActionResult Wallet()
        {
            ClaimsPrincipal currentUser = this.User;
            string User_OID = currentUser.FindFirst(ClaimTypes.NameIdentifier).Value;
            ViewModel viewModel = new ViewModel();
            viewModel.Banks = GetBanks(User_OID);
            viewModel.Bank = new Bank();
            viewModel.Deposits = GetDeposits(User_OID);
            viewModel.Deposit = new Deposit();
            viewModel.Tickets = GetTickets(User_OID);
            viewModel.Ticket = new Ticket();
            viewModel.Contanti = viewModel.Banks.First();
            return View(viewModel);
        }
        //KNOWN MOVEMENTS Intermediate view  
        public ActionResult KnownMovements()
        {
            ClaimsPrincipal currentUser = this.User;
            string User_OID = currentUser.FindFirst(ClaimTypes.NameIdentifier).Value;
            IEnumerable<KnownMovement> KnownMovements = GetAllItems<KnownMovement>(nameof(KnownMovements), User_OID);
            ViewModel viewModel = new ViewModel();
            viewModel.KnownMovements = KnownMovements;
            viewModel.KnownMovement = new KnownMovement();
            int sendFlag = (int)(TempData.ContainsKey("sendFlagKM") ? TempData["sendFlagKM"] : 0);
            viewModel.state = sendFlag;
            return View(viewModel);
        }
        //TRANSACTIOONS Intermediate view
        public ActionResult Transactions(string selectedType, string selectedCode, string selectedYear, string selectedMonth, int page = 0)
        {
            ClaimsPrincipal currentUser = this.User;
            string User_OID = currentUser.FindFirst(ClaimTypes.NameIdentifier).Value;
            IEnumerable<Transaction> Transactions = GetAllItems<Transaction>(nameof(Transactions), User_OID);
            ViewModel viewModel = new ViewModel();

            //############################################################################################################################
            //FILTRI ANNO E MESE PER GRAFICO SALDO
            //############################################################################################################################
            //Trovo gli anni "unici"
            var UniqueYear = Transactions.GroupBy(x => x.TrsDateTime.Year)
                                    .OrderBy(x => x.Key)
                                    .Select(x => new { Year = x.Key })
                                    .ToList();
            //Creo la lista di anni "unici" per il dropdown filter del grafico saldo
            List<SelectListItem> itemlistYear = new List<SelectListItem>();
            foreach (var year in UniqueYear)
            {
                SelectListItem subitem = new SelectListItem() { Text = year.Year.ToString(), Value = year.Year.ToString() };
                itemlistYear.Add(subitem);
            }
            //Passo alla view la lista
            ViewBag.ItemList = itemlistYear;
            //Se al caricamento della pagina ho selezionato un anno (not empty), salvo in Balances i saldi di quell'anno
            if (!String.IsNullOrEmpty(selectedYear)) Transactions = Transactions.AsQueryable().Where(x => x.TrsDateTime.Year.ToString() == selectedYear);
            //Trovo i mesi "unici"
            var UniqueMonth = Transactions.GroupBy(x => x.TrsDateTime.Month)
                        .OrderBy(x => x.Key)
                        .Select(x => new { Month = x.Key })
                        .ToList();
            //Creo la lista di mesi "unici" per il dropdown filter del grafico saldo
            List<SelectListItem> itemlistMonth = new List<SelectListItem>();
            foreach (var month in UniqueMonth)
            {
               // SelectListItem subitem = new SelectListItem() { Text = month.Month.ToString(), Value = month.Month.ToString() };
                SelectListItem subitem = new SelectListItem() { Text = MonthConverter(month.Month), Value = MonthConverter(month.Month) };
                itemlistMonth.Add(subitem);
            }
            //Passo alla view la lista
            ViewBag.ItemListMonth = itemlistMonth;
            //Se al caricamento della pagina ho selezionato un mese (not empty), salvo in Balances i saldi di quel mese
            if (!String.IsNullOrEmpty(selectedMonth)) Transactions = Transactions.AsQueryable().Where(x => MonthConverter(x.TrsDateTime.Month) == selectedMonth);
            //if (!String.IsNullOrEmpty(selectedMonth)) Balances = Balances.AsQueryable().Where(x => MonthConverter(x.BalDateTime.Month) == selectedMonth);
            //############################################################################################################################
            List<SelectListItem> types = new List<SelectListItem>();
            SelectListItem entrate = new SelectListItem() { Text = "Entrate", Value = "Entrate"};
            types.Add(entrate);
            SelectListItem uscite = new SelectListItem() { Text = "Uscite", Value = "Uscite" };
            types.Add(uscite);
            ViewBag.Type = types;
            
            if (!String.IsNullOrEmpty(selectedCode)) Transactions = Transactions.AsQueryable().Where(x => x.TrsCode == selectedCode);
            if (!String.IsNullOrEmpty(selectedType)) { 
                if(selectedType=="Entrate") Transactions = Transactions.AsQueryable().Where(x => x.TrsValue >= 0);
                else if (selectedType == "Uscite") Transactions = Transactions.AsQueryable().Where(x => x.TrsValue < 0);
            } 

            //Pagination
            Transactions = Transactions.Reverse();
            const int PageSize = 15;
            var count = Transactions.Count();
            var data = Transactions.Skip(page * PageSize).Take(PageSize).ToList();
            this.ViewBag.MaxPage = (count / PageSize) - (count % PageSize == 0 ? 1 : 0);
            this.ViewBag.Page = page;


            viewModel.Transactions = data;
            int sendFlag = (int)(TempData.ContainsKey("sendFlagTr") ? TempData["sendFlagTr"] : 0);
            viewModel.state = sendFlag;

            var UniqueCodes = Transactions.GroupBy(x => x.TrsCode)
                              .Select(x => x.First())
                              .ToList();
            List<SelectListItem> Codes = new List<SelectListItem>();
            foreach (var item in UniqueCodes)
            {
                SelectListItem code = new SelectListItem();
                code.Value = item.TrsCode;
                code.Text = item.TrsCode;
                Codes.Add(code);
            }
            TempData["Codes"] = Codes;
            viewModel.TransactionExt = new TransactionExt();
            return View(viewModel);
        }


        //ADD NEW Methods
        public IActionResult Credit_Add()
        {
            Credit model = new Credit();
            return View(model);
        }
        [HttpPost]
        public ActionResult Credit_Add(Credit c)
        {
            ClaimsPrincipal currentUser = this.User;
            c.Usr_OID = currentUser.FindFirst(ClaimTypes.NameIdentifier).Value;
            int result = AddItem<Credit>(nameof(Credit), c);
            if (result == 0)
            {
                TempData["sendFlagCred"] = 3;
                return RedirectToAction(nameof(Credits));
            }
            return View();
        }
        public IActionResult Debit_Add()
        {
            Debit model = new Debit();
            return View(model);
        }
        [HttpPost]
        public ActionResult Debit_Add(Debit d)
        {
            ClaimsPrincipal currentUser = this.User;
            d.Usr_OID = currentUser.FindFirst(ClaimTypes.NameIdentifier).Value;
            int result = AddItem<Debit>(nameof(Debit), d);
            if (result == 0)
            {
                TempData["sendFlagDeb"] = 3;
                return RedirectToAction(nameof(Debits));
            }
            return View();
        }
        public IActionResult Transaction_Add()
        {
            TransactionExt model = new TransactionExt();
            return View(model);
        }
        [HttpPost]
        public ActionResult Transaction_Add(TransactionExt t)
        {
            if (t.Type == false) t.TrsValue = -t.TrsValue;
            if (t.NewTrsCode != null) t.TrsCode = t.NewTrsCode;
            Transaction tr = new Transaction() { ID = t.ID, TrsCode = t.TrsCode, TrsTitle = t.TrsTitle, TrsDateTime = t.TrsDateTime, TrsValue = t.TrsValue, TrsNote = t.TrsNote };
            ClaimsPrincipal currentUser = this.User;
            tr.Usr_OID = currentUser.FindFirst(ClaimTypes.NameIdentifier).Value;
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri("https://personalfinanceappapi.azurewebsites.net/api/PersonalFinanceAPI/");
                var postTask = client.PostAsJsonAsync<Transaction>("AddTransaction", tr);
                postTask.Wait();
                var result = postTask.Result;
                if (result.IsSuccessStatusCode)
                {
                    TempData["sendFlagTr"] = 3;
                    Transaction_Balance_Update(tr.Usr_OID);
                    return RedirectToAction(nameof(Transactions));
                }
            }
            return View();
        }
        public IActionResult KnownMovement_Add()
        {
            KnownMovement model = new KnownMovement();
            return View(model);
        }
        [HttpPost]
        public ActionResult KnownMovement_Add(KnownMovement k)
        {
            ClaimsPrincipal currentUser = this.User;
            k.Usr_OID = currentUser.FindFirst(ClaimTypes.NameIdentifier).Value;
            if (k.KMValue < 0) k.KMType = "Uscita"; else if (k.KMValue >= 0) k.KMType = "Entrata";
            int result = AddItem<KnownMovement>(nameof(KnownMovement), k);
            if (result == 0)
            {
                TempData["sendFlagKM"] = 3;
                return RedirectToAction(nameof(KnownMovements));
            }
            return View();
        }
        public IActionResult Bank_Add()
        {
            Bank model = new Bank();
            return View(model);
        }
        [HttpPost]
        public ActionResult Bank_Add(Bank b)
        {
            ClaimsPrincipal currentUser = this.User;
            b.Usr_OID = currentUser.FindFirst(ClaimTypes.NameIdentifier).Value;
            int result = AddItem<Bank>(nameof(Bank), b);
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
            Deposit model = new Deposit();
            return View(model);
        }
        [HttpPost]
        public ActionResult Deposit_Add(Deposit d)
        {
            ClaimsPrincipal currentUser = this.User;
            d.Usr_OID = currentUser.FindFirst(ClaimTypes.NameIdentifier).Value;
            int result = AddItem<Deposit>(nameof(Deposit), d);
            if (result == 0)
            {
                TempData["sendFlagD"] = 3;
                return RedirectToAction(nameof(Wallet));
            }
            return View();
        }
        public IActionResult Ticket_Add()
        {
            Ticket model = new Ticket();
            return View(model);
        }
        [HttpPost]
        public ActionResult Ticket_Add(Ticket t)
        {
            ClaimsPrincipal currentUser = this.User;
            t.Usr_OID = currentUser.FindFirst(ClaimTypes.NameIdentifier).Value;
            int result = AddItem<Ticket>(nameof(Ticket), t);
            if (result == 0)
            {
                Balance_Update(t.Usr_OID);
                TempData["sendFlagT"] = 3;
                Balance_Update(t.Usr_OID);
                return RedirectToAction(nameof(Wallet));
            }
            return View();
        }



        //FAST UPDATE LOGIC
        //In caso di aggiunta di nuova banca/ticket, occorre solo aggiungere l'immagine sotto images con nome in minuscolo e "-" al posto degli spazi (in .jpeg)
        public ActionResult Fast_Update()
        {
            ClaimsPrincipal currentUser = this.User;
            string User_OID = currentUser.FindFirst(ClaimTypes.NameIdentifier).Value;
            ViewModel model = new ViewModel();
            model.Banks = GetAllItems<Bank>("Banks", User_OID);
            model.Tickets = GetAllItems<Ticket>("Tickets", User_OID);
            List<Bank> BankList = new List<Bank>();
            List<Ticket> TicketList = new List<Ticket>();
            foreach (var item in model.Banks)
            {
                BankList.Add(item);
            }
            model.BankList = BankList;
            foreach (var item in model.Tickets)
            {
                TicketList.Add(item);
            }
            model.TicketList = TicketList;
            return PartialView(model);
        }
        [HttpPost]
        public ActionResult Fast_Update(ViewModel model)
        {
            ClaimsPrincipal currentUser = this.User;
            string User_OID = currentUser.FindFirst(ClaimTypes.NameIdentifier).Value;
            List<Bank> BankList = model.BankList;
            IEnumerable<Bank> Banks = GetAllItems<Bank>(nameof(Banks), User_OID);
            List<Ticket> TicketList = model.TicketList;
            IEnumerable<Ticket> Tickets = GetAllItems<Ticket>(nameof(Tickets), User_OID);
            foreach (var item in BankList)
            {
                foreach (var bank in Banks)
                {
                    if (item.ID == bank.ID)
                    {
                        bank.BankValue = item.BankValue;
                        int result = EditItemID<Bank>(nameof(Bank), bank);
                    }
                }
            }
            Balance_Update(User_OID);

            return RedirectToAction(nameof(Index));
        }
        public int Balance_Update(string User_OID)
        {
            Balance b = new Balance();
            b.Usr_OID = User_OID;
            b.BalDateTime = DateTime.UtcNow;
            IEnumerable<Transaction> Transactions = GetAllItems<Transaction>(nameof(Transactions), User_OID);
            IEnumerable<Bank> Banks = GetAllItems<Bank>(nameof(Banks), User_OID);
            IEnumerable<Ticket> Tickets = GetAllItems<Ticket>(nameof(Tickets), User_OID);
            int tot = 0;
            int totTransaction = 0;

            foreach (var item in Banks)
            {
                tot += item.BankValue;
            }
            foreach (var item in Tickets)
            {
                tot += Convert.ToInt32(item.NumTicket) * item.TicketValue;
            }
            foreach (var item in Transactions)
            {
                totTransaction += item.TrsValue;
            }

            Transaction tr = new Transaction() { Usr_OID = User_OID, TrsCode = "Fast_Update", TrsTitle = "Allineamento Fast Update", TrsDateTime = DateTime.UtcNow, TrsValue = tot - totTransaction, TrsNote = "Allineamento Fast Update eseguito il" + DateTime.UtcNow };
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri("https://personalfinanceappapi.azurewebsites.net/api/PersonalFinanceAPI/");
                var postTask = client.PostAsJsonAsync<Transaction>("AddTransaction", tr);
                postTask.Wait();
                var result = postTask.Result;
            }
            b.ActBalance = tot;



            AddItem<Balance>(nameof(Balance), b);
            return 1;
        }
        public int Transaction_Balance_Update(string User_OID)
        {
            Balance b = new Balance();
            b.Usr_OID = User_OID;
            b.BalDateTime = DateTime.UtcNow;
            IEnumerable<Transaction> Transactions = GetAllItems<Transaction>(nameof(Transactions), User_OID);

            int totTransaction = 0;
            foreach (var item in Transactions)
            {
                totTransaction += item.TrsValue;
            }
            b.ActBalance = totTransaction;
            AddItem<Balance>(nameof(Balance), b);
            return 1;
        }
    }
}