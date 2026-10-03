using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;

namespace Okidoki.Api.Configuration;

public static class BearerTokenConfiguration
{
    extension(AuthenticationBuilder builder)
    {
        public AuthenticationBuilder ConfigureBearerToken()
            => builder.AddBearerToken(IdentityConstants.BearerScheme, options =>
            {
                // temporary fix for websocket in app when running on web
                options.Events.OnMessageReceived += context =>
                {
                    var accessToken = context.Request.Query["access_token"];
                    var path = context.HttpContext.Request.Path;
                    if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hubs/realtime"))
                    {
                        context.Token = accessToken;
                    }

                    return Task.CompletedTask;
                };
                options.BearerTokenExpiration = TimeSpan.FromMinutes(2.1 * 60);
                options.RefreshTokenExpiration = TimeSpan.FromDays(365);
            });
    }

}
