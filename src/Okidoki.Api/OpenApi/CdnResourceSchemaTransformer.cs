using System.Reflection;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;
using Okidoki.App.Contracts;

namespace Okidoki.Api.OpenApi;

public sealed class CdnResourceSchemaTransformer : IOpenApiSchemaTransformer
{
    public Task TransformAsync(OpenApiSchema schema, OpenApiSchemaTransformerContext context, CancellationToken cancellationToken)
    {
        var declaringType = context?.JsonPropertyInfo?.DeclaringType;
        if (declaringType is null)
            return Task.CompletedTask;

        var propertyName = context?.JsonPropertyInfo?.Name;

        if (string.IsNullOrWhiteSpace(propertyName))
            return Task.CompletedTask;

        var prop = declaringType.GetProperty(propertyName,
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.IgnoreCase);

        if (prop is null)
            return Task.CompletedTask;

        var hasCdnAttr = prop.GetCustomAttributes(inherit: true)
            .Any(a => a.GetType() == typeof(CdnResourceAttribute));

        if (!hasCdnAttr)
            return Task.CompletedTask;

        schema.Type = JsonSchemaType.Null | JsonSchemaType.String;
        return Task.CompletedTask;
    }
}