using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Graph;
using Microsoft.Identity.Web;
using Microsoft.Extensions.Configuration;
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
          

            ViewModel viewModel = new ViewModel();
            IEnumerable<Transaction> Transactions = GetAllItems<Transaction>("PersonalFinanceAPI", nameof(Transactions), User_OID);
            IEnumerable<Credit> Credits = GetAllItemsN<Credit>("Credits", User_OID);
            IEnumerable<Debit> Debits = GetAllItems<Debit>("PersonalFinanceAPI", nameof(Debits), User_OID);
            IEnumerable<Bank> Banks = GetAllItemsN<Bank>("Banks", User_OID);
            IEnumerable<Deposit> Deposits = GetAllItems<Deposit>("PersonalFinanceAPI", nameof(Deposits), User_OID);
            IEnumerable<Ticket> Tickets = GetAllItems<Ticket>("PersonalFinanceAPI", nameof(Tickets), User_OID);
            IEnumerable<Balance> Balances = GetAllItems<Balance>("PersonalFinanceAPI", nameof(Balances), User_OID);

            if (!Banks.Any())
            {
                Bank b = new Bank { Usr_OID = User_OID, BankName = "Contanti", Iban = null, ID = 0, BankValue = 0, BankNote = "Totale contanti" };
                int result = AddItem<Bank>(nameof(Bank), b);
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
            bool isPresent = false;
            foreach (var credit in Credits)
            {
                foreach (var item in Codes)
                {
                    if (credit.CredCode == item.Value) isPresent = true;
                }
                if (isPresent is true)
                {
                    SelectListItem code = new SelectListItem();
                    code.Value = credit.CredCode;
                    code.Text = credit.CredCode;
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
                    SelectListItem code = new SelectListItem();
                    code.Value = debit.DebCode;
                    code.Text = debit.DebCode;
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
            List<string> strCodes = new List<string>();
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

        private IEnumerable<T> GetAllItems<T>(string controller, string type, string User_OID)
        {
            IEnumerable<T> detections = null;
            string path = "api/" + controller + "/GetAll" + type + "?User_OID=" + User_OID;
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri("https://personalfinanceappapi.azurewebsites.net/");
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
            T detection = new();
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

        //DETAILS: Controller methods for detail action - GET-BY-ID



       
        public ActionResult Transaction_Details(int id)
        {
            Transaction Transaction = GetItemID<Transaction>(nameof(Transaction), id);
            return PartialView(Transaction);
        }
        public ActionResult Transaction_Details_Edit(int id, string User_OID)
        {
            IEnumerable<Transaction> Transactions = GetAllItems<Transaction>("PersonalFinanceAPI", nameof(Transactions), User_OID);
            IEnumerable<Credit> Credits = GetAllItems<Credit>("PersonalFinanceAPI", nameof(Credits), User_OID);
            IEnumerable<Debit> Debits = GetAllItems<Debit>("PersonalFinanceAPI", nameof(Debits), User_OID);
            Transaction t = GetItemID<Transaction>(nameof(Transaction), id);
            var UniqueCodes = Transactions.GroupBy(x => x.TrsCode)
                                          .Select(x => x.First())
                                          .ToList();
            List<SelectListItem> Codes = new List<SelectListItem>();
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
                    SelectListItem code = new SelectListItem
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
                    SelectListItem code = new SelectListItem
                    {
                        Value = debit.DebCode,
                        Text = debit.DebCode
                    };
                    Codes.Add(code);
                }
                isPresent = false;
            }
            TempData["Codes"] = Codes;            
          
            if (t.TrsValue < 0) t.Type = false; else t.Type = true;
            t.input_value = t.TrsValue.ToString();
            return PartialView(t);
        }


        //DELETE: Controller methods for Delete-single-entry action - They send 1 if succeded to let green confirmation popup appear (TempData["sendFlag.."])
        //public ActionResult Credit_Delete(int id)
        //{
        //    return Credit_Details(id);
        //}

        public ActionResult Debit_Delete(int id)
        {
            return Debit_Details(id);
        }
        [HttpPost]
        public ActionResult Debit_Delete(Debit d)
        {
            Debit deb = GetItemID<Debit>(nameof(Debit), d.ID);
            for(int i=0; i <= (deb.RtNum - deb.RtPaid); i++)
            {
                int res = DeleteItem("Expiration", (deb.Exp_ID + i));
            }
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




        //EDIT: Controller methods for Updating/Editing-single-entry action - They send 2 if succeded to let green confirmation popup appear (TempData["sendFlag.."])

       
        public ActionResult Debit_Edit(int id)
        {
            return Debit_Details(id);
        }
        [HttpPost]
        public ActionResult Debit_Edit(Debit d, int i, bool fromTransaction)
        {
            if (i!=1)
            {
                d.input_value = d.input_value.Replace(".", ",");
                d.DebValue = Convert.ToDouble(d.input_value);
                d.input_value_remain = d.input_value_remain.Replace(".", ",");
                d.RemainToPay = Convert.ToDouble(d.input_value_remain);
            }

            d.Usr_OID = this.User.FindFirst(ClaimTypes.NameIdentifier).Value;
            if(fromTransaction is false)
            {
                Debit oldDebit = GetItemID<Debit>("Debit", d.ID);
                for (int k = 0; k <= (oldDebit.RtNum - oldDebit.RtPaid); k++)
                {
                    int res = DeleteItem("Expiration", (oldDebit.Exp_ID + k));
                }
                for (int j = 0; j < d.RtNum; j++)
                {
                    Expiration exp = new Expiration();
                    exp.Usr_OID = d.Usr_OID;
                    exp.ExpTitle = d.DebTitle;
                    exp.ExpDescription = d.DebTitle + " - rata: " + (j + 1);
                    if (d.RtFreq == "Mesi")
                    {
                        exp.ExpDateTime = d.DebInsDate.AddMonths(j * d.Multiplier);
                    }
                    exp.ColorLabel = "red";
                    exp.ExpValue = d.DebValue / d.RtNum;
                    AddItem<Expiration>("Expiration", exp);
                }
                IEnumerable<Expiration> Expirations = GetAllItems<Expiration>("PersonalFinanceAPI", nameof(Expirations), d.Usr_OID);
                d.Exp_ID = Expirations.Last().ID - Convert.ToInt32(d.RtNum) + 1;
            }
            
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
        public ActionResult Transaction_Edit(Transaction t)
        {
            t.input_value = t.input_value.Replace(".", ",");
            t.TrsValue = Convert.ToDouble(t.input_value);
            if (t.Type == false) t.TrsValue = -Math.Abs(t.TrsValue);
            if (t.Type == true) t.TrsValue = Math.Abs(t.TrsValue);
            if (t.NewTrsCode != null) t.TrsCode = t.NewTrsCode;
            ClaimsPrincipal currentUser = this.User;
            t.Usr_OID = currentUser.FindFirst(ClaimTypes.NameIdentifier).Value;
            int result = EditItemID<Transaction>(nameof(Transaction), t);
            if (result == 0)
            {
                TempData["sendFlagTr"] = 2;
                Transaction_Balance_Update(t.Usr_OID);
                return RedirectToAction(nameof(Transactions));
            }
            return View();
        }
        


        //VIEWS
        //CREDITS DEBITS Intermediate view
        [AuthorizeForScopes(ScopeKeySection = "DownstreamApi:Scopes")]
        public ActionResult Credits_Debits()
        {
            string User_OID = GetUserData().Result; //Fetch User Data
            ViewBag.Expirations = GetAllItems<Expiration>("PersonalFinanceAPI", nameof(Expirations), User_OID).OrderBy(x => x.ExpDateTime.Month).Take(5).ToList(); //Fetch imminent expirations
            ViewModel viewModel = new ViewModel();
            viewModel.Credits = GetAllItems<Credit>("PersonalFinanceAPI", "Credits", User_OID);
            viewModel.Credit = new Credit();
            viewModel.Debits = GetAllItems<Debit>("PersonalFinanceAPI", "Debits", User_OID);
            viewModel.Debit = new Debit();
            return View(viewModel);
        }




        
        //TRANSACTIONS Intermediate view
        public ActionResult Transactions(string orderBy, string selectedType, string selectedCode, string selectedYear, string selectedMonth, int page = 0)
        {
            string User_OID = GetUserData().Result; //Fetch User Data
            ViewBag.Expirations = GetAllItems<Expiration>("PersonalFinanceAPI", nameof(Expirations), User_OID).OrderBy(x => x.ExpDateTime.Month).Take(5).ToList(); //Fetch imminent expirations

            IEnumerable<Transaction> Transactions = GetAllItems<Transaction>("PersonalFinanceAPI", nameof(Transactions), User_OID);
            IEnumerable<Credit> Credits = GetAllItems<Credit>("PersonalFinanceAPI", nameof(Credits), User_OID);
            IEnumerable<Debit> Debits = GetAllItems<Debit>("PersonalFinanceAPI", nameof(Debits), User_OID);
            ViewModel viewModel = new ViewModel();

            //############################################################################################################################
            //FILTRI ANNO E MESE PER GRAFICO SALDO
            //############################################################################################################################
            //Trovo gli anni "unici"
            var UniqueYear = Transactions.GroupBy(item => item.TrsDateTime.Year)
                    .Select(group => group.First())
                    .Select(item => item.TrsDateTime.Year)
                    .ToList();
            //Creo la lista di anni "unici" per il dropdown filter del grafico saldo
            List<SelectListItem> itemlistYear = new List<SelectListItem>();
            foreach (var year in UniqueYear) itemlistYear.Add(new SelectListItem() { Text = year.ToString(), Value = year.ToString() });
            //Passo alla view la lista
            ViewBag.ItemList = itemlistYear;
            //Se al caricamento della pagina ho selezionato un anno (not empty), salvo in Balances i saldi di quell'anno
            if (!String.IsNullOrEmpty(selectedYear)) Transactions = Transactions.AsQueryable().Where(x => x.TrsDateTime.Year.ToString() == selectedYear);
            //############################################################################################################################
            //Trovo i mesi "unici"
            var UniqueMonth = Transactions.GroupBy(item => item.TrsDateTime.Month)
                                .Select(group => group.First())
                                .Select(item => item.TrsDateTime.Month)
                                .ToList();
            //Creo la lista di mesi "unici" per il dropdown filter del grafico saldo
            List<SelectListItem> itemlistMonth = new List<SelectListItem>();
            foreach (var month in UniqueMonth) itemlistMonth.Add(new SelectListItem() { Text = MonthConverter(month), Value = MonthConverter(month) });            
            //Passo alla view la lista
            ViewBag.ItemListMonth = itemlistMonth;
            //Se al caricamento della pagina ho selezionato un mese (not empty), salvo in Balances i saldi di quel mese
            if (!String.IsNullOrEmpty(selectedMonth)) Transactions = Transactions.AsQueryable().Where(x => MonthConverter(x.TrsDateTime.Month) == selectedMonth);
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
            //############################################################################################################################
            //############################################################################################################################

            Transactions = Transactions.Reverse();
            //## ORDINAMENTO #############################################################################################################
            List<SelectListItem> orderByList = new List<SelectListItem>();
            SelectListItem datetimeAsc = new SelectListItem() { Text = "Data crescente", Value = "Data crescente" };
            SelectListItem datetimeDesc = new SelectListItem() { Text = "Data decrescente", Value = "Data decrescente" };
            SelectListItem categ = new SelectListItem() { Text = "Categoria", Value = "Categoria" };
            SelectListItem type = new SelectListItem() { Text = "Entrate/Uscite", Value = "Entrate/Uscite" };
            orderByList.Add(datetimeAsc);
            orderByList.Add(datetimeDesc);
            orderByList.Add(categ);
            orderByList.Add(type);

            ViewBag.OrderBy = orderByList;
            if (!String.IsNullOrEmpty(orderBy))
            {
                if (orderBy == "Data crescente") Transactions = Transactions.OrderBy(x => x.TrsDateTime);
                else if (orderBy == "Data decrescente") Transactions = Transactions.OrderByDescending(x => x.TrsDateTime);
                else if (orderBy == "Categoria") Transactions = Transactions.OrderBy(x => x.TrsCode);
                else if (orderBy == "Entrate/Uscite") Transactions = Transactions.OrderByDescending(x => x.TrsValue);
            }
            List<string> LastChoices = new List<string>();
            LastChoices.Add(orderBy);
            LastChoices.Add(selectedType);
            LastChoices.Add(selectedCode);
            LastChoices.Add(selectedMonth);
            LastChoices.Add(selectedYear);

            //Pagination
            ViewBag.LastChoices = LastChoices;
            const int PageSize = 20;
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
            bool isPresent = false;
            foreach (var credit in Credits)
            {
                foreach(var item in Codes)
                {
                    if (credit.CredCode == item.Value) isPresent = true;
                }
                if (isPresent is false) Codes.Add(new SelectListItem() { Text = credit.CredCode, Value = credit.CredCode });                
                isPresent = false;
            }
            foreach (var debit in Debits)
            {
                foreach (var item in Codes)
                {
                    if (debit.DebCode == item.Value) isPresent = true;
                }
                if (isPresent is false) Codes.Add(new SelectListItem() {Text = debit.DebCode, Value = debit.DebCode});                
                isPresent = false;
            }
            TempData["Codes"] = Codes;
            viewModel.Transaction = new Transaction();
            return View(viewModel);
        }
        //BUDGET Intermediate view        
        public ActionResult Budget()
        {
            string User_OID = GetUserData().Result; //Fetch User Data
            ViewBag.Expirations = GetAllItems<Expiration>("PersonalFinanceAPI", "Expirations", User_OID).OrderBy(x => x.ExpDateTime.Month).Take(5).ToList(); //Fetch imminent expirations
            List<Expiration> Expirations = GetAllItems<Expiration>("PersonalFinanceAPI", nameof(Expirations), User_OID).OrderBy(x => x.ExpDateTime.Month).Where(x => x.ExpDateTime.Month == DateTime.Now.Month).ToList(); //Fetch imminent expirations
            List<KnownMovement> KnownMovements = GetAllItems<KnownMovement>("PersonalFinanceAPI", nameof(KnownMovements), User_OID).ToList();
            bool found = false;
            foreach (var km in KnownMovements)
            {
                foreach (var exp in Expirations)
                {
                    if(km.KMTitle == exp.ExpTitle && km.KMValue == exp.ExpValue)
                    {
                        found = true;
                    }
                }
                if (found is false) Expirations.Add(new Expiration() { ExpTitle = km.KMTitle , ExpValue = km.KMValue});
                found = false;
            }

            ViewBag.In = Expirations.Where(x => x.ExpValue >= 0);
            ViewBag.Out = Expirations.Where(x => x.ExpValue < 0);
            
             ViewModel viewModel = new ViewModel();
            viewModel.Banks = GetAllItems<Bank>("PersonalFinanceAPI", "Banks", User_OID);
            viewModel.Bank = new Bank();
            viewModel.Deposits = GetAllItems<Deposit>("PersonalFinanceAPI", "Deposits", User_OID);
            viewModel.Deposit = new Deposit();
            viewModel.Tickets = GetAllItems<Ticket>("PersonalFinanceAPI", "Tickets", User_OID);
            viewModel.Ticket = new Ticket();
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
   




            IEnumerable<Balance> Balances = GetAllItems<Balance>("PersonalFinanceAPI", nameof(Balances), GetUserData().Result);
            double stimated_total = Balances.Last().ActBalance + bc.Corrective_Item_0 + bc.Corrective_Item_1 + bc.Corrective_Item_2 + bc.Corrective_Item_3;

            List<Expiration> Expirations = GetAllItems<Expiration>("PersonalFinanceAPI", nameof(Expirations), GetUserData().Result).OrderBy(x => x.ExpDateTime).Where(x => x.ExpDateTime <= bc.Future_Date).ToList();
            foreach(var item in Expirations)
            {
                if (item.ColorLabel == "orange") Expirations.Remove(item);
            }
            List<KnownMovement> KnownMovements = GetAllItems<KnownMovement>("PersonalFinanceAPI", nameof(KnownMovements), GetUserData().Result).ToList();
            //contare mesi tra date
            //Se >15 del mese aggiungi uno altrimenti togli
            //cicla per n volte quanti sono i mesi
            foreach (var km in KnownMovements)
            {
                Expirations.Add(new Expiration() { ExpTitle = km.KMTitle, ExpValue = km.KMValue });
              
            }
            foreach(var item in Expirations)
            {
                stimated_total += item.ExpValue;
            }

            //e.input_value = e.input_value.Replace(".", ",");
            //e.ExpValue = Convert.ToDouble(e.input_value);
   

            return RedirectToAction(nameof(Budget));
           
        }
        //ADD NEW Methods

        
        public IActionResult Transaction_Add()
        {
            return View(new Transaction());
        }
        [HttpPost]
        public ActionResult Transaction_Add(Transaction t)
        {
            t.input_value = t.input_value.Replace(".", ",");
            t.TrsValue = Convert.ToDouble(t.input_value);
            if (t.Type == false) t.TrsValue = -t.TrsValue;
            if (t.NewTrsCode != null) t.TrsCode = t.NewTrsCode;
            t.Usr_OID = this.User.FindFirst(ClaimTypes.NameIdentifier).Value;
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri("https://personalfinanceappapi.azurewebsites.net/api/PersonalFinanceAPI/");
                var postTask = client.PostAsJsonAsync<Transaction>("AddTransaction", t);
                postTask.Wait();
                var result = postTask.Result;
                if (result.IsSuccessStatusCode)
                {
                    TempData["sendFlagTr"] = 3;
                    Transaction_Balance_Update(t.Usr_OID);
                    Transaction_Credit_Debit_Update(t);
                    return RedirectToAction(nameof(Transactions));
                }
            }            
            return View();
        }

        



        //FAST UPDATE LOGIC
        //In caso di aggiunta di nuova banca/ticket, occorre solo aggiungere l'immagine sotto images con nome in minuscolo e "-" al posto degli spazi (in .jpeg)
        public ActionResult Fast_Update()
        {
            string User_OID = this.User.FindFirst(ClaimTypes.NameIdentifier).Value;
            ViewModel model = new();
            model.Banks = GetAllItems<Bank>("PersonalFinanceAPI", "Banks", User_OID);
            model.Tickets = GetAllItems<Ticket>("PersonalFinanceAPI", "Tickets", User_OID);
            List<Bank> BankList = new List<Bank>();
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
            string User_OID = this.User.FindFirst(ClaimTypes.NameIdentifier).Value;
            List<Bank> BankList = model.BankList;
            IEnumerable<Bank> Banks = GetAllItems<Bank>("PersonalFinanceAPI", nameof(Banks), User_OID);
            List<Ticket> TicketList = model.TicketList;
            IEnumerable<Ticket> Tickets = GetAllItems<Ticket>("PersonalFinanceAPI", nameof(Tickets), User_OID);
            foreach (var item in BankList)
            {
                foreach (var bank in Banks)
                {
                    if (item.ID == bank.ID)
                    {
                        item.input_value = item.input_value.Replace(".", ",");
                        bank.BankValue = Convert.ToDouble(item.input_value);
                        int result = EditItemID<Bank>(nameof(Bank), bank);
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
                        int result = EditItemID<Ticket>(nameof(Ticket), ticket);
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
            IEnumerable<Transaction> Transactions = GetAllItems<Transaction>("PersonalFinanceAPI", nameof(Transactions), User_OID);
            IEnumerable<Bank> Banks = GetAllItems<Bank>("PersonalFinanceAPI", nameof(Banks), User_OID);
            IEnumerable<Ticket> Tickets = GetAllItems<Ticket>("PersonalFinanceAPI", nameof(Tickets), User_OID);
            double tot = 0;
            double totTransaction = 0;

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

            Transaction tr = new Transaction() { Usr_OID = User_OID, TrsCode = "Fast_Update", TrsTitle = "Allineamento Fast Update", TrsDateTime = DateTime.UtcNow, TrsValue = tot - totTransaction, TrsNote = "Allineamento Fast Update eseguito il " + DateTime.UtcNow };
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
            IEnumerable<Transaction> Transactions = GetAllItems<Transaction>("PersonalFinanceAPI", nameof(Transactions), User_OID);

            double totTransaction = 0;
            foreach (var item in Transactions)
            {
                totTransaction += item.TrsValue;
            }
            b.ActBalance = totTransaction;
            AddItem<Balance>(nameof(Balance), b);
            return 1;
        }

        public int Transaction_Credit_Debit_Update(Transaction t)
        {
            IEnumerable<Credit> Credits = GetAllItems<Credit>("PersonalFinanceAPI", nameof(Credits), t.Usr_OID);
            IEnumerable<Debit> Debits = GetAllItems<Debit>("PersonalFinanceAPI", nameof(Debits), t.Usr_OID);

            if (t.TrsValue < 0)
            {
                foreach (var debit in Debits)
                {
                    if(t.TrsCode == debit.DebCode)
                    {
                        debit.RemainToPay += t.TrsValue;
                        debit.RtPaid = debit.RtPaid + (-t.TrsValue) / (debit.DebValue / debit.RtNum);
                        DeleteItem(nameof(Expiration), (debit.Exp_ID + Convert.ToInt32(debit.RtPaid - 1)));
                        
                        if(debit.RemainToPay <= 0)
                        {
                            Debit_Delete(debit);
                        }
                        else
                        {
                            Debit_Edit(debit, 1, true);
                        }
                    }

                }
                if (t.TrsCode.StartsWith("CRE"))
                {
                    Credit model = new()
                    {
                        Usr_OID = t.Usr_OID,
                        CredCode = t.TrsCode,
                        CredDateTime = DateTime.UtcNow,
                        CredValue = t.TrsValue,
                        CredTitle = "Prestito/Anticipo",
                        CredNote = ""
                    };
                    Credit_Add(model, 1);
                }

            }
            if(t.TrsValue > 0)
            {
               foreach(var credit in Credits)
                {
                    if (t.TrsCode == credit.CredCode)
                    {
                          credit.CredValue -= t.TrsValue;
                        if (credit.CredValue <= 0)
                        {
                            Credit_Delete(credit);
                        }
                        else
                        {
                            Credit_Edit(credit, 1);
                        }
                    }
                }
                if (t.TrsCode.StartsWith("DEB"))
                {
                    Debit model = new();
                    model.Usr_OID = t.Usr_OID;
                    model.DebCode = t.TrsCode;
                    model.DebInsDate = DateTime.UtcNow;
                    model.DebValue = -t.TrsValue;
                    model.DebTitle = "Prestito/Anticipo";
                    model.DebNote = "";
                    model.RemainToPay = -t.TrsValue;
                    model.RtPaid = 0;
                    model.RtNum = 1;
                    Debit_Add(model, 1);
                }
            }            
            return 1;
        }


        public IEnumerable<T> GetAllItemsN<T>(string controller, string User_OID)
        {
            IEnumerable<T> detections = null;
            string path = "api/" + controller + "/All" + "?User_OID=" + User_OID;
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri("https://personalfinanceappapi.azurewebsites.net/");
                var responseTask = client.GetAsync(path);
                responseTask.Wait();
                var result = responseTask.Result;
                var readTask = result.Content.ReadAsAsync<List<T>>();
                readTask.Wait();
                detections = readTask.Result;
            }
            return (detections);
        }
        public T GetItemIDN<T>(string controller, int id, string User_OID) where T : new()
        {
            T detection = new();
            string path = "api/" + controller + "/Details" + "?id=" + id + "&User_OID=" + User_OID;
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri("https://personalfinanceappapi.azurewebsites.net/");
                var responseTask = client.GetAsync(path);
                responseTask.Wait();
                var result = responseTask.Result;
                var readTask = result.Content.ReadAsAsync<T>();
                readTask.Wait();
                detection = readTask.Result;
            }
            return (detection);
        }
        public int AddItemN<T>(string controller, T obj) where T : new()
        {
            string path = "api/" + controller + "/Add";
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri("https://personalfinanceappapi.azurewebsites.net/");
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
        public int EditItemIDN<T>(string controller, T obj) where T : new()
        {
            string path = "api/" + controller + "/Update";
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri("https://personalfinanceappapi.azurewebsites.net/");
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
        public int DeleteItemN(string controller, int id, string User_OID)
        {
            string path = "api/" + controller + "/Delete" + "?id=" + id + "&User_OID=" + User_OID;
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri("https://personalfinanceappapi.azurewebsites.net/");
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














        //REFACTORED



    }








}





