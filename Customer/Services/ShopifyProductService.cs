using ShopifySharp;
using ShopifySharp.Lists;
using ShopifySharp.Filters;

class ShopifyProductService
{
    private readonly ProductService _productService;

    public ShopifyProductService(string shopUrl, string accessToken)
    {
        _productService = new ProductService(shopUrl, accessToken);
    }

    public async Task<ListResult<Product>> ListProductsAsync()
    {
        // Retrieve all products from the store
        var productList = await _productService.ListAsync();
        return productList;
    }

    public async Task<List<Product>> GetAllProductsAsync()
    {
        var Retries = 5;
        var WaitTime = 100;
        var products = new List<Product>();
        long? lastId = 0;

        try
        {
            while (lastId >= 0)
            {
                var filter = new ProductListFilter
                {
                    SinceId = lastId,
                };

                var productList = await _productService.ListAsync(filter);
                if (productList != null && productList.Items.Any())
                {
                    products.AddRange(productList.Items);
                    lastId = productList.Items.Last().Id;
                }
                else
                {
                    break;
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error while receiving product list from {_productService.ToString}");
            Console.WriteLine(ex.Message);
            Thread.Sleep(WaitTime);
        }

        return products;
    }


    public async Task<Product> GetProductById(long id)
    {
        // Retrieve a specific product from the store
        var product = await _productService.GetAsync(id);
        return product;
    }

    public async Task<Product> CreateProductAsync(Product product)
    {
        // Create a new product in the store
        var createdProduct = await _productService.CreateAsync(product);
        return createdProduct;
    }

    public async Task<Product> UpdateProductAsync(long id, Product product)
    {
        // Update an existing product in the store
        var updatedProduct = await _productService.UpdateAsync(id, product);
        return updatedProduct;
    }


    public async Task DeleteAsync(long id)
    {
        // Delete a product from the store
        await _productService.DeleteAsync(id);
    }
}
