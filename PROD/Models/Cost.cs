using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PROD.Models
{
    public class Cost
    {
        public int RentID { get; set; }

        public int KmsCovered { get; set; }

        public decimal Price { get; set; }

        public decimal Tax { get; set; }

        public decimal TotalCost { get; set; }
    }
}