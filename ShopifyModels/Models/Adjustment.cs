using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShopifyCustomer.Models
{
    public class Adjustment
    {
        string CustomerAccountNumber { get; set; }
        string CustomerAccountName { get; set; }
        string CustomerShipToLocation { get; set; }
        string ShipToSiteNumber { get; set; }
        string SBU { get; set; }
        string PriceListName { get; set; }
        string ListPrice { get; set; }
        string SellingPrice { get; set; }
        string UOM { get; set; }
        string ValueFrom { get; set; }
        string ValueTo { get; set; }
        string SurchargePercent { get; set; }
        string SurchargeAmmount { get; set; }
        string DiscountPercent { get; set; }
        string DiscountAmount { get; set; }

    }
}
