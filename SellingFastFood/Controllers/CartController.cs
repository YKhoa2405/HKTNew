using PayPal.Api;
using SellingFastFood.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Twilio;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;

namespace SellingFastFood.Controllers
{
    public class CartController : Controller
    {
        SellingFastFoodDBEntities da = new SellingFastFoodDBEntities();

        public List<CartModel> GetListCart()
        {
            List<CartModel> carts = Session["CartModel"] as List<CartModel>;    

            if(carts == null)
            {
                carts = new List<CartModel>();
                Session["CartModel"] = carts;
            }
            return carts;
        }

        public ActionResult ListCarts() 
        {
            List<CartModel> carts = GetListCart();
            ViewBag.CountProduct = carts.Sum(s=>s.Quantity);

            string formatTotal = string.Format("{0:#,##0} VNĐ", carts.Sum(s => s.Total));
            ViewBag.Total = formatTotal;
            return View(carts);
        }

        public ActionResult AddToCart(int id)
        {

            if (Session["user"] == null)
            {
                return RedirectToAction("Login", "Home");
            }

            List<CartModel> carts = GetListCart();

            CartModel c = carts.Find(s=>s.ProductionID== id);
            if (c != null)
            {
                c.Quantity++;
            }
            else
            {
                c = new CartModel(id);
                carts.Add(c);
            }
            return RedirectToAction("ListCarts");
        }

        //Giảm số lượng
        public ActionResult DecreaseQuantity(int id)
        {

            List<CartModel> carts = GetListCart();

            CartModel c = carts.Find(s => s.ProductionID == id);
            if (c != null && c.Quantity > 1)
            {
                c.Quantity--;
            }

            return RedirectToAction("ListCarts");
        }

        //Tăng số lượng
        public ActionResult IncreaseQuantity(int id)
        {
            List<CartModel> carts = GetListCart();

            CartModel c = carts.Find(s => s.ProductionID == id);
            if (c != null)
            {
                c.Quantity++;
            }

            return RedirectToAction("ListCarts");
        }

        //Xóa sản phẩm
        public ActionResult DeleteProductToCart(int id)
        {
            List<CartModel> carts = GetListCart();
            CartModel c = carts.Find(s => s.ProductionID == id);
            if (c != null)
            {
                carts.Remove(c);
            }


            return RedirectToAction("ListCarts");
        }

        public  ActionResult OrderInfo()
        {
            List<CartModel> carts = GetListCart();

            string formatTotal = string.Format("{0:#,##0} VNĐ", carts.Sum(s => s.Total));
            ViewBag.Total = formatTotal;
            return View(carts);
        }

        public ActionResult OrderProduct(string DeliveryAdress, string NameShip, string PhoneShip,int paymentMethod) 
        {
            string message = $"Đặt hàng thành công. Hãy chú ý điện thoại để nhận cuộc gọi từ nhân viên giao hàng";
            List<CartModel> carts = GetListCart();
            
            Models.Order order = new Models.Order();
            order.OrderName = "Đơn hàng"+"-"+ DateTime.Now.ToString("yyyyMMddHHmmss");
            order.OrderCreationeDate = DateTime.Now;
            order.OrderStatus = 1;
            order.PaymentMethod = paymentMethod;
            order.TotalMoney= carts.Sum(s => s.Total);
            order.UserID = int.Parse(Session["UserID"].ToString());
            order.DeliveryAddress = DeliveryAdress;
            order.NameShip = NameShip;
            order.PhoneShip = PhoneShip;


            da.Orders.Add(order);
            da.SaveChanges();

            List<OrderDetail> orderDetailList = new List<OrderDetail>();
            foreach(var item in carts)
            {
                OrderDetail od = new OrderDetail(); 
                od.OrderID = order.OrderID;
                od.ProductID = item.ProductionID;
                od.Quantity= item.Quantity;
                
                orderDetailList.Add(od);
                
            }

            da.OrderDetails.AddRange(orderDetailList);
            da.SaveChanges();
            Session["CartModel"] = null;

            if (paymentMethod == 1)
            {
/*                Sms(PhoneShip, message);*/
                return RedirectToAction("CheckOutSuccess");
            }
            return View();

        }

        //paypal

        public ActionResult CheckOutSuccess()
        {
            return View();
        }

        public ActionResult CheckOutError()
        {
            return View();
        }
        public void Sms(string phoneNumber, string message)
        {
            string _accountSid = ConfigurationManager.AppSettings["TwilioAccountSid"];
            string _authToken = ConfigurationManager.AppSettings["TwilioAuthToken"];
            string _fromPhoneNumber = ConfigurationManager.AppSettings["TwilioFromPhoneNumber"];

            TwilioClient.Init(_accountSid, _authToken);

            if (!phoneNumber.StartsWith("+"))
            {
                if (phoneNumber.StartsWith("0"))
                {
                    phoneNumber = phoneNumber.Substring(1);
                }
                phoneNumber = $"+84{phoneNumber}";
            }

            // Gửi tin nhắn
            var messageOptions = new CreateMessageOptions(new PhoneNumber(phoneNumber))
            {
                From = new PhoneNumber(_fromPhoneNumber),
                Body = message
            };

            var msg = MessageResource.Create(messageOptions);
        }


        public ActionResult PaymentWithPaypal(string Cancel = null)
        {
            APIContext apiContext = PaypalConfiguration.GetAPIContext();
            try
            {
                string payerId = Request.Params["PayerID"];
                if (string.IsNullOrEmpty(payerId))
                {
                    string baseURI = Request.Url.Scheme + "://" + Request.Url.Authority + "/cart/PaymentWithPayPal?";
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
                        return RedirectToAction("CheckOutError");

                    }
                }
            }
            catch (Exception ex)
            {
                return RedirectToAction("CheckOutError");

            }

            return RedirectToAction("CheckOutSuccess");
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
            List<CartModel> carts = GetListCart();
            var itemList = new ItemList()
            {
                items = new List<Item>()
            };
            foreach (var item in carts)
            {
                var priceInUSD = Math.Round((decimal)(item.Price / 25500), 2);
                itemList.items.Add(new Item()
                {
                    name = item.ProductName,
                    currency = "USD",
                    price = priceInUSD.ToString("F2", CultureInfo.InvariantCulture),//định dạng
                    quantity = item.Quantity.ToString(),
                    sku = "sku"
                });
            }

            var payer = new Payer()
            {
                payment_method = "paypal"
            };
            var redirUrls = new RedirectUrls()
            {
                cancel_url = redirectUrl + "&Cancel=true",
                return_url = redirectUrl
            };
            var totalInUSD = Math.Round((decimal)(carts.Sum(s => s.Total) / 25500), 2);
            var details = new Details()
            {
                tax = "0",
                shipping = "0",
                subtotal = totalInUSD.ToString("F2", CultureInfo.InvariantCulture)
            };
            var amount = new Amount()
            {
                currency = "USD",
                total = totalInUSD.ToString("F2", CultureInfo.InvariantCulture),
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




    }
}