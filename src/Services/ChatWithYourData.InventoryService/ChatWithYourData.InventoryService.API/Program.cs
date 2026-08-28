using ChatWithYourData.InventoryService.API.GraphQL.DataLoaders;
using ChatWithYourData.InventoryService.API.GraphQL.Mutations;
using ChatWithYourData.InventoryService.API.GraphQL.Queries;
using ChatWithYourData.InventoryService.API.GraphQL.Types;
using ChatWithYourData.InventoryService.Application.Features.Products.Commands;
using ChatWithYourData.InventoryService.Infrastructure;
using ChatWithYourData.InventoryService.Infrastructure.Persistence;
using FluentValidation;

var builder = WebApplication.CreateBuilder(args);

// Configure port
builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenAnyIP(5001); // InventoryService on port 5001
});

// Application Services
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(CreateProductCommand).Assembly));
builder.Services.AddValidatorsFromAssembly(typeof(CreateProductCommandValidator).Assembly);

// Infrastructure Services
builder.Services.AddInventoryInfrastructure(builder.Configuration);

// Register DataLoaders
builder.Services.AddScoped<CategoryByIdDataLoader>();
builder.Services.AddScoped<WarehouseByIdDataLoader>();
builder.Services.AddScoped<ProductByIdDataLoader>();

// GraphQL Configuration with Projections, Filtering, Sorting, and DataLoaders
builder.Services
    .AddGraphQLServer()
    .AddQueryType<InventoryQueries>()
    .AddMutationType<InventoryMutations>()
    .AddType<ProductType>()
    .AddType<StockItemType>()
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
    var dbContext = scope.ServiceProvider.GetRequiredService<InventoryDbContext>();
    await InventoryDbSeeder.SeedAsync(dbContext);
}

app.MapGraphQL();

app.MapGet("/", () => "ChatWithYourData InventoryService GraphQL is running on /graphql");

app.Run();

public partial class Program { }
