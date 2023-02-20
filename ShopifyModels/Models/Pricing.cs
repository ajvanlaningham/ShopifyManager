using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShopifyCustomer.Models
{
    public class Pricing
    {
        public string Name { get; set; }
        public string ListHeaderId { get; set; }
        public string Item { get; set; }
        public string UPrice { get; set; }
        public string UOM { get; set; }
        public string MinQty { get; set; }
        public string MaxQty { get; set; }
        public string StartDateActive { get; set;  }

    }
}
