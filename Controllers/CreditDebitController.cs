using Microsoft.AspNetCore.Mvc;
using PersonalFinanceFrontEnd.Models;
using Microsoft.Identity.Web;

namespace PersonalFinanceFrontEnd.Controllers
{
    public partial class PersonalFinanceController
    {
        //CREDITS DEBITS Intermediate view
        [AuthorizeForScopes(ScopeKeySection = "DownstreamApi:Scopes")]
        public ActionResult Credits_Debits()
        {
            string User_OID = GetUserData().Result; //Fetch User Data
            ViewModel viewModel = new();
            viewModel.Credits = GetAllItems<Credit>("Credits", User_OID);
            viewModel.Debits = GetAllItems<Debit>("Debits", User_OID);
            return View(viewModel);
        }
    }
}
