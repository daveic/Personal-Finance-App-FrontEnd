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

            TrsToView TrsToView = new()
            {
                Transactions = data
            };
            ViewBag.state = (int)(TempData.ContainsKey("sendFlagTr") ? TempData["sendFlagTr"] : 0);

            
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
            if (t.TrsTitle != null)
            {
                if (t.TrsTitle.StartsWith("DEB") || t.TrsTitle.StartsWith("CRE"))
                {
                    TempData["sendFlagTr"] = 5;
                    return RedirectToAction(nameof(Transactions));
                }
            }
            if(t.DebCredChoice != null)
            {
                if (t.DebCredChoice.StartsWith("DEB"))
                {
                    var Debits = GetAllItems<Debit>("Debits", t.Usr_OID);
                    foreach (var debit in Debits)
                    {
                        if (t.DebCredChoice == debit.DebCode)
                        {
                           // (item.DebValue / item.RtNum)
                            if (t.DebCredInValue > debit.RemainToPay)
                            {
                                TempData["sendFlagTr"] = 4;
                                return RedirectToAction(nameof(Transactions));
                            }
                        }
                    }
                } else if (t.DebCredChoice.StartsWith("CRE")) 
                {
                    var Credits = GetAllItems<Credit>("Credits", t.Usr_OID);
                    foreach (var credit in Credits)
                    {
                        if (t.DebCredChoice == credit.CredCode)
                        {
                            if (t.DebCredInValue > credit.CredValue) 
                            {
                                TempData["sendFlagTr"] = 2;
                                return RedirectToAction(nameof(Transactions));
                            }
                        }
                    }
                }
            }
             else
            {
                if (t.Input_value != null)
                {
                    t.Input_value = t.Input_value.Replace(".", ",");
                    t.TrsValue = Convert.ToDouble(t.Input_value);
                }
                if (t.Type == false) t.TrsValue = -t.TrsValue;
                if (t.NewTrsCode != null) t.TrsCode = t.NewTrsCode;
                t.DebCredChoice ??= "";
                t.TrsCode ??= "";
                t.TrsTitle ??= "";
                t.TrsDateTimeExp ??= DateTime.MinValue;
                if (t.TrsDateTime == DateTime.MinValue) t.TrsDateTime = DateTime.Now;
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
                        BalanceUpdate(t.Usr_OID, false);
                        TempData["sendFlagTr"] = 3;
                        return RedirectToAction(nameof(Transactions));
                    }
                }
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
            int result = DeleteItemN("Transactions", t.ID, GetUserData().Result);
            if (result == 0)
            {
                TempData["sendFlagTr"] = 1;
                BalanceUpdate(t.Usr_OID, false);
                return RedirectToAction(nameof(Transactions));
            }
            return View();
        }
    }
}
