using static Pattrn.Tests.TestAssertions;

namespace Pattrn.Tests;

public sealed class TryMatchTests
{
    [Test]
    public void TryMatchReturnsTrueAndWrittenCountWhenDestinationIsLargeEnough()
    {
        var index = PattrnIndex<string, int>
            .Builder("*")
            .Add(["market", "NASDAQ", "MSFT"], 1)
            .Add(["market", "NASDAQ", "*"], 2)
            .Build();
        Span<int> destination = stackalloc int[2];

        var succeeded = index.TryMatch(["market", "NASDAQ", "MSFT"], destination, out var written);

        ShouldBeTrue(succeeded, "Expected TryMatch to succeed.");
        ShouldEqual(written, 2);
        ShouldSetEqual(destination[..written].ToArray(), [1, 2]);
    }

    [Test]
    public void TryMatchReturnsFalseAndZeroWrittenWhenDestinationIsTooSmall()
    {
        var index = PattrnIndex<string, int>
            .Builder("*")
            .Add(["market", "NASDAQ", "MSFT"], 1)
            .Add(["market", "NASDAQ", "*"], 2)
            .Build();
        Span<int> destination = stackalloc int[1];

        var succeeded = index.TryMatch(["market", "NASDAQ", "MSFT"], destination, out var written);

        ShouldBeFalse(succeeded, "Expected TryMatch to fail instead of throwing.");
        ShouldEqual(written, 0);
    }


    [Test]
    public void TryMatchDoesNotWriteDestinationWhenDestinationIsTooSmall()
    {
        var index = PattrnIndex<string, int>
            .Builder("*")
            .Add(["market", "NASDAQ", "MSFT"], 1)
            .Add(["market", "NASDAQ", "*"], 2)
            .Build();
        var destination = new[] { 42 };

        var succeeded = index.TryMatch(["market", "NASDAQ", "MSFT"], destination, out var written);

        ShouldBeFalse(succeeded, "Expected TryMatch to fail instead of writing a partial result.");
        ShouldEqual(written, 0);
        ShouldEqual(destination[0], 42);
    }

    [Test]
    public void TryMatchMemoryOverloadReturnsTrueAndWrittenCount()
    {
        var index = PattrnIndex<string, int>
            .Builder("*")
            .Add(["market", "NASDAQ", "MSFT"], 1)
            .Build();
        ReadOnlyMemory<string> path = new[] { "market", "NASDAQ", "MSFT" };
        Span<int> destination = stackalloc int[1];

        var succeeded = index.TryMatch(path, destination, out var written);

        ShouldBeTrue(succeeded, "Expected memory overload to succeed.");
        ShouldEqual(written, 1);
        ShouldEqual(destination[0], 1);
    }

    [Test]
    public void MatchStillThrowsWhenDestinationIsTooSmall()
    {
        var index = PattrnIndex<string, int>
            .Builder("*")
            .Add(["market", "NASDAQ", "MSFT"], 1)
            .Add(["market", "NASDAQ", "*"], 2)
            .Build();
        var destination = new int[1];

        var exception = ShouldThrow<ArgumentException>(() => index.Match(["market", "NASDAQ", "MSFT"], destination));

        ShouldEqual(exception.ParamName, "destination");
    }
}
