using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShopifyCustomer.Models
{
    public class StatusOrder
    {
        public string OrderNumber { get; set; }
        public string CustomerPONumber { get; set; }
        public string LineNumber { get; set; }
        public string OrderedItem { get; set; }
        public string OrderedQuantity { get; set; }
        public string OrderQuantityUOM { get; set; }
        public string ShippedQuantity { get; set; }
        public string BackOrderQuantity { get; set; }
        public string HDRStatusCode { get; set; }
        public string LineStatusCode { get; set; }
        public string RequestDate { get; set; }
        public string ScheduleShipDate { get; set; }
        public string ActualShipDate { get; set; }
        public string UnitSellingPrice { get; set; }

    }
}
