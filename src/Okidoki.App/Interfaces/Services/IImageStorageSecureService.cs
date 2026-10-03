namespace Okidoki.App.Interfaces.Services;

public interface IImageStorageSecureService
{
    Uri GetReadUri(string resourceName, TimeSpan lifetime);
}
