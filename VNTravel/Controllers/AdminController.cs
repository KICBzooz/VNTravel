using PagedList;
using PayPal;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Entity;
using System.Drawing;
using System.EnterpriseServices.CompensatingResourceManager;
using System.Linq;
using System.Web;
using System.Web.Http.Results;
using System.Web.Mvc;
using System.Web.UI;
using VNTravel.Models;
using Account = VNTravel.Models.Account;

namespace VNTravel.Controllers
{
    public class AdminController : Controller
    {
        // GET: Admin
        webtraveldbEntities db = new webtraveldbEntities();
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Logout()
        {
            Session.Clear();
            return RedirectToAction("Index", "Home");
        }

        public ActionResult ListVoucher()
        {
            return View(db.Vouchers.ToList());
        }



        [HttpGet]
        public ActionResult CreateVoucher() { return View(); }

        [HttpPost]
        public ActionResult CreateVoucher(Voucher voucher)
        {
            voucher.ID_voucher = int.Parse(RandomID.rdID_Voucher());
            db.Vouchers.Add(voucher);
            db.SaveChanges();

            
            return RedirectToAction("Index");
        }

        [HttpGet]
        public ActionResult EditVoucher(int id)
        {
            return View(db.Vouchers.Where(s => s.ID_voucher == id).FirstOrDefault());
            
        }

        [HttpPost]
        public ActionResult EditVoucher(Voucher voucher)
        {
            db.Entry(voucher).State = System.Data.Entity.EntityState.Modified;
            db.SaveChanges();
            return RedirectToAction("ListVoucher");
        }

        [HttpGet]
        public ActionResult DetailVoucher(int id)
        {
            return View(db.Vouchers.Where(s => s.ID_voucher == id).FirstOrDefault());

        }

        [HttpGet]
        public ActionResult DeleteVoucher(int id)
        {
            return View(db.Vouchers.Where(s => s.ID_voucher == id).FirstOrDefault());

        }

        [HttpPost]
        public ActionResult DeleteVoucher(int id, Voucher voucher)
        {
            try
            {
                voucher = db.Vouchers.Where(s => s.ID_voucher == id).FirstOrDefault();
                db.Vouchers.Remove(voucher);
                db.SaveChanges();
                return RedirectToAction("ListVoucher");

            }
            catch
            {
                ViewBag.Message = "Delete Fail, please check data again";
                return View();
            }
        }


        //Quản lý đơn hàng (Admin)
        public ActionResult QLDonHang(string currentFilter, string SearchString, int? page)
        {
            var bill = new List<Bill>();
            if (SearchString != null)
            { page = 1; }
            else
            {
                SearchString = currentFilter;
            }
            if (!string.IsNullOrEmpty(SearchString))
            {
                bill = db.Bills.Where(s => s.Bill_id.ToString().Contains(SearchString)).ToList();
            }
            else
            {
                bill = db.Bills.ToList();
            }
            ViewBag.CurrentFilter = SearchString;
            int pageSize = 6;
            int pageNumber = (page ?? 1);
            bill = bill.OrderByDescending(n => n.Bill_id).ToList();
            return View(bill.ToPagedList(pageNumber, pageSize));
        }

        //Quản lý khách hàng (Admin)
        public ActionResult QLKhachHang(string currentFilter, string SearchString, int? page)
        {
            var kh = new List<Customer>();
            if (SearchString != null)
            { page = 1; }
            else
            {
                SearchString = currentFilter;
            }
            if (!string.IsNullOrEmpty(SearchString))
            {
                kh = db.Customers.Where(s => s.Customer_name.Contains(SearchString)).ToList();
            }
            else
            {
                kh = db.Customers.ToList();
            }
            ViewBag.CurrentFilter = SearchString;
            int pageSize = 6;
            int pageNumber = (page ?? 1);
            kh = kh.OrderByDescending(n => n.Customer_name).ToList();
            return View(kh.ToPagedList(pageNumber, pageSize));
        }

        

        //Đối tác quản lý.
        //Quản lý đơn hàng Room

        public ActionResult QLDonHangPartner(string currentFilter, string SearchString, int? page)
        {
            string UsernamePartner = Session["Partner"].ToString();
            var Accompany = db.Accompanies.Where(s => s.Username == UsernamePartner).FirstOrDefault();
            var result = db.Rooms
                .Where(t => t.Accompany_id == Accompany.Accompany_ID)
                .Join(
                    db.DetailRoomBills,
                    room => room.Room_number,
                    detailBill => detailBill.Room_id,
                    (room, detailBill) => new { Room = room, DetailBill = detailBill }
                )
                .Join(
                    db.Bills,
                    joinResult => joinResult.DetailBill.Bill_id,
                    bill => bill.Bill_id,
                    (joinResult, bill) => bill
                )
                .Distinct()
                .ToList();
            if (SearchString != null)
            { page = 1; }
            else
            {
                SearchString = currentFilter;
            }
            if (!string.IsNullOrEmpty(SearchString))
            {
                
            }
            else
            {
                
            }
            ViewBag.CurrentFilter = SearchString;
            int pageSize = 6;
            int pageNumber = (page ?? 1);

            return View(result.ToPagedList(pageNumber, pageSize));
        } 

        public ActionResult QLKhachHangPartner(string currentFilter, string SearchString, int? page)
        {
            string UsernamePartner = Session["Partner"].ToString();
            var Accompany = db.Accompanies.Where(s => s.Username == UsernamePartner).FirstOrDefault();
            var customers = db.Rooms
                .Where(t => t.Accompany_id == Accompany.Accompany_ID)
                .Join(
                    db.DetailRoomBills,
                    room => room.Room_number,
                    detail => detail.Room_id,
                    (room, detail) => detail.Customer
                )
                .Distinct()
                .ToList();
            if (SearchString != null)
            { page = 1; }
            else
            {
                SearchString = currentFilter;
            }
            if (!string.IsNullOrEmpty(SearchString))
            {
                customers.Where(s => s.Customer_phone.Contains(SearchString)).ToList();
            }
            else
            {
                customers.ToList();
            }
            ViewBag.CurrentFilter = SearchString;
            int pageSize = 6;
            int pageNumber = (page ?? 1);
            //khPartner = khPartner.OrderByDescending(n => n.Customer_id == detailBill.Customer_id).ToList();
            return View(customers.ToPagedList(pageNumber, pageSize));
        }

        [HttpGet]
        public ActionResult ViewBill(int id)
        {
            var bill = db.Bills.Where(s => s.Bill_id == id).FirstOrDefault();
            var detailbill = db.DetailRoomBills.Where(s => s.Bill_id == id).FirstOrDefault();
            var Customer = db.Customers.Where(s => s.Customer_id == detailbill.Customer_id).FirstOrDefault();
            ViewBag.Room = detailbill.Room_id;
            ViewBag.Checkin = detailbill.Check_in;
            ViewBag.Checkout = detailbill.Check_out;
            ViewBag.Service = db.DetailRoomBills.Where(s => s.Bill_id == id).Count();
            ViewBag.Name = Customer.Customer_name;
            ViewBag.Email = Customer.Customer_email;
            ViewBag.Sdt = Customer.Customer_phone;
            return View(bill);
        }

        [HttpGet]
        public ActionResult Renevue()
        {
            string id = Session["Accompany_ID"].ToString();
            var ac = db.Accompanies.Where(s => s.Accompany_ID.ToString() == id).FirstOrDefault();
            ViewBag.AcName = ac.Accompany_name;
            ViewBag.AcEmail = ac.Accompany_email;
            return View(db.Revenues.Where(s => s.Accompany_ID.ToString() == id).ToList());
        }

        [HttpPost]
        public ActionResult Renevue(string Note, int month, int year)
        {
            Revenue renevue = new Revenue();
            string id = Session["Accompany_ID"].ToString();
            var ac = db.Accompanies.Where(s => s.Accompany_ID.ToString() == id).FirstOrDefault();
            renevue.Accompany_ID = ac.Accompany_ID;
            renevue.CreateDate = DateTime.Now.Date;

            double totalAmount;
            if (ac.Accompany_typeid == 1)
            {
                var result = db.Rooms
                    .Where(t => t.Accompany_id == ac.Accompany_ID)
                    .Join(
                        db.DetailRoomBills,
                        room => room.Room_number,
                        detailBill => detailBill.Room_id,
                        (room, detailBill) => new { Room = room, DetailBill = detailBill }
                    )
                    .Join(
                        db.Bills,
                        joinResult => joinResult.DetailBill.Bill_id,
                        bill => bill.Bill_id,
                        (joinResult, bill) => bill
                    )
                    .Distinct()
                    .ToList();
                     totalAmount = result.Where(b => b.Status == "Đã thanh toán"
                                                && b.Bill_date.HasValue
                                                && b.Bill_date.Value.Month == month
                                                && b.Bill_date.Value.Year == year)
                .Sum(b => (double?)(b.Bill_total ?? 0)) ?? 0;
            }
            else
            {
                var result = db.Tickets
                .Where(t => t.Accompany_ID == ac.Accompany_ID)
                .Join(
                    db.DetailTicketBills,
                    ticket => ticket.Ticket_id,
                    detailBill => detailBill.Ticket_id,
                    (ticket, detailBill) => new { Ticket = ticket, DetailBill = detailBill }
                )
                .Join(
                    db.Bills,
                    joinResult => joinResult.DetailBill.Bill_id,
                    bill => bill.Bill_id,
                    (joinResult, bill) => bill
                )
                .Distinct()
                .ToList();
                totalAmount = result.Where(b => b.Status == "Đã thanh toán"
                                                && b.Bill_date.HasValue
                                                && b.Bill_date.Value.Month == month
                                                && b.Bill_date.Value.Year == year)
                .Sum(b => (double?)(b.Bill_total ?? 0)) ?? 0;
            }
            
            renevue.Amount = totalAmount;
            if(Note != null)
            {
                renevue.Note = Note;
            }
            else
            {
                renevue.Note = "";
            }
            db.Revenues.Add(renevue);
            db.SaveChanges();
            return RedirectToAction("Renevue");
        }

        //Quản lý đơn hàng Flight

        public ActionResult QLDonHangPartnerFlight(string currentFilter, string SearchString, int? page)
        {
            string UsernamePartner = Session["Partner"].ToString();
            var Accompany = db.Accompanies.Where(s => s.Username == UsernamePartner).FirstOrDefault();
            var result = db.Tickets
                .Where(t => t.Accompany_ID == Accompany.Accompany_ID)
                .Join(
                    db.DetailTicketBills,
                    ticket => ticket.Ticket_id,
                    detailBill => detailBill.Ticket_id,
                    (ticket, detailBill) => new { Ticket = ticket, DetailBill = detailBill }
                )
                .Join(
                    db.Bills,
                    joinResult => joinResult.DetailBill.Bill_id,
                    bill => bill.Bill_id,
                    (joinResult, bill) => bill
                )
                .Distinct()
                .ToList();
            if (SearchString != null)
            { page = 1; }
            else
            {
                SearchString = currentFilter;
            }
            if (!string.IsNullOrEmpty(SearchString))
            {

            }
            else
            {
                
            }
            ViewBag.CurrentFilter = SearchString;
            int pageSize = 6;
            int pageNumber = (page ?? 1);

            return View(result.ToPagedList(pageNumber, pageSize));
        }

        public ActionResult QLKhachHangPartnerFlight(string currentFilter, string SearchString, int? page)
        {
            string UsernamePartner = Session["Partner"].ToString();
            var Accompany = db.Accompanies.Where(s => s.Username == UsernamePartner).FirstOrDefault();
            var customers = db.Tickets
                .Where(t => t.Accompany_ID == Accompany.Accompany_ID)
                .Join(
                    db.DetailTicketBills,
                    ticket => ticket.Ticket_id,
                    detail => detail.Ticket_id,
                    (ticket, detail) => detail.Customer
                )
                .Distinct()
                .ToList();

            if (SearchString != null)
            { page = 1; }
            else
            {
                SearchString = currentFilter;
            }
            if (!string.IsNullOrEmpty(SearchString))
            {
                customers.Where(s => s.Customer_phone.Contains(SearchString)).ToList();
            }
            else
            {
                customers.ToList();
            }
            ViewBag.CurrentFilter = SearchString;
            int pageSize = 6;
            int pageNumber = (page ?? 1);
            customers = customers.OrderByDescending(n => n.Customer_phone).ToList();
            return View(customers.ToPagedList(pageNumber, pageSize));
        }

        [HttpGet]
        public ActionResult ViewBillTicket(int id)
        {
            var bill = db.Bills.Where(s => s.Bill_id == id).FirstOrDefault();
            var detailbill = db.DetailTicketBills.Where(s => s.Bill_id == id).FirstOrDefault();
            var ticket = db.Tickets.Where(s => s.Ticket_id == detailbill.Ticket_id).FirstOrDefault();
            var Customer = db.Customers.Where(s => s.Customer_id == detailbill.Customer_id).FirstOrDefault();
            ViewBag.Ticket = ticket.Flight_Number;
            ViewBag.Checkin = ticket.BeginDate;
            ViewBag.Checkout = ticket.EndDate;
            ViewBag.Service = db.DetailTicketBills.Where(s => s.Bill_id == id).Count();
            ViewBag.Name = Customer.Customer_name;
            ViewBag.Email = Customer.Customer_email;
            ViewBag.Sdt = Customer.Customer_phone;
            return View(bill);
        }
        //
        // quản lý admin đối tác
        public ActionResult QuanLyDoiTac(string search = "")
        {
            // get unread gmail.
            SendEmail email = new SendEmail();
            int count = email.CountEmail();
            ViewData["Greeting"] = count;
            // searck and return list
            List<Accompany> listCompany = db.Accompanies.Where(row => row.Accompany_name.Contains(search)).ToList();
            if (listCompany.Count == 0)
            {
                listCompany = db.Accompanies.Where(row => row.Accompany_ID.ToString().Contains(search)).ToList();
                if (listCompany.Count == 0)
                {
                    listCompany = db.Accompanies.Where(row => row.Accompany_Type.accompany_name.Contains(search)).ToList();

                }
            }
            ViewBag.Search = search;
            if (listCompany.Count == 0)
            {
                search = "";
                listCompany = db.Accompanies.Where(row => row.Accompany_name.Contains(search)).ToList();
                ViewBag.Error = "Không tìm thấy vui lòng nhập lại";
                return View(listCompany);

            }
            return View(listCompany);
        }
        public ActionResult ThemDoiTac()
        {

            return View();
        }
        [HttpPost]
        public ActionResult ThemDoiTac(Accompany contract)
        {
            db.Accompanies.Add(contract);
            db.SaveChanges();
            Session["ContractData"] = contract;

            return RedirectToAction("DetailContract");
        }
        public ActionResult DetailContract()
        {
            return View();
        }
        [HttpPost]
        public ActionResult DetailContract(DetailContract contract)
        {
            contract.Admin_id = (int)Session["Admin_ID"];
            contract.ID_Payment = 1;
            contract.Contract_status = 1;
            db.DetailContracts.Add(contract);
            db.SaveChanges();
            return RedirectToAction("UpdateAccompany", "admin", new { @ID = contract.Accompany_ID });
        }
        public ActionResult UpdateAccompany(int ID)
        {
            var accompany = db.Accompanies.Where(m => m.Accompany_ID == ID).FirstOrDefault();
            var acc = db.Accompanies.Find(accompany.Accompany_ID);
            ViewData["ContractID"] = acc;
            return View(acc);
        }

        [HttpPost]
        public ActionResult UpdateAccompany(Accompany contract)
        {
            var accompany = db.Accompanies.Where(m => m.Accompany_ID == contract.Accompany_ID).FirstOrDefault();
            var acc = db.Accompanies.Find(accompany.Accompany_ID);
            acc.Accompany_name = contract.Accompany_name;
            acc.Accompany_email = contract.Accompany_email;
            acc.Accompany_phone = contract.Accompany_phone;
            db.SaveChanges();
            return View(acc);
        }
        public ActionResult ViewDetailContract(int ID)
        {
            var accompany = db.DetailContracts.Where(m => m.Accompany_ID == ID).ToList();
            foreach (var item in accompany)
            {
                if (item.Contract_status == 1)
                {
                    var acc = db.DetailContracts.Where(m => m.Contract_status == 1).FirstOrDefault();
                    var contract = db.Accompanies.Find(item.Accompany_ID);
                    Session["ContractData"] = contract;
                    return View(acc);
                }
            }
            TempData["Error"] = "Chưa có hợp đồng hoặc hợp đồng hết hiệu lực!";
            return RedirectToAction("UpdateAccompany", new { @ID = ID });

        }
        // create and send mail account
        public ActionResult CreateAccount(int accompanyid, string accompanyname, string accompanyemail)
        {
            // send mail 
            SendEmail sendEmail = new SendEmail();
            sendEmail.SendAccount(accompanyemail, accompanyname);
            // crea account
            Account account = new Account();
            account.Username = accompanyemail;
            account.pass = "Pass123";
            account.account_role = 1;
            db.Accounts.Add(account);
            db.SaveChanges();
            return RedirectToAction("SaveAccountToAcc", new { @ID = accompanyid, @User = accompanyemail });
        }
        public ActionResult SaveAccountToAcc(int id, string user)
        {
            var accompany = db.Accompanies.Where(m => m.Accompany_ID == id).FirstOrDefault();
            var acc = db.Accompanies.Find(accompany.Accompany_ID);
            acc.Username = user;
            db.SaveChanges();
            TempData["SendMail"] = user;
            return RedirectToAction("UpdateAccompany", new { @ID = id });
        }

        public ActionResult ManageTicket(string search = "")
        {
            List<Ticket> tickets = db.Tickets.Where(row => row.Ticket_name.Contains(search)).ToList();
            if (tickets.Count == 0)
            {
                tickets = db.Tickets.Where(row => row.Ticket_id.ToString().Contains(search)).ToList();
                if (tickets.Count == 0)
                {
                    tickets = db.Tickets.Where(row => row.Flight_Number.ToString().Contains(search)).ToList();
                }
            }
            ViewBag.Search = search;
            if (tickets.Count == 0)
            {
                search = "";
                tickets = db.Tickets.Where(row => row.Ticket_id.ToString().Contains(search)).ToList();
                ViewBag.Error = "Không tìm thấy vui lòng nhập lại";
                return View(tickets);
            }
            return View(tickets);
        }
        public ActionResult TicketApproved(int ID)
        {
            var accompany = db.Tickets.Where(m => m.Ticket_id == ID).FirstOrDefault();
            var acc = db.Tickets.Find(accompany.Ticket_id);
            acc.Ticket_Status = 1;
            db.SaveChanges();
            return RedirectToAction("ManageTicket");
        }
        public ActionResult TicketHidden(int ID)
        {
            var accompany = db.Tickets.Where(m => m.Ticket_id == ID).FirstOrDefault();
            var acc = db.Tickets.Find(accompany.Ticket_id);
            acc.Ticket_Status = 2;
            db.SaveChanges();
            return RedirectToAction("ManageTicket");
        }
        public ActionResult ManageRoom(string search = "")
        {
            List<Room> rooms = db.Rooms.Where(row => row.Room_number.ToString().Contains(search)).ToList();
            if (rooms.Count == 0)
            {
                rooms = db.Rooms.Where(row => row.Room_Type1.room_name.ToString().Contains(search)).ToList();
                if (rooms.Count == 0)
                {
                    rooms = db.Rooms.Where(row => row.Room_status.ToString().Contains(search)).ToList();
                }
            }
            ViewBag.Search = search;
            if (rooms.Count == 0)
            {
                search = "";
                rooms = db.Rooms.Where(row => row.Room_number.ToString().Contains(search)).ToList();
                ViewBag.Error = "Không tìm thấy vui lòng nhập lại";
                return View(rooms);
            }
            return View(rooms);
        }
        public ActionResult RoomApproved(int ID)
        {
            var accompany = db.Rooms.Where(m => m.Room_number == ID).FirstOrDefault();
            var acc = db.Rooms.Find(accompany.Room_number);
            acc.Room_status = 1;
            db.SaveChanges();
            return RedirectToAction("manageroom");
        }
        public ActionResult RoomHidden(int ID)
        {
            var accompany = db.Rooms.Where(m => m.Room_number == ID).FirstOrDefault();
            var acc = db.Rooms.Find(accompany.Room_number);
            acc.Room_status = 2;
            db.SaveChanges();
            return RedirectToAction("manageroom");
        }
        public ActionResult ManageService(string search = "")
        {
            List<TravelService> services = db.TravelServices.Where(row => row.ID_service.ToString().Contains(search)).ToList();
            if (services.Count == 0)
            {
                services = db.TravelServices.Where(row => row.serviceName.Contains(search)).ToList();
            }
            ViewBag.Search = search;
            if (services.Count == 0)
            {
                search = "";
                services = db.TravelServices.Where(row => row.ID_service.ToString().Contains(search)).ToList();
                ViewBag.Error = "Không tìm thấy vui lòng nhập lại";
                return View(services);
            }
            return View(services);
        }
        public ActionResult ServiceApproved(int ID)
        {
            var accompany = db.TravelServices.Where(m => m.ID_service == ID).FirstOrDefault();
            var acc = db.TravelServices.Find(accompany.ID_service);
            acc.Service_Status = 1;
            db.SaveChanges();
            return RedirectToAction("ManageService");
        }
        public ActionResult ServiceHidden(int ID)
        {
            var accompany = db.TravelServices.Where(m => m.ID_service == ID).FirstOrDefault();
            var acc = db.TravelServices.Find(accompany.ID_service);
            acc.Service_Status = 2;
            db.SaveChanges();
            return RedirectToAction("ManageService");
        }
        // voucher
        public ActionResult ManageVoucher(string search = "")
        {

            List<Voucher> vouchers = db.Vouchers.Where(row => row.ID_voucher.ToString().Contains(search)).ToList();
            if (search != "")
            {
                var list = vouchers.Where(row => row.Voucher_name.Contains(search)).ToList();
                if (list.Count == 0)
                {
                    var list1 = vouchers.Where(row => row.Accompany_ID.ToString().Contains(search)).ToList();
                    if (list1.Count == 0)
                    {
                        ViewBag.Error = "Không tìm thấy vui lòng nhập lại";
                        return View(vouchers);
                    }
                    return View(list1);
                }
                return View(list);
            }
            ViewBag.Search = search;
            return View(vouchers);
        }
        public ActionResult AddVoucher(Voucher voucher)
        {
            db.Vouchers.Add(voucher);
            db.SaveChanges();
            Session["ContractData"] = voucher;
            return RedirectToAction("ManageVoucher");
        }
        public ActionResult DeleteVoucherONE(int ID)
        {
            var data = db.Vouchers.Find(ID);
            db.Vouchers.Remove(data);
            db.SaveChanges();
            return RedirectToAction("ManageVoucher");
        }
        public ActionResult ManageChat()
        {
            //List<ChatComment> chats = db.ChatComments.GroupBy(m => m.User_Comment).Select(e => e.FirstOrDefault()).ToList();
            List<ChatComment> chats = db.ChatComments.GroupBy(m => m.User_Comment).Select(e => e.OrderByDescending(c => c.CCM_ID)).Select(e => e.FirstOrDefault()).ToList();
            return View(chats);
        }
        public ActionResult SendReply(ChatReply reply)
        {
            reply.CreateOn = DateTime.Now;
            db.ChatReplies.Add(reply);
            db.SaveChanges();
            Session["Reply"] = reply;
            return RedirectToAction("ManageChat");
        }
    }
}