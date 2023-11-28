using PayPal.Api;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Services;
using VNTravel.Models;
using Payment = PayPal.Api.Payment;

namespace VNTravel.Controllers
{
    public class FlightController : Controller
    {
        // GET: Flight
        webtraveldbEntities db = new webtraveldbEntities();

        public ActionResult Flight(string search = "", string sortOrder = "")
        {
            ViewBag.PriceSort = String.IsNullOrEmpty(sortOrder) ? "price_desc" : "price";
            ViewBag.NameSort = sortOrder == "name" ? "name_desc" : "name";
            ViewBag.StartTimeSort = sortOrder == "start_time" ? "start_time_desc" : "start_time";
            ViewBag.EndTimeSort = sortOrder == "end_time" ? "end_time_desc" : "end_time";
            //
            var list = from s in db.Tickets select s;
            List<Ticket> tickets = db.Tickets.Where(row => row.Ticket_id.ToString().Contains(search)).ToList();
            if (tickets.Count == 0)
            {
                tickets = db.Tickets.Where(row => row.BeginDate.ToString().Contains(search)).ToList();
                if (tickets.Count == 0)
                {
                    tickets = db.Tickets.Where(row => row.EndDate.ToString().Contains(search)).ToList();
                    if(tickets.Count == 0)
                    {
                        tickets = db.Tickets.Where(row => row.Ticket_name.Contains(search)).ToList();
                    }

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
            //
            if (sortOrder != "")
            {
                switch (sortOrder)
                {
                    case "price_desc":
                        list = list.OrderByDescending(s => s.Price.price1); return View(list);
                    case "name":
                        list = list.OrderBy(s => s.Ticket_name); return View(list);
                    case "name_desc":
                        list = list.OrderByDescending(s => s.Ticket_name); return View(list);
                    case "price":
                        list = list.OrderBy(s => s.Price.price1); return View(list);
                    case "start_time":
                        list = list.OrderBy(s => s.BeginDate); return View(list);
                    case "start_time_desc":
                        list = list.OrderByDescending(s => s.BeginDate); return View(list);
                    case "end_time":
                        list = list.OrderBy(s => s.EndDate); return View(list);
                    case "end_time_desc":
                        list = list.OrderByDescending(s => s.EndDate); return View(list);
                }
            }
            return View(tickets);
        }

        [HttpGet]
        public ActionResult DetailFlight(int id)
        {
            return View();
        }

        [HttpGet]
        public ActionResult Book(int id)
        {
            double price = 0, totalPrice_Service = 0;
            int quantityService = 0, id_bill = 0;
            if (Session["Username"] == null)
            {
                string returnUrl = Request.UrlReferrer?.ToString();
                Session["BookFlight"] = "BookFlight";
                return Redirect(returnUrl);
            }
            else
            {
                string username = Session["Username"].ToString();
                var cus = db.Customers.Where(s => s.Username == username).FirstOrDefault();
                if (cus.Customer_name == null || cus.Customer_email == null || cus.Customer_phone == null)
                {
                    List<int> selectedServiceIDs = TempData["SelectedServiceIDs"] as List<int>;
                    if (selectedServiceIDs == null)
                    {
                        TempData["idTicket"] = id;
                        return RedirectToAction("FormBook");
                    }
                    else { }
                }
                else
                {
                    List<int> selectedServiceIDs = TempData["SelectedServiceIDs"] as List<int>;
                    if (selectedServiceIDs == null)
                    {
                        TempData["idTicket"] = id;
                        return RedirectToAction("Serivce", new { idTicket = id });
                    }
                    else
                    {
                        quantityService = selectedServiceIDs.Count();
                        string voucher = (string)TempData["Voucher"];

                        Ticket ticket = db.Tickets.Where(s => s.Ticket_id == id).FirstOrDefault();
                        Price priceTicket = db.Prices.Where(s => s.ID_price == ticket.ID_price).FirstOrDefault();
                        price = (double)priceTicket.price1;

                        Bill bill = new Bill();
                        bill.Bill_date = DateTime.Now;
                        bill.Bill_type = 1; //1 là ticket
                        db.Bills.Add(bill);
                        id_bill = bill.Bill_id;
                        if (selectedServiceIDs.Count == 0)
                        {
                            var getPrice = db.Prices.Where(s => s.price1 == 0).FirstOrDefault();
                            var getServiceType = db.ServiceTypes.Where(s => s.ID_price == getPrice.ID_price).FirstOrDefault();
                            var getService = db.TravelServices.Where(s => s.ID_service_type == getServiceType.ID_service_type).FirstOrDefault();

                            DetailTicketBill dtTicket = new DetailTicketBill();
                            dtTicket.Customer_id = cus.Customer_id;
                            dtTicket.Ticket_id = ticket.Ticket_id;
                            dtTicket.Bill_id = bill.Bill_id;
                            dtTicket.ID_service = getService.ID_service;
                            db.DetailTicketBills.Add(dtTicket);

                            if (voucher == "")
                            {
                                bill.Bill_total = priceTicket.price1;
                                bill.Status = "Chờ xác nhận";
                                db.Bills.Add(bill);

                            }
                            else
                            {
                                var pVoucher = db.Vouchers.Where(s => s.Voucher_name.Equals(voucher)).FirstOrDefault();
                                double moneyBill = (double)priceTicket.price1;
                                double percent = (double)(moneyBill * pVoucher.Voucher_percent) / 100;
                                bill.ID_voucher = pVoucher.ID_voucher;
                                bill.Status = "Chờ xác nhận";
                                bill.Bill_total = (int)(moneyBill - percent);
                                pVoucher.Quantily -= 1;
                                db.Entry(pVoucher).State = System.Data.Entity.EntityState.Modified;
                                db.Bills.Add(bill);
                            }
                        }
                        else
                        {
                            int totalMoneyService = 0;
                            foreach (int serviceID in selectedServiceIDs)
                            {
                                DetailTicketBill dtTicket = new DetailTicketBill();
                                dtTicket.Customer_id = cus.Customer_id;
                                dtTicket.Ticket_id = ticket.Ticket_id;
                                dtTicket.Bill_id = bill.Bill_id;
                                dtTicket.ID_service = serviceID;
                                var service = db.TravelServices.Where(s => s.ID_service == serviceID).FirstOrDefault();
                                var sType = db.ServiceTypes.Where(s => s.ID_service_type == service.ID_service_type).FirstOrDefault();
                                var pService = db.Prices.Where(s => s.ID_price == sType.ID_price).FirstOrDefault();
                                totalMoneyService += pService.price1.Value;
                                db.DetailTicketBills.Add(dtTicket);
                            }
                            totalPrice_Service = totalMoneyService;
                            if (voucher == "")
                            {
                                bill.Bill_total = priceTicket.price1 + totalMoneyService;
                                bill.Status = "Chờ xác nhận";
                                db.Bills.Add(bill);
                            }
                            else
                            {
                                var pVoucher = db.Vouchers.Where(s => s.Voucher_name.Equals(voucher)).FirstOrDefault();
                                double moneyBill = (double)priceTicket.price1 + totalMoneyService;
                                double percent = (double)(moneyBill * pVoucher.Voucher_percent) / 100;
                                bill.ID_voucher = pVoucher.ID_voucher;
                                bill.Status = "Chờ xác nhận";
                                bill.Bill_total = (int)(moneyBill - percent);
                                pVoucher.Quantily -= 1;
                                db.Entry(pVoucher).State = System.Data.Entity.EntityState.Modified;
                                db.Bills.Add(bill);
                            }
                        }
                        db.SaveChanges();
                        TempData["idBill"] = bill.Bill_id;
                        TempData["NameCustomer"] = cus.Customer_name;
                        TempData["DateBill"] = bill.Bill_date;
                        TempData["TotalBill"] = bill.Bill_total;
                    }
                }
            }
            TempData["priceTicket"] = price;
            TempData["totalPrice_Service"] = totalPrice_Service;
            TempData["quantityService"] = quantityService;
            ViewBag.quantityService = quantityService;
            return RedirectToAction("ViewBill", new { idTicket = id});

        }

        public ActionResult FormBook()
        {
            return View();
        }

        [HttpPost]
        public ActionResult FormBook(string name, string email, string phone)
        {
            string username = Session["Username"].ToString();
            Customer cus = db.Customers.Where(s => s.Username == username).FirstOrDefault();
            cus.Customer_name = name;
            cus.Customer_email = email;
            cus.Customer_phone = phone;
            db.Entry(cus).State = System.Data.Entity.EntityState.Modified;
            db.SaveChanges();
            return RedirectToAction("Serivce");
        }

        [HttpGet]
        public ActionResult Serivce(int idTicket)
        {
            var Ticket = db.Tickets.Where(s => s.Ticket_id == idTicket).FirstOrDefault();
            List<TravelService> services = db.TravelServices.Where(s => s.Accompany_ID == Ticket.Accompany_ID).ToList();
            List<TravelService> serviceViewModels = services.Select(s => new TravelService
            {
                ID_service = s.ID_service,
                serviceName = s.serviceName,
                IsSelected = false

            }).ToList();
            return View(services);
        }

        [HttpPost]
        public ActionResult SelectedService(List<TravelService> selectedServices, string voucher)
        {
            List<int> id = new List<int>();
            if(selectedServices != null)
            {
                foreach (var service in selectedServices)
                {
                    if (service.IsSelected)
                    {
                        id.Add(service.ID_service);
                    }
                }
            }
            
            var pVoucher = db.Vouchers.Where(s => s.Voucher_name.Equals(voucher)).FirstOrDefault();
            if (pVoucher != null)
            {
                if (pVoucher.Quantily > 0)
                {
                    TempData["Voucher"] = voucher;
                }
                else
                {
                    TempData["Voucher"] = "";

                }
            }
            else
            {
                TempData["Voucher"] = "";
            }
            TempData["SelectedServiceIDs"] = id;
            int idTicket = (int)TempData["idTicket"];
            return RedirectToAction("Book", new { id = idTicket });
        }

        [HttpGet]
        public ActionResult PayFromHistory(int id)
        {
            Session["id"] = id;
            return RedirectToAction("PaymentWithPaypal");
        }

        public ActionResult PaymentWithPaypal(string Cancel = null)
        {
            APIContext apiContext = PaypalConfiguration.GetAPIContext();
            try
            {

                string payerId = Request.Params["PayerID"];
                if (string.IsNullOrEmpty(payerId))
                {

                    string baseURI = Request.Url.Scheme + "://" + Request.Url.Authority + "/Flight/PaymentWithPayPal?";
                    var guid = Convert.ToString((new Random()).Next(100000));
                    var createdPayment = this.CreatePayment(apiContext, baseURI + "guid=" + guid);
                    var links = createdPayment.links.GetEnumerator();
                    string paypalRedirectUrl = null;
                    while (links.MoveNext())
                    {
                        Links lnk = links.Current;
                        if (lnk.rel.ToLower().Trim().Equals("approval_url"))
                        {
                            paypalRedirectUrl = lnk.href;
                        }
                    }
                    Session.Add(guid, createdPayment.id);
                    return Redirect(paypalRedirectUrl);
                }
                else
                {
                    var guid = Request.Params["guid"];
                    var executedPayment = ExecutePayment(apiContext, payerId, Session[guid] as string);
                    if (executedPayment.state.ToLower() != "approved")
                    {
                        return View("FailureView");
                    }
                }
            }
            catch (Exception ex)
            {
                return View("FailureView");
            }
            Bill bill;
            if (Session["id"] != null)
            {
                string id_bill = Session["id"].ToString();
                int id = Convert.ToInt32(id_bill);
                bill = db.Bills.Where(s => s.Bill_id == id).FirstOrDefault();
                Session.Remove("id");
            }
            else
            {
                int id_bill = (int)TempData["idBill"];
                bill = db.Bills.Where(s => s.Bill_id == id_bill).FirstOrDefault();
            }
            bill.Status = "Đã thanh toán";
            db.Entry(bill).State = System.Data.Entity.EntityState.Modified;
            db.SaveChanges();
            return RedirectToAction("SuccessView", new { idBill = bill.Bill_id });
        }
        private PayPal.Api.Payment payment;
        private Payment ExecutePayment(APIContext apiContext, string payerId, string paymentId)
        {
            var paymentExecution = new PaymentExecution()
            {
                payer_id = payerId
            };
            this.payment = new Payment()
            {
                id = paymentId
            };
            return this.payment.Execute(apiContext, paymentExecution);
        }

        private Payment CreatePayment(APIContext apiContext, string redirectUrl)
        {
            double total = 0;
            if (Session["ToTal"] != null)
            {
                string money = Session["ToTal"].ToString();
                Session.Remove("ToTal");
                total = Double.Parse(money);

            }
            else
            {
                string id_bill = Session["id"].ToString();
                int id = Convert.ToInt32(id_bill);
                var bill = db.Bills.Where(s => s.Bill_id == id).FirstOrDefault();
                total = (double)bill.Bill_total;
            }
            var itemList = new ItemList()
            {
                items = new List<Item>()
            };
            itemList.items.Add(new Item()
            {
                name = "Ticket",
                currency = "USD",
                price = total.ToString(),
                quantity = "1",
                sku = "sku"
            });
            var payer = new Payer()
            {
                payment_method = "paypal"
            };
            var redirUrls = new RedirectUrls()
            {
                cancel_url = redirectUrl + "&Cancel=true",
                return_url = redirectUrl
            };
            var details = new Details()
            {
                tax = "0",
                shipping = "0",
                subtotal = total.ToString(),
            };
            var amount = new Amount()
            {
                currency = "USD",
                total = total.ToString(),
                details = details
            };
            var transactionList = new List<Transaction>();
            var paypalOrderId = DateTime.Now.Ticks;
            transactionList.Add(new Transaction()
            {
                description = $"Invoice #{paypalOrderId}",
                invoice_number = paypalOrderId.ToString(),
                amount = amount,
                item_list = itemList
            });
            this.payment = new Payment()
            {
                intent = "sale",
                payer = payer,
                transactions = transactionList,
                redirect_urls = redirUrls
            };
            return this.payment.Create(apiContext);

        }


        public ActionResult ViewBill(int idTicket)
        {
            var Ticket = db.Tickets.Where(s => s.Ticket_id == idTicket).FirstOrDefault();
            ViewBag.NameCustomer = TempData["NameCustomer"];
            ViewBag.DateBill = TempData["DateBill"];
            ViewBag.PriceTicket = TempData["priceTicket"];
            ViewBag.PriceService = TempData["totalPrice_Service"];
            ViewBag.Begin = Ticket.BeginDate;
            ViewBag.End = Ticket.EndDate;
            ViewBag.TotalBill = TempData["TotalBill"];

            TempData["Ticket"] = ViewBag.PriceTicket;
            TempData["Service"] = ViewBag.PriceService;
            TempData["quantity"] = TempData["quantityService"];
            Session["ToTal"] = ViewBag.TotalBill;
            return View();
        }

        public ActionResult FailureView()
        {
            return View();
        }

        [HttpGet]
        public ActionResult SuccessView(int idBill)
        {
            TempData.Clear();
            var detail = db.DetailTicketBills.Where(s => s.Bill_id == idBill).FirstOrDefault();
            var ticket = db.Tickets.Where(s => s.Ticket_id == detail.Ticket_id).FirstOrDefault();
            var cus = db.Customers.Where(s => s.Customer_id == detail.Customer_id).FirstOrDefault();
            ViewBag.HoTen = cus.Customer_name;
            ViewBag.Phone = cus.Customer_phone;
            ViewBag.Email = cus.Customer_email;
            return View(ticket);
        }

        [HttpPost]
        public ActionResult Comment(string cmt, int Ticket_id)
        {
            if (Session["Username"] != null)
            {
                string username = Session["UserName"].ToString();
                Comment comment = new Comment();
                comment.Ticket_id = Ticket_id;
                comment.Content = cmt;
                comment.Username = username;
                db.Comments.Add(comment);
                db.SaveChanges();
                var newCommentHtml = $@"
                <div>
                    <strong>{username}:</strong> {cmt}
                </div>";
                return Content(newCommentHtml);
            }
            Session["Comment"] = "Comment";
            return Content("<script>window.location.reload();</script>");
        }

        [HttpGet]
        public ActionResult ManageComments(int Ticket_id, int id_comment)
        {
            string returnUrl = Request.UrlReferrer?.ToString();
            string id = Session["Accompany_ID"].ToString();
            var ac = db.Accompanies.Where(s => s.Accompany_ID.ToString() == id).FirstOrDefault();
            var ticket = db.Tickets.Where(s => s.Ticket_id == Ticket_id).FirstOrDefault();
            if (ticket.Accompany_ID == ac.Accompany_ID)
            {
                var comment = db.Comments.Where(s => s.ID_Comment == id_comment && s.Ticket_id == Ticket_id).FirstOrDefault();
                db.Comments.Remove(comment);
                db.SaveChanges();
                Session["RemoveCMT"] = "RemoveCMT";
                return Redirect(returnUrl);
            }
            Session["ManageCommentsFlight"] = "ManageCommentsFlight";
            return Redirect(returnUrl);

        }
        [HttpPost]
        public ActionResult _Fav(int ID)
        {
            /*List<string> errors = new List<string>();*/ // You might want to return an error if something wrong happened
            var fav = db.ListFavourites.Find(ID);
            fav.status_fav = true;
            //Do DB Processing   
            db.SaveChanges();
            return Json(JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult _UnFav(int ID)
        {
            /*List<string> errors = new List<string>();*/ // You might want to return an error if something wrong happened
            var fav = db.ListFavourites.Find(ID);
            fav.status_fav = false;
            //Do DB Processing   
            db.SaveChanges();
            return Json(JsonRequestBehavior.AllowGet);
        }

    }
}