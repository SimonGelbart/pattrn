using Microsoft.Extensions.DependencyInjection;
using Pattrn.DependencyInjection;

namespace Pattrn.DependencyInjection.Tests;

public sealed class DependencyInjectionSurfaceTests
{
    [Test]
    public async Task RegistrationSourceCanContributeGenericPatternSegmentsWithoutAccessingBuilder()
    {
        var services = new ServiceCollection();

        services
            .AddPathPatternRegistrationSource<string, string, ParameterRegistrationSource>()
            .AddPattrnIndex<string, string>(registration => registration
                .UseWildcard("*")
                .FromRegisteredSources());

        using var provider = services.BuildServiceProvider();

        var index = provider.GetRequiredService<IPattrnIndex<string, string>>();
        var matches = index.MatchDetailed(["orders", "123"]);

        await Assert.That(matches).Count().IsEqualTo(1);
        await Assert.That(matches[0].Value).IsEqualTo("order-handler");
        await Assert.That(matches[0].Captures).Count().IsEqualTo(1);
        await Assert.That(matches[0].Captures[0].Name).IsEqualTo("id");
        await Assert.That(matches[0].Captures[0].Value).IsEqualTo("123");
    }

    private sealed class ParameterRegistrationSource : IPattrnRegistrationSource<string, string>
    {
        public void AddRegistrations(PattrnRegistrationContext<string, string> context)
        {
            context.AddPattern(
                [
                    PatternSegment<string>.Literal("orders"),
                    PatternSegment<string>.Parameter("id")
                ],
                "order-handler");
        }
    }
}
