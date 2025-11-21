using Microsoft.AspNetCore.Mvc;
using WebApi.Infrastructure.Models.Requests;
using WebApi.Services;

namespace WebApi.Controllers;

[ApiController]
[Route("[controller]/[action]")]
public class AuthController(AuthService service) : Controller
{
    [HttpGet]
    public IActionResult LoginAdmin()
    {
        return View();
    }
    
    [HttpPost]
    public async Task<IActionResult> LoginAdmin([FromForm] Login request)
    {
        try
        {
            var token = await service.LoginAdmin(request);

            if (string.IsNullOrEmpty(token))
            {
                return RedirectToAction("LoginAdmin", "Auth");
            }
            
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,      
                SameSite = SameSiteMode.Strict,
                Expires = DateTimeOffset.UtcNow.AddHours(1)
            };

            Response.Cookies.Append("AuthToken", token, cookieOptions);

            return RedirectToAction("Index", "Admin");
        }
        catch
        {
            return RedirectToAction("LoginAdmin", "Auth");
        }
    }

    [HttpPost]
    public async Task<ActionResult<bool>> Register(Register request)
    {
        try
        {
            return StatusCode(200, await service.Register(request));
        }
        catch (Exception e)
        {
            return StatusCode(500, e.Message);
        }
    }

    [HttpPost]
    public async Task<ActionResult<string?>> LoginClient(Login request)
    {
        try
        {
            return StatusCode(200, await service.Login(request));
        }
        catch (Exception e)
        {
            return StatusCode(500, e.Message);
        }
    }

}