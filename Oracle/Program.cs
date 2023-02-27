﻿using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using SharedModels.Models.APIModel;
using ShopifySharp;
using System.Collections.Generic;

namespace ShopifyProduct

{
    public class Program
    {
        public static async Task Main(string[] args)
        {

            if (args.Length == 0)
            {
                Console.WriteLine("no argument");
                return;
            }
            var pricingJson = args[0];

            var config = new ConfigurationBuilder().AddUserSecrets<Program>().Build();

            string ProdShopUrl = config["ProdShopUrl"];
            string ProdSecret = config["ProdSecretKey"];
            string devshopUrl = config["DevShopUrl"];
            string devaccessToken = config["DevSecretKey"];

            IICSPricing MessageList = JsonConvert.DeserializeObject<IICSPricing>(pricingJson);
            Console.WriteLine("Deserialized json file");

            try
            {
                // Create a new Shopify service with the given shop URL and access token
                var service = new ShopifyProductService(devshopUrl, devaccessToken);
                var variantService = new ProductVariantService(devshopUrl, devaccessToken);
                var shopifyProducts = await service.GetAllProductsAsync();
                Console.WriteLine("Product list from Shopify store");
                Console.WriteLine($"There are {shopifyProducts.Count} products in the List");
               

                foreach (Pricing pricingItem in MessageList.pricing_list.pricing)
                {
                    //break up sku string 
                    string baseSku = pricingItem.item.Split('/').First();
                    //check if the product exists in shopify
                    Product shopifyProduct = shopifyProducts.Find(p => p.Variants.FirstOrDefault().SKU.Split('-').First() == baseSku);
                    

                    if (shopifyProduct != null)
                    {
                        Console.WriteLine($"{pricingItem.item} Found");
                        
                        //update the product in shopify with the pricing infomation
                        ProductVariant currentVariant = GetCurrentVariant(pricingItem, shopifyProduct);
                        Console.WriteLine("Found the right varient to edit");
                        bool newVariant = currentVariant.SKU == null;
                        string skuString = GetProductSKUString(pricingItem);
                        
                        if (currentVariant.Price != pricingItem.uprice || currentVariant.SKU != skuString)
                        {
                            var productVariant = new ProductVariant();
                            //if minumum quantity, sku is the pricingItem.Item + "-min_qty", if no min qty, sku is just pricing.item.
                            //TODO : CLEAN THIS UP. YOU CAN DO THIS IN LIKE HALF THE LINES
                            if (pricingItem.min_qty != null)
                            {
                                var productVariant1 = new ProductVariant()
                                {
                                    Title = pricingItem.min_qty.ToString(),
                                    Price = pricingItem.uprice,
                                    CompareAtPrice = currentVariant.CompareAtPrice ?? null,
                                    FulfillmentService = currentVariant.FulfillmentService ?? null,
                                    Barcode = currentVariant.Barcode ?? null,
                                    SKU = skuString,
                                    Option1 = currentVariant.Option1 ?? pricingItem.min_qty.ToString(),
                                    Option2 = currentVariant.Option2 ?? null,
                                    Option3 = currentVariant.Option3 ?? null
                                };
                                productVariant = productVariant1;
                            }
                            else
                            {
                                var productVariant2 = new ProductVariant()
                                {
                                    Title = pricingItem.min_qty.ToString(),
                                    Price = pricingItem.uprice,
                                    CompareAtPrice = currentVariant.CompareAtPrice ?? null,
                                    FulfillmentService = currentVariant.FulfillmentService ?? null,
                                    Barcode = currentVariant.Barcode ?? null,
                                    SKU = pricingItem.item,
                                    Option1 = currentVariant.Option1 ?? pricingItem.min_qty.ToString(),
                                    Option2 = currentVariant.Option2 ?? null,
                                    Option3 = currentVariant.Option3 ?? null
                                };
                                productVariant = productVariant2;
                            }
                            Console.WriteLine("created new Variant. Updating in shopify...");

                            if ( !newVariant)
                            {
                                long id = currentVariant.Id.Value;
                                productVariant.Id = id;
                                await variantService.UpdateAsync(id, productVariant);
                                Console.WriteLine($"Product {pricingItem.item} has been updated in Shopify store");
                            }
                            else
                            {
                                await variantService.CreateAsync(shopifyProduct.Id.Value, productVariant);
                                shopifyProduct = await service.GetProductById(shopifyProduct.Id.Value);
                                int i = 1;
                                string defaultTitle = "Default Title";
                                
                                foreach (var variant in shopifyProduct.Variants)
                                {
                                    if (variant.Title == defaultTitle )
                                    {
                                       string title = pricingItem.min_qty.ToString();
                                       variant.Title = title;
                                       variant.Option1 = title;
                                       await service.UpdateProductAsync(shopifyProduct.Id.Value, shopifyProduct);
                                    }
                                    variant.Position = i;
                                    i++;
                                }
                                shopifyProduct = await service.GetProductById(shopifyProduct.Id.Value);

                                ProductVariant updatedVariant = new ProductVariant();
                                foreach (ProductVariant variant in shopifyProduct.Variants)
                                {
                                    if (variant.Title == pricingItem.min_qty.ToString())
                                    {
                                        updatedVariant = variant;
                                        Console.WriteLine("Got updated Variant");
                                    }
                                }
                                if (updatedVariant != null)
                                {
                                    var _inventoryLevelService = new InventoryLevelService(devshopUrl, devaccessToken);
                                    InventoryLevel response = await _inventoryLevelService.SetAsync(new InventoryLevel
                                    {
                                        //the location ID was a real nightmare to find if you didn't set the store up with one originally. If you ever need it, it's in the shopify admin portal > settings > location and then you have to pull it from the URL. 
                                        //TODO : load the location ID in to the config file. 
                                        LocationId = 77496058160,
                                        Available = 1000000,
                                        InventoryItemId = updatedVariant.InventoryItemId
                                    });

                                    Console.WriteLine("Updated Variant inventory level");
                                }

                                Console.WriteLine($"Product {pricingItem.item} has been created in shopify store");
                            }
                        }
                        else
                        {
                            Console.WriteLine("No update needed");
                        }
                    }
                    else
                    {
                        Console.WriteLine("shopify Product was null");
                        //TODO: create a new product
                    }
                }                
            }
            catch (Exception ex)
            {
                    Console.WriteLine("Error connecting to the store:");
                    Console.WriteLine(ex.Message);
            }
        }

        private static ProductVariant GetCurrentVariant(Pricing pricingItem, Product product)
        {
            List<ProductVariant> variants = (List<ProductVariant>)product.Variants;

            if (pricingItem.min_qty.GetValueOrDefault() != 0)
            {
                foreach (var variant in variants)
                {
                    if (pricingItem.min_qty.ToString() == variant.SKU.Split('-').Last() || variant.SKU == pricingItem.item )
                    {
                        return variant;
                    }
                }
                ProductVariant newVariant = new ProductVariant();
                return newVariant;
            }
            
            return variants.FirstOrDefault();            
        }

        private static string GetProductSKUString(Pricing pricingItem)
        {
            if (pricingItem.min_qty == null || pricingItem.min_qty == 0)
            {
                return pricingItem.item;
            }
            else
            {
                return pricingItem.item + "-" + pricingItem.min_qty.ToString();
            }
        }


        private static async Task CompareAndUpdate(List<Product> list1, List<Product> list2, ShopifyProductService productService)
        {

            
            var WaitTime = 100;
            var newProducts = list1.Except(list2).Concat(list2.Except(list1));

            while (newProducts.Any())
            {              
                foreach (Product product in newProducts)
                {
                    try
                    {
                        var newProduct = new Product();
                        {
                            newProduct = product;
                        }
                        Console.WriteLine($"Creating new Product: {product.Title} in Dev Store");
                        await productService.CreateProductAsync(newProduct);
                        Thread.Sleep(WaitTime);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Error while creating new products");
                        Console.WriteLine(ex.Message);
                        await Task.Delay(WaitTime);
                    }
                }
            }
        }

        private static async Task RemoveDuplicates(List<Product> products, ShopifyProductService service)
        {
            var uniqueProducts = new Dictionary<string, long>();
            //var uniqueProducts = new Dictionary<long, string>();
            var WaitTime = 100;

            foreach (Product product in products)
            {
                try
                {
                    if (product.Id != null)
                    {
                        if (uniqueProducts.ContainsKey(product.Title))
                        {
                            Console.WriteLine($"Duplicate product found. Deleting {product.Title}");
                            await service.DeleteAsync(product.Id.Value);
                            Thread.Sleep(WaitTime);
                        }
                        else
                        {
                            uniqueProducts[product.Title] = product.Id.Value;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error while Deleting duplicates");
                    Console.WriteLine(ex.Message);
                    await Task.Delay(WaitTime);
                }
            }
        }

        private async Task UpdateOrCreateProductsFromJson(List<Product> jsonProducts, List<Product> currentProducts, ShopifyProductService service)
        {
            foreach (var product in jsonProducts)
            {
                var tempProduct = currentProducts.FirstOrDefault(p => p.Title == product.Title);
                if (tempProduct == null)
                {
                    await service.CreateProductAsync(tempProduct);
                }
                else if (!AreProductsEqual(product, tempProduct))
                {
                    product.Id = tempProduct.Id;
                    await service.UpdateProductAsync(product.Id.Value,product);
                }
            }
        }

        private bool AreProductsEqual(Product product1, Product product2)
        {
            product1.Id = product2.Id;
            return product1.Equals(product2);
        }
    }
}