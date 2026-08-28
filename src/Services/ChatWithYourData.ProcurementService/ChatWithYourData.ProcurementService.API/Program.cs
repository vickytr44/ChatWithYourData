using ChatWithYourData.ProcurementService.API.GraphQL.DataLoaders;
using ChatWithYourData.ProcurementService.API.GraphQL.Mutations;
using ChatWithYourData.ProcurementService.API.GraphQL.Queries;
using ChatWithYourData.ProcurementService.API.GraphQL.Types;
using ChatWithYourData.ProcurementService.Application.Features.Procurement.Commands;
using ChatWithYourData.ProcurementService.Infrastructure;
using ChatWithYourData.ProcurementService.Infrastructure.Persistence;
using FluentValidation;

var builder = WebApplication.CreateBuilder(args);

// Configure port
builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenAnyIP(5003); // ProcurementService on port 5003
});

// Application Services
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(CreateVendorCommand).Assembly));
builder.Services.AddValidatorsFromAssembly(typeof(CreateVendorCommandValidator).Assembly);

// Infrastructure Services
builder.Services.AddProcurementInfrastructure(builder.Configuration);

// Register DataLoaders
builder.Services.AddScoped<VendorByIdDataLoader>();
builder.Services.AddScoped<PoLinesByPoIdDataLoader>();

// GraphQL Configuration with Projections, Filtering, Sorting, and DataLoaders
builder.Services
    .AddGraphQLServer()
    .AddQueryType<ProcurementQueries>()
    .AddMutationType<ProcurementMutations>()
    .AddType<PurchaseOrderType>()
    .AddProjections()
    .AddFiltering()
    .AddSorting();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod();
    });
});

var app = builder.Build();

app.UseCors("AllowAll");

// Initialize and Seed Database
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ProcurementDbContext>();
    await ProcurementDbSeeder.SeedAsync(dbContext);
}

app.MapGraphQL();

app.MapGet("/", () => "ChatWithYourData ProcurementService GraphQL is running on /graphql");

app.Run();

public partial class Program { }
