using ChatWithYourData.FinanceService.Application.Common.Interfaces;
using ChatWithYourData.FinanceService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ChatWithYourData.FinanceService.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddFinanceInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection") 
            ?? "Data Source=finance.db";

        services.AddDbContext<FinanceDbContext>(options =>
            options.UseSqlite(connectionString));

        services.AddScoped<IFinanceDbContext>(provider => 
            provider.GetRequiredService<FinanceDbContext>());

        return services;
    }
}
