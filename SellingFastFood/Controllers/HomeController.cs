using SellingFastFood.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Web.Mvc;


namespace SellingFastFood.Controllers
{
    public class HomeController : Controller
    {
        SellingFastFoodDBEntities db = new SellingFastFoodDBEntities();
        public ActionResult Index()
        {
            int currentMonth = DateTime.Now.Month;
            ViewBag.CurrentMonth = currentMonth;

            var firstDayOfMonth = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);

            var topSellingProducts = db.OrderDetails
                            .Where(od => od.Order.OrderCreationeDate >= firstDayOfMonth)
                            .GroupBy(od => od.Product)
                            .OrderByDescending(g => g.Sum(od => od.Quantity))
                            .Select(g => g.Key)
                            .Take(20)
                            .ToList();
            return View(topSellingProducts);
        }

        [HttpGet]
        public ActionResult Register()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Register(User user)
        {
            user.UserRole = 1;
            user.Password = GetMD5(user.Password);
            db.Users.Add(user);
            db.SaveChanges();

            return RedirectToAction("Login");
        }

        [HttpGet]
        public ActionResult Login()
        {
            return View();
        }
        [HttpPost]
        public ActionResult Login(User user)
        {
            if(ModelState.IsValid)
            {
                var f_email = user.Email;
                var f_password = GetMD5(user.Password);
                var checkUser = db.Users.SingleOrDefault(u=>u.Email.Equals(f_email) && u.Password.Equals(f_password));
                if (checkUser != null)
                {
                    if (user.UserRole == 1)
                    {
                        Session["User"] = checkUser;
                        Session["UserID"] = checkUser.UserID;
                        return RedirectToAction("Index","Home");

                    }
                    else
                    {
                        return RedirectToAction("Products","Admin");
                    }
                }
                else
                {
                    ViewBag.LoginFail = "Email hoặc mật khẩu không chính xác";
                }
            }
            return View();
        }

        public ActionResult Logout()
        {
            Session.Clear();
            return RedirectToAction("Login");
        }

        public ActionResult ProfileUser(int id)
        {
            var listOrder = db.Orders.Where(d => d.UserID == id).ToList();
            ViewBag.listOrderDetail = db.OrderDetails.Include("Product").ToList();
            return View(listOrder);
        }
        //Mã hóa mật khẩu
        public static string GetMD5(string str)
        {
            MD5 md5 = new MD5CryptoServiceProvider();
            byte[] fromData = Encoding.UTF8.GetBytes(str);
            byte[] targetData = md5.ComputeHash(fromData);
            string byte2String = null;

            for (int i = 0; i < targetData.Length; i++)
            {
                byte2String += targetData[i].ToString("x2");

            }
            return byte2String;
        }
    }
}