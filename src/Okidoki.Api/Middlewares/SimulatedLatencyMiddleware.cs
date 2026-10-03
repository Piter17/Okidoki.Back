namespace Okidoki.Api.Middlewares;

public class SimulatedLatencyMiddleware(
    RequestDelegate next,
    TimeSpan min,
    TimeSpan max)
{
    private readonly RequestDelegate _next = next;
    private readonly int _minDelayInMs = (int)min.TotalMilliseconds;
    private readonly int _maxDelayInMs = (int)max.TotalMilliseconds;
    private readonly ThreadLocal<Random> _random = new(() => new Random());

    public async Task Invoke(HttpContext context)
    {
        int delayInMs = _random.Value?.Next(
            _minDelayInMs,
            _maxDelayInMs
        ) ?? 0;

        await Task.Delay(delayInMs);
        await _next(context);
    }
}

public static class SimulatedLatencyExtensions
{
    public static IApplicationBuilder UseSimulatedLatency(
        this IApplicationBuilder app,
        TimeSpan min,
        TimeSpan max
    )
    {
        return app.UseMiddleware<SimulatedLatencyMiddleware>(min, max);
    }
}