using IdGen.DependencyInjection;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Identity;
using Microsoft.OpenApi;
using System.Reflection;
using Okidoki.Api;
using Okidoki.Api.Controllers;
using Okidoki.Api.Extensions;
using Okidoki.Api.Hubs;
using Okidoki.Api.Initialization;
using Okidoki.Api.OpenApi;
using Okidoki.App.Exceptions;
using Okidoki.Domain.Entities;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);
var isOpenApiGeneration = Assembly.GetEntryAssembly()?.GetName().Name == "GetDocument.Insider";

if (isOpenApiGeneration)
{
    builder.Configuration["ConnectionStrings:AppDb"] ??= "Server=localhost;Database=OpenApiBuild;User Id=unused;Password=unused;TrustServerCertificate=True";
    builder.Configuration["ConnectionStrings:BlobStorage"] ??= "UseDevelopmentStorage=true";
}

builder.Services.AddIdGen(123);
builder.Services.AddSingleton<IEmailSender<Identity>, NoOpEmailSender>();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddApplicationServices(builder.Configuration);
builder.Services.AddOpenApi(options =>
{
    options.AddDocumentTransformer<IdentityApiDocumentTransformer>();
    options.AddDocumentTransformer<BearerSecuritySchemeTransformer>();
    options.AddOperationTransformer<NormalizeOperationNamesTransformer>();
    options.AddSchemaTransformer(new OutputTypeTransformer(new() {
        {typeof(long), new OpenApiSchema() {Type = JsonSchemaType.String, Format = null }},
        {typeof(long?), new OpenApiSchema() {Type = JsonSchemaType.String | JsonSchemaType.Null, Format = null }}
    }));
    options.AddSchemaTransformer<CdnResourceSchemaTransformer>();
});
if (!isOpenApiGeneration)
{
    builder.Services.AddHostedService<BlobStorageInitializer>();
    builder.Services.AddHostedService<CacheInitalizer>();
}

builder.Services.AddCors(options =>
{
    options.AddPolicy("CorsPolicy", policy =>
    {
        policy
            .AllowAnyHeader()
            .AllowAnyMethod()
            .WithOrigins("https://okidoki.fun", "https://test.okidoki.fun", "http://localhost:6226", "https://localhost:6226")
            .AllowCredentials();
    });
});

var app = builder.Build();

if (!isOpenApiGeneration)
{
    app.Use(async (context, next) =>
    {
        context.Response.Headers.Append("Access-Control-Allow-Credentials", "true");
        await next();
    });

    app.UseCors("CorsPolicy");

    app.UseExceptionHandler(exceptionApp => exceptionApp.Run(async context =>
    {
        var exception = context.Features.Get<IExceptionHandlerFeature>()?.Error;
        context.Response.StatusCode = exception is GuildPermissionDeniedException or UnauthorizedAccessException
            ? StatusCodes.Status403Forbidden
            : StatusCodes.Status500InternalServerError;
        await context.Response.WriteAsJsonAsync(new
        {
            error = context.Response.StatusCode == StatusCodes.Status403Forbidden
                ? "You do not have permission to perform this operation."
                : "An unexpected error occurred.",
        });
    }));

    // app.UseSimulatedLatency(TimeSpan.FromSeconds(2), TimeSpan.FromSeconds(4));

    app.UseAuthentication();
    app.UseAuthorization();
}

app.MapGroup(AuthController.ControllerRoute)
    .MapIdentityApi<Identity>()
    .AllowAnonymous();

app.MapGroup(AuthController.ControllerRoute)
    .MapPost("/register", () => Results.NotFound())
    .AllowAnonymous()
    .WithOrder(-1);

if (!isOpenApiGeneration)
{
    app.MapHub<RealTimeHub>("/hubs/realtime");
}

app.MapControllers();

if (!isOpenApiGeneration && app.Environment.IsDevelopment())
{
    app.MapOpenApi("/openapi/{documentName}.yaml").AllowAnonymous();
    app.MapOpenApi("/openapi/{documentName}.json").AllowAnonymous();
    app.MapScalarApiReference(options => options.AddPreferredSecuritySchemes("Bearer")).AllowAnonymous();
}

app.Run();
