namespace Pattrn.Tests.Concurrency;

public sealed class ConcurrencyTests
{
    [Test]
    public async Task CompiledIndexSupportsConcurrentReaders()
    {
        var builder = PattrnIndexBuilder<string, string>.Create("*");
        builder.Add(["market", "NASDAQ", "MSFT"], "exact");
        builder.Add(["market", "NASDAQ", "*"], "wildcard");
        builder.Add(["market", "*", "MSFT"], "wildcard-middle");
        var index = builder.Build();

        Parallel.For(0, 10_000, _ =>
        {
            var result = index.MatchValuesToArray(["market", "NASDAQ", "MSFT"]);
            if (result.Length != 3)
            {
                throw new InvalidOperationException($"Expected 3 matches, got {result.Length}.");
            }
        });

        await Assert.That(index.MatchValuesToArray(["market", "NASDAQ", "MSFT"]).Length).IsEqualTo(3);
    }
}

