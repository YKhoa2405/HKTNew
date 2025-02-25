﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using PayPal.Api;
using SellingFastFood.Models;

namespace SellingFastFood.Areas.Admin.Controllers
{
    public class CategoriesController : Controller
    {
        private SellingFastFoodDBEntities db = new SellingFastFoodDBEntities();

        // GET: Admin/Categories
        public ActionResult Index(string search = "")
        {
            var categoryQuery = db.Categories.AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                categoryQuery = categoryQuery.Where(c => c.CategoryName.Contains(search));
            }

            var categories = categoryQuery.ToList();
            return View(categories);
        }


        // GET: Admin/Categories/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Category category = db.Categories.Find(id);
            if (category == null)
            {
                return HttpNotFound();
            }
            return View(category);
        }

        // GET: Admin/Categories/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Admin/Categories/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "CategoryID,CategoryName")] Category category)
        {
            if (ModelState.IsValid)
            {
                db.Categories.Add(category);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(category);
        }

        // GET: Admin/Categories/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Category category = db.Categories.Find(id);
            if (category == null)
            {
                return HttpNotFound();
            }
            return View(category);
        }

        // POST: Admin/Categories/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "CategoryID,CategoryName")] Category category)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    db.Entry(category).State = EntityState.Modified;
                    db.SaveChanges();
                    return RedirectToAction("Index");
                }
                catch (DbEntityValidationException ex)
                {
                    foreach (var validationErrors in ex.EntityValidationErrors)
                    {
                        foreach (var validationError in validationErrors.ValidationErrors)
                        {
                            ModelState.AddModelError(validationError.PropertyName, validationError.ErrorMessage);
                        }
                    }
                }
            }
            return View(category);
        }


        // GET: Admin/Categories/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Category category = db.Categories.Find(id);
            if (category == null)
            {
                return HttpNotFound();
            }
            return View(category);
        }

        // POST: Admin/Categories/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Category category = db.Categories.Find(id);
            db.Categories.Remove(category);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        public ActionResult TopProduct()
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

        public ActionResult MonthlyRevenue()
        {
            var monthlyRevenue = db.Orders
                .GroupBy(o => new { Year = o.OrderCreationeDate.Value.Year, Month = o.OrderCreationeDate.Value.Month })
                .Select(g => new MonthlyRevenueViewModel
                {
                    Year = g.Key.Year,
                    Month = g.Key.Month,
                    TotalRevenue = (decimal)g.Sum(o => o.TotalMoney)
                })
                .OrderBy(m => m.Year)
                .ThenBy(m => m.Month)
                .ToList();

            return View(monthlyRevenue);
        }

    }
}
