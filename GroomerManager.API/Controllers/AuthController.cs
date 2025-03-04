using GroomerManager.API.Auth;
using GroomerManager.API.Controllers.Common;
using GroomerManager.Application.Auth;
using GroomerManager.Application.Common.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace GroomerManager.API.Controllers;

[Route("api/[controller]/[action]")]
[ApiController]
public class AuthController : BaseController
{
    private readonly IMediator _mediator;
    private readonly CookieSettings? _cookieSetting;
    private readonly IDateTime _dateTime;
    
    public AuthController(ILogger<AuthController> logger, IMediator mediator, IOptions<CookieSettings> cookieSetting, IDateTime dateTime) : base(logger, mediator)
    {
        _mediator = mediator;
        _cookieSetting = cookieSetting != null ? cookieSetting.Value : null;
        _dateTime = dateTime;
    }
    
    [HttpPost]
    public async Task<ActionResult<LoginCommand.LoginResponse>> Login([FromBody] LoginCommand.LoginRequest request)
    {
        var result = await _mediator.Send(request);
        SetTokenCookie(result.Token);
        SetTokenCookie(result.RefreshToken, true);
        return Ok(result);
    }
    
    [HttpPost]
    public async Task<ActionResult<GeneratePasswordCommand.GeneratePasswordResponse>> GeneratePassword([FromBody] GeneratePasswordCommand.GeneratePasswordRequest request)
    {
        var result = await _mediator.Send(request);
        return Ok(result);
    }
    
    [HttpGet]
    public async Task<ActionResult<RefreshTokenCommand.RefreshTokenResponse>> RefreshToken()
    {
        var result = await _mediator.Send(new RefreshTokenCommand.RefreshTokenRequest() {});
        SetTokenCookie(result.Token);
        SetTokenCookie(result.RefreshToken, true);
        return Ok(result);
    }
    
    [HttpGet]
    public async Task<ActionResult<LogoutCommand.LogoutResponse>> Logout()
    {
        var logoutResult = await _mediator.Send(new LogoutCommand.LogoutRequest());
        DeleteTokenCookie();
        DeleteTokenCookie(true);
        return Ok(logoutResult);
    }
    
    private void SetTokenCookie(string token, bool isRefreshToken = false)
    {
        var cookieOption = new CookieOptions()
        {
            HttpOnly = true,
            Secure = true,
            Expires =   _dateTime.Now.AddMinutes(isRefreshToken ? 24*60*35 : 24*60*30).DateTime,
            SameSite = SameSiteMode.Lax,
        };

        if (_cookieSetting != null)
        {
            cookieOption = new CookieOptions()
            {
                HttpOnly = cookieOption.HttpOnly,
                Expires = cookieOption.Expires,
                Secure = _cookieSetting.Secure,
                SameSite = _cookieSetting.SameSite
            };
        }

        Response.Cookies.Append(isRefreshToken ?  CookieSettings.REFRESH_COKIE_NAME : CookieSettings.COOKIE_NAME, token, cookieOption);
    }
    
    private void DeleteTokenCookie(bool isRefreshToken = false)
    {
        Response.Cookies.Delete(isRefreshToken ?  CookieSettings.REFRESH_COKIE_NAME : CookieSettings.COOKIE_NAME, new CookieOptions()
        {
            HttpOnly = true,
        });
    }
}