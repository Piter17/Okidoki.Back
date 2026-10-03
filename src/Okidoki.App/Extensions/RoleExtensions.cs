using Okidoki.Domain.Entities;

namespace Okidoki.App.Extensions;

public static class RoleExtensions
{
    extension(ICollection<GuildRole> collection)
    {
        public GuildRole Everyone => collection.Single(x => x.Name == GuildRole.EveryoneRoleName);
    }
}
