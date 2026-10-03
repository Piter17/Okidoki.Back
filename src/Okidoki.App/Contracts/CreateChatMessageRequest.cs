namespace Okidoki.App.Contracts;

public sealed class CreateChatMessageRequest
{
    public required string Content { get; set; } = string.Empty;
}
