using ChatWithYourData.ProcurementService.API.GraphQL;
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

// GraphQL Configuration — subgraph with source-generated types
builder.Services
    .AddGraphQLServer("procurement-api")
    .AddSourceSchemaDefaults()
    .AddDefaultSettings()
    .ModifyPagingOptions(o =>
    {
        o.DefaultPageSize = 25;
        o.MaxPageSize = 150;
        o.IncludeTotalCount = true;
        o.NullOrdering = GreenDonut.Data.NullOrdering.NativeNullsLast;
    })
    .AddProcurementTypes()
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
