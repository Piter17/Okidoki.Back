using Microsoft.AspNetCore.SignalR;

namespace Okidoki.Api.Extensions;

public static class HubCallerContextExtensions
{
    extension(HubCallerContext context)
    {
        public long UserId => long.Parse(context.UserIdentifier!);
    }
}
