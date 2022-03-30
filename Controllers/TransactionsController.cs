using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using PersonalFinanceFrontEnd.Models;

namespace PersonalFinanceFrontEnd.Controllers
{
    public partial class PersonalFinanceController
    {
        public ActionResult Transactions(string orderBy, string selectedType, string selectedCode, string selectedYear, string selectedMonth, int page = 0)
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
            //IEnumerable<Transaction> Transactions = GetAllItemsN<Transaction>("Transactions", User_OID);
            //IEnumerable<Credit> Credits = GetAllItemsN<Credit>("Credit", User_OID);
            //IEnumerable<Debit> Debits = GetAllItemsN<Debit>("Debit", User_OID);
            //ViewModel viewModel = new ViewModel();

            ////############################################################################################################################
            ////FILTRI ANNO E MESE PER GRAFICO SALDO
            ////############################################################################################################################
            ////Trovo gli anni "unici"
            //var UniqueYear = Transactions.GroupBy(item => item.TrsDateTime.Year)
            //        .Select(group => group.First())
            //        .Select(item => item.TrsDateTime.Year)
            //        .ToList();
            ////Creo la lista di anni "unici" per il dropdown filter del grafico saldo
            //List<SelectListItem> itemlistYear = new List<SelectListItem>();
            //foreach (var year in UniqueYear) itemlistYear.Add(new SelectListItem() { Text = year.ToString(), Value = year.ToString() });
            ////Passo alla view la lista
            ViewBag.ItemList = TrsAPI.ItemListYear;
            ////Se al caricamento della pagina ho selezionato un anno (not empty), salvo in Balances i saldi di quell'anno
            if (!String.IsNullOrEmpty(selectedYear)) TrsAPI.Trs = TrsAPI.Trs.AsQueryable().Where(x => x.TrsDateTime.Year.ToString() == selectedYear);
            ////############################################################################################################################
            ////Trovo i mesi "unici"
            //var UniqueMonth = Transactions.GroupBy(item => item.TrsDateTime.Month)
            //                    .Select(group => group.First())
            //                    .Select(item => item.TrsDateTime.Month)
            //                    .ToList();
            ////Creo la lista di mesi "unici" per il dropdown filter del grafico saldo
            //List<SelectListItem> itemlistMonth = new List<SelectListItem>();
            //foreach (var month in UniqueMonth) itemlistMonth.Add(new SelectListItem() { Text = MonthConverter(month), Value = MonthConverter(month) });
            ////Passo alla view la lista
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
            SelectListItem type = new() { Text = "Entrate/Uscite", Value = "Entrate/Uscite" };
            orderByList.Add(datetimeAsc);
            orderByList.Add(datetimeDesc);
            orderByList.Add(categ);
            orderByList.Add(type);

            ViewBag.OrderBy = orderByList;
            if (!String.IsNullOrEmpty(orderBy))
            {
                if (orderBy == "Data crescente") TrsAPI.Trs = TrsAPI.Trs.OrderBy(x => x.TrsDateTime);
                else if (orderBy == "Data decrescente") TrsAPI.Trs = TrsAPI.Trs.OrderByDescending(x => x.TrsDateTime);
                else if (orderBy == "Categoria") TrsAPI.Trs = TrsAPI.Trs.OrderBy(x => x.TrsCode);
                else if (orderBy == "Entrate/Uscite") TrsAPI.Trs = TrsAPI.Trs.OrderByDescending(x => x.TrsValue);
            }
            List<string> LastChoices = new()
            {
                orderBy,
                selectedType,
                selectedCode,
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

            TrsToView TrsToView = new();
            TrsToView.Transactions = data;
            ViewBag.state = (int)(TempData.ContainsKey("sendFlagTr") ? TempData["sendFlagTr"] : 0);

            //var UniqueCodes = Transactions.GroupBy(x => x.TrsCode)
            //                  .Select(x => x.First())
            //                  .ToList();
            //List<SelectListItem> Codes = new();
            //foreach (var item in UniqueCodes)
            //{
            //    SelectListItem code = new SelectListItem();
            //    code.Value = item.TrsCode;
            //    code.Text = item.TrsCode;
            //    Codes.Add(code);
            //}
            //bool isPresent = false;
            //foreach (var credit in Credits)
            //{
            //    foreach (var item in Codes)
            //    {
            //        if (credit.CredCode == item.Value) isPresent = true;
            //    }
            //    if (isPresent is false) Codes.Add(new SelectListItem() { Text = credit.CredCode, Value = credit.CredCode });
            //    isPresent = false;
            //}
            //foreach (var debit in Debits)
            //{
            //    foreach (var item in Codes)
            //    {
            //        if (debit.DebCode == item.Value) isPresent = true;
            //    }
            //    if (isPresent is false) Codes.Add(new SelectListItem() { Text = debit.DebCode, Value = debit.DebCode });
            //    isPresent = false;
            //}
            TempData["Codes"] = TrsAPI.Codes;
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
            ViewBag.DebitListRat = detection.DebitsRat;
            ViewBag.DebitList = detection.DebitsMono;
            ViewBag.CreditList = detection.CreditsMono;
            return View(TrsToView);
        }



        public ActionResult Transaction_Details(int id)
        {
            return PartialView(GetItemIDN<Transaction>("Transactions", id, GetUserData().Result));
        }


        public IActionResult Transaction_Add()
        {
            return View(new Transaction());
        }
        [HttpPost]
        public ActionResult Transaction_Add(Transaction t)
        {
            if (t.Input_value != null)
            {
                t.Input_value = t.Input_value.Replace(".", ",");
                t.TrsValue = Convert.ToDouble(t.Input_value);
            }
            if (t.Type == false) t.TrsValue = -t.TrsValue;
            if (t.NewTrsCode != null) t.TrsCode = t.NewTrsCode;
            if (t.DebCredChoice is null) t.DebCredChoice = "";
            if (t.TrsCode is null) t.TrsCode = "";
            if (t.TrsTitle is null) t.TrsTitle = "";
            if (t.TrsDateTimeExp is null) t.TrsDateTimeExp = DateTime.MinValue;
            if (t.TrsDateTime == DateTime.MinValue) t.TrsDateTime = DateTime.Now;
            t.Usr_OID = GetUserData().Result;
            //if cred or deb code is existing, throw error - 
            //if debcredinput is > remain to pay o creditvalue, throw error
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri("https://personalfinanceappapi.azurewebsites.net/api/Transactions/");
                var postTask = client.PostAsJsonAsync<Transaction>("Add", t);
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
        public int Transaction_Balance_Update(string User_OID)
        {
            Balance b = new()
            {
                Usr_OID = User_OID,
                BalDateTime = DateTime.UtcNow
            };
            IEnumerable<Transaction> Transactions = GetAllItemsN<Transaction>("Transactions", User_OID);

            double totTransaction = 0;
            foreach (var item in Transactions)
            {
                totTransaction += item.TrsValue;
            }
            b.ActBalance = totTransaction;
            AddItemN<Balance>("Balances", b);
            return 1;
        }
        //public ActionResult Transaction_Edit(int id)
        //{
        //    return Transaction_Details_Edit(id, GetUserData().Result);
        //}
        //public ActionResult Transaction_Details_Edit(int id, string User_OID)
        //{
        //    Transaction t = GetItemIDN<Transaction>("Transactions", id, User_OID);

        //    if (t.TrsValue < 0) t.Type = false; else t.Type = true;
        //    t.Input_value = t.TrsValue.ToString();
        //    return PartialView(t);
        //}

        //[HttpPost]
        //public ActionResult Transaction_Edit(Transaction t)
        //{
        //    t.Input_value = t.Input_value.Replace(".", ",");
        //    t.TrsValue = Convert.ToDouble(t.Input_value);
        //    if (t.Type == false) t.TrsValue = -Math.Abs(t.TrsValue);
        //    if (t.Type == true) t.TrsValue = Math.Abs(t.TrsValue);
        //    if (t.NewTrsCode != null) t.TrsCode = t.NewTrsCode;
        //    t.Usr_OID = GetUserData().Result;
        //    int result = EditItemIDN<Transaction>("Transactions", t);
        //    if (result == 0)
        //    {
        //        TempData["sendFlagTr"] = 2;
        //        Transaction_Balance_Update(t.Usr_OID);
        //        return RedirectToAction(nameof(Transactions));
        //    }
        //    return View();
        //}

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
                TempData["sendFlagTr"] = 1;
                Transaction_Balance_Update(GetUserData().Result);
                return RedirectToAction(nameof(Transactions));
            }
            return View();
        }
    }
}
