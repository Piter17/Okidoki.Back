using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Okidoki.Api.Extensions;

namespace Okidoki.Api.Initialization;

public class BlobStorageInitializer(
        BlobServiceClient client,
        ILogger<BlobStorageInitializer> logger
    ) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        try
        {
            var properties = await client.GetPropertiesAsync(cancellationToken);

            var corsRule = new BlobCorsRule
            {
                AllowedOrigins = "https://okidoki.fun,http://localhost:6226,https://localhost:6226,https://test.okidoki.fun",
                AllowedMethods = "GET,HEAD,OPTIONS",
                AllowedHeaders = "*",
                ExposedHeaders = "*",
                MaxAgeInSeconds = 3600
            };

            var isSame = properties.Value.Cors.Count == 1 &&
                properties.Value.Cors[0].HasSameValuesAs(corsRule);

            if (isSame == false)
            {
                logger.LogInformation("Initializing Blob Storage CORS settings...");
                properties.Value.Cors.Clear();
                properties.Value.Cors.Add(corsRule);
                await client.SetPropertiesAsync(properties, cancellationToken: cancellationToken);

                logger.LogInformation("Blob Storage CORS settings applied successfully.");
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to initialize Blob Storage CORS settings.");
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
