using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShopifyCustomer.Models
{
    public class Discount
    {
        public string ModifierName { get; set; }
        public string ModifierNumer { get; set; }
        public string DiscountPercent { get; set; }
        public string CustomerAccount { get; set; }
        public string ShipToLocation { get; set; }

    }
}
