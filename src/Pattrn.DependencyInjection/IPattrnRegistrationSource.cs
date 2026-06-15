namespace Pattrn.DependencyInjection;

/// <summary>
/// Contributes pattern/value registrations to a compiled <see cref="PattrnIndex{TSegment, TValue}"/> during dependency-injection startup.
/// </summary>
/// <typeparam name="TSegment">The segment type used by registered patterns and incoming paths.</typeparam>
/// <typeparam name="TValue">The value type returned when a registered pattern matches.</typeparam>
public interface IPattrnRegistrationSource<TSegment, TValue>
    where TSegment : notnull
{
    /// <summary>
    /// Adds this source's pattern/value registrations to the supplied registration context.
    /// </summary>
    /// <param name="context">The registration context for the index currently being built.</param>
    void AddRegistrations(PattrnRegistrationContext<TSegment, TValue> context);
}
