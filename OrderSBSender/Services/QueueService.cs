using Microsoft.Azure.ServiceBus;
using Microsoft.Extensions.Configuration;
using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace OrderSBSender.Services
{
    public class QueueService
    {
        public string ConnectionString { get; set; }
        public string QueueName { get; set; }
        public QueueService(string connectionString, string queuename)
        {
            ConnectionString = connectionString;
            QueueName = queuename;
        }
        public async Task SendMessageAsync<T>(T serviceBusMessage)
        {
            try
            {
                var queueClient = new QueueClient(ConnectionString, QueueName);
                string messageBody = JsonSerializer.Serialize(serviceBusMessage);
                var message = new Message(Encoding.UTF8.GetBytes(messageBody));

                await queueClient.SendAsync(message);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }
    }
}
