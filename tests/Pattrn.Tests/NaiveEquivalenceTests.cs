using static Pattrn.Tests.TestAssertions;

namespace Pattrn.Tests;

public sealed class NaiveEquivalenceTests
{
    [Test]
    public void OptimizedMatcherMatchesNaiveMatcherForRandomExactWildcardInputs()
    {
        var random = new Random(19790420);
        string[] alphabet = ["market", "NASDAQ", "NYSE", "MSFT", "AAPL", "price", "trade"];

        for (var scenario = 0; scenario < 500; scenario++)
        {
            var includePrefixMatches = random.Next(2) == 0;
            var deduplicateValues = random.Next(2) == 0;
            var builder = PattrnIndexBuilder<string, int>.Create("*");
            var registrations = new List<Registration<int>>();
            var registrationCount = random.Next(1, 75);

            for (var i = 0; i < registrationCount; i++)
            {
                var pattern = CreateRandomPattern(random, alphabet, maxLength: 6);
                var value = random.Next(0, 25);
                registrations.Add(new Registration<int>(pattern, value));
                builder.Add(pattern, value);
            }

            var options = new MatchOptions(includePrefixMatches ? PrefixMatchMode.IncludePrefixPatterns : PrefixMatchMode.Disabled, deduplicateValues ? DuplicateValueMatchMode.Deduplicate : DuplicateValueMatchMode.PreserveDuplicates);
            var index = builder.Build(options);

            for (var attempt = 0; attempt < 5; attempt++)
            {
                var path = CreateRandomPath(random, alphabet, maxLength: 6);
                var optimized = index.MatchToArray(path).Order().ToArray();
                var naive = MatchNaively(
                    registrations,
                    path,
                    wildcardSegment: "*",
                    segmentComparer: StringComparer.Ordinal,
                    options,
                    EqualityComparer<int>.Default).Order().ToArray();

                ShouldSequenceEqual(
                    optimized,
                    naive,
                    $"Scenario {scenario}, attempt {attempt} differed for path [{string.Join('.', path)}].");
            }
        }
    }

    [Test]
    public void OptimizedMatcherMatchesNaiveMatcherWithCustomSegmentAndValueComparers()
    {
        var random = new Random(20260412);
        string[] alphabet = ["market", "NASDAQ", "nasdaq", "NYSE", "nyse", "MSFT", "msft", "AAPL", "aapl"];
        var segmentComparer = StringComparer.OrdinalIgnoreCase;
        var valueComparer = StringComparer.OrdinalIgnoreCase;

        for (var scenario = 0; scenario < 200; scenario++)
        {
            var includePrefixMatches = random.Next(2) == 0;
            var deduplicateValues = random.Next(2) == 0;
            var builder = PattrnIndexBuilder<string, string>.Create(
                wildcardSegment: "ANY",
                segmentComparer: segmentComparer,
                valueComparer: valueComparer);
            var registrations = new List<Registration<string>>();
            var registrationCount = random.Next(1, 50);

            for (var i = 0; i < registrationCount; i++)
            {
                var pattern = CreateRandomPattern(random, alphabet, maxLength: 5, wildcardSegment: i % 3 == 0 ? "any" : "ANY");
                var value = random.Next(0, 10) % 2 == 0
                    ? $"CLIENT-{random.Next(0, 10)}"
                    : $"client-{random.Next(0, 10)}";

                registrations.Add(new Registration<string>(pattern, value));
                builder.Add(pattern, value);
            }

            var options = new MatchOptions(includePrefixMatches ? PrefixMatchMode.IncludePrefixPatterns : PrefixMatchMode.Disabled, deduplicateValues ? DuplicateValueMatchMode.Deduplicate : DuplicateValueMatchMode.PreserveDuplicates);
            var index = builder.Build(options);

            for (var attempt = 0; attempt < 5; attempt++)
            {
                var path = CreateRandomPath(random, alphabet, maxLength: 5);
                var optimized = index.MatchToArray(path);
                var naive = MatchNaively(
                    registrations,
                    path,
                    wildcardSegment: "ANY",
                    segmentComparer,
                    options,
                    valueComparer).ToArray();

                if (deduplicateValues)
                {
                    optimized = NormalizeCaseInsensitiveValues(optimized);
                    naive = NormalizeCaseInsensitiveValues(naive);
                }
                else
                {
                    optimized = SortStrings(optimized);
                    naive = SortStrings(naive);
                }

                ShouldSequenceEqual(
                    optimized,
                    naive,
                    $"Custom comparer scenario {scenario}, attempt {attempt} differed for path [{string.Join('.', path)}].");
            }
        }
    }

    private static string[] NormalizeCaseInsensitiveValues(IEnumerable<string> values)
    {
        return values
            .Select(static value => value.ToUpperInvariant())
            .Order(StringComparer.Ordinal)
            .ToArray();
    }

    private static string[] SortStrings(IEnumerable<string> values)
    {
        return values
            .Order(StringComparer.OrdinalIgnoreCase)
            .ThenBy(static value => value, StringComparer.Ordinal)
            .ToArray();
    }

    private static string[] CreateRandomPattern(Random random, string[] alphabet, int maxLength, string wildcardSegment = "*")
    {
        var length = random.Next(0, maxLength + 1);
        var result = new string[length];

        for (var i = 0; i < result.Length; i++)
        {
            result[i] = random.Next(4) == 0 ? wildcardSegment : alphabet[random.Next(alphabet.Length)];
        }

        return result;
    }

    private static string[] CreateRandomPath(Random random, string[] alphabet, int maxLength)
    {
        var length = random.Next(0, maxLength + 1);
        var result = new string[length];

        for (var i = 0; i < result.Length; i++)
        {
            result[i] = alphabet[random.Next(alphabet.Length)];
        }

        return result;
    }

    private static IEnumerable<TValue> MatchNaively<TValue>(
        IEnumerable<Registration<TValue>> registrations,
        string[] path,
        string wildcardSegment,
        IEqualityComparer<string> segmentComparer,
        MatchOptions options,
        IEqualityComparer<TValue> valueComparer)
    {
        var results = new List<TValue>();
        var seen = options.DeduplicateValues ? new HashSet<TValue>(valueComparer) : null;

        foreach (var registration in registrations)
        {
            if (!Matches(registration.Pattern, path, wildcardSegment, segmentComparer, options.IncludePrefixMatches))
            {
                continue;
            }

            if (seen is null || seen.Add(registration.Value))
            {
                results.Add(registration.Value);
            }
        }

        return results;
    }

    private static bool Matches(
        string[] pattern,
        string[] path,
        string wildcardSegment,
        IEqualityComparer<string> segmentComparer,
        bool includePrefixMatches)
    {
        if (includePrefixMatches)
        {
            if (pattern.Length > path.Length)
            {
                return false;
            }
        }
        else if (pattern.Length != path.Length)
        {
            return false;
        }

        for (var i = 0; i < pattern.Length; i++)
        {
            if (!segmentComparer.Equals(pattern[i], wildcardSegment)
                && !segmentComparer.Equals(pattern[i], path[i]))
            {
                return false;
            }
        }

        return true;
    }

    private sealed record Registration<TValue>(string[] Pattern, TValue Value);
}
