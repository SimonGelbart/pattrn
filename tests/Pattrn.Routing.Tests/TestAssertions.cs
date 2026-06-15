namespace Pattrn.Routing.Tests;

internal static class TestAssertions
{
    internal static void ShouldBeTrue(bool condition, string message)
    {
        if (!condition)
        {
            throw new InvalidOperationException(message);
        }
    }

    internal static void ShouldBeFalse(bool condition, string message) => ShouldBeTrue(!condition, message);

    internal static void ShouldEqual<T>(T actual, T expected, string? message = null)
    {
        if (!EqualityComparer<T>.Default.Equals(actual, expected))
        {
            throw new InvalidOperationException(message ?? $"Expected {expected}, got {actual}.");
        }
    }

    internal static void ShouldSequenceEqual<T>(IEnumerable<T> actual, IEnumerable<T> expected, string? message = null)
    {
        var actualArray = actual.ToArray();
        var expectedArray = expected.ToArray();

        if (!actualArray.SequenceEqual(expectedArray))
        {
            throw new InvalidOperationException(
                message ?? $"Expected [{string.Join(", ", expectedArray)}], got [{string.Join(", ", actualArray)}].");
        }
    }

    internal static void ShouldSetEqual<T>(IEnumerable<T> actual, IEnumerable<T> expected, string? message = null)
    {
        ShouldSequenceEqual(actual.Order(), expected.Order(), message);
    }

    internal static TException ShouldThrow<TException>(Action action, string? message = null)
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

        throw new InvalidOperationException(message ?? $"Expected exception {typeof(TException).Name}.");
    }
}
