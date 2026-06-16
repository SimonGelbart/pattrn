using Microsoft.Extensions.DependencyInjection;
using Pattrn;
using Pattrn.DependencyInjection;

var services = new ServiceCollection();

services.AddPattrnIndex<string, string>(registration => registration
    .Configure(builder => builder
        .AddPattern(
            [PatternSegment<string>.Literal("market"), PatternSegment<string>.Literal("NASDAQ"), PatternSegment<string>.Wildcard()],
            "default-client-a")
        .AddPattern(
            [PatternSegment<string>.Literal("market"), PatternSegment<string>.Wildcard(), PatternSegment<string>.Literal("MSFT")],
            "default-client-b")));

services.AddPattrnIndex<string, string>("market-data", registration => registration
    .UseWildcard("*")
    .UseMatchOptions(MatchOptions.Prefix)
    .UseSegmentComparer(StringComparer.OrdinalIgnoreCase)
    .Configure(builder => builder
        .Add(["market"], "named-market-prefix-client")));

services.AddPattrnIndex<string, string>("events", registration => registration
    .Configure(builder => builder
        .Add(["events", "created"], "named-event-handler")));

services
    .AddPathPatternRegistrationSource<string, string, AdminRegistrationSource>()
    .AddPattrnIndex<string, string>("admin", registration => registration
        .UseWildcard("*")
        .FromRegisteredSources());

using var provider = services.BuildServiceProvider();

var defaultIndex = provider.GetRequiredService<IPattrnIndex<string, string>>();
var defaultMatches = defaultIndex.MatchToArray(["market", "NASDAQ", "MSFT"]);

var indexProvider = provider.GetRequiredService<IPattrnProvider<string, string>>();
var marketIndex = indexProvider.GetRequired("market-data");
var eventIndex = indexProvider.GetRequired("events");
var adminIndex = indexProvider.GetRequired("admin");

Console.WriteLine($"default: {string.Join(", ", defaultMatches)}");
Console.WriteLine($"market-data: {string.Join(", ", marketIndex.MatchToArray(["MARKET", "NASDAQ", "MSFT"]))}");
Console.WriteLine($"events: {string.Join(", ", eventIndex.MatchToArray(["events", "created"]))}");
Console.WriteLine($"admin: {string.Join(", ", adminIndex.MatchToArray(["admin", "users"]))}");

internal sealed class AdminRegistrationSource : IPattrnRegistrationSource<string, string>
{
    public void AddRegistrations(PattrnRegistrationContext<string, string> context) =>
        context.AddPattern(
            [PatternSegment<string>.Literal("admin"), PatternSegment<string>.Wildcard()],
            "admin-handler");
}
