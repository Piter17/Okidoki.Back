namespace Okidoki.App.Contracts;

public sealed class UpdateChatMessageRequest
{
    public required string Content { get; set; } = string.Empty;
}
