using ChatWithYourData.SalesService.Application.Common.Interfaces;
using ChatWithYourData.SalesService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ChatWithYourData.SalesService.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddSalesInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection") 
            ?? "Data Source=sales.db";

        services.AddDbContext<SalesDbContext>(options =>
            options.UseSqlite(connectionString));

        services.AddScoped<ISalesDbContext>(provider => 
            provider.GetRequiredService<SalesDbContext>());

        return services;
    }
}
