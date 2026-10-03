using System.Text.Json;
using System.Text.Json.Serialization;
using Okidoki.App.Interfaces.Services;

namespace Okidoki.Api.Json;

public sealed class CdnResourceAddSasConverter(IImageStorageSecureService imageStorageService) : JsonConverter<string>
{
    public override string? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == System.Text.Json.JsonTokenType.Null)
            return null;

        var value = reader.GetString();
        return value;
    }

    public override void Write(Utf8JsonWriter writer, string value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(imageStorageService.GetReadUri(value, TimeSpan.FromDays(90)).ToString());
    }
}

