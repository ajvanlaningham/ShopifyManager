using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShopifyCustomer.Models
{
    public class Adjustment
    {
        public string cust_account_number { get; set; }
        public string cust_account_name { get; set; }
        public string cust_ship_to_loc { get; set; }
        public string ship_to_site_number { get; set; }
        public string sbu { get; set; }
        public string item { get; set; }
        public string price_list_name { get; set; }
        public double list_price { get; set; }
        public double selling_price { get; set; }
        public string uom { get; set; }
        public int? value_from { get; set; }
        public int? value_to { get; set; }
        public object surcharge_percent { get; set; }
        public object surcharge_amount { get; set; }
        public int? discount_percent { get; set; }
        public double? discount_amount { get; set; }
    }

    public class AdjustmentList
    {
        public List<Adjustment> adjustment { get; set; }
    }

    public class RootAdjustment
    {
        public AdjustmentList adjustment_list { get; set; }
    }
}
