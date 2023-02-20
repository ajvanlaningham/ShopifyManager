﻿using System;
using System.Net.Http;
using Newtonsoft.Json;
using Microsoft.Azure.ServiceBus;
using Microsoft.Extensions.Configuration;
using OrderSBSender.Services;
using ShopifySharp;
using SharedModels.Models.APIModel;

namespace Orders
{
    internal class Program
    {
        static async Task Main(string[] args)
        {

            var config = new ConfigurationBuilder().AddUserSecrets<Program>().Build();
            // Replace with your Shopify store URL
            string shopifyStoreUrl = config["ProdShopUrl"];

            // Replace with your API key and password
            string apiKey = config["ProdAPIKey"];
            string password = config["ProdSecretKey"];
            string serviceBusConnectionString = config["DevConnection"];
            string queueName = config["OrderQueue"];

            // Create a new HttpClient
            using (var client = new HttpClient())
            {
                // Set the base URL for the Shopify API
                client.BaseAddress = new Uri(shopifyStoreUrl + "/admin/");

                // Set the authentication headers
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(apiKey + ":" + password)));

                // Send a GET request to the orders endpoint
                var response = client.GetAsync("orders.json?limit=1").Result;
                var _queueService = new QueueService(serviceBusConnectionString, queueName);
                var _customerService = new CustomerService(shopifyStoreUrl, password);

                // If the response was successful, deserialize the JSON content into a dynamic object
                if (response.IsSuccessStatusCode)
                {
                    OrdersRoot orders = JsonConvert.DeserializeObject<OrdersRoot>(response.Content.ReadAsStringAsync().Result);

                    Console.WriteLine(orders);

                     //Loop through the orders
                    foreach (var order in orders.orders)
                    {

                        var orderObj = new SharedModels.Models.OrderObject()
                        {
                            custom_rec = new SharedModels.Models.CustomerReccord()
                            {
                                cust_acct_num = await GetAccountNumber(_customerService, order.customer.id),
                                site_use_id = await GetSiteUseID(_customerService, order.customer.id),
                                address1 = order.shipping_address.address1,
                                address2 = order.shipping_address.address2 ?? "",
                                address3 = "",
                                address4 = "",
                                city = order.shipping_address.city,
                                state = order.shipping_address.province,
                                postal_code = order.shipping_address.zip,
                                location_id = "", //mapping table for the oracle format
                                location = "",// test blank
                                contact_id = "", //blank?
                                contact_first_name = order.customer.first_name,
                                contact_middle_name = "",
                                contact_last_name = order.customer.last_name,
                                contact_email = order.customer.email,
                                phone_country_code = GetPhoneNumberComponent(order.customer.phone, "countrycode"),
                                phone_area_code = GetPhoneNumberComponent(order.customer.phone, "areacode"),
                                phone_number = GetPhoneNumberComponent(order.customer.phone, "localnumber"),
                            },

                            header = new SharedModels.Models.Header()
                            {
                                ordered_date = ConvertDateTimeString(order.processed_at),
                                orig_sys_document_reference = order.id, //shopify order number
                                cust_po_number = order.note ?? "",
                                hdr_payment_terms_code = await GetSiteUseID(_customerService, order.customer.id) == "0" ? "net-30" : "",
                                hdr_payment_type_code = "",
                                hdr_freight_charges_code = "",
                                hdr_fob_point_code = "",
                                hdr_freight_carrier_code = "",
                                hdr_frieght_terms_code = "",
                            },

                            lines_list = new SharedModels.Models.LinesList()
                            {
                                line = new List<SharedModels.Models.Line>()
                            },

                            p_ou = "US_CNR_OU" // TODO:  double check re: canadian customers 

                        };

                        var LinesList = new List<SharedModels.Models.Line>();

                        foreach (SharedModels.Models.APIModel.LineItem line in order.line_items)
                        {
                            SharedModels.Models.Line newLine = new SharedModels.Models.Line()
                            {
                                unit_price = line.price,
                                calculate_price_flag = "N",
                                ppg_item_number = line.sku,
                                customer_part_number = line.product_id,
                                ordered_quantity = line.quantity,
                                ordered_quantity_uom = GetQuantityConverter(line.grams), //ea, lbs, gal
                                promise_date = "", //hopfully blank, check Ordered date +5
                                earliest_acceptable_date = "", //hopfully blank, check Ordered date +5,
                                request_date = ConvertDateTimeString(order.created_at),
                                scheduled_ship_date = ConvertDateTimeString(order.created_at.AddDays(2)), //+2
                                delivery_lead_time = 5, //+5
                                expedited_ship_flag = "N", //reevalute later 
                                freight_carrier_code = "GENERIC",//order.shipping_lines.FirstOrDefault().carrier_identifier,
                                freight_terms_code = null, //order.shipping_lines.FirstOrDefault().code,
                                ship_method_code = order.shipping_lines.FirstOrDefault().delivery_category,
                                order_discount = order.shipping_lines.FirstOrDefault().discounted_price,
                                Freight_Charges_Code = order.shipping_lines.FirstOrDefault().price,
                                fob_point_code = ""  //mystery field, check with mark
                            };
                            LinesList.Add(newLine);
                        }

                        orderObj.lines_list.line = LinesList;
                        var json = JsonConvert.SerializeObject(orderObj);
                        Console.WriteLine(json);
                        await _queueService.SendMessageAsync(orderObj);
                        Console.WriteLine($"Order {order.id} sent to queue");
                    };
                    Console.WriteLine("all orders sent to queue");
                    Console.WriteLine("Press enter to close");
                    Console.ReadLine();
                }
                else
                {
                    Console.WriteLine("Failed to retrieve orders. Error: " + response.ReasonPhrase);
                }
            }
        }

        public async static Task<string> GetAccountNumber(CustomerService _service, long customerID)
        {
            ShopifySharp.Customer customer = await _service.GetAsync(customerID);
            string[] tags = customer.Tags.Split(',').ToArray();
            foreach (string tag in tags)
            {
                if (tag.StartsWith("AR_"))
                {
                    return tag.Substring(3);
                }
                else if (tag.Contains("INC") || tag.Contains("REF"))
                {
                    return tag;
                }                
            }
            return "0";
        }

        public async static Task<string> GetSiteUseID(CustomerService _service, long customerID)
        {
            ShopifySharp.Customer customer = await _service.GetAsync(customerID);
            string[] tags = customer.Tags.Split(',').ToArray();
            foreach (string tag in tags)
            {
                if (tag.StartsWith("SUID"))
                {
                    return tag.Substring(4);
                }
            }
            return "0";
        }

        public static string GetQuantityConverter(int grams)
        {
            //TODO: update switch case in case of alternatives/new products
            switch(grams)
            {
                case 454:
                    return "LBS";
            }
            return "LBS";
        }

        public static string ConvertDateTimeString(DateTime dateTime)
        {
            return dateTime.ToString("yyyy-MM-dd hh:mm:ss");
        }
        public static string GetPhoneNumberComponent(string phoneNumber, string component)
        {
            // Check if the input phone number is valid
            if (string.IsNullOrEmpty(phoneNumber))
            {
                return "";
            }

            // Remove any non-numeric characters from the phone number
            var numericPhoneNumber = new String(phoneNumber.Where(Char.IsDigit).ToArray());

            // Initialize the country code, area code, and local number variables
            string countryCode = "";
            string areaCode = "";
            string localNumber = "";

            // Check the length of the numeric phone number
            if (numericPhoneNumber.Length == 10)
            {
                // If the length is 10, the phone number has no country code and the area code is the first 3 digits
                areaCode = numericPhoneNumber.Substring(0, 3);
                localNumber = numericPhoneNumber.Substring(3);
            }
            else if (numericPhoneNumber.Length == 11)
            {
                // If the length is 11, the phone number has a country code and the area code is the next 3 digits
                countryCode = numericPhoneNumber.Substring(0, 1);
                areaCode = numericPhoneNumber.Substring(1, 3);
                localNumber = numericPhoneNumber.Substring(4);
            }
            else
            {
                // If the length is not 10 or 11, return an empty string
                return "";
            }

            // Return the requested component of the phone number
            switch (component.ToLower())
            {
                case "countrycode":
                    return countryCode;
                case "areacode":
                    return areaCode;
                case "localnumber":
                    return localNumber;
                default:
                    return "";
            }
        }

    }
}