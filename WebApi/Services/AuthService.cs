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
    public async Task<string?> LoginAdmin(Login request)
    {
        if (request is { UserName: "admin", Password: "admin123" })
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Environment.GetEnvironmentVariable("JWT_SECRET") ?? "super_secret_key_12345";
            
            Console.WriteLine(key);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity([
                    new Claim(ClaimTypes.Name, request.UserName),
                    new Claim(ClaimTypes.Role, "Admin")
                ]),
                Expires = DateTime.UtcNow.AddHours(1),
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
                    SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);

            return tokenHandler.WriteToken(token);
        }

        return null;
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

    public async Task<string?> Login(Login request)
    {
        var user = component.Users.FirstOrDefault(u =>
            u.Username == request.UserName && u.Password == request.Password);
        
        if (user == null || user.IsBlocked)
            return null;
        
        user.LastLogin = DateTime.Now;
        await component.Update(user);

        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Environment.GetEnvironmentVariable("JWT_SECRET") ?? "super_secret_key_12345";
        
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity([
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, request.UserName),
                new Claim(ClaimTypes.Role, "Client")
            ]),
            Expires = DateTime.UtcNow.AddHours(3),
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
                SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }
}