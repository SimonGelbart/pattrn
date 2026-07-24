#pragma warning disable CS1591
namespace Pattrn.Registrations;

/// <summary>Stable identity for a canonical pattern registration.</summary>
public readonly record struct RegistrationId
{
    public RegistrationId(Guid value)
    {
        if (value == Guid.Empty)
        {
            throw new ArgumentException("A registration ID must not be empty.", nameof(value));
        }

        Value = value;
    }

    public Guid Value { get; }

    public static RegistrationId New() => new(Guid.NewGuid());

    public override string ToString() => Value.ToString();

    public string ToString(string? format, IFormatProvider? formatProvider) => Value.ToString(format, formatProvider);
}
