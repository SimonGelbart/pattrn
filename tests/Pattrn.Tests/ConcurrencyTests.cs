using static Pattrn.Tests.TestAssertions;

namespace Pattrn.Tests;

public sealed class ConcurrencyTests
{
    [Test]
    public void CompiledIndexSupportsConcurrentReaders()
    {
        var builder = PattrnIndexBuilder<string, string>.Create("*");
        builder.Add(["market", "NASDAQ", "MSFT"], "exact");
        builder.Add(["market", "NASDAQ", "*"], "wildcard");
        builder.Add(["market", "*", "MSFT"], "wildcard-middle");
        var index = builder.Build();

        Parallel.For(0, 10_000, _ =>
        {
            var result = index.MatchToArray(["market", "NASDAQ", "MSFT"]);
            if (result.Length != 3)
            {
                throw new InvalidOperationException($"Expected 3 matches, got {result.Length}.");
            }
        });

        ShouldEqual(index.MatchToArray(["market", "NASDAQ", "MSFT"]).Length, 3);
    }
}
