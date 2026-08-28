using ChatWithYourData.ProcurementService.Application.Common.Interfaces;
using ChatWithYourData.ProcurementService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ChatWithYourData.ProcurementService.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddProcurementInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection") 
            ?? "Data Source=procurement.db";

        services.AddDbContext<ProcurementDbContext>(options =>
            options.UseSqlite(connectionString));

        services.AddScoped<IProcurementDbContext>(provider => 
            provider.GetRequiredService<ProcurementDbContext>());

        return services;
    }
}
