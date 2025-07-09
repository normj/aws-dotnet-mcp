using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AWS.NET.MCP.Utils;

public class CaseInsensitiveEnumConverter<T> : TypeConverter where T : struct, Enum
{
    public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        => sourceType == typeof(string);

    public override object ConvertFrom(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value)
    {
        if (value is string s && Enum.TryParse<T>(s, ignoreCase: true, out var result))
            return result;

        throw new FormatException($"Cannot convert '{value}' to {typeof(T).Name}.");
    }
}