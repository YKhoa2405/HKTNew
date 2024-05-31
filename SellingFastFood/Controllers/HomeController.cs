using SellingFastFood.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Data;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Web.Mvc;
using Twilio.Rest.Preview.Wireless.Sim;


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

            var firstDayOfNextMonth = firstDayOfMonth.AddMonths(1);


            var topSellingProducts = db.OrderDetails
                            .Where(od => od.Order.OrderCreationeDate >= firstDayOfMonth && od.Order.OrderCreationeDate < firstDayOfNextMonth)
                            .GroupBy(od => od.Product)  // Sử dụng ProductID để nhóm
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
            if (!IsValidGmail(user.Email))
            {
                ModelState.AddModelError("Email", "Email không đúng định dạng.");
                return View(user);
            }
            if (!IsValidPhoneNumber(user.Phone) )
            {
                ModelState.AddModelError("Phone", "Số điện thoại không hợp lệ.");
                return View(user);
            }
            if (string.IsNullOrEmpty(user.Password))
            {
                ModelState.AddModelError("Password", "Vui lòng nhập mật khẩu");
                return View(user);
            }
            string connectionString = GetAdoNetConnectionString();
            bool emailExists, phoneExists;

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                using (SqlCommand cmd = new SqlCommand("sp_CheckExistence", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Email", user.Email);
                    cmd.Parameters.AddWithValue("@Phone", user.Phone);

                    SqlParameter emailExistsParam = new SqlParameter("@EmailExists", SqlDbType.Bit)
                    {
                        Direction = ParameterDirection.Output
                    };
                    cmd.Parameters.Add(emailExistsParam);

                    SqlParameter phoneExistsParam = new SqlParameter("@PhoneExists", SqlDbType.Bit)
                    {
                        Direction = ParameterDirection.Output
                    };
                    cmd.Parameters.Add(phoneExistsParam);
                    conn.Open();
                    cmd.ExecuteNonQuery();

                    emailExists = (bool)emailExistsParam.Value;
                    phoneExists = (bool)phoneExistsParam.Value;
                }
            }

            if (emailExists)
            {
                ModelState.AddModelError("Email", "Email đã tồn tại.");
            }

            if (phoneExists)
            {
                ModelState.AddModelError("Phone", "Số điện thoại đã tồn tại.");
            }
            if (emailExists || phoneExists)
            {
                return View(user);
            }
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
            if (string.IsNullOrWhiteSpace(user.Email))
            {
                ModelState.AddModelError("Password", "Vui lòng nhập email.");
            }
            if (string.IsNullOrWhiteSpace(user.Password))
            {
                ModelState.AddModelError("Password", "Vui lòng nhập mật khẩu.");
            }
            if (ModelState.IsValid)
            {
                var f_email = user.Email;
                var f_password = GetMD5(user.Password);
                var checkUser = db.Users.SingleOrDefault(u => u.Email.Equals(f_email) && u.Password.Equals(f_password)); //SingleOrDefault kiểm tra xem email, password nào trùng trong csdl
                var checkAdmin = db.Users.SingleOrDefault(u => u.Email.Equals(f_email) && u.Password.Equals(f_password) && u.UserRole == 2);
                if (checkAdmin != null)
                {
                    return RedirectToAction("Products", "Admin");

                }
                if (checkUser != null)
                {
                    Session["User"] = checkUser; //lưu info user vào session
                    Session["UserID"] = checkUser.UserID;
                    return RedirectToAction("Index", "Home");
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

        private string GetAdoNetConnectionString()
        {
            var entityConnectionString = ConfigurationManager.ConnectionStrings["SellingFastFoodDBEntities"].ConnectionString;
            var entityConnection = new System.Data.Entity.Core.EntityClient.EntityConnectionStringBuilder(entityConnectionString);
            return entityConnection.ProviderConnectionString;
        }
        private bool IsValidGmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email && email.EndsWith("@gmail.com");
            }
            catch
            {
                return false;
            }
        }
        private bool IsValidPhoneNumber(string phone)
        {
            if (phone == null)
            {
                return false;
            }
            return phone.Length == 10 && phone.All(char.IsDigit) && phone[0] == '0';
        }
    }
}