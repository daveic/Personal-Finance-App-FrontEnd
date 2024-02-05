using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using PersonalFinanceFrontEnd.Models;

namespace PersonalFinanceFrontEnd.Controllers
{
    public partial class PersonalFinanceController
    {
        public ActionResult Transactions(string orderBy, string selectedType, string selectedCode, string selectedBank, string selectedYear, string selectedMonth, int page = 0)
        {
            string User_OID = GetUserData().Result; //Fetch User Data
            //GET
            string path = "?User_OID=" + User_OID;
            Transactions TrsAPI = new();
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri("https://personalfinanceappapi.azurewebsites.net/api/Transactions/Main");
                var responseTask = client.GetAsync(path);
                responseTask.Wait();
                var result = responseTask.Result;
                var readTask = result.Content.ReadAsAsync<Transactions>();
                readTask.Wait();
                TrsAPI = readTask.Result;
            }

            ////Passo alla view la lista
            ViewBag.ItemList = TrsAPI.ItemListYear;
            ////Se al caricamento della pagina ho selezionato un anno (not empty), salvo in Balances i saldi di quell'anno
            if (!String.IsNullOrEmpty(selectedYear)) TrsAPI.Trs = TrsAPI.Trs.AsQueryable().Where(x => x.TrsDateTime.Year.ToString() == selectedYear);
            ////############################################################################################################################
            
            ViewBag.ItemListMonth = TrsAPI.ItemListMonth;
            ////Se al caricamento della pagina ho selezionato un mese (not empty), salvo in Balances i saldi di quel mese
            if (!String.IsNullOrEmpty(selectedMonth)) TrsAPI.Trs = TrsAPI.Trs.AsQueryable().Where(x => MonthConverter(x.TrsDateTime.Month) == selectedMonth);
            ////############################################################################################################################

            List<SelectListItem> types = new()
            {
                new SelectListItem() { Text = "Entrate", Value = "Entrate" },
                new SelectListItem() { Text = "Uscite", Value = "Uscite" }
            };
            ViewBag.Type = types;

            if (!String.IsNullOrEmpty(selectedCode)) TrsAPI.Trs = TrsAPI.Trs.AsQueryable().Where(x => x.TrsCode == selectedCode);
            if (!String.IsNullOrEmpty(selectedBank)) TrsAPI.Trs = TrsAPI.Trs.AsQueryable().Where(x => x.TrsBank == selectedBank);
            if (!String.IsNullOrEmpty(selectedType))
            {
                if (selectedType == "Entrate") TrsAPI.Trs = TrsAPI.Trs.AsQueryable().Where(x => x.TrsValue >= 0);
                else if (selectedType == "Uscite") TrsAPI.Trs = TrsAPI.Trs.AsQueryable().Where(x => x.TrsValue < 0);
            }
            //############################################################################################################################
            //############################################################################################################################

            TrsAPI.Trs = TrsAPI.Trs.Reverse();
            //## ORDINAMENTO #############################################################################################################
            List<SelectListItem> orderByList = new();
            SelectListItem datetimeAsc = new() { Text = "Data crescente", Value = "Data crescente" };
            SelectListItem datetimeDesc = new() { Text = "Data decrescente", Value = "Data decrescente" };
            SelectListItem categ = new() { Text = "Categoria", Value = "Categoria" };
            SelectListItem conto = new() { Text = "Conto", Value = "Conto" };
            SelectListItem type = new() { Text = "Entrate/Uscite", Value = "Entrate/Uscite" };
            orderByList.Add(datetimeAsc);
            orderByList.Add(datetimeDesc);
            orderByList.Add(categ);
            orderByList.Add(conto);
            orderByList.Add(type);

            ViewBag.OrderBy = orderByList;
            if (!String.IsNullOrEmpty(orderBy))
            {
                if (orderBy == "Data crescente") TrsAPI.Trs = TrsAPI.Trs.OrderBy(x => x.TrsDateTime);
                else if (orderBy == "Data decrescente") TrsAPI.Trs = TrsAPI.Trs.OrderByDescending(x => x.TrsDateTime);
                else if (orderBy == "Categoria") TrsAPI.Trs = TrsAPI.Trs.OrderBy(x => x.TrsCode);
                else if (orderBy == "Conto") TrsAPI.Trs = TrsAPI.Trs.OrderBy(x => x.TrsBank);
                else if (orderBy == "Entrate/Uscite") TrsAPI.Trs = TrsAPI.Trs.OrderByDescending(x => x.TrsValue);
            }
            List<string> LastChoices = new()
            {
                orderBy,
                selectedType,
                selectedCode,
                selectedBank,
                selectedMonth,
                selectedYear
            };

            //Pagination
            ViewBag.LastChoices = LastChoices;
            const int PageSize = 20;
            var count = TrsAPI.Trs.Count();
            var data = TrsAPI.Trs.Skip(page * PageSize).Take(PageSize).ToList();
            this.ViewBag.MaxPage = (count / PageSize) - (count % PageSize == 0 ? 1 : 0);
            this.ViewBag.Page = page;

            TrsToView TrsToView = new()
            {
                Transactions = data
            };
            ViewBag.state = (int)(TempData.ContainsKey("sendFlagTr") ? TempData["sendFlagTr"] : 0);

            TempData["Codes"] = TrsAPI.Codes;
            TempData["Banks"] = TrsAPI.BankList;
            TrsToView.Transaction = new Transaction();
            TransactionDetailsEdit detection = new();
            path = "api/Transactions/DetailsEdit?User_OID=" + GetUserData().Result;
            using (HttpClient client = new())
            {
                client.BaseAddress = new Uri("https://personalfinanceappapi.azurewebsites.net/");
                client.GetAsync(path).Wait();
                client.GetAsync(path).Result.Content.ReadAsAsync<TransactionDetailsEdit>().Wait();
                detection = client.GetAsync(path).Result.Content.ReadAsAsync<TransactionDetailsEdit>().Result;
            }
            //Recupero la lista di transazioni di questo mese, così da poter filtrare i debiti/crediti già pagati/riscossi
            IEnumerable<Transaction> AllTransactions = GetAllItems<Transaction>("Transactions", User_OID);
            IEnumerable<Transaction> monthTransactions = AllTransactions.OrderBy(x => x.TrsDateTime).Where(x => x.TrsDateTime.Month == DateTime.Now.Month);
            TransactionDetailsEdit detectionToShow = new();
            detectionToShow = detection;
            foreach (var debRat in detection.DebitsRat.ToList())
            {
                foreach (var tr in monthTransactions)
                {
                    if (tr.TrsCode == debRat.DebCode)
                    {
                        detectionToShow.DebitsRat.Remove(debRat);
                    }
                }
            }

            ViewBag.DebitListRat = detectionToShow.DebitsRat;
            ViewBag.DebitList = detection.DebitsMono;
            ViewBag.CreditList = detection.CreditsMono;
            DateTime Now = DateTime.Now;
            
            var MonthTransactions = AllTransactions.Where(y => y.TrsDateTime.Month == Now.Month).Intersect(AllTransactions.Where(x => x.TrsDateTime.Year == Now.Year));

            ViewBag.MonthExpirationsOnExp = detection.MonthExpirationsOnExp;

            var monthExpNotDone = detection.MonthExpirationsOnExp.Where(p => !MonthTransactions.Any(p2 => p2.TrsCode == p.ExpTitle));
            ViewBag.MonthExpirations = monthExpNotDone;
            return View(TrsToView);
        }



        public ActionResult Transaction_Details(int id)
        {
            return PartialView(GetItemID<Transaction>("Transactions", id, GetUserData().Result));
        }


        public IActionResult Transaction_Add()
        {
            return View(new Transaction());
        }
        [HttpPost]
        public ActionResult Transaction_Add(Transaction t)
        {
            t.Usr_OID = GetUserData().Result;
            IEnumerable<Transaction> AllTransactions = GetAllItems<Transaction>("Transactions", t.Usr_OID);
            IEnumerable<Transaction> transactions = AllTransactions.OrderBy(x => x.TrsDateTime).Where(x => x.TrsDateTime.Month == DateTime.Now.Month);

            if (t.TrsTitle != null && t.DebCredChoice!= "NDeb" && t.DebCredChoice != "NCre")
            {                
                if (t.TrsTitle.StartsWith("DEB") || t.TrsTitle.StartsWith("CRE") || t.TrsTitle.StartsWith("MVF") || t.TrsTitle.StartsWith("SCD"))
                {
                    _notyf.Error("Il titolo della transazione non può iniziare per DEB, CRE, MVF o SCD. Inserimento annullato.");                    
                    return RedirectToAction(nameof(Index));
                }
                if (t.TrsCode is null && t.NewTrsCode is null)
                {
                    _notyf.Error("Occorre specificare una categoria. Transazione annullata.");                    
                    return RedirectToAction(nameof(Index));
                }    
                  
            }
            if(t.DebCredChoice != null)
            {
                if (t.DebCredChoice.StartsWith("DEB"))
                {
                    var Debits = GetAllItems<Debit>("Debits", t.Usr_OID);
                    bool isDebRat = false;
                    foreach (var debit in Debits)
                    {
                        if (t.DebCredChoice == debit.DebCode)
                        {
                            if (debit.RtNum > 1) isDebRat = true;
                            if (t.DebCredInValue > Math.Round(debit.RemainToPay, 2))
                            {
                                _notyf.Error("Importo inserito maggiore del valore del credito. Transazione annullata.");                                
                                return RedirectToAction(nameof(Index));
                            }
                        }
                    }
                    foreach (var tr in transactions)
                    {
                        if(t.DebCredChoice == tr.TrsCode && tr.TrsValue < 0 && isDebRat == true)
                        {
                            _notyf.Error("La rata mensile di questo debito è già stata pagata. Transazione annullata.");
                            return RedirectToAction(nameof(Index));
                        }
                    }

                } else if (t.DebCredChoice.StartsWith("CRE")) 
                {
                    var Credits = GetAllItems<Credit>("Credits", t.Usr_OID);
                    foreach (var credit in Credits)
                    {
                        if (t.DebCredChoice == credit.CredCode)
                        {
                            if (t.DebCredInValue > Math.Round(credit.CredValue, 2)) 
                            {
                                _notyf.Error("Importo inserito maggiore del valore del debito. Transazione annullata.");                                
                                return RedirectToAction(nameof(Index));
                            }
                        }
                    }
                }  else if (t.DebCredChoice.StartsWith("SCD") || t.DebCredChoice.StartsWith("MVF"))
                {
                    t.TrsCode = t.DebCredChoice;
                    using var client = new HttpClient();
                    client.BaseAddress = new Uri("https://personalfinanceappapi.azurewebsites.net/api/Transactions/");
                    var postTask = client.PostAsJsonAsync<Transaction>("Add", t);
                    postTask.Wait();
                    var result = postTask.Result;
                    if (result.IsSuccessStatusCode)
                    {
                        BankUpdater(t.TrsBank, t.TrsValue);
                        _notyf.Success("Transazione inserita correttamente.");
                        return RedirectToAction(nameof(Index));
                    }
                    else
                    {
                        _notyf.Error("Errore API: T1 - NoSuccess. Verifica che il movimento sia replicato nelle scadenze");
                        return RedirectToAction(nameof(Expirations));
                    }
                }
            }
            
            if (t.Input_value != null)
            {
                t.Input_value = t.Input_value.Replace(",", ".");
                t.TrsValue = Convert.ToDouble(t.Input_value);
            }
            if (t.Type == false) t.TrsValue = -t.TrsValue;
            if (t.NewTrsCode != null) t.TrsCode = t.NewTrsCode;
            t.DebCredChoice ??= "";
            t.TrsCode ??= "";
            t.TrsTitle ??= "";
            t.TrsDateTimeExp ??= DateTime.MinValue;
            if (t.TrsDateTime == DateTime.MinValue) t.TrsDateTime = DateTime.Now;
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri("https://personalfinanceappapi.azurewebsites.net/api/Transactions/");
                var postTask = client.PostAsJsonAsync<Transaction>("Add", t);
                postTask.Wait();
                var result = postTask.Result;
                if (result.IsSuccessStatusCode)
                {
                    BankUpdater(t.TrsBank, t.TrsValue);
                    _notyf.Success("Transazione inserita correttamente.");
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    _notyf.Error("Errore API: T2 - NoSuccess.");                    
                }
            }
            return RedirectToAction(nameof(Index));
        }
       
        public ActionResult Transaction_Delete(int id)
        {
            return Transaction_Details(id);
        }
        [HttpPost]
        public ActionResult Transaction_Delete(Transaction t)
        {
            int result = DeleteItemN("Transactions", t.ID, GetUserData().Result);
            if (result == 0)
            {
                _notyf.Warning("Transazione rimossa correttamente.");
                BalanceUpdate(t.Usr_OID, 0);
                return RedirectToAction(nameof(Transactions));
            }
            else
            {
                _notyf.Error("Errore API: T3 - NoSuccess.");                
            }
            return RedirectToAction(nameof(Transactions));
        }

        public bool BankUpdater(string BankName, double value)
        {
            IEnumerable<Bank> Banks = GetAllItems<Bank>("Banks", GetUserData().Result);
            foreach (Bank b in Banks)
            {
                if (b.BankName == BankName)
                {
                    b.BankValue += value;
                    int result = EditItemIDN<Bank>("Banks", b);
                    if (result == 0)
                    {
                        BalanceUpdate(b.Usr_OID, 0);
                        return true;
                    }
                }
            }
            return false;
        }
    }
}
