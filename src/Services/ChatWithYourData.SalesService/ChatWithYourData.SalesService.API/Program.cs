using ChatWithYourData.SalesService.API.GraphQL;
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

// GraphQL Configuration — subgraph with source-generated types
builder.Services
    .AddGraphQLServer("sales-api")
    .AddSourceSchemaDefaults()
    .AddDefaultSettings()
    .ModifyPagingOptions(o =>
    {
        o.DefaultPageSize = 25;
        o.MaxPageSize = 150;
        o.IncludeTotalCount = true;
        o.NullOrdering = GreenDonut.Data.NullOrdering.NativeNullsLast;
    })
    .AddSalesTypes()
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
