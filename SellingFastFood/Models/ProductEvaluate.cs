using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SellingFastFood.Models
{
    public class ProductEvaluate
    {
        public Product Product { get; set; }
        public List<Evaluate> Evaluates { get; set; }
    }
}