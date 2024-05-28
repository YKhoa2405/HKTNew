using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SellingFastFood.Models
{
    public class MonthlyRevenueViewModel
    {
        public int Year { get; set; }
        public int Month { get; set; }
        public decimal TotalRevenue { get; set; }
    }
}