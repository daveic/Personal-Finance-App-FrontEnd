using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;
using PersonalFinanceFrontEnd.Models;



namespace PersonalFinanceFrontEnd.Controllers
{

    public class PersonalFinanceController : Controller
    {
        public ActionResult Index(string selectedYear, string selectedMonth, string SelectedRegion, string SelectedDate, int page = 0)
        {

            ViewModel viewModel = new ViewModel();
            IEnumerable<Transaction> Transactions = GetAllItems<Transaction>(nameof(Transactions));
            IEnumerable<Credit> Credits = GetAllItems<Credit>(nameof(Credits));
            IEnumerable<Debit> Debits = GetAllItems<Debit>(nameof(Debits));
            IEnumerable<Bank> Banks = GetAllItems<Bank>(nameof(Banks));
            IEnumerable<Deposit> Deposits = GetAllItems<Deposit>(nameof(Deposits));
            IEnumerable<Ticket> Tickets = GetAllItems<Ticket>(nameof(Tickets));
            IEnumerable<Balance> Balances = GetAllItems<Balance>(nameof(Balances));


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




















            /*          
                        DateTime dateTime = Convert.ToDateTime(SelectedDate);
                        UniqueData uniqueData = new UniqueData();
                        TempStatistics tempStatistics = new TempStatistics();

                        tempStatistics.Region = GetAreaListItem(detections.AsEnumerable().GroupBy(x => x.WorldLocation.Region));
                        tempStatistics.Province = GetAreaListItem(detections.AsEnumerable().GroupBy(x => x.WorldLocation.Province));
                        tempStatistics.City = GetAreaListItem(detections.AsEnumerable().GroupBy(x => x.WorldLocation.City));



                        uniqueData.UniqueDate = UniqueDate.Select(m => new SelectListItem { Value = m.DateTime.ToString(), Text = m.DateTime.ToString() }).ToList();
                        uniqueData.UniqueCity = tempStatistics.City.Select(x => new SelectListItem { Value = x.AreaName, Text = x.AreaName }).ToList();
                        uniqueData.UniqueProvince = tempStatistics.Province.Select(x => new SelectListItem { Value = x.AreaName, Text = x.AreaName }).ToList();
                        uniqueData.UniqueRegion = tempStatistics.Region.Select(x => new SelectListItem { Value = x.AreaName, Text = x.AreaName }).ToList();

                        
                        if (!String.IsNullOrEmpty(SelectedCity)) detections = detections.Where(s => s.WorldLocation.City == SelectedCity);
                        if (!String.IsNullOrEmpty(SelectedProvince)) detections = detections.Where(x => x.WorldLocation.Province == SelectedProvince);
                        if (!String.IsNullOrEmpty(SelectedRegion)) detections = detections.Where(x => x.WorldLocation.Region == SelectedRegion);

                        //Pagination
                        const int PageSize = 3;
                        var count = detections.Count();
                        var data = detections.Skip(page * PageSize).Take(PageSize).ToList();

                        this.ViewBag.MaxPage = (count / PageSize) - (count % PageSize == 0 ? 1 : 0);
                        this.ViewBag.Page = page;
                   */
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
                SelectListItem subitem = new SelectListItem() { Text = month.Month.ToString(), Value = month.Month.ToString() };
                itemlistMonth.Add(subitem);
            }
            //Passo alla view la lista
            ViewBag.ItemListMonth = itemlistMonth;
            //Se al caricamento della pagina ho selezionato un mese (not empty), salvo in Balances i saldi di quel mese
            if (!String.IsNullOrEmpty(selectedMonth)) Balances = Balances.AsQueryable().Where(x => x.BalDateTime.Month.ToString() == selectedMonth);
            //############################################################################################################################


            //############################################################################################################################
            //Rimuovo l'orario dal DateTime e salvo come json
            foreach (var item in Balances)
            {
                item.BalDateTime = item.BalDateTime.Date;
            }
            string json = JsonConvert.SerializeObject(Balances);
            //Passo alla view la lista aggiornata e convertita
            ViewBag.Balances = json;

            var UniqueCodes = Transactions.GroupBy(x => x.TrsCode)
                                            .OrderBy(x => x.Key)
                                            .Select(x => new { Code = x.Key })
                                            .ToList();
            List<SelectListItem> Codes = new List<SelectListItem>();
            foreach (var item in UniqueCodes)
            {
                SelectListItem code = new SelectListItem();
                code.Value = item.Code;
                code.Text = item.Code;
                Codes.Add(code);

            }
            viewModel.TransactionType = new TransactionType();
            viewModel.TransactionType.Codes = Codes;

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




        private IEnumerable<T> GetAllItems<T> (string type)
        {
            IEnumerable<T> detections = null;
            string path = "GetAll" + type;
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





        public ActionResult Credit_Details (int id)
        {
            Credit Credit = GetItemID<Credit>(nameof(Credit), id); 
            return PartialView(Credit);
        }
        public ActionResult Debit_Details(int id)
        {
            Debit Debit = GetItemID<Debit>(nameof(Debit), id);
            return PartialView(Debit);
        }
        public ActionResult Transaction_Details(int id)
        {
            Transaction Transaction = GetItemID<Transaction>(nameof(Transaction), id);   
            return PartialView(Transaction);
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

        //DELETE: Controller methods for Delete-single-entry action - They send 1 if succeded to let green confirmation popup appear (TempData["sendFlag.."])
        public ActionResult Credit_Delete(int id)
        {
            return Credit_Details(id);
        }
        [HttpPost]
        public ActionResult Credit_Delete(Credit c)
        {
            int credId = c.ID;
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri("https://personalfinanceappapi.azurewebsites.net/api/PersonalFinanceAPI/");
                var postTask = client.DeleteAsync($"DeleteCredit?id={credId}");
                postTask.Wait();
                var result = postTask.Result;
                if (result.IsSuccessStatusCode)
                {
                    TempData["sendFlagCred"] = 1;
                    return RedirectToAction(nameof(Credits));
                }
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
            int debitId = d.ID;
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri("https://personalfinanceappapi.azurewebsites.net/api/PersonalFinanceAPI/");
                var postTask = client.DeleteAsync($"DeleteDebit?id={debitId}");
                postTask.Wait();
                var result = postTask.Result;
                if (result.IsSuccessStatusCode)
                {
                    TempData["sendFlagDeb"] = 1;
                    return RedirectToAction(nameof(Debits));
                }
            }
            return View();
        }
        public ActionResult Transaction_Delete(int id)
        {
            return Transaction_Details(id);
        }
        [HttpPost]
        public ActionResult Transaction_Delete(Transaction d)
        {
            int transactionId = d.ID;
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri("https://personalfinanceappapi.azurewebsites.net/api/PersonalFinanceAPI/");
                var postTask = client.DeleteAsync($"DeleteTransaction?id={transactionId}");
                postTask.Wait();
                var result = postTask.Result;
                if (result.IsSuccessStatusCode)
                {
                    TempData["sendFlagTr"] = 1;
                    return RedirectToAction(nameof(Transactions));
                }
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
            int KnownMovementId = k.ID;
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri("https://personalfinanceappapi.azurewebsites.net/api/PersonalFinanceAPI/");
                var postTask = client.DeleteAsync($"DeleteKnownMovement?id={KnownMovementId}");
                postTask.Wait();
                var result = postTask.Result;
                if (result.IsSuccessStatusCode)
                {
                    TempData["sendFlagKM"] = 1;
                    return RedirectToAction(nameof(KnownMovements));
                }
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
            int BankId = b.ID;
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri("https://personalfinanceappapi.azurewebsites.net/api/PersonalFinanceAPI/");
                var postTask = client.DeleteAsync($"DeleteBank?id={BankId}");
                postTask.Wait();
                var result = postTask.Result;
                if (result.IsSuccessStatusCode)
                {
                    TempData["sendFlagB"] = 1;
                    return RedirectToAction(nameof(Wallet));
                }
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
            int DepositId = d.ID;
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri("https://personalfinanceappapi.azurewebsites.net/api/PersonalFinanceAPI/");
                var postTask = client.DeleteAsync($"DeleteDeposit?id={DepositId}");
                postTask.Wait();
                var result = postTask.Result;
                if (result.IsSuccessStatusCode)
                {
                    TempData["sendFlagD"] = 1;
                    return RedirectToAction(nameof(Wallet));
                }
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
            int TicketId = t.ID;
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri("https://personalfinanceappapi.azurewebsites.net/api/PersonalFinanceAPI/");
                var postTask = client.DeleteAsync($"DeleteTicket?id={TicketId}");
                postTask.Wait();
                var result = postTask.Result;
                if (result.IsSuccessStatusCode)
                {
                    TempData["sendFlagT"] = 1;
                    return RedirectToAction(nameof(Wallet));
                }
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
            return Transaction_Details(id);
        }
        [HttpPost]
        public ActionResult Transaction_Edit(Transaction t)
        {
            int result = EditItemID<Transaction>(nameof(Transaction), t);
            if (result == 0)
            {
                TempData["sendFlagTr"] = 2;
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
            int result = EditItemID<Bank>(nameof(Bank), b);
            if (result == 0)
            {
                TempData["sendFlagB"] = 2;
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
            return Deposit_Details(id);
        }
        [HttpPost]
        public ActionResult Ticket_Edit(Ticket t)
        {
            int result = EditItemID<Ticket>(nameof(Ticket), t);
            if (result == 0)
            {
                TempData["sendFlagT"] = 2;
                return RedirectToAction(nameof(Wallet));
            }
            return View();
        }






/*    private List<AreaListItem> GetAreaListItem (IEnumerable<IGrouping<string, TempDetection>> areaGroup)
    {
        List<AreaListItem> AreaList = new List<AreaListItem>();
        foreach (var item in areaGroup) {
            AreaListItem areaListItem = new AreaListItem();
            areaListItem.AreaName = item.Key;
            areaListItem.TempData.MaxTemp = item.OrderByDescending(g => g.TempMax).FirstOrDefault().TempMax;
            areaListItem.TempData.MinTemp = item.OrderByDescending(g => g.TempMin).Last().TempMin;
            AreaList.Add(areaListItem);
            }
         return (AreaList);
    }
    */
        //CREDITS DEBITS Intermediate view
        public ActionResult Credits_Debits()
        {
            ViewModel viewModel = new ViewModel();
            viewModel.Credits = GetCredits();
            viewModel.Credit = new Credit();
            viewModel.Debits = GetDebits();
            viewModel.Debit = new Debit();
            return View(viewModel);
        }

        //WALLET Intermediate view
        public ActionResult Wallet()
        {
            ViewModel viewModel = new ViewModel();
            viewModel.Banks = GetBanks();
            viewModel.Bank = new Bank();
            viewModel.Deposits = GetDeposits();
            viewModel.Deposit = new Deposit();
            viewModel.Tickets = GetTickets();
            viewModel.Ticket = new Ticket();
            return View(viewModel);
        }

        //GET ALL Methods
        public IEnumerable<Bank> GetBanks()
        {
            IEnumerable<Bank> Banks = GetAllItems<Bank>(nameof(Banks));
            return Banks;
        }
        public IEnumerable<Deposit> GetDeposits()
        {
            IEnumerable<Deposit> Deposits = GetAllItems<Deposit>(nameof(Deposits));
            return Deposits;
        }
        public IEnumerable<Ticket> GetTickets()
        {
            IEnumerable<Ticket> Tickets = GetAllItems<Ticket>(nameof(Tickets));
            return Tickets;
        }
        public ActionResult Credits()
        {
            ViewModel viewModel = new ViewModel();
            viewModel.Credits = GetCredits();
            int sendFlag = (int)(TempData.ContainsKey("sendFlagCred") ? TempData["sendFlagCred"] : 0);            
            viewModel.state = sendFlag;
            return View(viewModel);
        }
        public IEnumerable<Credit> GetCredits()
        {
            IEnumerable<Credit> Credits = GetAllItems<Credit>(nameof(Credits));
            return Credits;
        }
        public ActionResult Debits()
        {
            ViewModel viewModel = new ViewModel();
            viewModel.Debits = GetDebits();
            int sendFlag = (int)(TempData.ContainsKey("sendFlagDeb") ? TempData["sendFlagDeb"] : 0);
            viewModel.state = sendFlag;
            return View(viewModel);
        }
        public IEnumerable<Debit> GetDebits()
        {
            IEnumerable<Debit> Debits = GetAllItems<Debit>(nameof(Debits));
            return Debits;
        }
        public ActionResult Transactions(string selectedCode, string selectedYear, string selectedMonth, int page = 0)
        {
            IEnumerable<Transaction> Transactions = GetAllItems<Transaction>(nameof(Transactions));
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
                SelectListItem subitem = new SelectListItem() { Text = month.Month.ToString(), Value = month.Month.ToString() };
                itemlistMonth.Add(subitem);
            }
            //Passo alla view la lista
            ViewBag.ItemListMonth = itemlistMonth;
            //Se al caricamento della pagina ho selezionato un mese (not empty), salvo in Balances i saldi di quel mese
            if (!String.IsNullOrEmpty(selectedMonth)) Transactions = Transactions.AsQueryable().Where(x => x.TrsDateTime.Month.ToString() == selectedMonth);
            //############################################################################################################################

            if (!String.IsNullOrEmpty(selectedCode)) Transactions = Transactions.AsQueryable().Where(x => x.TrsCode == selectedCode);



            //Pagination
            const int PageSize = 6;
            var count = Transactions.Count();
            var data = Transactions.Skip(page * PageSize).Take(PageSize).ToList();

            this.ViewBag.MaxPage = (count / PageSize) - (count % PageSize == 0 ? 1 : 0);
            this.ViewBag.Page = page;

            viewModel.Transactions = data;

            int sendFlag = (int)(TempData.ContainsKey("sendFlagTr") ? TempData["sendFlagTr"] : 0);
            viewModel.state = sendFlag;








            var UniqueCodes = Transactions.GroupBy(x => x.TrsCode)
                                            .OrderBy(x => x.Key)
                                            .Select(x => new { Code = x.Key })
                                            .ToList();
            List<SelectListItem> Codes = new List<SelectListItem>();
            foreach (var item in UniqueCodes)
            {
                SelectListItem code = new SelectListItem();
                code.Value = item.Code;
                code.Text = item.Code;
                Codes.Add(code);
     
            }
            viewModel.TransactionType = new TransactionType();
            viewModel.TransactionType.Codes = Codes;

            return View(viewModel);      
        }
        public ActionResult KnownMovements()
        {
            IEnumerable<KnownMovement> KnownMovements = GetAllItems<KnownMovement>(nameof(KnownMovements));
            ViewModel viewModel = new ViewModel();

            viewModel.KnownMovements = KnownMovements;
            viewModel.KnownMovement = new KnownMovement();
            int sendFlag = (int)(TempData.ContainsKey("sendFlagKM") ? TempData["sendFlagKM"] : 0);
            viewModel.state = sendFlag;
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
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri("https://personalfinanceappapi.azurewebsites.net/api/PersonalFinanceAPI/");
                var postTask = client.PostAsJsonAsync<Credit>("AddCredit", c);
                postTask.Wait();
                var result = postTask.Result;
                if (result.IsSuccessStatusCode)
                {
                    TempData["sendFlagCred"] = 3;
                    return RedirectToAction(nameof(Credits));
                }
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
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri("https://personalfinanceappapi.azurewebsites.net/api/PersonalFinanceAPI/");
                var postTask = client.PostAsJsonAsync<Debit>("AddDebit", d);
                postTask.Wait();
                var result = postTask.Result;
                if (result.IsSuccessStatusCode)
                {
                    TempData["sendFlagDeb"] = 3;
                    return RedirectToAction(nameof(Debits));
                }
            }
            return View();
        }
        public IActionResult Transaction_Add()
        {

            TransactionType model = new TransactionType();
            return View(model);
        }
        [HttpPost]
        public ActionResult Transaction_Add(TransactionType t)
        {
            if (t.Type == false) t.TrsValue = - t.TrsValue;
            Transaction tr = new Transaction() { ID=t.ID, TrsCode=t.TrsCode, TrsTitle=t.TrsTitle, TrsDateTime =t.TrsDateTime, TrsValue=t.TrsValue, TrsNote=t.TrsNote};
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri("https://personalfinanceappapi.azurewebsites.net/api/PersonalFinanceAPI/");
                var postTask = client.PostAsJsonAsync<Transaction>("AddTransaction", tr);
                postTask.Wait();
                var result = postTask.Result;
                if (result.IsSuccessStatusCode)
                {
                    TempData["sendFlagTr"] = 3;
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
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri("https://personalfinanceappapi.azurewebsites.net/api/PersonalFinanceAPI/");
                var postTask = client.PostAsJsonAsync<KnownMovement>("AddKnownMovement", k);
                postTask.Wait();
                var result = postTask.Result;
                if (result.IsSuccessStatusCode)
                {
                    TempData["sendFlagKM"] = 3;
                    return RedirectToAction(nameof(KnownMovements));
                }
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
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri("https://personalfinanceappapi.azurewebsites.net/api/PersonalFinanceAPI/");
                var postTask = client.PostAsJsonAsync<Bank>("AddBank", b);
                postTask.Wait();
                var result = postTask.Result;
                if (result.IsSuccessStatusCode)
                {
                    TempData["sendFlagB"] = 3;
                    return RedirectToAction(nameof(Wallet));
                }
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
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri("https://personalfinanceappapi.azurewebsites.net/api/PersonalFinanceAPI/");
                var postTask = client.PostAsJsonAsync<Deposit>("AddDeposit", d);
                postTask.Wait();
                var result = postTask.Result;
                if (result.IsSuccessStatusCode)
                {
                    TempData["sendFlagD"] = 3;
                    return RedirectToAction(nameof(Wallet));
                }
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
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri("https://personalfinanceappapi.azurewebsites.net/api/PersonalFinanceAPI/");
                var postTask = client.PostAsJsonAsync<Ticket>("AddTicket", t);
                postTask.Wait();
                var result = postTask.Result;
                if (result.IsSuccessStatusCode)
                {
                    TempData["sendFlagT"] = 3;
                    return RedirectToAction(nameof(Wallet));
                }
            }
            return View();
        }



        //FAST UPDATE LOGIC
        //In caso di aggiunta o rimozione di nuova banca/ticket, nel Controller occorre solo modificare il metodo FU_Details e Fast_Update come segue:

        public ActionResult Fast_Update(int id)
        {
            return FU_Details(id);
        }

        //Aggiungere:
        //FU_item.FU_ID_B1 = Banks.ElementAt(1).ID; <- Modificare "1" con il nuovo numero di banca
        //FU_item.FU_Value_B1 = Banks.ElementAt(1).BankValue; <- Modificare "1" con il nuovo numero di banca
        public ActionResult FU_Details(int id)
        {
            IEnumerable<Bank> Banks = GetAllItems<Bank>(nameof(Banks));
            IEnumerable<Ticket> Tickets = GetAllItems<Ticket>(nameof(Tickets));
            Fast_Update_Item FU_item = new Fast_Update_Item();

            FU_item.FU_ID = id;

            FU_item.FU_ID_C = 0;
            FU_item.FU_Value_C = Banks.ElementAt(0).BankValue;
                
            FU_item.FU_ID_B1 = Banks.ElementAt(1).ID;
            FU_item.FU_Value_B1 = Banks.ElementAt(1).BankValue;

            FU_item.FU_ID_B2 = Banks.ElementAt(2).ID;
            FU_item.FU_Value_B2 = Banks.ElementAt(2).BankValue;

            FU_item.FU_ID_B3 = Banks.ElementAt(3).ID;
            FU_item.FU_Value_B3 = Banks.ElementAt(3).BankValue;

            FU_item.FU_ID_B4 = Banks.ElementAt(4).ID;
            FU_item.FU_Value_B4 = Banks.ElementAt(4).BankValue;

            FU_item.FU_ID_T = Tickets.ElementAt(0).ID;
            FU_item.FU_Value_T = Tickets.ElementAt(0).TicketValue;

            return PartialView(FU_item);
        }

        [HttpPost]
        //Aggiungere:
        //Banks.ElementAt(1).BankValue = FU_item.FU_Value_B1; <- Modificare sempre "1" col nuovo numero
        //result = EditItemID<Bank>(nameof(Bank), Banks.ElementAt(1)); <- Modificare sempre "1" col nuovo numero
        public ActionResult Fast_Update (Fast_Update_Item FU_item)
        {
            IEnumerable<Bank> Banks = GetAllItems<Bank>(nameof(Banks));
            IEnumerable<Ticket> Tickets = GetAllItems<Ticket>(nameof(Tickets));
            Banks.ElementAt(0).BankValue = FU_item.FU_Value_C;
            int result = EditItemID<Bank>(nameof(Bank), Banks.ElementAt(0));
            Banks.ElementAt(1).BankValue = FU_item.FU_Value_B1;
            result = EditItemID<Bank>(nameof(Bank), Banks.ElementAt(1));
            Banks.ElementAt(2).BankValue = FU_item.FU_Value_B2;
            result = EditItemID<Bank>(nameof(Bank), Banks.ElementAt(2));
            Banks.ElementAt(3).BankValue = FU_item.FU_Value_B3;
            result = EditItemID<Bank>(nameof(Bank), Banks.ElementAt(3));
            Banks.ElementAt(4).BankValue = FU_item.FU_Value_B4;
            result = EditItemID<Bank>(nameof(Bank), Banks.ElementAt(4));
            Tickets.ElementAt(0).TicketValue = FU_item.FU_Value_T;
            result = EditItemID<Ticket>(nameof(Ticket), Tickets.ElementAt(0));
            return RedirectToAction(nameof(Index));
        }
    }

}





