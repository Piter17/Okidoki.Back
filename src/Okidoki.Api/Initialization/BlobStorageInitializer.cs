using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

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
            logger.LogInformation("Initializing Blob Storage CORS settings...");

            var properties = await client.GetPropertiesAsync(cancellationToken);

            properties.Value.Cors.Clear();
            properties.Value.Cors.Add(new BlobCorsRule
            {
                AllowedOrigins = "https://okidoki.fun, http://localhost:6226, https://localhost:6226, https://test.okidoki.fun",
                AllowedMethods = "GET, HEAD, OPTIONS",
                AllowedHeaders = "*",
                ExposedHeaders = "*",
                MaxAgeInSeconds = 3600
            });

            await client.SetPropertiesAsync(properties, cancellationToken: cancellationToken);

            logger.LogInformation("Blob Storage CORS settings applied successfully.");
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
