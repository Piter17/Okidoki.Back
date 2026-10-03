using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Options;
using Okidoki.Api.Extensions;
using Okidoki.App.Contracts;
using Okidoki.App.Interfaces.Services;

namespace Okidoki.Api.Json;

public sealed class JsonOptionsSetup(IImageStorageSecureService imageStorageService)
    : IConfigureOptions<Microsoft.AspNetCore.Http.Json.JsonOptions>,
      IConfigureOptions<Microsoft.AspNetCore.Mvc.JsonOptions>,
      IConfigureOptions<JsonHubProtocolOptions>
{
    public void Configure(Microsoft.AspNetCore.Http.Json.JsonOptions options)
    {
        Configure(options.SerializerOptions, imageStorageService);
    }

    public void Configure(Microsoft.AspNetCore.Mvc.JsonOptions options)
    {
        Configure(options.JsonSerializerOptions, imageStorageService);
    }

    public void Configure(JsonHubProtocolOptions options)
    {
        Configure(options.PayloadSerializerOptions, imageStorageService);
    }

    internal static void Configure(JsonSerializerOptions options, IImageStorageSecureService imageStorageService)
    {
        options.Converters.Add(new JsonStringEnumConverter());
        options.Converters.Add(new LongToStringConverter());
        options.NumberHandling = JsonNumberHandling.Strict;
        options.ReferenceHandler = ReferenceHandler.IgnoreCycles;
        options.TypeInfoResolver = new DefaultJsonTypeInfoResolver()
            .WithAddedModifier(
                JsonExtensions.WithPropertyConverterFor<CdnResourceAttribute>(
                    new CdnResourceAddSasConverter(imageStorageService)
                )
            );
    }

}

