using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ProductService.Application.Interfaces;
using ProductService.Infrastructure.Data;
using ProductService.Infrastructure.Repositories;
using ProductService.Infrastructure.Services;

namespace ProductService.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<ProductDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));

        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<IUserServiceClient, UserServiceClient>();

        services.AddHttpClient<IUserServiceClient, UserServiceClient>(client =>
        {
            client.BaseAddress = new Uri(configuration["UserService:BaseUrl"]);
            client.DefaultRequestHeaders.Add("Accept", "application/json");
        });

        return services;
    }
}