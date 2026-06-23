using System.Xml.Linq;
using static Pattrn.Tests.TestAssertions;

namespace Pattrn.Tests;

public sealed class PackageMetadataTests
{
    [Test]
    public void PackageMetadataIsCentralizedAndUsesRepositoryMetadata()
    {
        var root = FindRepositoryRoot();
        var directoryBuildProps = XDocument.Load(Path.Combine(root.FullName, "Directory.Build.props"));
        var packageVersion = directoryBuildProps.Descendants("PattrnVersion").SingleOrDefault()?.Value;
        var repositoryType = directoryBuildProps.Descendants("RepositoryType").SingleOrDefault()?.Value;
        var repositoryUrl = directoryBuildProps.Descendants("RepositoryUrl").SingleOrDefault()?.Value;
        var publishRepositoryUrl = directoryBuildProps.Descendants("PublishRepositoryUrl").SingleOrDefault()?.Value;

        ShouldEqual(packageVersion, "0.1.0-alpha.1", "The pre-beta package version should be centralized.");
        ShouldEqual(repositoryType, "git", "Package metadata should point at the real Git repository.");
        ShouldEqual(repositoryUrl, "https://github.com/SimonGelbart/pattrn", "Package metadata should use the public repository URL.");
        ShouldEqual(publishRepositoryUrl, "true", "NuGet packages should publish repository metadata.");

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

            ShouldEqual(license, "MIT", $"{projectPath} should use the MIT SPDX license expression.");
            ShouldEqual(document.Descendants("Version").SingleOrDefault()?.Value, null, $"{projectPath} should inherit the centralized version.");
            ShouldEqual(document.Descendants("RepositoryType").SingleOrDefault()?.Value, null, $"{projectPath} should inherit centralized repository metadata.");
            ShouldEqual(document.Descendants("RepositoryUrl").SingleOrDefault()?.Value, null, $"{projectPath} should inherit centralized repository metadata.");
        }
    }


    [Test]
    public void PackageProjectsUsePackageScopedReadmes()
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

            ShouldEqual(readmeFile, "README.md", $"{pair.Key} should pack a README.md file for NuGet.");
            ShouldBeTrue(File.Exists(pair.Value), $"Missing expected package README source: {pair.Value}");

            var noneItems = document.Descendants("None").ToArray();
            var packsExpectedReadme = noneItems.Any(item =>
                string.Equals(item.Attribute("Pack")?.Value, "true", StringComparison.OrdinalIgnoreCase) &&
                string.Equals(item.Attribute("PackagePath")?.Value, pair.Value.EndsWith("README.md", StringComparison.OrdinalIgnoreCase) ? @"\" : "README.md", StringComparison.Ordinal));

            ShouldBeTrue(packsExpectedReadme, $"{pair.Key} should pack the expected README source for NuGet.");
        }
    }


    [Test]
    public void SourceDistributionDoesNotContainGeneratedOrTemporaryArtifacts()
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

        ShouldSequenceEqual(generatedDirectories, Array.Empty<string>(), "Source distribution should not include BenchmarkDotNet.Artifacts directories.");

        var temporaryFiles = root.EnumerateFiles("*", SearchOption.AllDirectories)
            .Where(file => !file.FullName.Split(Path.DirectorySeparatorChar).Any(ignoredDirectoryNames.Contains))
            .Where(file =>
                file.Extension.Equals(".tmp", StringComparison.OrdinalIgnoreCase) ||
                file.Extension.Equals(".log", StringComparison.OrdinalIgnoreCase))
            .Select(file => Path.GetRelativePath(root.FullName, file.FullName))
            .ToArray();

        ShouldSequenceEqual(temporaryFiles, Array.Empty<string>(), "Source distribution should not include temporary files or raw logs.");
    }

    [Test]
    public void SourceDistributionContainsOfflineBuildGuidance()
    {
        var root = FindRepositoryRoot();

        var expectedFiles = new[]
        {
            Path.Combine(root.FullName, "docs", "development", "building-offline.md")
        };

        foreach (var expectedFile in expectedFiles)
        {
            ShouldBeTrue(File.Exists(expectedFile), $"Missing expected offline build guidance: {expectedFile}");
        }
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
