using Pattrn.DependencyInjection;

namespace Pattrn.DependencyInjection.Tests;

public sealed class FluentRegistrationBuilderTests
{
    [Test]
    public async Task BuilderExposesNameFlags()
    {
        PattrnRegistrationBuilder<string, string>? observed = null;

        var services = new Microsoft.Extensions.DependencyInjection.ServiceCollection();
        services.AddPattrnIndex<string, string>("events", registration =>
        {
            observed = registration;
            registration
                .UseWildcard("*")
                .Configure(builder => builder.Add(["events", "created"], "handler"));
        });

        await Assert.That(observed).IsNotNull();
        await Assert.That(observed!.Name).IsEqualTo("events");
        await Assert.That(observed.IsNamed).IsTrue();
        await Assert.That(observed.IsDefault).IsFalse();
    }
}
