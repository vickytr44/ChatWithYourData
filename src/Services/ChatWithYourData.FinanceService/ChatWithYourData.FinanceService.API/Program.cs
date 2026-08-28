using ChatWithYourData.FinanceService.API.GraphQL;
using ChatWithYourData.FinanceService.Application.Features.Finance.Commands;
using ChatWithYourData.FinanceService.Infrastructure;
using ChatWithYourData.FinanceService.Infrastructure.Persistence;
using FluentValidation;

var builder = WebApplication.CreateBuilder(args);

// Configure port
builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenAnyIP(5004); // FinanceService on port 5004
});

// Application Services
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(CreateAccountCommand).Assembly));
builder.Services.AddValidatorsFromAssembly(typeof(CreateAccountCommandValidator).Assembly);

// Infrastructure Services
builder.Services.AddFinanceInfrastructure(builder.Configuration);

// GraphQL Configuration — subgraph with source-generated types
builder.Services
    .AddGraphQLServer("finance-api")
    .AddSourceSchemaDefaults()
    .AddDefaultSettings()
    .ModifyPagingOptions(o =>
    {
        o.DefaultPageSize = 25;
        o.MaxPageSize = 150;
        o.IncludeTotalCount = true;
        o.NullOrdering = GreenDonut.Data.NullOrdering.NativeNullsLast;
    })
    .AddFinanceTypes()
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
    var dbContext = scope.ServiceProvider.GetRequiredService<FinanceDbContext>();
    await FinanceDbSeeder.SeedAsync(dbContext);
}

app.MapGraphQL();

app.MapGet("/", () => "ChatWithYourData FinanceService GraphQL is running on /graphql");

app.Run();

public partial class Program { }
