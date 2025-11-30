using ProductService.Application.Interfaces;
using System.Text;
using System.Text.Json;

namespace ProductService.Infrastructure.Services;

public class UserServiceClient : IUserServiceClient
{
    private readonly HttpClient _httpClient;

    public UserServiceClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<bool> ValidateUserAsync(Guid userId)
    {
        try
        {
            var response = await _httpClient.GetAsync($"api/users/{userId}");
            return response.IsSuccessStatusCode;
        }
        catch (HttpRequestException)
        {
            return false;
        }
    }

    public async Task<bool> IsUserActiveAsync(Guid userId)
    {
        try
        {
            var response = await _httpClient.GetAsync($"api/users/{userId}");

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                using var document = JsonDocument.Parse(content);

                if (document.RootElement.TryGetProperty("status", out var statusProperty) &&
                    statusProperty.GetString() == "Active")
                {
                    return true;
                }
            }

            return false;
        }
        catch (HttpRequestException)
        {
            return false;
        }
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