using ChatWithYourData.FinanceService.API.GraphQL.DataLoaders;
using ChatWithYourData.FinanceService.API.GraphQL.Mutations;
using ChatWithYourData.FinanceService.API.GraphQL.Queries;
using ChatWithYourData.FinanceService.API.GraphQL.Types;
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

// Register DataLoaders
builder.Services.AddScoped<AccountByIdDataLoader>();
builder.Services.AddScoped<JournalLinesByEntryIdDataLoader>();
builder.Services.AddScoped<PaymentsByInvoiceIdDataLoader>();

// GraphQL Configuration with Projections, Filtering, Sorting, and DataLoaders
builder.Services
    .AddGraphQLServer()
    .AddQueryType<FinanceQueries>()
    .AddMutationType<FinanceMutations>()
    .AddType<JournalEntryType>()
    .AddType<InvoiceType>()
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
    var dbContext = scope.ServiceProvider.GetRequiredService<FinanceDbContext>();
    await FinanceDbSeeder.SeedAsync(dbContext);
}

app.MapGraphQL();

app.MapGet("/", () => "ChatWithYourData FinanceService GraphQL is running on /graphql");

app.Run();

public partial class Program { }
