using Microsoft.AspNetCore.Identity;

namespace WebApi.Services;

public static class PasswordService
{
    private static readonly PasswordHasher<object> Hasher = new();

    public static string HashPassword(string password)
    {
        return Hasher.HashPassword(null!, password);
    }

    public static bool VerifyPassword(string hashedPassword, string password)
    {
        var result = Hasher.VerifyHashedPassword(null!, hashedPassword, password);

        return result is PasswordVerificationResult.Success or PasswordVerificationResult.SuccessRehashNeeded;
    }
}