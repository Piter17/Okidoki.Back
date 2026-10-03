namespace Okidoki.App.Interfaces.Services;

public interface ICacheLoader
{
    Task InitUserAsync(long userId);
    Task InitAsync(CancellationToken cancellationToken = default);
}