using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using WebApi.Infrastructure.Components;
using WebApi.Infrastructure.Models.Requests;
using WebApi.Infrastructure.Models.Storage;

namespace WebApi.Services;

public class AuthService(DataComponent component)
{
    private static string GenerateRefreshToken()
    {
        return Convert.ToBase64String(Guid.NewGuid().ToByteArray());
    }

    private string GetJwtKey()
    {
        return Environment.GetEnvironmentVariable("JWT_SECRET")
               ?? "super_secret_key_12345";
    }

    private string GenerateAccessToken(int userId, string username, string role)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes(GetJwtKey());

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, userId.ToString()),
            new(ClaimTypes.Name, username),
            new(ClaimTypes.Role, role)
        };

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddHours(role == "Admin" ? 1 : 3),
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(key),
                SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }

    public async Task<(string accessToken, string refreshToken)?> Refresh(string refreshToken)
    {
        var userToken = component.UserTokens
            .Include(x => x.User)
            .FirstOrDefault(u => u.RefreshToken == refreshToken);

        if (userToken is null) return null;

        if (userToken.ExpiresAt <= DateTime.UtcNow)
            await component.Delete<UserToken>(userToken.Id);

        var accessToken = GenerateAccessToken(userToken.User.Id, userToken.User.Username, "Client");
        
        var newRefreshToken = GenerateRefreshToken();
        
        userToken.RefreshToken = newRefreshToken;

        await component.Update(userToken);
        
        return (accessToken, newRefreshToken);
    }

    public async Task<Infrastructure.Models.Responses.Login?> Login(Login request)
    {
        var response =  new Infrastructure.Models.Responses.Login();
        
        string? role;
        int userId;
        User? user = null;

        if (request is { UserName: "admin", Password: "admin123" })
        {
            role = "Admin";
            userId = 0;
        }
        else
        {
            user = component.Users.FirstOrDefault(u =>
                u.Username == request.UserName && u.Password == request.Password);

            if (user == null || user.IsBlocked)
                return null;

            role = "Client";
            userId = user.Id;

            user.LastLogin = DateTime.Now;
            user.ModifiedAt = DateTime.UtcNow;
        }

        var accessToken = GenerateAccessToken(userId, request.UserName, role);

        if (user == null)
        {
            response.AccessToken = accessToken;
            return response;
            
        }

        var refreshToken = GenerateRefreshToken();
        
        var userToken = new UserToken()
        {
            UserId = user.Id,
            RefreshToken = refreshToken,
        };

        await component.Insert(userToken);

        response.AccessToken = accessToken;
        response.RefreshToken = refreshToken;
        
        return response;
    }

    public async Task<bool> Register(Register request)
    {
        var user = await component.Users.FirstOrDefaultAsync(u =>
            u.Username == request.Username);

        if (user != null)
            throw new Exception("Имя пользователя занято.");

        var newUser = new User
        {
            FirstName = request.FirstName,
            LastName = request.LastName,
            Username = request.Username,
            Password = request.Password,
            LastLogin = DateTime.MaxValue
        };

        return await component.Insert(newUser);
    }
}