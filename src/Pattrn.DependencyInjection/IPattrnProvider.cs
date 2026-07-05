using Microsoft.Extensions.DependencyInjection;

namespace Pattrn.DependencyInjection;

/// <summary>
/// Resolves named compiled <see cref="PattrnIndex{TSegment, TValue}"/> instances registered through dependency injection.
/// </summary>
/// <typeparam name="TSegment">The segment type used by registered patterns and incoming paths.</typeparam>
/// <typeparam name="TValue">The value type returned when a registered pattern matches.</typeparam>
public interface IPattrnProvider<TSegment, TValue>
    where TSegment : notnull
{
    /// <summary>
    /// Gets a required named index.
    /// </summary>
    /// <param name="name">The name used when the index was registered.</param>
    /// <returns>The named compiled index.</returns>
    /// <exception cref="ArgumentException"><paramref name="name"/> is null, empty, or white space.</exception>
    /// <exception cref="InvalidOperationException">No index was registered with the supplied name.</exception>
    PattrnIndex<TSegment, TValue> GetRequired(string name);

    /// <summary>
    /// Attempts to get a named index.
    /// </summary>
    /// <param name="name">The name used when the index was registered.</param>
    /// <param name="index">When this method returns, contains the named index if one was found.</param>
    /// <returns><see langword="true"/> when the named index was found; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentException"><paramref name="name"/> is null, empty, or white space.</exception>
    bool TryGet(string name, out PattrnIndex<TSegment, TValue>? index);
}

internal sealed class PattrnProvider<TSegment, TValue>(IServiceProvider serviceProvider)
    : IPattrnProvider<TSegment, TValue>
    where TSegment : notnull
{
    public PattrnIndex<TSegment, TValue> GetRequired(string name)
    {
        ThrowIfInvalidName(name);
        return serviceProvider.GetRequiredKeyedService<PattrnIndex<TSegment, TValue>>(name);
    }

    public bool TryGet(string name, out PattrnIndex<TSegment, TValue>? index)
    {
        ThrowIfInvalidName(name);
        index = serviceProvider.GetKeyedService<PattrnIndex<TSegment, TValue>>(name);
        return index is not null;
    }

    private static void ThrowIfInvalidName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("The index name cannot be null, empty, or white space.", nameof(name));
        }
    }
}
