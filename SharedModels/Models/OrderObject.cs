using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharedModels.Models
{
    public class OrderObject
    {
        public CustomerReccord custom_rec { get; set; }
        public Header header { get; set; }
        public LinesList lines_list { get; set; }
        public string p_ou { get; set; } // US_CNR_OU
    }

    public class CustomerReccord
    {
        public string cust_acct_num { get; set; } //Customer ID number? oracle number 
        public string site_use_id { get; set; }  //unique reference to be stored on our end (blank first time or "0") 
        public string address1 { get; set; }
        public string address2 { get; set; }
        public string address3 { get; set; }
        public string address4 { get; set; }
        public string city { get; set; }
        public string state { get; set; }
        public string country { get; set; }
        public string postal_code { get; set; }
        public string location_id { get; set; } // get map table from oracle to format correctly (blank is fine, might be sent back to be stored)
        public string location { get; set; } // What does oracle want? (reference to the customer, must be unique unique)
        public string contact_id { get; set; } //What is this? (blank)
        public string contact_first_name { get; set; }
        public string contact_middle_name { get; set; }
        public string contact_last_name { get; set; }
        public string contact_email { get; set; }
        public string phone_country_code { get; set; }
        public string phone_area_code { get; set; }
        public string phone_number { get; set; }

    }

    public class Header
    {
        public string ordered_date { get; set; }
        public long orig_sys_document_reference { get; set; } //shopify order number.
        public string cust_po_number { get; set; }
        public string hdr_payment_terms_code { get; set; } //blank if returning customer, new customer = net-30 (or whatever day it is, check with anthony) 
        public string hdr_payment_type_code { get; set; }
        public string hdr_freight_charges_code { get; set; } //blank and check
        public string hdr_fob_point_code { get; set; } //try blank
        public string hdr_freight_carrier_code { get; set; } //try blank
        public string hdr_frieght_terms_code { get; set; } //try blank 
    }

    public class LinesList
    {
        public List<Line> line { get; set; }

    }

    public class Line
    {
        public string unit_price { get; set; }
        public string calculate_price_flag { get; set; } //"N"
        public string ppg_item_number { get; set; }
        public object customer_part_number { get; set; }
        public int ordered_quantity { get; set; }
        public string ordered_quantity_uom { get; set; }
        public string promise_date { get; set; } //start blank, might be "order date+5" 
        public string earliest_acceptable_date { get; set; } //start blank, might be "order date+5" 
        public string request_date { get; set; } //"orderDate+2"
        public string scheduled_ship_date { get; set; } //"OrderDate+2"
        public int delivery_lead_time { get; set; } //blank, or "OrderDate+5"
        public string expedited_ship_flag { get; set; } //"N", check back in later (if flag appears, send it) 
        public string freight_carrier_code { get; set; } //
        public string freight_terms_code { get; set; } //GENERIC
        public object ship_method_code { get; set; }
        public string order_discount { get; set; }
        public string Freight_Charges_Code { get; set; }
        public string fob_point_code { get; set; } //blank, but check in with Mark and team for site carrier
    }
}
