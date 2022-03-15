using System;
using System.Collections.Generic;
using System.IO;
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
using PersonalFinanceFrontEnd.Models;

namespace PersonalFinanceFrontEnd.Controllers
{
    public partial class PersonalFinanceController
    {
        public ActionResult Transaction_Details(int id)
        {
            return PartialView(GetItemIDN<Transaction>("Transactions", id, GetUserData().Result));
        }
        public ActionResult Transaction_Delete(int id)
        {
            return Transaction_Details(id);
        }
    }
}
