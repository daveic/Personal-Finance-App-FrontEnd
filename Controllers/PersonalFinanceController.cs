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





       










        


        //VIEWS
        //CREDITS DEBITS Intermediate view
        [AuthorizeForScopes(ScopeKeySection = "DownstreamApi:Scopes")]
        public ActionResult Credits_Debits()
        {
            string User_OID = GetUserData().Result; //Fetch User Data
            ViewModel viewModel = new();
            viewModel.Credits = GetAllItems<Credit>("PersonalFinanceAPI", "Credits", User_OID);
            viewModel.Credit = new Credit();
            viewModel.Debits = GetAllItems<Debit>("PersonalFinanceAPI", "Debits", User_OID);
            viewModel.Debit = new Debit();
            return View(viewModel);
        }




        
        //TRANSACTIONS Intermediate view
       
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
            
             ViewModel viewModel = new();
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

        


        



        //FAST UPDATE LOGIC
        //In caso di aggiunta di nuova banca/ticket, occorre solo aggiungere l'immagine sotto images con nome in minuscolo e "-" al posto degli spazi (in .jpeg)
        public ActionResult Fast_Update()
        {
            string User_OID = this.User.FindFirst(ClaimTypes.NameIdentifier).Value;
            ViewModel model = new();
            model.Banks = GetAllItems<Bank>("PersonalFinanceAPI", "Banks", User_OID);
            model.Tickets = GetAllItems<Ticket>("PersonalFinanceAPI", "Tickets", User_OID);
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
            Balance b = new();
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

            Transaction tr = new() { Usr_OID = User_OID, TrsCode = "Fast_Update", TrsTitle = "Allineamento Fast Update", TrsDateTime = DateTime.UtcNow, TrsValue = tot - totTransaction, TrsNote = "Allineamento Fast Update eseguito il " + DateTime.UtcNow };
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri("https://personalfinanceappapi.azurewebsites.net/api/PersonalFinanceAPI/");
                var postTask = client.PostAsJsonAsync<Transaction>("AddTransaction", tr);
                postTask.Wait();
                var result = postTask.Result;
            }
            b.ActBalance = tot;

            AddItemN<Balance>("Balances", b);
            return 1;
        }
        public int Transaction_Balance_Update(string User_OID)
        {
            Balance b = new()
            {
                Usr_OID = User_OID,
                BalDateTime = DateTime.UtcNow
            };
            IEnumerable<Transaction> Transactions = GetAllItems<Transaction>("PersonalFinanceAPI", nameof(Transactions), User_OID);

            double totTransaction = 0;
            foreach (var item in Transactions)
            {
                totTransaction += item.TrsValue;
            }
            b.ActBalance = totTransaction;
            AddItemN<Balance>("Balances", b);
            return 1;
        }

        




    }




}
