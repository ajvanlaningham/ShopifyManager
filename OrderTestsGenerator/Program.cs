using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using OfficeOpenXml;
using OrderSBSender.Services;
using SharedModels.JSONSamples.Testing;
using ShopifySharp;


namespace OrderTestsGenerator
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            var config = new ConfigurationBuilder().AddUserSecrets<Program>().Build();
            string serviceBusConnectionString = config["DevConnection"];
            string queueName = config["OrderQueue"];
            string excelFilePath = config["FilePath"];
            string shopifyStoreUrl = config["ProdShopUrl"];
            string password = config["ProdSecretKey"];

            var _queueService = new QueueService(serviceBusConnectionString, queueName);
            var _shopifyProductService = new ShopifyProductService(shopifyStoreUrl, password);

            List<OracleTestCustomer> TestList = new List<OracleTestCustomer>();
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            var package = new ExcelPackage(new FileInfo(excelFilePath));
            using (package)
            {
                var worksheet = package.Workbook.Worksheets[0];
                for (int row = 2; row <= worksheet.Dimension.End.Row; row++)
                {
                    var customer = new OracleTestCustomer();
                    customer.CUSTOMER_NAME = worksheet.Cells[row, 1].Value?.ToString();
                    customer.ACCOUNT = worksheet.Cells[row, 2].Value?.ToString();
                    customer.LOCATION = worksheet.Cells[row, 3].Value?.ToString();
                    customer.SITE_NUMBER = worksheet.Cells[row, 4].Value?.ToString();
                    customer.ADDRESS_LINE_1 = worksheet.Cells[row, 5].Value?.ToString();
                    customer.ADDRESS_LINE_2 = worksheet.Cells[row, 6].Value?.ToString();
                    customer.ADDRESS_LINE_3 = worksheet.Cells[row, 7].Value?.ToString();
                    customer.ADDRESS_LINE_4 = worksheet.Cells[row, 8].Value?.ToString();
                    customer.CITY = worksheet.Cells[row, 9].Value?.ToString();
                    customer.STATE = worksheet.Cells[row, 10].Value?.ToString();
                    customer.PROVINCE = worksheet.Cells[row, 11].Value?.ToString();
                    customer.ZIP_CODE = worksheet.Cells[row, 12].Value?.ToString();
                    customer.COUNTRY = worksheet.Cells[row, 13].Value?.ToString();
                    TestList.Add(customer);
                }
            }


            for (int i = 0; i < 1; i++) //Change 1 to TestList.Count()
            {
                OracleTestCustomer customer = TestList[i];
                var orderObj = new SharedModels.Models.OrderObject()
                {
                    custom_rec = new SharedModels.Models.CustomerReccord()
                    {
                        cust_acct_num = customer.ACCOUNT,
                        site_use_id = null, //customer.SITE_NUMBER,
                        address1 = customer.ADDRESS_LINE_1,
                        address2 = customer.ADDRESS_LINE_2,
                        address3 = customer.ADDRESS_LINE_3,
                        address4 = customer.ADDRESS_LINE_4,
                        city = customer.CITY,
                        state = customer.STATE,
                        postal_code = customer.ZIP_CODE,
                        location_id = "",
                        location = "",
                        contact_id = "",
                        contact_first_name = customer.CUSTOMER_NAME,
                        contact_middle_name = "",
                        contact_last_name = "",
                        contact_email = "",
                        phone_country_code = "",
                        phone_area_code = "",
                        phone_number = "",
                    },

                    header = new SharedModels.Models.Header()
                    {
                        ordered_date = ConvertDateTimeString(DateTime.Now),
                        orig_sys_document_reference = 12345, //idiot luggage code 
                        cust_po_number = "",
                        hdr_payment_terms_code =  "net-30",
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

                    p_ou = "US_CNR_OU" 
                };
                var LinesList = new List<SharedModels.Models.Line>();
                List<Product> Products = await GetRandomProduct(_shopifyProductService);

                foreach (Product product in Products)
                {
                    ProductVariant variant = product.Variants.FirstOrDefault();
                    if (variant.SKU != null && variant.SKU.Contains("Panel"))
                    {
                        variant = product.Variants.ToList()[1];
                    }
                    SharedModels.Models.Line newLine = new SharedModels.Models.Line()
                    {
                        unit_price = variant.Price.Value.ToString(),
                        calculate_price_flag = "N",
                        ppg_item_number = variant.SKU,
                        customer_part_number = variant.InventoryItemId,
                        ordered_quantity = 1,
                        ordered_quantity_uom = "lbs", 
                        promise_date = "", 
                        earliest_acceptable_date = "",
                        request_date = ConvertDateTimeString(DateTime.Now),
                        scheduled_ship_date = ConvertDateTimeString(DateTime.Now.AddDays(2)),
                        delivery_lead_time = 5, 
                        expedited_ship_flag = "N",  
                        freight_carrier_code = "GENERIC",
                        freight_terms_code = null,
                        ship_method_code = "",
                        order_discount = "",
                        Freight_Charges_Code = null,
                        fob_point_code = ""
                    };
                    LinesList.Add(newLine);
                }

                orderObj.lines_list.line = LinesList;
                var json = JsonConvert.SerializeObject(orderObj);
                Console.WriteLine(json);
                await _queueService.SendMessageAsync(orderObj);
                Console.WriteLine($"Order sent to queue");
            }
            
        }

        public static async Task<List<Product>> GetRandomProduct(ShopifyProductService productService)
        {
            var random = new Random(); //summon the chaos wizard
            var count = random.Next(1, 6); // 1d6 chaos damage

            Console.WriteLine($"rolled {count} chaos damage");
            var productList = new List<Product>();

            var bigProductList = await productService.GetAllProductsAsync();

            for (int i = 0; i < count; i++)
            {
                var index = random.Next(0, bigProductList.Count);
                productList.Add(bigProductList[index]);
            }

            return productList;
        }

        public static string ConvertDateTimeString(DateTime dateTime)
        {
            return dateTime.ToString("yyyy-MM-dd hh:mm:ss");
        }

        public static string CheckVariantSku(string productSku)
        {
            string sku;
            if(productSku.Contains("Panel"))
            {
                sku = productSku.Split("-").First();
            }
            else
            {
                sku = productSku;
            }
            return sku;
        }
    }
}