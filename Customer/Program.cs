using System;
using System.Net.Http;
using Newtonsoft.Json;
using Microsoft.Azure.ServiceBus;
using ShopifySharp;
using Microsoft.Extensions.Configuration;
using ShopifyCustomer.Models;

namespace ShopifyCustomer
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            var config = new ConfigurationBuilder().AddUserSecrets<Program>().Build();
            string devShopifyStoreUrl = config["DevShopUrl"];
            string prodShopifyStoreUrl = config["ProdShopUrl"];
            string prodPassword = config["ProdSecretKey"];
            string apiKey = config["APIKey"];
            string password = config["DevSecretKey"];
            string serviceBusConnectionString = config["DevConnection"];
            string queueName = config["DiscountQueue"];

            var _devCustomerService = new CustomerService(devShopifyStoreUrl, password);
            var _prodCustomerService = new CustomerService(prodShopifyStoreUrl, prodPassword);
            var _productService = new ShopifyProductService(devShopifyStoreUrl, apiKey);
            var _bigListGetter = new ShopifyCustomerService(_prodCustomerService);

            List<Customer> totalCustomers = await _bigListGetter.GetAllCustomersAsync();
            int i = totalCustomers.Count();
            foreach (Customer customer in totalCustomers)
            {
                _devCustomerService.CreateAsync(customer);
                _devCustomerService.GetAsync(customer.Id);
                Console.WriteLine($"Created Customer {customer.FirstName} {customer.LastName}... {--i} more to go");
                Thread.Sleep(100);
            }
            Console.WriteLine("Finished creating customers in dev store");


            // Deserialize the customer JSON into a list of Adjustments
            //var jsonPath = @"C:\Users\u526137\source\repos\Shopify\Oracle\ShopifyProduct\SharedModels\JSONSamples\Adjustment.json";
            //var json = File.ReadAllText(jsonPath);
            //RootAdjustment AdjustmentRoot = JsonConvert.DeserializeObject<RootAdjustment>(json);
                        
            //List<Customer> customers = await _bigListGetter.GetAllCustomersAsync();

            //if (customers != null)
            //{
            //    try
            //    {
            //        foreach (Adjustment adjustment in AdjustmentRoot.adjustment_list.adjustment)
            //        {
            //            Customer customer = GetCorrectCustomer(customers, adjustment);
            //            if (customer != null)
            //            {
            //                Console.WriteLine($"Found customer {customer.FirstName} {customer.LastName}");


            //            }
            //            else
            //            {
            //                //TODO create a new customer in shopify and add to customer list. 


            //            }
            //        }
            //    }
            //    catch (Exception ex)
            //    {
            //        Console.WriteLine("Process error");
            //        Console.WriteLine(ex.Message);
            //    }
            //}
           
        }

        //private static Customer GetCorrectCustomer(List<Customer> customerList, Adjustment adjustment)
        //{

        //}
    }
}
