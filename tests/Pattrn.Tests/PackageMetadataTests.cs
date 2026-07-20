using System.Xml.Linq;
using TUnit.Assertions.Enums;

namespace Pattrn.Tests;

public sealed class PackageMetadataTests
{
    [Test]
    public async Task PackageMetadataIsCentralizedAndUsesRepositoryMetadata()
    {
        var root = FindRepositoryRoot();
        var directoryBuildProps = XDocument.Load(Path.Combine(root.FullName, "Directory.Build.props"));
        var packageVersion = directoryBuildProps.Descendants("PattrnVersion").SingleOrDefault()?.Value;
        var repositoryType = directoryBuildProps.Descendants("RepositoryType").SingleOrDefault()?.Value;
        var repositoryUrl = directoryBuildProps.Descendants("RepositoryUrl").SingleOrDefault()?.Value;
        var publishRepositoryUrl = directoryBuildProps.Descendants("PublishRepositoryUrl").SingleOrDefault()?.Value;

        await Assert.That(packageVersion).IsEqualTo("0.1.0-alpha.1").Because("The pre-beta package version should be centralized.");
        await Assert.That(repositoryType).IsEqualTo("git").Because("Package metadata should point at the real Git repository.");
        await Assert.That(repositoryUrl).IsEqualTo("https://github.com/SimonGelbart/pattrn").Because("Package metadata should use the public repository URL.");
        await Assert.That(publishRepositoryUrl).IsEqualTo("true").Because("NuGet packages should publish repository metadata.");

        var packageProjects = new[]
        {
            Path.Combine(root.FullName, "src", "Pattrn", "Pattrn.csproj"),
            Path.Combine(root.FullName, "src", "Pattrn.Strings", "Pattrn.Strings.csproj"),
            Path.Combine(root.FullName, "src", "Pattrn.DependencyInjection", "Pattrn.DependencyInjection.csproj"),
            Path.Combine(root.FullName, "src", "Pattrn.Routing", "Pattrn.Routing.csproj")
        };

        foreach (var projectPath in packageProjects)
        {
            var document = XDocument.Load(projectPath);
            var license = document.Descendants("PackageLicenseExpression").SingleOrDefault()?.Value;

            await Assert.That(license).IsEqualTo("MIT").Because($"{projectPath} should use the MIT SPDX license expression.");
            await Assert.That(document.Descendants("Version").SingleOrDefault()?.Value).IsNull().Because($"{projectPath} should inherit the centralized version.");
            await Assert.That(document.Descendants("RepositoryType").SingleOrDefault()?.Value).IsNull().Because($"{projectPath} should inherit centralized repository metadata.");
            await Assert.That(document.Descendants("RepositoryUrl").SingleOrDefault()?.Value).IsNull().Because($"{projectPath} should inherit centralized repository metadata.");
        }
    }


    [Test]
    public async Task PackageProjectsUsePackageScopedReadmes()
    {
        var root = FindRepositoryRoot();
        var expected = new Dictionary<string, string>
        {
            [Path.Combine(root.FullName, "src", "Pattrn", "Pattrn.csproj")] = Path.Combine(root.FullName, "docs", "packages", "pattrn.md"),
            [Path.Combine(root.FullName, "src", "Pattrn.Strings", "Pattrn.Strings.csproj")] = Path.Combine(root.FullName, "docs", "packages", "pattrn-strings.md"),
            [Path.Combine(root.FullName, "src", "Pattrn.DependencyInjection", "Pattrn.DependencyInjection.csproj")] = Path.Combine(root.FullName, "docs", "packages", "pattrn-dependency-injection.md"),
            [Path.Combine(root.FullName, "src", "Pattrn.Routing", "Pattrn.Routing.csproj")] = Path.Combine(root.FullName, "docs", "packages", "pattrn-routing.md")
        };

        foreach (var pair in expected)
        {
            var document = XDocument.Load(pair.Key);
            var readmeFile = document.Descendants("PackageReadmeFile").SingleOrDefault()?.Value;

            await Assert.That(readmeFile).IsEqualTo("README.md").Because($"{pair.Key} should pack a README.md file for NuGet.");
            await Assert.That(File.Exists(pair.Value)).IsTrue().Because($"Missing expected package README source: {pair.Value}");

            var noneItems = document.Descendants("None").ToArray();
            var packsExpectedReadme = noneItems.Any(item =>
                string.Equals(item.Attribute("Pack")?.Value, "true", StringComparison.OrdinalIgnoreCase) &&
                string.Equals(item.Attribute("PackagePath")?.Value, pair.Value.EndsWith("README.md", StringComparison.OrdinalIgnoreCase) ? @"\" : "README.md", StringComparison.Ordinal));

            await Assert.That(packsExpectedReadme).IsTrue().Because($"{pair.Key} should pack the expected README source for NuGet.");
        }
    }


    [Test]
    public async Task SourceDistributionDoesNotContainGeneratedOrTemporaryArtifacts()
    {
        var root = FindRepositoryRoot();
        var ignoredDirectoryNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            ".git",
            ".vs",
            "bin",
            "obj"
        };

        var generatedDirectories = root.EnumerateDirectories("*", SearchOption.AllDirectories)
            .Where(directory => !directory.FullName.Split(Path.DirectorySeparatorChar).Any(ignoredDirectoryNames.Contains))
            .Where(directory => string.Equals(directory.Name, "BenchmarkDotNet.Artifacts", StringComparison.OrdinalIgnoreCase))
            .Select(directory => Path.GetRelativePath(root.FullName, directory.FullName))
            .ToArray();

        await Assert.That(generatedDirectories).IsEquivalentTo(Array.Empty<string>(), CollectionOrdering.Matching)
            .Because("Source distribution should not include BenchmarkDotNet.Artifacts directories.");

        var temporaryFiles = root.EnumerateFiles("*", SearchOption.AllDirectories)
            .Where(file => !file.FullName.Split(Path.DirectorySeparatorChar).Any(ignoredDirectoryNames.Contains))
            .Where(file =>
                file.Extension.Equals(".tmp", StringComparison.OrdinalIgnoreCase) ||
                file.Extension.Equals(".log", StringComparison.OrdinalIgnoreCase))
            .Select(file => Path.GetRelativePath(root.FullName, file.FullName))
            .ToArray();

        await Assert.That(temporaryFiles).IsEquivalentTo(Array.Empty<string>(), CollectionOrdering.Matching)
            .Because("Source distribution should not include temporary files or raw logs.");
    }

    private static DirectoryInfo FindRepositoryRoot()
    {
        var current = new DirectoryInfo(AppContext.BaseDirectory);
        while (current is not null)
        {
            if (File.Exists(Path.Combine(current.FullName, "Pattrn.sln")))
            {
                return current;
            }

            current = current.Parent;
        }

        throw new InvalidOperationException("Could not locate the repository root from the test output directory.");
    }
}
