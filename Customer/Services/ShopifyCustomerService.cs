using ShopifySharp;
using ShopifySharp.Lists;
using ShopifySharp.Filters;

class ShopifyCustomerService
{
    private readonly CustomerService _customerService;

    public ShopifyCustomerService(CustomerService service)
    {
        _customerService = service;
    }

    public async Task<List<Customer>> GetAllCustomersAsync()
    {
        var retries = 5;
        var waitTime = 100;
        var customers = new List<Customer>();
        long? lastId = 0;

        try
        {
            while (lastId >= 0)
            {
                var filter = new CustomerListFilter
                {
                    SinceId = lastId,
                };

                var customerList = await _customerService.ListAsync(filter);
                if (customerList != null && customerList.Items.Any())
                {
                    customers.AddRange(customerList.Items);
                    lastId = customerList.Items.Last().Id;
                }
                else
                {
                    break;
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error while receiving customer list from {_customerService.ToString}");
            Console.WriteLine(ex.Message);
            Thread.Sleep(waitTime);
        }

        return customers;
    }
}
