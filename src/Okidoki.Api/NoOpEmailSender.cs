using Microsoft.AspNetCore.Identity;
using Okidoki.Domain.Entities;

namespace Okidoki.Api;

public sealed class NoOpEmailSender : IEmailSender<Identity>
{
    public Task SendConfirmationLinkAsync(Identity user, string email, string confirmationLink)
    {
        return Task.CompletedTask;
    }

    public Task SendPasswordResetCodeAsync(Identity user, string email, string resetCode)
    {
        return Task.CompletedTask;
    }

    public Task SendPasswordResetLinkAsync(Identity user, string email, string resetLink)
    {
        return Task.CompletedTask;
    }
}
