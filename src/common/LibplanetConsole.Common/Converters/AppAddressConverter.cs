using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using Libplanet.Crypto;

namespace LibplanetConsole.Common.Converters;

internal sealed class AppAddressConverter : TypeConverter
{
    public override bool CanConvertFrom(ITypeDescriptorContext? context, Type sourceType)
        => sourceType == typeof(string) || base.CanConvertFrom(context, sourceType);

    public override bool CanConvertTo(
        ITypeDescriptorContext? context, [NotNullWhen(true)] Type? destinationType)
        => destinationType == typeof(string) || base.CanConvertTo(context, destinationType);

    public override object? ConvertFrom(
        ITypeDescriptorContext? context, CultureInfo? culture, object value)
    {
        if (value is string text)
        {
            return ParseOrDefault(text);
        }

        return base.ConvertFrom(context, culture, value);
    }

    public override object? ConvertTo(
        ITypeDescriptorContext? context, CultureInfo? culture, object? value, Type destinationType)
    {
        if (value is Address address)
        {
            return address.ToString();
        }

        return base.ConvertTo(context, culture, value, destinationType);
    }

    public static Address? ParseOrDefault(string text)
        => text == string.Empty ? null : new Address(text);
}
