using CampusTrade.Backend.Models; // 里面包含 ApiResponse 结构
using CampusTrade.Backend.Models.DTOs;
using CampusTrade.Backend.Services;
using Microsoft.AspNetCore.Mvc;

namespace CampusTrade.Backend.Controllers;

[ApiController]
// 如果前端没有加 api 前缀，就写 [Route("auth")]；如果加了就写 [Route("api/auth")]
// 结合之前的逻辑，我们采用最标准的 [Route("api/auth")]。如果前端报错404，可以去掉 "api/"
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    /// <summary>
    /// POST /api/auth/login -> 用户登录接口
    /// </summary>
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
    {
        try
        {
            var result = await _authService.LoginAsync(request);
            if (result == null)
            {
                // 密码错误或用户不存在，返回 400 错误
                return BadRequest(ApiResponse<object>.Error(400, "学号或密码错误"));
            }

            // 登录成功，包裹你们统一的 ApiResponse
            return Ok(ApiResponse<AuthResponseDto>.Success(result));
        }
        catch (Exception ex)
        {
            // 捕捉例如“账号被封禁”等业务异常
            return BadRequest(ApiResponse<object>.Error(400, ex.Message));
        }
    }

    /// <summary>
    /// POST /api/auth/register -> 用户注册接口
    /// </summary>
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequestDto request)
    {
        try
        {
            var isSuccess = await _authService.RegisterAsync(request);
            if (!isSuccess)
            {
                return BadRequest(ApiResponse<object>.Error(400, "注册失败，该学号已被占用"));
            }

            return Ok(ApiResponse<string>.Success("注册成功"));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<object>.Error(400, ex.Message));
        }
    }
}