using System.Diagnostics;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;
using Okidoki.App.Extensions;

namespace Okidoki.Api.OpenApi;

public class NormalizeOperationNamesTransformer : IOpenApiOperationTransformer
{
    public Task TransformAsync(OpenApiOperation operation, OpenApiOperationTransformerContext context, CancellationToken cancellationToken)
    {
        var method = context.Description.HttpMethod;
        var path = context.Description.RelativePath;
        if (operation.OperationId is null &&
            context.Description.ActionDescriptor is ControllerActionDescriptor action)
        {
            operation.OperationId = action.MethodInfo.Name;
            return Task.CompletedTask;
        }

        if (operation.OperationId is null
            && !string.IsNullOrEmpty(method)
            && !string.IsNullOrEmpty(path))
        {
            var pathSplits = path.Split('/').Skip(1).ToArray();

            var newName = GetNewName(pathSplits);

            var opId = string.Concat(new[] { string.Concat(
                method.CapitalizeFirstLetter(true),
                newName.CapitalizeFirstLetter()
            )});

            operation.OperationId = opId;
        }
        return Task.CompletedTask;
    }

    private string GetNewName(String[] splits, bool isRecurse = false) => splits switch
            {
                [] => throw new Exception(),
                [var controller] => isRecurse ? controller.SkipLastS() : controller,
                [var controller, var last] => IsParam(last)
                    ? isRecurse ? controller.SkipLastS() : controller
                    : last,
                [var controller, .. var rest, var last] => IsParam(last)
                    ? GetNewName([controller, ..rest], true)
                    : last,
            };
    private bool IsParam(String value) => value is ['{',..,'}'];
}
