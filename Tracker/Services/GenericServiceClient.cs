using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Tracker.Models;
namespace Tracker.Services
{
    public class GenericServiceClient
    {
        private static readonly HttpClient client = new HttpClient();
        private const string BaseUrl = "http://localhost:5090";

       

        /// <summary>
        /// Checks inventory for an item by name or item number
        /// </summary>  
        /// <param name="itemNameOrNumber">The item name or item number to search for</param>
        /// <returns>Response with item details and quantity, or null if not found</returns>
        public async Task<InventoryResponse> CheckInventoryAsync(string itemNameOrNumber)
        {
            try
            {
                // URL encode the item name (spaces become %20)
                var encodedItem = Uri.EscapeDataString(itemNameOrNumber);

                // Build the URL
                var url = $"{BaseUrl}/api/inventory/check?item={encodedItem}";

                // Make the HTTP GET request
                var response = await client.GetAsync(url);

                // Check if item was found
                if (response.IsSuccessStatusCode)
                {
                    // Read the JSON response
                    var json = await response.Content.ReadAsStringAsync();

                    // Deserialize JSON to InventoryResponse object
                    var result = JsonSerializer.Deserialize<InventoryResponse>(json);

                    return result;
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    // Item not found - return null
                    return null;
                }
                else
                {
                    throw new Exception($"API Error: {response.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Inventory Check Failed: {ex.Message}");
                throw;
            }
        }
    }
}
