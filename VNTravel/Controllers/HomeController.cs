using System;
using System.Collections.Generic;
using System.EnterpriseServices.CompensatingResourceManager;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using VNTravel.Models;

namespace VNTravel.Controllers
{
    public class HomeController : Controller
    {
        webtraveldbEntities db = new webtraveldbEntities();

        public ActionResult Index()
       {
            var currentDate = DateTime.Now;
            var validVouchers = db.Vouchers
                .Where(v => v.BeginDate <= currentDate && v.ExpiredDate >= currentDate)
                .FirstOrDefault();
            
            if (validVouchers != null)
            {
                if(validVouchers.Quantily > 0)
                {
                    DateTime begin = ((DateTime)validVouchers.BeginDate).Date;
                    DateTime expired = ((DateTime)validVouchers.ExpiredDate).Date;
                    string formattedStartDate = begin.ToString("dd-MM-yyyy");
                    string formattedEndDate = expired.ToString("dd-MM-yyyy");
                    ViewBag.Begin = formattedStartDate;
                    ViewBag.End = formattedEndDate;
                    Session["Voucher"] = validVouchers;
                    ViewBag.Percent = validVouchers.Voucher_percent;
                    ViewBag.VoucherName = validVouchers.Voucher_name;
                }
            }
            return View();
        }

        [HttpPost]
        public ActionResult Login(string username, string password)
        {
            string returnUrl = Request.UrlReferrer?.ToString();
            var account = db.Accounts.Where(a => a.Username == username && a.pass == password).FirstOrDefault();
            if(account == null)
            {
                Session["login"] = "LoginFail";
                return RedirectToAction("Index");
            }
            //if (Request.Form["save-info-checkbox"] == "on")
            //{
            //    // Sử dụng Cookie để lưu trạng thái đăng nhập
            //    HttpCookie loginCookie = new HttpCookie("LoginCookie");
            //    loginCookie["Username"] = username;
            //    loginCookie.Expires = DateTime.Now.AddMonths(1);
            //    Response.Cookies.Add(loginCookie);
            //    Session["UserName"] = account.Username;
            //    return RedirectToAction("Index");
            //}
            else
            {
                if(account.account_role == 0) //0 là kh
                {
                    Session["UserName"] = account.Username;
                    return Redirect(returnUrl);
                }
                else if(account.account_role == 1) //1 là đối tác
                {
                    var accompany = db.Accompanies.Where(s => s.Username == account.Username).FirstOrDefault();
                    Session["Partner_Username"] = accompany.Accompany_name;
                    Session["Partner_Email"] = accompany.Accompany_email;
                    Session["Partner"] = account.Username;
                    Session["Accompany_ID"] = accompany.Accompany_ID;
                    if (accompany.Accompany_typeid == 1)
                    {
                        Session["Hotel"] = "Hotel";
                    }
                    else
                    {
                        Session["Fight"] = "Flight";
                    }
                    return RedirectToAction("Index","Admin");
                }
                else //Admin
                {
                    Session["Admin"] = account.Username;
                    return RedirectToAction("Index", "Admin");
                }

            }
        }

        [HttpPost]
        public ActionResult Register(string username, string password, string repassword, string name, string email, string phone)
        {
            var account = db.Accounts.Where(a => a.Username == username).FirstOrDefault();
            if(account != null)
            {
                Session["register"] = "RegisterFail";
                return RedirectToAction("Index");
            }
            else
            {
                if (password == repassword)
                {
                    Account ac = new Account();
                    ac.Username = username;
                    ac.pass = password;
                    ac.account_role = 0; //0 la khach hang
                    db.Accounts.Add(ac);

                    Customer customer = new Customer();
                    customer.Customer_id = int.Parse(RandomID.rdID_Customer());
                    customer.Customer_name = name;
                    customer.Customer_email = email;
                    customer.Customer_phone = phone;
                    customer.Username = ac.Username;

                    db.Customers.Add(customer);
                    db.SaveChanges();
                    Session["UserName"] = ac.Username;
                    return RedirectToAction("Index");
                }
                Session["registerPass"] = "RegisterPassFail";
                return RedirectToAction("Index");
            }
        }

        
        public ActionResult ViewInfo()
        {
            string username = Session["UserName"].ToString();
            var account = db.Accounts.Where(a => a.Username == username).FirstOrDefault();
            var cus = db.Customers.Where(c => c.Username == username).FirstOrDefault();
            ViewBag.FullName = cus.Customer_name;
            ViewBag.Email = cus.Customer_email;
            ViewBag.Phone = cus.Customer_phone;
            TempData.Clear();
            return View(account);
        }

        public ActionResult EditInfo()
        {
            string username = Session["UserName"].ToString();
            var account = db.Accounts.Where(a => a.Username == username).FirstOrDefault();
            var cus = db.Customers.Where(c => c.Username == username).FirstOrDefault();
            ViewBag.Username = username;
            return View(cus);
        }

        [HttpPost]
        public ActionResult EditInfo(Customer cus)
        {
            db.Entry(cus).State = System.Data.Entity.EntityState.Modified;
            db.SaveChanges();
            return RedirectToAction("ViewInfo");
        }

        public ActionResult EditAccount()
        {
            string username = Session["UserName"].ToString();
            var account = db.Accounts.Where(a => a.Username == username).FirstOrDefault();
            var cus = db.Customers.Where(c => c.Username == username).FirstOrDefault();
            ViewBag.FullName = cus.Customer_name;
            ViewBag.Email = cus.Customer_email;
            ViewBag.Phone = cus.Customer_phone;
            return View(account);
        }

        [HttpPost]
        public ActionResult EditAccount(Account acc)
        {
            db.Entry(acc).State = System.Data.Entity.EntityState.Modified;
            db.SaveChanges();
            Session["announce"] = "Success";
            return RedirectToAction("ViewInfo");
        }

        public ActionResult History()
        {
            TempData.Clear();
            string username = Session["UserName"].ToString();
            var account = db.Accounts.Where(a => a.Username == username).FirstOrDefault();
            var cus = db.Customers.Where(c => c.Username == account.Username).FirstOrDefault();
            var distinctDetailRoomBills = db.DetailRoomBills
                .Where(detail => detail.Customer_id == cus.Customer_id)
                .GroupBy(detail => detail.Bill_id)
                .Select(group => group.FirstOrDefault())
                .ToList();
            var distinctDetailTicketBills = db.DetailTicketBills
               .Where(detail => detail.Customer_id == cus.Customer_id)
               .GroupBy(detail => detail.Bill_id)
               .Select(group => group.FirstOrDefault())
               .ToList();
            List<Bill> bills = new List<Bill>();
            foreach (var detail in distinctDetailRoomBills)
            {
                var bill = db.Bills.Where(s => s.Bill_id == detail.Bill_id).FirstOrDefault();
                bills.Add(bill);
            }
            foreach (var detail in distinctDetailTicketBills)
            {
                var bill = db.Bills.Where(s => s.Bill_id == detail.Bill_id).FirstOrDefault();
                bills.Add(bill);
            }
            return View(bills.ToList());
        }

        [HttpGet]
        public ActionResult CancelBill(int id)
        {
            Bill bill = db.Bills.Where(s => s.Bill_id == id).FirstOrDefault();
            bill.Status = "Đã hủy";
            db.Entry(bill).State = System.Data.Entity.EntityState.Modified;
            db.SaveChanges();
            return RedirectToAction("History");
        }


        [HttpGet]
        public ActionResult SearchHotel()
        {
            List<Room> rooms = Session["Rooms"] as List<Room>;
            return View(rooms.ToList());
        }

        [HttpPost]
        public ActionResult SearchHotel(string location)
        {
            List<Room> rooms = db.Rooms
            .Where(room => string.IsNullOrEmpty(location) || room.Location.Contains(location))
            .ToList();
            Session["Rooms"] = rooms;
            return RedirectToAction("SearchHotel");
        }

        [HttpGet]
        public ActionResult SearchFlight()
        {
            List<Ticket> tickets = Session["Tickets"] as List<Ticket>;
            return View(tickets.ToList());
        }

        [HttpPost]
        public ActionResult SearchFlight(string departure, string arrival, string checkin, string checkout)
        {
            DateTime? checkinDate = null;
            DateTime? checkoutDate = null;

            if (!string.IsNullOrEmpty(checkin))
            {
                checkinDate = DateTime.Parse(checkin);
            }

            if (!string.IsNullOrEmpty(checkout))
            {
                checkoutDate = DateTime.Parse(checkout);
            }

            List<Ticket> tickets = db.Tickets
                .Where(ticket =>
                    (string.IsNullOrEmpty(departure) || ticket.Departure.Contains(departure)) &&
                    (string.IsNullOrEmpty(arrival) || ticket.destination.Contains(arrival)) &&
                    (!checkinDate.HasValue || ticket.BeginDate >= checkinDate.Value) &&
                    (!checkoutDate.HasValue || ticket.EndDate <= checkoutDate.Value)
                )
                .ToList();

            Session["Tickets"] = tickets;
            return RedirectToAction("SearchFlight");
        }
        public ActionResult Favourite()
        {
            string username = Session["UserName"].ToString();
            var account = db.ListFavourites.Where(a => a.Username == username).ToList();
            return View(account);  
        }
        public ActionResult _Fav(int ID)
        {
            /*List<string> errors = new List<string>();*/ // You might want to return an error if something wrong happened
            var fav = db.ListFavourites.Find(ID);
            fav.status_fav = true;
            //Do DB Processing   
            db.SaveChanges();
            return RedirectToAction("Favourite", "Home");
        }
        public ActionResult _UnFav(int ID)
        {
            /*List<string> errors = new List<string>();*/ // You might want to return an error if something wrong happened
            var fav = db.ListFavourites.Find(ID);
            fav.status_fav = false;
            //Do DB Processing   
            db.SaveChanges();
            return RedirectToAction("Favourite", "Home");
        }
    }
}