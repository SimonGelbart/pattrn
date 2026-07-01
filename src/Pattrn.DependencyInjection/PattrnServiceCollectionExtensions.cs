using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Pattrn.DependencyInjection;

/// <summary>
/// Extension methods for registering compiled <see cref="PattrnIndex{TSegment, TValue}"/> instances in an <see cref="IServiceCollection"/>.
/// </summary>
public static class PattrnServiceCollectionExtensions
{
    /// <summary>
    /// Registers a singleton compiled <see cref="PattrnIndex{TSegment, TValue}"/> using a fluent registration builder.
    /// </summary>
    public static IServiceCollection AddPattrnIndex<TSegment, TValue>(
        this IServiceCollection services,
        Action<PattrnRegistrationBuilder<TSegment, TValue>> configureRegistration)
        where TSegment : notnull
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configureRegistration);

        var registrationBuilder = new PattrnRegistrationBuilder<TSegment, TValue>(name: null);
        configureRegistration(registrationBuilder);
        return AddPattrnIndex(services, registrationBuilder.BuildRegistration());
    }

    /// <summary>
    /// Registers a named singleton compiled <see cref="PattrnIndex{TSegment, TValue}"/> using a fluent registration builder.
    /// </summary>
    public static IServiceCollection AddPattrnIndex<TSegment, TValue>(
        this IServiceCollection services,
        string name,
        Action<PattrnRegistrationBuilder<TSegment, TValue>> configureRegistration)
        where TSegment : notnull
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configureRegistration);

        var registrationBuilder = new PattrnRegistrationBuilder<TSegment, TValue>(name);
        configureRegistration(registrationBuilder);
        return AddPattrnIndex(services, registrationBuilder.BuildRegistration());
    }

    /// <summary>
    /// Registers a singleton registration source that contributes pattern/value registrations during index construction.
    /// </summary>
    public static IServiceCollection AddPathPatternRegistrationSource<TSegment, TValue, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TSource>(
        this IServiceCollection services)
        where TSegment : notnull
        where TSource : class, IPattrnRegistrationSource<TSegment, TValue>
    {
        ArgumentNullException.ThrowIfNull(services);
        services.AddSingleton<IPattrnRegistrationSource<TSegment, TValue>, TSource>();
        return services;
    }

    private static IServiceCollection AddPattrnIndex<TSegment, TValue>(
        IServiceCollection services,
        PattrnIndexRegistration<TSegment, TValue> registration)
        where TSegment : notnull
    {
        ArgumentNullException.ThrowIfNull(registration);

        return registration.Name is null
            ? AddDefaultPattrnIndex(services, registration)
            : AddNamedPattrnIndex(services, registration.Name, registration);
    }

    private static IServiceCollection AddDefaultPattrnIndex<TSegment, TValue>(
        IServiceCollection services,
        PattrnIndexRegistration<TSegment, TValue> registration)
        where TSegment : notnull
    {
        ThrowIfDefaultIndexAlreadyRegistered<TSegment, TValue>(services);

        services.AddSingleton<PattrnIndex<TSegment, TValue>>(serviceProvider =>
            BuildIndex(serviceProvider, registration));

        services.AddSingleton<IPattrnIndex<TSegment, TValue>>(
            static serviceProvider => serviceProvider.GetRequiredService<PattrnIndex<TSegment, TValue>>());

        return services;
    }

    private static IServiceCollection AddNamedPattrnIndex<TSegment, TValue>(
        IServiceCollection services,
        string name,
        PattrnIndexRegistration<TSegment, TValue> registration)
        where TSegment : notnull
    {
        ThrowIfInvalidName(name);
        ThrowIfNamedIndexAlreadyRegistered<TSegment, TValue>(services, name);

        services.TryAddSingleton<IPattrnProvider<TSegment, TValue>, PattrnProvider<TSegment, TValue>>();

        services.AddKeyedSingleton<PattrnIndex<TSegment, TValue>>(name, (serviceProvider, _) =>
            BuildIndex(serviceProvider, registration));

        services.AddKeyedSingleton<IPattrnIndex<TSegment, TValue>>(name, static (serviceProvider, key) =>
            serviceProvider.GetRequiredKeyedService<PattrnIndex<TSegment, TValue>>(key));

        return services;
    }

    private static PattrnIndex<TSegment, TValue> BuildIndex<TSegment, TValue>(
        IServiceProvider serviceProvider,
        PattrnIndexRegistration<TSegment, TValue> registration)
        where TSegment : notnull
    {
        var builder = registration.Options.CreateBuilder();
        registration.Configure(serviceProvider, builder);
        return builder.Build(registration.Options.MatchOptions);
    }

    private static void ThrowIfDefaultIndexAlreadyRegistered<TSegment, TValue>(IServiceCollection services)
        where TSegment : notnull
    {
        if (services.Any(static descriptor =>
            !descriptor.IsKeyedService &&
            (descriptor.ServiceType == typeof(PattrnIndex<TSegment, TValue>) ||
             descriptor.ServiceType == typeof(IPattrnIndex<TSegment, TValue>))))
        {
            throw new InvalidOperationException(
                $"A default path pattern index for segment type '{typeof(TSegment).FullName}' and value type '{typeof(TValue).FullName}' is already registered.");
        }
    }

    private static void ThrowIfNamedIndexAlreadyRegistered<TSegment, TValue>(IServiceCollection services, string name)
        where TSegment : notnull
    {
        if (services.Any(descriptor =>
            descriptor.IsKeyedService &&
            Equals(descriptor.ServiceKey, name) &&
            (descriptor.ServiceType == typeof(PattrnIndex<TSegment, TValue>) ||
             descriptor.ServiceType == typeof(IPattrnIndex<TSegment, TValue>))))
        {
            throw new InvalidOperationException(
                $"A named path pattern index called '{name}' for segment type '{typeof(TSegment).FullName}' and value type '{typeof(TValue).FullName}' is already registered.");
        }
    }

    private static void ThrowIfInvalidName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("The index name cannot be null, empty, or white space.", nameof(name));
        }
    }
}
