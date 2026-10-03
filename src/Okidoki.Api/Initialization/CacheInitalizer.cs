using Okidoki.App.Interfaces.Services;

namespace Okidoki.Api.Initialization;

public class CacheInitalizer(IServiceScopeFactory scopeFactory
    ) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await using var scope = scopeFactory.CreateAsyncScope();
        var cacheLoader = scope.ServiceProvider.GetRequiredService<ICacheLoader>();
        await cacheLoader.InitAsync(cancellationToken);
    }

    public Task StopAsync(CancellationToken cancellationToken)
        => Task.CompletedTask;
}
