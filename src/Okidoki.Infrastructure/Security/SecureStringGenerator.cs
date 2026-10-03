using System.Security.Cryptography;
using Okidoki.App.Interfaces.Services;

namespace Okidoki.Infrastructure.Security;

public class SecureStringGenerator : ISecureStringGenerator
{
    public string GenerateInvitationCode()
    {
        var bytesLength = 10;
        var alphabet = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";
        return RandomNumberGenerator.GetString(alphabet, bytesLength);
    }
}
