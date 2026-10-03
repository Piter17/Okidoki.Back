using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;
using Okidoki.App.Contracts;

namespace Okidoki.Api.OpenApi;

public class OutputTypeTransformer(Dictionary<Type, OpenApiSchema> transformDictionary) : IOpenApiSchemaTransformer
{
    public Task TransformAsync(OpenApiSchema schema, OpenApiSchemaTransformerContext context, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (transformDictionary.TryGetValue(context.JsonTypeInfo.Type, out var newSchema))
        {
            schema.Type = newSchema.Type;
            schema.Format = newSchema.Format;
        }

        return Task.CompletedTask;
    }
}
