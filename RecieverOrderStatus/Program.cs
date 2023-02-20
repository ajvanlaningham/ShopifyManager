using Microsoft.Azure.ServiceBus;
using Microsoft.Extensions.Configuration;

namespace RecieverOrderStatus
{
    internal class Program
    {
        static IQueueClient _queueClient;
        static async Task Main(string[] args)
        {
            var config = new ConfigurationBuilder().AddUserSecrets<Program>().Build();
            // Replace with your Shopify store URL
            string shopifyStoreUrl = config["DevShopUrl"];

            // Replace with your API key and password
            string apiKey = config["APIKey"];
            string password = config["DevSecretKey"];
            string serviceBusConnectionString = config["QueueConnection"];
            string queueName = config["OrderStatusQueue"];
            string queueKey = config["OrderStatusKey"];

            _queueClient = new QueueClient(serviceBusConnectionString, queueName);
        }
    }
}