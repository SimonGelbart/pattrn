using Microsoft.Extensions.DependencyInjection;
using Pattrn.DependencyInjection;

namespace Pattrn.DependencyInjection.Tests;

public sealed class DependencyInjectionTests
{
    [Test]
    public async Task RegistersInterfaceAndConcreteAsSameSingleton()
    {
        var services = new ServiceCollection();

        services.AddPattrnIndex<string, string>(registration => registration
            .UseWildcard("*")
            .Configure(builder => builder.Add(["market", "NASDAQ", "*"], "client-a")));

        using var provider = services.BuildServiceProvider();

        var concrete = provider.GetRequiredService<PattrnIndex<string, string>>();
        var abstraction = provider.GetRequiredService<IPattrnIndex<string, string>>();
        var secondAbstraction = provider.GetRequiredService<IPattrnIndex<string, string>>();

        await Assert.That(ReferenceEquals(concrete, abstraction)).IsTrue();
        await Assert.That(ReferenceEquals(abstraction, secondAbstraction)).IsTrue();
    }

    [Test]
    public async Task RegisteredIndexMatchesConfiguredPatterns()
    {
        var services = new ServiceCollection();

        services.AddPattrnIndex<string, string>(registration => registration
            .UseWildcard("*")
            .Configure(builder => builder
                .Add(["market", "NASDAQ", "*"], "client-a")
                .Add(["market", "*", "MSFT"], "client-b")));

        using var provider = services.BuildServiceProvider();

        var index = provider.GetRequiredService<IPattrnIndex<string, string>>();
        var matches = index.MatchToArray(["market", "NASDAQ", "MSFT"]);

        await Assert.That(matches).IsEquivalentTo(["client-a", "client-b"]);
    }

    [Test]
    public async Task FluentBuilderAppliesOptionsAndComparers()
    {
        var services = new ServiceCollection();

        services.AddPattrnIndex<string, string>(registration => registration
            .UseWildcard("*")
            .UseMatchOptions(MatchOptions.Prefix)
            .UseSegmentComparer(StringComparer.OrdinalIgnoreCase)
            .UseValueComparer(StringComparer.OrdinalIgnoreCase)
            .Configure(builder => builder
                .Add(["market"], "CLIENT-A")
                .Add(["MARKET"], "client-a")));

        using var provider = services.BuildServiceProvider();

        var index = provider.GetRequiredService<IPattrnIndex<string, string>>();
        var matches = index.MatchToArray(["MARKET", "NASDAQ", "MSFT"]);

        await Assert.That(index.Options).IsEqualTo(MatchOptions.Prefix);
        await Assert.That(matches).IsEquivalentTo(["CLIENT-A"]);
    }

    [Test]
    public async Task NamedIndexCanBeResolvedThroughProviderAndKeyedService()
    {
        var services = new ServiceCollection();

        services.AddPattrnIndex<string, string>("market-data", registration => registration
            .UseWildcard("*")
            .Configure(builder => builder.Add(["market", "NASDAQ", "*"], "client-a")));

        using var provider = services.BuildServiceProvider();

        var namedProviderIndex = provider.GetRequiredService<IPattrnProvider<string, string>>().GetRequired("market-data");
        var keyedIndex = provider.GetRequiredKeyedService<IPattrnIndex<string, string>>("market-data");

        await Assert.That(ReferenceEquals(namedProviderIndex, keyedIndex)).IsTrue();
        await Assert.That(namedProviderIndex.MatchToArray(["market", "NASDAQ", "MSFT"])).IsEquivalentTo(["client-a"]);
    }

    [Test]
    public async Task RegisteredSourcesUseConstructorInjectionInsteadOfServiceProviderContext()
    {
        var services = new ServiceCollection();

        services.AddSingleton(new RouteRegistration(["market", "NASDAQ", "*"], "client-a"));
        services
            .AddPathPatternRegistrationSource<string, string, ConstructorInjectedRegistrationSource>()
            .AddPattrnIndex<string, string>(registration => registration
                .UseWildcard("*")
                .FromRegisteredSources());

        using var provider = services.BuildServiceProvider();

        var index = provider.GetRequiredService<IPattrnIndex<string, string>>();
        var matches = index.MatchToArray(["market", "NASDAQ", "MSFT"]);

        await Assert.That(matches).IsEquivalentTo(["client-a"]);
    }

    [Test]
    public async Task RegistrationSourceReceivesNameContext()
    {
        var services = new ServiceCollection();

        services
            .AddPathPatternRegistrationSource<string, string, NameAwareRegistrationSource>()
            .AddPattrnIndex<string, string>("admin", registration => registration
                .UseWildcard("*")
                .FromRegisteredSources())
            .AddPattrnIndex<string, string>("public", registration => registration
                .UseWildcard("*")
                .FromRegisteredSources());

        using var provider = services.BuildServiceProvider();
        var indexProvider = provider.GetRequiredService<IPattrnProvider<string, string>>();

        await Assert.That(indexProvider.GetRequired("admin").MatchToArray(["admin", "users"])).IsEquivalentTo(["admin-handler"]);
        await Assert.That(indexProvider.GetRequired("public").MatchToArray(["public", "home"])).IsEquivalentTo(["public-handler"]);
    }

    [Test]
    public async Task DuplicateDefaultRegistrationFailsFast()
    {
        var services = new ServiceCollection();

        services.AddPattrnIndex<string, string>(registration => registration
            .UseWildcard("*")
            .Configure(builder => builder.Add(["a"], "a")));

        var exception = Capture<InvalidOperationException>(() =>
            services.AddPattrnIndex<string, string>(registration => registration
                .UseWildcard("*")
                .Configure(builder => builder.Add(["b"], "b"))));

        await Assert.That(exception.Message.Contains("already registered", StringComparison.Ordinal)).IsTrue();
    }

    [Test]
    public async Task DuplicateNamedRegistrationFailsFast()
    {
        var services = new ServiceCollection();

        services.AddPattrnIndex<string, string>("routes", registration => registration
            .UseWildcard("*")
            .Configure(builder => builder.Add(["a"], "a")));

        var exception = Capture<InvalidOperationException>(() =>
            services.AddPattrnIndex<string, string>("routes", registration => registration
                .UseWildcard("*")
                .Configure(builder => builder.Add(["b"], "b"))));

        await Assert.That(exception.Message.Contains("already registered", StringComparison.Ordinal)).IsTrue();
    }

    [Test]
    public async Task RegistrationCanUseTokenlessBuilderByDefault()
    {
        var services = new ServiceCollection();

        services.AddPattrnIndex<string, string>(registration => registration
            .Configure(builder => builder
                .Add(["market", "*"], "literal-star")
                .AddPattern(
                    [PatternSegment<string>.Literal("market"), PatternSegment<string>.Wildcard()],
                    "explicit-wildcard")));

        using var provider = services.BuildServiceProvider();
        var index = provider.GetRequiredService<IPattrnIndex<string, string>>();

        await Assert.That(index.MatchToArray(["market", "MSFT"])).IsEquivalentTo(["explicit-wildcard"]);
        await Assert.That(index.MatchToArray(["market", "*"])).IsEquivalentTo(["literal-star", "explicit-wildcard"]);
    }

    [Test]
    public async Task RegistrationSourcesCanUseExplicitPatternSegmentsWithoutWildcardToken()
    {
        var services = new ServiceCollection();

        services
            .AddPathPatternRegistrationSource<string, string, ExplicitPatternRegistrationSource>()
            .AddPattrnIndex<string, string>(registration => registration.FromRegisteredSources());

        using var provider = services.BuildServiceProvider();
        var index = provider.GetRequiredService<IPattrnIndex<string, string>>();

        await Assert.That(index.MatchDetailedToArray(["orders", "123"]).Single().Captures.Single().Value).IsEqualTo("123");
    }


    private static TException Capture<TException>(Action action)
        where TException : Exception
    {
        try
        {
            action();
        }
        catch (TException exception)
        {
            return exception;
        }

        throw new InvalidOperationException($"Expected exception of type {typeof(TException).FullName}.");
    }

    private sealed record RouteRegistration(string[] Pattern, string Value);

    private sealed class ConstructorInjectedRegistrationSource(RouteRegistration route)
        : IPattrnRegistrationSource<string, string>
    {
        public void AddRegistrations(PattrnRegistrationContext<string, string> context) =>
            context.Add(route.Pattern, route.Value);
    }

    private sealed class ExplicitPatternRegistrationSource : IPattrnRegistrationSource<string, string>
    {
        public void AddRegistrations(PattrnRegistrationContext<string, string> context) =>
            context.AddPattern(
                [PatternSegment<string>.Literal("orders"), PatternSegment<string>.Parameter("id")],
                "orders-handler");
    }

    private sealed class NameAwareRegistrationSource : IPattrnRegistrationSource<string, string>
    {
        public void AddRegistrations(PattrnRegistrationContext<string, string> context)
        {
            if (context.Name == "admin")
            {
                context.Add(["admin", "*"], "admin-handler");
                return;
            }

            if (context.Name == "public")
            {
                context.Add(["public", "*"], "public-handler");
            }
        }
    }
}
