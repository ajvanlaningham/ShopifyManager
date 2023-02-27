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

        }

        public async Task SendStatusAsync()
        {

        }
        public async Task GetCustomerAsync()
        {

        }
    }
}
