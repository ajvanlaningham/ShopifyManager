using SharedModels.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderStatusListener
{
    
    class OrderStatusUpdater 
    {
        private readonly OrderConfirmation _confirmation;
        
        public OrderStatusUpdater(OrderConfirmation confirmation)
        {
            _confirmation = confirmation;
        }

        public async Task UpdateCustomerAsync()
        {
            Console.WriteLine($"updated customer or w/e with {_confirmation.LocationId}");
        }

        public async Task SendStatusAsync()
        {
            Console.WriteLine($"Sending the status with this number {_confirmation.OrderNumber}");
        }
        public async Task GetCustomerAsync()
        {

        }
    }
}
