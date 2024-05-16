using SellingFastFood.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

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

        public ActionResult AddToCart(int id, string  action)
        {

            if (Session["user"] == null)
            {
                return RedirectToAction("Login", "Home");
            }

            List<CartModel> carts = GetListCart();

            CartModel c = carts.Find(s=>s.ProductionID== id);
            if (c != null)
            {
                if (action == "increase")
                {
                    c.Quantity++;
                }
                else if (action == "decrease" && c.Quantity > 1)
                {
                    c.Quantity--;
                }
            }
            else
            {
                c = new CartModel(id);
                carts.Add(c);
            }
            return RedirectToAction("ListCarts");
        }

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

        public ActionResult OrderProduct(string DeliveryAdress, string NameShip, string PhoneShip) 
        {
            List<CartModel> carts = GetListCart();

            Order order = new Order();
            order.OrderName = "Đơn hàng"+"-"+ DateTime.Now.ToString("yyyyMMddHHmmss");
            order.OrderCreationeDate = DateTime.Now;
            order.OrderStatus = 1;
            order.PaymentMethod = 1;
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



            return RedirectToAction("CheckOutSuccess");
        }

        public ActionResult CheckOutSuccess()
        {
            return View();
        }


    }
}