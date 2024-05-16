using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SellingFastFood.Models
{
    public class CartModel
    {
        SellingFastFoodDBEntities da = new SellingFastFoodDBEntities(); 
        public int ProductionID { get; set; }
        public string ProductName { get; set; }
        public decimal? Price { get; set; }
        public int Quantity {  get; set; }
        public decimal? Total { get { return Price*Quantity; } }

        public string ImageProduct {  get; set; }
        public CartModel(int productID)
        { 
            this.ProductionID = productID;
            Product p = da.Products.Single(n=>n.ProductID == productID);
            ProductName = p.ProductName;
            Price = p.Price;
            Quantity = 1;
            ImageProduct = p.ImageProduct;
        }
    }
}