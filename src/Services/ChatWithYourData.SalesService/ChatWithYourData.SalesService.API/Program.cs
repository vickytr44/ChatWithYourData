using ChatWithYourData.SalesService.API.GraphQL.DataLoaders;
using ChatWithYourData.SalesService.API.GraphQL.Mutations;
using ChatWithYourData.SalesService.API.GraphQL.Queries;
using ChatWithYourData.SalesService.API.GraphQL.Types;
using ChatWithYourData.SalesService.Application.Features.Sales.Commands;
using ChatWithYourData.SalesService.Infrastructure;
using ChatWithYourData.SalesService.Infrastructure.Persistence;
using FluentValidation;

var builder = WebApplication.CreateBuilder(args);

// Configure port
builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenAnyIP(5002); // SalesService on port 5002
});

// Application Services
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(CreateCustomerCommand).Assembly));
builder.Services.AddValidatorsFromAssembly(typeof(CreateCustomerCommandValidator).Assembly);

// Infrastructure Services
builder.Services.AddSalesInfrastructure(builder.Configuration);

// Register DataLoaders
builder.Services.AddScoped<CustomerByIdDataLoader>();
builder.Services.AddScoped<OrderLinesByOrderIdDataLoader>();

// GraphQL Configuration with Projections, Filtering, Sorting, and DataLoaders
builder.Services
    .AddGraphQLServer()
    .AddQueryType<SalesQueries>()
    .AddMutationType<SalesMutations>()
    .AddType<SalesOrderType>()
    .AddType<SalesOrderLineType>()
    .AddType<ProductType>()
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
    var dbContext = scope.ServiceProvider.GetRequiredService<SalesDbContext>();
    await SalesDbSeeder.SeedAsync(dbContext);
}

app.MapGraphQL();

app.MapGet("/", () => "ChatWithYourData SalesService GraphQL is running on /graphql");

app.Run();

public partial class Program { }
