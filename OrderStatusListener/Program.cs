using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Microsoft.Azure.ServiceBus;
using System.Text;
using SharedModels.Models;

namespace OrderStatusListener
{
    internal class Program
    {
        const string connectionString = "";
        const string queueName = "";
        static IQueueClient queueClient;
        static async Task Main(string[] args)
        {
            var config = new ConfigurationBuilder().AddUserSecrets<Program>().Build();

            string SBConnection = config["SBConnectionString"];
            string queueName = config["QueueName"];
            queueClient = new QueueClient(SBConnection, queueName);

            RegisterMessageHandler();

            Console.ReadLine();

            await queueClient.CloseAsync();
        }

        static void RegisterMessageHandler()
        {
            var messageHandlerOptions = new MessageHandlerOptions(ExceptionReceivedHandler)
            {
                MaxConcurrentCalls = 1,
                AutoComplete = false
            };

            queueClient.RegisterMessageHandler(ProcessMessageAsync, messageHandlerOptions);
        }

        static async Task ProcessMessageAsync(Message message, CancellationToken token)
        {
            var payload = Encoding.UTF8.GetString(message.Body);

            try
            {
                var orderStatus = JsonConvert.DeserializeObject<OrderConfirmation>(payload);
                if (orderStatus.ErrorMessage == "" || orderStatus.ErrorMessage == null)
                {
                    OrderStatusUpdater updater = new OrderStatusUpdater(orderStatus);
                    updater.UpdateCustomerAsync();
                    updater.SendStatusAsync();
                }
                else
                {
                    Console.WriteLine("Error returned from Oracle Order creation");
                    Console.WriteLine($"Error message: {orderStatus.ErrorMessage}");
                }

                await queueClient.CompleteAsync(message.SystemProperties.LockToken);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error while processing message: {ex.Message}");
                await queueClient.AbandonAsync(message.SystemProperties.LockToken);
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