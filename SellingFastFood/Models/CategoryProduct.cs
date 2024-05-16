using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SellingFastFood.Models
{
    public class CategoryProduct
    {
        public List<Product> Products { get; set;}
        public List<Category> Categories { get; set;} 
    }
}