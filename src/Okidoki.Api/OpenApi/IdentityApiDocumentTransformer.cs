using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;
using Okidoki.Api.Controllers;

namespace Okidoki.Api.OpenApi;

public sealed class IdentityApiDocumentTransformer : IOpenApiDocumentTransformer
{
    private const string IdentityNewTag = "Auth";
    private const string IdentityRoute = $"/{AuthController.ControllerRoute}";
    private const string IdentityRegistrationRoute = $"{IdentityRoute}/register";

    public Task TransformAsync(OpenApiDocument document, OpenApiDocumentTransformerContext context, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (document.Paths.TryGetValue(IdentityRegistrationRoute, out var registrationPath))
        {
            registrationPath.Operations.Remove(HttpMethod.Post);

            if (registrationPath.Operations.Count == 0)
            {
                document.Paths.Remove(IdentityRegistrationRoute);
            }
        }

        var authTag = new OpenApiTagReference(IdentityNewTag, document);
        foreach (var (path, pathItem) in document.Paths.Where(x => x.Key.StartsWith(IdentityRoute, StringComparison.OrdinalIgnoreCase)))
        {
            foreach (var operation in pathItem.Operations.Values)
            {
                operation.Tags = new HashSet<OpenApiTagReference> { authTag };
            }
        }

        return Task.CompletedTask;
    }
}
