using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

namespace Okidoki.Api.Extensions;

public static partial class JsonExtensions
{
    public static Action<JsonTypeInfo> WithPropertyConverterFor<TAttribute>(
        JsonConverter? converter,
        bool checkCanConvert = false)
        where TAttribute : System.Attribute => typeInfo =>
    {
        if (typeInfo.Kind != JsonTypeInfoKind.Object)
            return;

        foreach (var property in typeInfo.Properties)
        {
            if (property
                .AttributeProvider?
                .GetCustomAttributes(typeof(TAttribute), true)
                .Length > 0
                && (!checkCanConvert || converter?.CanConvert(property.PropertyType) is true))
            {
                property.CustomConverter = converter;
            }

        }
    };
}