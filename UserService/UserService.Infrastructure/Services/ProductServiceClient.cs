using System.Text;
using System.Text.Json;
using UserService.Application.Interfaces;

namespace UserService.Infrastructure.Services;

public class ProductServiceClient : IProductServiceClient
{
    private readonly HttpClient _httpClient;

    public ProductServiceClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task UpdateProductsUserStatusAsync(Guid userId, bool isActive)
    {
        try
        {
            var request = new { UserId = userId, IsActive = isActive };
            var content = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("api/products/update-user-status", content);

            if (!response.IsSuccessStatusCode)
            {
                throw new HttpRequestException($"Failed to update products status for user {userId}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error updating products status: {ex.Message}");
        }
    }
}