using Microsoft.AspNet.Identity;
using Microsoft.Owin.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Security.Policy;
using System.Security.Principal;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Xml.Linq;
using VNTravel.Models;
using static VNTravel.Controllers.AccountController;

namespace VNTravel.Controllers
{
    public class AccountGoogleController : Controller
    {
        // GET: AccountGoogle
        webtraveldbEntities db = new webtraveldbEntities();

        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult ExternalLogin()
        {
            return new ChallengeResult("Google", Url.Action("ExternalLoginCallback", "AccountGoogle"));
        }


        [AllowAnonymous]
        public async Task<ActionResult> ExternalLoginCallback()
        {

            var loginInfo = await HttpContext.GetOwinContext().Authentication.GetExternalLoginInfoAsync();
            if (loginInfo == null)
            {
                return RedirectToAction("Index", "Home");
            }
            var email = loginInfo.ExternalIdentity.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
            var account = db.Accounts.Where(s => s.Username.Equals(loginInfo.Email)).FirstOrDefault();
            if (account != null)
            {
                Session["UserName"] = account.Username;
                return RedirectToAction("Index", "Home");
            }

            Account ac = new Account();
            ac.Username = email;
            ac.account_role = 0;
            db.Accounts.Add(ac);

            Customer customer = new Customer();
            customer.Customer_id = int.Parse(RandomID.rdID_Customer());
            customer.Customer_email = email;
            customer.Username = ac.Username;
            db.Customers.Add(customer);

            db.SaveChanges();
            Session["UserName"] = ac.Username;
            return RedirectToAction("Index", "Home");

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult LogOff()
        {
            Session.Clear();
            return RedirectToAction("Index", "Home");
        }
    }
}