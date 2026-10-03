using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Sas;
using IdGen;
using Microsoft.Extensions.Logging;
using Okidoki.App.Interfaces.Services;

namespace Okidoki.Infrastructure.Storage;

public sealed class AzureBlobImageStorageService(
    BlobServiceClient blobServiceClient,
    IIdGenerator<long> idGenerator,
    ILogger<AzureBlobImageStorageService> logger) : IImageStorageService
{
    public Task<ImageUploadResult> UploadUserAvatarAsync(
        long userId,
        Stream content,
        string fileName,
        string? contentType,
        CancellationToken cancellationToken = default)
        => UploadAsync("user-data", userId.ToString(), content, fileName, contentType, cancellationToken);

    public Task<ImageUploadResult> UploadGuildImageAsync(
        long guildId,
        Stream content,
        string fileName,
        string? contentType,
        CancellationToken cancellationToken = default)
        => UploadAsync("guild-content", guildId.ToString(), content, fileName, contentType, cancellationToken);

    public async Task DeleteAsync(string resourceName, CancellationToken cancellationToken = default)
    {
        var separatorIndex = resourceName.IndexOf('/');
        if (separatorIndex <= 0 || separatorIndex == resourceName.Length - 1)
        {
            throw new ArgumentException("Resource name must have the format 'container/blob'.", nameof(resourceName));
        }

        var container = blobServiceClient.GetBlobContainerClient(resourceName[..separatorIndex]);
        var blob = container.GetBlobClient(resourceName[(separatorIndex + 1)..]);
        await blob.DeleteIfExistsAsync(cancellationToken: cancellationToken);
    }

    private async Task<ImageUploadResult> UploadAsync(
        string containerName,
        string pathPrefix,
        Stream content,
        string fileName,
        string? contentType,
        CancellationToken cancellationToken)
    {
        try
        {
            var container = blobServiceClient.GetBlobContainerClient(containerName);
            await container.CreateIfNotExistsAsync(cancellationToken: cancellationToken);

            var extension = Path.GetExtension(fileName).ToLowerInvariant();
            var blobName = $"{pathPrefix}/{idGenerator.CreateId()}{extension}";
            var blob = container.GetBlobClient(blobName);
            var options = new BlobUploadOptions
            {
                HttpHeaders = new BlobHttpHeaders { ContentType = contentType },
            };

            var uploadResult = await blob.UploadAsync(content, options, cancellationToken);
            return new ImageUploadResult(true, blob.Uri);
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Unable to upload image to {ContainerName}.", containerName);
            return new ImageUploadResult(false, Error: exception);
        }
    }
}
