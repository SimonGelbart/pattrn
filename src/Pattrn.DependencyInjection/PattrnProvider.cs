using Microsoft.Extensions.DependencyInjection;

namespace Pattrn.DependencyInjection;

internal sealed class PattrnProvider<TSegment, TValue>(IServiceProvider serviceProvider)
    : IPattrnProvider<TSegment, TValue>
    where TSegment : notnull
{
    public IPattrnIndex<TSegment, TValue> GetRequired(string name)
    {
        ThrowIfInvalidName(name);
        return serviceProvider.GetRequiredKeyedService<IPattrnIndex<TSegment, TValue>>(name);
    }

    public bool TryGet(string name, out IPattrnIndex<TSegment, TValue>? index)
    {
        ThrowIfInvalidName(name);
        index = serviceProvider.GetKeyedService<IPattrnIndex<TSegment, TValue>>(name);
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
