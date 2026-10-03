namespace Okidoki.App.Interfaces.Services;

public interface IImageStorageService
{
    Task<ImageUploadResult> UploadUserAvatarAsync(long userId, Stream content, string fileName, string? contentType, CancellationToken cancellationToken = default);
    Task<ImageUploadResult> UploadGuildImageAsync(long guildId, Stream content, string fileName, string? contentType, CancellationToken cancellationToken = default);
    Task DeleteAsync(string resourceName, CancellationToken cancellationToken = default);
}

public sealed record ImageUploadResult(bool Success, Uri? ResourceUri = null, Exception? Error = null);
