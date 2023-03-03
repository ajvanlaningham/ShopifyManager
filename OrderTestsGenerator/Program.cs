using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using OfficeOpenXml;
using OrderSBSender.Services;
using SharedModels.JSONSamples.Testing;
using SharedModels.Models;
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
            string shopifyStoreUrl = config["DevShopUrl"];
            string password = config["DevSecretKey"];

           

            var random = new Random();
            int rowNumber =random.Next(2,278);

            var _queueService = new QueueService(serviceBusConnectionString, queueName);
            var _shopifyProductService = new ShopifyProductService(shopifyStoreUrl, password);

            List<OracleTestCustomer> TestList = new List<OracleTestCustomer>();
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            var package = new ExcelPackage(new FileInfo(excelFilePath));
            using (package)
            {
                var worksheet = package.Workbook.Worksheets[0];
                for (int row = rowNumber ; row <= worksheet.Dimension.End.Row; row++)
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
                string testJson = File.ReadAllText(@"C:\Users\u526137\source\repos\Shopify\Oracle\ShopifyProduct\SharedModels\JSONSamples\Testing\OrdersOut\Order1.json");
                OrderObject orderObj = JsonConvert.DeserializeObject<OrderObject>(testJson);
                //var orderObj = new SharedModels.Models.OrderObject()
                //{
                //    CustomRec = new SharedModels.Models.CustomerReccord()
                //    {
                //        CustAcctNum = customer.ACCOUNT,
                //        SiteUseId = null, //customer.SITE_NUMBER,
                //        Address1 = customer.ADDRESS_LINE_1,
                //        Address2 = customer.ADDRESS_LINE_2,
                //        Address3 = customer.ADDRESS_LINE_3,
                //        Address4 = customer.ADDRESS_LINE_4,
                //        City = customer.CITY,
                //        State = customer.STATE,
                //        PostalCode = customer.ZIP_CODE,
                //        CountryCode = customer.COUNTRY,
                //        LocationId = "",
                //        Location = "",
                //        ContactId = "",
                //        FirstName = customer.CUSTOMER_NAME,
                //        MiddleName = "",
                //        LastName = "",
                //        Email = "",
                //        PhoneCountryCode = "",
                //        AreaCode = "",
                //        PhoneNumber = "",
                //    },

                //    Header = new SharedModels.Models.Header()
                //    {
                //        OrderedDate = ConvertDateTimeString(DateTime.Now),
                //        OrderNumber = 12345, //idiot luggage code 
                //        PONumber = "",
                //        PaymentTermsCode =  "net-30",
                //        PaymentTypeCode = "",
                //        FreightChargesCode = "",
                //        FobPointCode = "",
                //        FreightCarrierCode = "",
                //        FreightTermsCode = "",
                //    },

                //    LinesList = new SharedModels.Models.LinesList()
                //    {
                //        Lines = new List<SharedModels.Models.Line>()
                //    },

                //    POU = "US_CNR_OU" 
                //};
                //var LinesList = new List<SharedModels.Models.Line>();
                //List<Product> Products = await GetRandomProduct(_shopifyProductService);

                //foreach (Product product in Products)
                //{
                //    ProductVariant variant = product.Variants.FirstOrDefault();
                //    if (variant.SKU != null && variant.SKU.Contains("Panel"))
                //    {
                //        variant = product.Variants.ToList()[1];
                //    }
                //    SharedModels.Models.Line newLine = new SharedModels.Models.Line()
                //    {
                //        UnitPrice = variant.Price.Value.ToString(),
                //        CalcPriceFlag = "N",
                //        SKUBase = variant.SKU,
                //        CustomerPartNumber = variant.InventoryItemId,
                //        OrderedQuantity = 1,
                //        UnitOfMeasure = "lbs", 
                //        PromiseDate = "", 
                //        EarliestAcceptableDate = ConvertDateTimeString(DateTime.Now.AddDays(5)),
                //        RequestDate = ConvertDateTimeString(DateTime.Now),
                //        ShipDate = null,
                //        LeadTime = 5, 
                //        ExpediteFlag = "N",  
                //        FreightCarrierCode = "GENERIC",
                //        FreightTermsCode = null,
                //        ShipMethodCode = "",
                //        OrderDiscount = "",
                //        FreightChargesCode = null,
                //        FobPointCode = ""
                //    };
                //    LinesList.Add(newLine);
                //}

                //orderObj.LinesList.Lines = LinesList;
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
                if (bigProductList[index].Variants.FirstOrDefault().SKU.Contains('/'))
                {
                    productList.Add(bigProductList[index]);
                }
                else
                {
                    Product newProduct = new Product();
                    var newSku = bigProductList[index].Variants.Last().SKU.Split('-').First();
                    newProduct = bigProductList[index];
                    newProduct.Variants.First().SKU = newSku;
                    productService.UpdateProductAsync(bigProductList[index].Id.Value, newProduct);
                    if (newProduct.Variants.First().SKU.Contains('/'))
                    {
                        productList.Add(newProduct);
                    }
                }
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