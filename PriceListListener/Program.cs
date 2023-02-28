using System;
using System.Text;
using System.Threading;
using Microsoft.Azure.ServiceBus;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using SharedModels.Models.APIModel;
using Microsoft.Azure.ServiceBus.InteropExtensions;
using ShopifyProduct;

namespace PriceListListener
{
    class Program
    {
        static IQueueClient _queueClient;
        static async Task Main(string[] args)
        {
            var config = new ConfigurationBuilder().AddUserSecrets<Program>().Build();

            var serviceBusConnectionString = config["SBConnectionString"];
            var queueName = config["QueueName"];
            _queueClient = new QueueClient(serviceBusConnectionString, queueName);

            var messageHandlerOptions = new MessageHandlerOptions(ExceptionReceivedHandler)
            {
                MaxConcurrentCalls = 1,
                AutoComplete = false
            };

            _queueClient.RegisterMessageHandler(ProcessMessagesAsync, messageHandlerOptions);

            Console.ReadLine();

            await _queueClient.CloseAsync();
        }

        private async static Task ProcessMessagesAsync(Message message, CancellationToken token)
        {
            
           var body = message.GetBody<string>();
           if (body != null)
           {
                IICSPricing pricing = JsonConvert.DeserializeObject<IICSPricing>(body);

                await ShopifyProduct.Program.Main(new string[] { JsonConvert.SerializeObject(pricing) });
                await _queueClient.CompleteAsync(message.SystemProperties.LockToken);
            }
        }

        static Task ExceptionReceivedHandler(ExceptionReceivedEventArgs exceptionReceivedEventArgs)
        {
            Console.WriteLine($"Message handler encountered an exception {exceptionReceivedEventArgs.Exception}.");
            var context = exceptionReceivedEventArgs.ExceptionReceivedContext;
            Console.WriteLine("Exception context for troubleshooting:");
            Console.WriteLine($"- Endpoint: {context.Endpoint}");
            Console.WriteLine($"- Entity Path: {context.EntityPath}");
            Console.WriteLine($"- Executing Action: {context.Action}");
            return Task.CompletedTask;
        }
    }
}
