using SellingFastFood.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.Mvc;

namespace SellingFastFood.Controllers
{

    public class ProductController : Controller
    {
        SellingFastFoodDBEntities db = new SellingFastFoodDBEntities();
        public ActionResult Index(string search = "", string sortOrder="")
        {
            //Chuyển đổi dữ liệu danh sách sang đổi tượng để so sánh
            var productQuery = db.Products.AsQueryable();

            if(search !=null)
            {
                productQuery = productQuery.Where(p=>p.ProductName.Contains(search));
            }


            switch (sortOrder)
            {
                case "priceAsc":
                    productQuery = productQuery.OrderBy(p => p.Price);
                    break;
                case "priceDesc":
                    productQuery = productQuery.OrderByDescending(p => p.Price);
                    break;
                default:
                    // Mặc định sắp xếp theo một điều kiện nào đó khác (nếu cần)
                    break;
            }

            //kiểm tra xem có tìm thấy sản phẩm nào không
            var products = productQuery.ToList();
            if (products.Count == 0)
            {
                ViewBag.Message = "Không có sản phẩm này, hãy thử lại.";
            }

            var CategoryProduct = new CategoryProduct
            {
                Categories = db.Categories.ToList(),
                Products = productQuery.ToList(),
            };
            return View(CategoryProduct);
        }

        public ActionResult DetailProducts(int id)
        {
            int EvaluateCount = db.Evaluates.Count(e=>e.ProductID== id);

            var ProductEvaluate = new ProductEvaluate
            {
                Product = db.Products.Where(p => p.ProductID == id).FirstOrDefault(),
                Evaluates = db.Evaluates.Where(p=>p.ProductID==id).ToList(),
            };

            ViewBag.CountEva = EvaluateCount;
            return View(ProductEvaluate);
        }

        public ActionResult ProductByCategory(int id)
        {
            var CategoryProduct = new CategoryProduct
            {
                Categories = db.Categories.ToList(),
                Products = db.Products.Where(c=>c.CategoryID==id).ToList(),
            };
            return View(CategoryProduct);
        }

        [HttpPost]
        public ActionResult Evaluate(Evaluate evaluare)
        {
            if (Session["user"] == null)
            {
                return RedirectToAction("Login", "Home");
            }

            Evaluate eva = new Evaluate();
            eva.EvaluateContent = evaluare.EvaluateContent;
            eva.EvaluateDate = DateTime.Now;
            eva.UserID = int.Parse(Session["UserID"].ToString());
            eva.Rating = evaluare.Rating;
            eva.ProductID = evaluare.ProductID;

            db.Evaluates.Add(eva);
            db.SaveChanges();



            return RedirectToAction("DetailProducts", new { id = evaluare.ProductID });

        }

        public ActionResult ShowEvaluate(int id)
        {
            var ListEvaluate = db.Evaluates.Where(e=>e.ProductID==id).ToList();
            return View(ListEvaluate);
        }
    }
}