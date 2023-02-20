using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharedModels.Models.APIModel
{
    // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse);
    public class Pricing
    {
        public string name { get; set; }
        public int list_header_id { get; set; }
        public string item { get; set; }
        public decimal uprice { get; set; }
        public string uom { get; set; }
        public int? min_qty { get; set; }
        public int? max_qty { get; set; }
        public string start_date_active { get; set; }
    }

    public class PricingList
    {
        public List<Pricing>? pricing { get; set; }
    }

    public class IICSPricing
    {
        public PricingList? pricing_list { get; set; }
    }
}
