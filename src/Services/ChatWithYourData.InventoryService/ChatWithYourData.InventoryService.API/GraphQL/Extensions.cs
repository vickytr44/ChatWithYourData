using HotChocolate.Execution.Configuration;

namespace ChatWithYourData.InventoryService.API.GraphQL;

public static class Extensions
{
    private const string Production = nameof(Production);

    public static IRequestExecutorBuilder AddDefaultSettings(
        this IRequestExecutorBuilder builder,
        bool registerNodeInterface = false)
    {
        var environmentName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");

        builder.AddGlobalObjectIdentification(
            o =>
            {
                o.RegisterNodeInterface = registerNodeInterface;
                o.MarkNodeFieldAsLookup = true;
            });
        builder.AddMutationConventions();
        builder.AddPagingArguments();
        builder.AddQueryContext();
        builder.ModifyCostOptions(x => x.EnforceCostLimits = false);

        if (!Production.Equals(environmentName, StringComparison.OrdinalIgnoreCase))
        {
            builder.ExportSchemaOnStartup();
        }

        return builder;
    }
}
