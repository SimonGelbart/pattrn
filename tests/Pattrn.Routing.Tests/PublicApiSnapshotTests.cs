using System.Reflection;
using static Pattrn.Routing.Tests.TestAssertions;

namespace Pattrn.Routing.Tests;

public sealed class PublicApiSnapshotTests
{
    [Test]
    public void PublicApiMatchesShippedSnapshot()
    {
        var actual = string.Join(Environment.NewLine, GetPublicApi()) + Environment.NewLine;
        var snapshotPath = Path.Combine(AppContext.BaseDirectory, "PublicApi.Shipped.txt");

        var updateSnapshotPath = Environment.GetEnvironmentVariable("PUBLIC_API_SNAPSHOT_PATH");
        if (!string.IsNullOrWhiteSpace(updateSnapshotPath))
        {
            File.WriteAllText(updateSnapshotPath, actual);
        }

        if (!File.Exists(snapshotPath))
        {
            throw new FileNotFoundException("The public API snapshot was not copied to the test output directory.", snapshotPath);
        }

        var expected = File.ReadAllText(snapshotPath).ReplaceLineEndings();
        actual = actual.ReplaceLineEndings();

        ShouldEqual(actual, expected, "The public API changed. Review the change, then update PublicApi.Shipped.txt intentionally.");
    }

    private static IReadOnlyList<string> GetPublicApi()
    {
        var assembly = typeof(RoutePattern).Assembly;
        var lines = new List<string>();

        foreach (var type in assembly.GetExportedTypes().OrderBy(type => type.FullName, StringComparer.Ordinal))
        {
            lines.Add(FormatType(type));

            foreach (var constructor in type.GetConstructors(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                         .OrderBy(FormatConstructor, StringComparer.Ordinal))
            {
                lines.Add($"  {FormatConstructor(constructor)}");
            }

            foreach (var property in type.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly)
                         .OrderBy(property => property.Name, StringComparer.Ordinal))
            {
                lines.Add($"  {FormatProperty(property)}");
            }

            foreach (var method in type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly)
                         .Where(method => !method.IsSpecialName || method.Name.StartsWith("op_", StringComparison.Ordinal))
                         .OrderBy(FormatMethod, StringComparer.Ordinal))
            {
                lines.Add($"  {FormatMethod(method)}");
            }
        }

        return lines;
    }

    private static string FormatType(Type type)
    {
        var kind = type.IsInterface ? "interface" : type.IsValueType ? "struct" : "class";
        return $"{kind} {FormatTypeName(type)}";
    }

    private static string FormatConstructor(ConstructorInfo constructor)
    {
        return $".ctor({FormatParameters(constructor.GetParameters())})";
    }

    private static string FormatProperty(PropertyInfo property)
    {
        var accessors = property.SetMethod is null ? "get" : "get; set";
        var staticPrefix = (property.GetMethod ?? property.SetMethod)?.IsStatic == true ? "static " : string.Empty;
        return $"{staticPrefix}{FormatTypeName(property.PropertyType)} {property.Name} {{ {accessors}; }}";
    }

    private static string FormatMethod(MethodInfo method)
    {
        var staticPrefix = method.IsStatic ? "static " : string.Empty;
        var genericArguments = method.IsGenericMethodDefinition
            ? $"<{string.Join(", ", method.GetGenericArguments().Select(argument => argument.Name))}>"
            : string.Empty;

        return $"{staticPrefix}{FormatTypeName(method.ReturnType)} {method.Name}{genericArguments}({FormatParameters(method.GetParameters())})";
    }

    private static string FormatParameters(ParameterInfo[] parameters)
    {
        return string.Join(", ", parameters.Select(FormatParameter));
    }

    private static string FormatParameter(ParameterInfo parameter)
    {
        var prefix = parameter.IsOut ? "out " : parameter.ParameterType.IsByRef ? "ref " : string.Empty;
        var parameterType = parameter.ParameterType.IsByRef
            ? parameter.ParameterType.GetElementType()!
            : parameter.ParameterType;

        var defaultValue = parameter.HasDefaultValue
            ? $" = {FormatDefaultValue(parameter.DefaultValue, parameterType)}"
            : string.Empty;

        return $"{prefix}{FormatTypeName(parameterType)} {parameter.Name}{defaultValue}";
    }

    private static string FormatDefaultValue(object? value, Type parameterType)
    {
        return value switch
        {
            null when parameterType.IsValueType => "default",
            null => "null",
            string text => $"\"{text}\"",
            char character => $"'{character}'",
            bool boolean => boolean ? "true" : "false",
            _ => value.ToString() ?? string.Empty
        };
    }

    private static string FormatTypeName(Type type)
    {
        if (type.IsGenericParameter)
        {
            return type.Name;
        }

        if (type.IsArray)
        {
            return $"{FormatTypeName(type.GetElementType()!)}[]";
        }

        if (type.IsByRef)
        {
            return FormatTypeName(type.GetElementType()!);
        }

        if (type == typeof(void)) return "void";
        if (type == typeof(bool)) return "bool";
        if (type == typeof(byte)) return "byte";
        if (type == typeof(char)) return "char";
        if (type == typeof(decimal)) return "decimal";
        if (type == typeof(double)) return "double";
        if (type == typeof(float)) return "float";
        if (type == typeof(int)) return "int";
        if (type == typeof(long)) return "long";
        if (type == typeof(object)) return "object";
        if (type == typeof(short)) return "short";
        if (type == typeof(string)) return "string";

        if (type.IsGenericType)
        {
            var typeDefinitionName = type.GetGenericTypeDefinition().FullName ?? type.Name;
            var tickIndex = typeDefinitionName.IndexOf('`', StringComparison.Ordinal);
            if (tickIndex >= 0)
            {
                typeDefinitionName = typeDefinitionName[..tickIndex];
            }

            return $"{typeDefinitionName}<{string.Join(", ", type.GetGenericArguments().Select(FormatTypeName))}>";
        }

        return type.FullName ?? type.Name;
    }
}
