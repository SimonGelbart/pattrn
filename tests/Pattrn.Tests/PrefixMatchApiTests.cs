using static Pattrn.Tests.TestAssertions;

namespace Pattrn.Tests;

public sealed class PrefixMatchApiTests
{
    [Test]
    public void MatchPrefixToArrayReturnsPrefixMatchesWhileExactMatchToArrayRemainsExactOnly()
    {
        var index = PattrnIndex<string, string>.Builder().Add(["market"], "prefix").Add(["market", "NASDAQ"], "deeper").Build(MatchOptions.Prefix);
        ShouldSequenceEqual(index.MatchToArray(["market", "NASDAQ"]), ["deeper"]);
        ShouldSequenceEqual(index.MatchPrefixToArray(["market", "NASDAQ"]), ["prefix", "deeper"]);
    }

    [Test]
    public void TryMatchPrefixReturnsTrueAndZeroWrittenWhenNoPrefixValuesMatch()
    {
        var index = PattrnIndex<string, string>.Builder().Add(["orders"], "orders").Build();
        var destination = new[] { "sentinel" };
        var succeeded = index.TryMatchPrefix(["customers", "42"], destination, out var written);
        ShouldBeTrue(succeeded, "Expected TryMatchPrefix to succeed for zero matches when capacity is sufficient.");
        ShouldEqual(written, 0);
        ShouldEqual(destination[0], "sentinel");
    }

    [Test]
    public void TryMatchPrefixReturnsTrueWhenDestinationIsExactlyLargeEnough()
    {
        var index = PattrnIndex<string, string>.Builder().Add(["api"], "api").Add(["api", "orders"], "orders").Build();
        var destination = new string[2];
        var succeeded = index.TryMatchPrefix(["api", "orders"], destination, out var written);
        ShouldBeTrue(succeeded, "Expected TryMatchPrefix to succeed with exact capacity.");
        ShouldEqual(written, 2);
        ShouldSequenceEqual(destination[..written].ToArray(), ["api", "orders"]);
    }

    [Test]
    public void TryMatchPrefixReturnsFalseAndDoesNotWriteWhenDestinationIsTooSmall()
    {
        var index = PattrnIndex<string, string>.Builder().Add(["api"], "api").Add(["api", "orders"], "orders").Build();
        var destination = new[] { "sentinel" };
        var succeeded = index.TryMatchPrefix(["api", "orders"], destination, out var written);
        ShouldBeFalse(succeeded, "Expected TryMatchPrefix to fail when destination is too small.");
        ShouldEqual(written, 0);
        ShouldEqual(destination[0], "sentinel");
    }

    [Test]
    public void MatchPrefixToArrayPreservesPrefixOrderingAndDuplicateBehavior()
    {
        var index = PattrnIndex<string, string>.Builder().Add(["api"], "same").Add(["api", "orders"], "same").Build(new MatchOptions(PrefixMatchMode.IncludePrefixPatterns, DuplicateValueMatchMode.PreserveDuplicates));
        ShouldSequenceEqual(index.MatchPrefixToArray(["api", "orders"]), ["same", "same"]);
    }

    [Test]
    public void TryMatchPrefixMemoryOverloadReturnsPrefixMatches()
    {
        var index = PattrnIndex<string, string>.Builder().Add(["api"], "api").Build();
        ReadOnlyMemory<string> path = new[] { "api", "orders" };
        var destination = new string[1];
        var succeeded = index.TryMatchPrefix(path, destination, out var written);
        ShouldBeTrue(succeeded, "Expected memory overload to succeed.");
        ShouldEqual(written, 1);
        ShouldEqual(destination[0], "api");
    }
}
