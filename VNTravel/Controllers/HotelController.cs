using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Services.Description;
using VNTravel.Models;
using System.Net.Http;
using System.Text;
using System.Security.Cryptography;
using PayPal.Api;
using Payment = PayPal.Api.Payment;
using System.Diagnostics;
using System.Data.SqlClient;
using System.Diagnostics.Contracts;

namespace VNTravel.Controllers
{
    public class HotelController : Controller
    {
        // GET: Hotel
        webtraveldbEntities db = new webtraveldbEntities();

        //public bool IsSelected { get; set; }
        public ActionResult Hotel(string search = "", string sortOrder="")
        {
            //
            ViewBag.PriceSort = String.IsNullOrEmpty(sortOrder) ? "price_desc" : "room";
            ViewBag.RoomTypeSort = sortOrder == "room_type" ? "room_type_desc" : "room_type";
            //
            var list = from s in db.Rooms select s;
            List<Room> rooms = db.Rooms.Where(row => row.Room_number.ToString().Contains(search)).ToList();
            if (rooms.Count == 0)
            {
                rooms = db.Rooms.Where(row => row.Room_Type1.Price.price1.ToString().Contains(search)).ToList();
                if (rooms.Count == 0)
                {
                    rooms = db.Rooms.Where(row => row.Room_Type1.room_name.Contains(search)).ToList();

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
            //
            if (sortOrder != "")
            {
                switch (sortOrder)
                {
                    case "price_desc":
                        list = list.OrderByDescending(s => s.Room_Type1.Price.price1); return View(list);
                    case "room_type":
                        list = list.OrderBy(s => s.Room_type); return View(list);
                    case "room_type_desc":
                        list = list.OrderByDescending(s => s.Room_type); return View(list);
                    case "room":
                        list = list.OrderBy(s => s.Room_Type1.Price.price1); return View(list);
                }
            }
            return View(rooms);
        }

        [HttpGet]
        public ActionResult DetailRoom(int id)
        {
            var list = db.Rooms.Where(s => s.Room_number == id).FirstOrDefault();
            var listsuggest = db.Rooms.Where(m=>m.Room_Type1.room_name.Equals(list.Room_Type1.room_name)).ToList();
            Session["ListSuggest"] = listsuggest;
            return View(list);
        }

        [HttpGet]
        public ActionResult Book(int id)
        {
            double price = 0, totalPrice_Service = 0;
            int quantityService = 0, id_bill = 0;
            if (Session["Username"] == null)
            {
                string returnUrl = Request.UrlReferrer?.ToString();
                Session["BookHotel"] = "BookHotel";
                return Redirect(returnUrl);
            }
            else
            {
                string username = Session["Username"].ToString();
                var cus = db.Customers.Where(s => s.Username == username).FirstOrDefault();
                if(cus.Customer_name == null || cus.Customer_email == null || cus.Customer_phone == null)
                {
                    List<int> selectedServiceIDs = TempData["SelectedServiceIDs"] as List<int>;
                    if (selectedServiceIDs == null)
                    {
                        TempData["idRoom"] = id;
                        return RedirectToAction("FormBook");
                    }
                    else
                    {
                        //DateTime begin = (DateTime)TempData["Begin"];
                        //DateTime end = (DateTime)TempData["End"];
                        //string voucher = (string)TempData["End"];

                        //Room room = db.Rooms.Where(s => s.Room_number == id).FirstOrDefault();
                        //Room_Type room_Type = db.Room_Type.Where(s => s.RoomTypeid == room.Room_type).FirstOrDefault();
                        //Price priceRoom = db.Prices.Where(s => s.ID_price == room_Type.ID_price).FirstOrDefault();

                        //Bill bill = new Bill();
                        //bill.Bill_date = DateTime.Now;
                        //bill.Bill_type = 0; //0 là Room
                        //db.Bills.Add(bill);

                        //if (selectedServiceIDs == null)
                        //{
                        //    DetailRoomBill dtbill = new DetailRoomBill();
                        //    dtbill.Customer_id = cus.Customer_id;
                        //    dtbill.Room_id = room.Room_number;
                        //    dtbill.Bill_id = bill.Bill_id;
                        //    dtbill.Check_in = begin;
                        //    dtbill.Check_out = end;
                        //    db.DetailRoomBills.Add(dtbill);
                        //    TimeSpan time = end - begin;
                        //    int day = (int)Math.Abs(time.TotalDays); // Số ngày đặt
                        //    if (voucher == null)
                        //    {
                        //        bill.Bill_total = day * priceRoom.price1;

                        //    }
                        //    var pVoucher = db.Vouchers.Where(s => s.Voucher_name.Equals(voucher)).FirstOrDefault();
                        //    double moneyBill = (double)(day * priceRoom.price1);
                        //    double percent = (double)(moneyBill * pVoucher.Voucher_percent) / 100;
                        //    bill.Bill_total = (int)(moneyBill - percent);
                        //    db.Bills.Add(bill);
                        //}
                        //else
                        //{
                        //    DetailRoomBill dtbill = new DetailRoomBill();
                        //    int totalMoneyService = 0;
                        //    foreach (int serviceID in selectedServiceIDs)
                        //    {
                        //        dtbill.Customer_id = cus.Customer_id;
                        //        dtbill.Room_id = room.Room_number;
                        //        dtbill.Bill_id = bill.Bill_id;
                        //        dtbill.Check_in = begin;
                        //        dtbill.Check_out = end;
                        //        dtbill.ID_service = serviceID;
                        //        var sType = db.ServiceTypes.Where(s => s.ID_service_type == serviceID).FirstOrDefault();
                        //        var pService = db.Prices.Where(s => s.ID_price == sType.ID_price).FirstOrDefault();
                        //        totalMoneyService += pService.price1.Value;
                        //        db.DetailRoomBills.Add(dtbill);
                        //    }
                        //    TimeSpan time = end - begin;
                        //    int day = (int)Math.Abs(time.TotalDays); // Số ngày đặt
                        //    if (voucher == null)
                        //    {
                        //        bill.Bill_total = (day * priceRoom.price1) + totalMoneyService;
                        //        db.Bills.Add(bill);

                        //    }
                        //    var pVoucher = db.Vouchers.Where(s => s.Voucher_name.Equals(voucher)).FirstOrDefault();
                        //    double moneyBill = (double)(day * priceRoom.price1) + totalMoneyService;
                        //    double percent = (double)(moneyBill * pVoucher.Voucher_percent) / 100;
                        //    bill.Bill_total = (int)(moneyBill - percent);
                        //    db.Bills.Add(bill);
                        //}
                    }
                }
                else
                {
                    List<int> selectedServiceIDs = TempData["SelectedServiceIDs"] as List<int>;
                    if(selectedServiceIDs == null) {
                        TempData["idRoom"] = id;
                        return RedirectToAction("Serivce", new { idRoom = id});
                    }
                    else
                    {
                        quantityService = selectedServiceIDs.Count();
                        DateTime begin = (DateTime)TempData["Begin"];
                        DateTime end = (DateTime)TempData["End"];
                        string voucher = (string)TempData["Voucher"];

                        Room room = db.Rooms.Where(s => s.Room_number == id).FirstOrDefault();
                        Room_Type room_Type = db.Room_Type.Where(s => s.RoomTypeid == room.Room_type).FirstOrDefault();
                        Price priceRoom = db.Prices.Where(s => s.ID_price == room_Type.ID_price).FirstOrDefault();
                        price = (double)priceRoom.price1;

                        Bill bill = new Bill();
                        bill.Bill_date = DateTime.Now;
                        bill.Bill_type = 0; //0 là Room
                        db.Bills.Add(bill);
                        id_bill = bill.Bill_id;
                        if (selectedServiceIDs.Count == 0)
                        {
                            var getPrice = db.Prices.Where(s => s.price1 == 0).FirstOrDefault();
                            var getServiceType = db.ServiceTypes.Where(s => s.ID_price == getPrice.ID_price).FirstOrDefault();
                            var getService = db.TravelServices.Where(s => s.ID_service_type == getServiceType.ID_service_type).FirstOrDefault();

                            DetailRoomBill dtbill = new DetailRoomBill();
                            dtbill.Customer_id = cus.Customer_id;
                            dtbill.Room_id = room.Room_number;
                            dtbill.Bill_id = bill.Bill_id;
                            dtbill.ID_service = getService.ID_service;
                            dtbill.Check_in = begin;
                            dtbill.Check_out = end;
                            db.DetailRoomBills.Add(dtbill);
                            TimeSpan time = end - begin;
                            int day = (int)Math.Abs(time.TotalDays); // Số ngày đặt
                            if (voucher == "")
                            {
                                bill.Bill_total = day * priceRoom.price1;
                                bill.Status = "Chờ xác nhận";
                                db.Bills.Add(bill);

                            }
                            else
                            {
                                var pVoucher = db.Vouchers.Where(s => s.Voucher_name.Equals(voucher)).FirstOrDefault();
                                double moneyBill = (double)(day * priceRoom.price1);
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
                                DetailRoomBill dtbill = new DetailRoomBill();
                                dtbill.Customer_id = cus.Customer_id;
                                dtbill.Room_id = room.Room_number;
                                dtbill.Bill_id = bill.Bill_id;
                                dtbill.Check_in = begin;
                                dtbill.Check_out = end;
                                dtbill.ID_service = serviceID;
                                var service = db.TravelServices.Where(s => s.ID_service == serviceID).FirstOrDefault();
                                var sType = db.ServiceTypes.Where(s => s.ID_service_type == service.ID_service_type).FirstOrDefault();
                                var pService = db.Prices.Where(s => s.ID_price == sType.ID_price).FirstOrDefault();
                                totalMoneyService += pService.price1.Value;
                                db.DetailRoomBills.Add(dtbill);
                            }
                            totalPrice_Service = totalMoneyService;
                            TimeSpan time = end - begin;
                            int day = (int)Math.Abs(time.TotalDays); // Số ngày đặt
                            if (voucher == "")
                            {
                                bill.Bill_total = (day * priceRoom.price1) + totalMoneyService;
                                bill.Status = "Chờ xác nhận";
                                db.Bills.Add(bill);
                            }
                            else
                            {
                                var pVoucher = db.Vouchers.Where(s => s.Voucher_name.Equals(voucher)).FirstOrDefault();
                                double moneyBill = (double)(day * priceRoom.price1) + totalMoneyService;
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
            TempData["priceRoom"] = price;
            TempData["totalPrice_Service"] = totalPrice_Service;
            TempData["quantityService"] = quantityService;
            ViewBag.quantityService = quantityService;
            return RedirectToAction("ViewBill");

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
        public ActionResult Serivce(int idRoom)
        {
            var Room = db.Rooms.Where(s => s.Room_number == idRoom).FirstOrDefault();
            List<TravelService> services = db.TravelServices.Where(s => s.Accompany_ID == Room.Accompany_id).ToList();
            List<TravelService> serviceViewModels = services.Select(s => new TravelService
            {
                ID_service = s.ID_service,
                serviceName = s.serviceName,
                IsSelected = false

            }).ToList();
            return View(services);
        }

        [HttpPost]
        public ActionResult SelectedService(List<TravelService> selectedServices, string voucher, DateTime begin, DateTime end)
        {
            List<int> id = new List<int>();
            if (selectedServices != null)
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
                TempData["Voucher"] = "";
            }
            else
            {
                TempData["Voucher"] = "";
            }
            TempData["SelectedServiceIDs"] = id;
            TempData["Begin"] = begin;
            TempData["End"] = end;
            int idRoom = (int)TempData["idRoom"];
            return RedirectToAction("Book", new { id = idRoom});
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

                    string baseURI = Request.Url.Scheme + "://" + Request.Url.Authority + "/Hotel/PaymentWithPayPal?";
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
            return RedirectToAction("ViewBill", "Admin", new { id = bill.Bill_id });
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
            double total;
            if(Session["ToTal"] != null)
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
                name = "Room",
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
                subtotal = total.ToString()
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

        public ActionResult ViewBill()
        {
            ViewBag.NameCustomer = TempData["NameCustomer"];
            ViewBag.DateBill = TempData["DateBill"];
            ViewBag.PriceRoom = TempData["PriceRoom"];
            ViewBag.PriceService = TempData["totalPrice_Service"];
            ViewBag.Begin = TempData["Begin"];
            ViewBag.End = TempData["End"];
            ViewBag.TotalBill = TempData["TotalBill"];

            TempData["Room"] = ViewBag.PriceRoom;
            TempData["Service"] = ViewBag.PriceService;
            TempData["quantity"] = TempData["quantityService"];
            Session["ToTal"] = ViewBag.TotalBill;

            return View();
        }

        public ActionResult FailureView()
        {
            return View();
        }

        //[HttpGet]
        //public ActionResult SuccessView(int id)
        //{
        //    TempData.Clear();
        //    return RedirectToAction("ViewBill","Admin", new { id = id });
        //}

        [HttpPost]
        public ActionResult Comment(string cmt, int Room_number)
        {
            if (Session["Username"] != null)
            {
                string username = Session["UserName"].ToString();
                Comment comment = new Comment();
                comment.Room_number = Room_number;
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
        public ActionResult ManageComments(int Room_number, int id_comment)
        {
            string returnUrl = Request.UrlReferrer?.ToString();
            string id = Session["Accompany_ID"].ToString();
            var ac = db.Accompanies.Where(s => s.Accompany_ID.ToString() == id).FirstOrDefault();
            var room = db.Rooms.Where(s => s.Room_number == Room_number).FirstOrDefault();
            if (room.Accompany_id == ac.Accompany_ID)
            {
                var comment = db.Comments.Where(s => s.ID_Comment == id_comment && s.Room_number == Room_number).FirstOrDefault();
                db.Comments.Remove(comment);
                db.SaveChanges();
                Session["RemoveCMT"] = "RemoveCMT";
                return Redirect(returnUrl);
            }
            Session["ManageCommentsHotel"] = "ManageCommentsHotel";
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
            return Json( JsonRequestBehavior.AllowGet);
        }
        //[HttpPost]
        //public ActionResult _AddFav(int id, string Username, int Room_number)
        //{
        //    /*List<string> errors = new List<string>();*/ // You might want to return an error if something wrong happened
        //    ListFavourite list = new ListFavourite();
        //    list.Username = Username;
        //    list.Room_number = Room_number;
        //    list.status_fav = true;
        //    db.ListFavourites.Add(list);
        //    //Do DB Processing   
        //    db.SaveChanges();
        //    return Json(JsonRequestBehavior.AllowGet);
        //}
    }
}