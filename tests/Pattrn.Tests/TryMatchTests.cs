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
    public void TryMatchReturnsTrueAndZeroWrittenWhenNoMatchAndDestinationIsSufficient()
    {
        var index = PattrnIndex<string, int>
            .Builder("*")
            .Add(["market", "NASDAQ", "MSFT"], 1)
            .Build();
        var destination = new[] { 42 };

        var succeeded = index.TryMatch(["market", "NASDAQ", "AAPL"], destination, out var written);

        ShouldBeTrue(succeeded, "Expected TryMatch to succeed for zero matches when the destination is sufficient.");
        ShouldEqual(written, 0);
        ShouldEqual(destination[0], 42);
    }

    [Test]
    public void TryMatchReturnsTrueWhenDestinationIsExactlyLargeEnough()
    {
        var index = PattrnIndex<string, int>
            .Builder("*")
            .Add(["market", "NASDAQ", "MSFT"], 1)
            .Add(["market", "NASDAQ", "*"], 2)
            .Build();
        Span<int> destination = stackalloc int[2];

        var succeeded = index.TryMatch(["market", "NASDAQ", "MSFT"], destination, out var written);

        ShouldBeTrue(succeeded, "Expected TryMatch to succeed when destination capacity exactly matches the result count.");
        ShouldEqual(written, 2);
        ShouldSetEqual(destination[..written].ToArray(), [1, 2]);
    }

    [Test]
    public void TryMatchPreservesDuplicateValueBehaviorForExactMatches()
    {
        var index = PattrnIndex<string, string>
            .Builder()
            .Add(["orders", "new"], "handler")
            .Add(["orders", "new"], "handler")
            .Build(MatchOptions.PreserveDuplicates);
        var destination = new string[2];

        var succeeded = index.TryMatch(["orders", "new"], destination, out var written);

        ShouldBeTrue(succeeded, "Expected TryMatch to preserve exact duplicate matches when configured.");
        ShouldEqual(written, 2);
        ShouldSequenceEqual(destination[..written].ToArray(), ["handler", "handler"]);
    }

    [Test]
    public void ExactDetailedMatchesPopulateConsumedSegmentCount()
    {
        var index = PattrnIndex<string, string>
            .Builder()
            .Add(["market", "NASDAQ", "MSFT"], "client-a")
            .Build();
        var matches = new PatternMatch<string>[1];
        Span<PatternCapture<string>> captures = [];

        var succeeded = index.TryMatchDetailed(["market", "NASDAQ", "MSFT"], matches, captures, out var matchesWritten, out var capturesWritten);

        ShouldBeTrue(succeeded, "Expected detailed exact match to succeed.");
        ShouldEqual(matchesWritten, 1);
        ShouldEqual(capturesWritten, 0);
        ShouldEqual(matches[0].ConsumedSegmentCount, 3);
    }
}
