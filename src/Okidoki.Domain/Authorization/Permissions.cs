namespace Okidoki.Domain.Authorization;

[Flags]
public enum Permissions : ulong
{
    None = 0,
    ReadContent = 1UL << 0,
    SendMessages = 1UL << 1,
    SendAttachments = 1UL << 2,
    CreateChannels = 1UL << 3,
    RemoveChannels = 1UL << 4,
    KickUser = 1UL << 5,
    BanUser = 1UL << 6,
    PingEveryone = 1UL << 7,
}