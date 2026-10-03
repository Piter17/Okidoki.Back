using Azure.Storage.Blobs;
using Azure.Storage.Sas;
using Okidoki.App.Interfaces.Services;

namespace Okidoki.Infrastructure.Storage;

public class ImageStorageSecureService(BlobServiceClient blobServiceClient) : IImageStorageSecureService
{
    public Uri GetReadUri(string resourceName, TimeSpan lifetime)
    {
        var uri = new Uri(resourceName);

        var splits = uri.AbsolutePath[1..].Split("/", count: 2);
        var containerClient = blobServiceClient.GetBlobContainerClient(splits[0]);
        var blobClient = containerClient.GetBlobClient(splits[1]);

        var signed = blobClient.GenerateSasUri(BlobSasPermissions.Read, DateTime.UtcNow.Add(lifetime));
        return signed;
    }
}
