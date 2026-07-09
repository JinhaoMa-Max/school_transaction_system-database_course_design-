using CampusTrade.Backend.Models;
using CampusTrade.Backend.Models.DTOs;
using CampusTrade.Backend.Services;
using Microsoft.AspNetCore.Mvc;

namespace CampusTrade.Backend.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly IUploadService _uploadService;

    public AuthController(IAuthService authService, IUploadService uploadService)
    {
        _authService = authService;
        _uploadService = uploadService;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
    {
        try
        {
            var result = await _authService.LoginAsync(request);
            return Ok(LoginHttpResponseDto.Success(result, "登录成功"));
        }
        catch (AuthException ex)
        {
            return ToErrorResult(ex);
        }
        catch
        {
            return StatusCode(500, ApiResponse<object>.Fail(500, "服务器内部错误"));
        }
    }

    [HttpPost("logout")]
    public IActionResult Logout()
    {
        return Ok(ApiResponse<object>.Success(new { success = true }, "退出登录成功"));
    }

    [HttpGet("current")]
    public async Task<IActionResult> Current()
    {
        try
        {
            var user = await _authService.GetCurrentUserAsync(ReadBearerToken());
            return Ok(UserHttpResponseDto.Success(user));
        }
        catch (AuthException ex)
        {
            return ToErrorResult(ex);
        }
        catch
        {
            return StatusCode(500, ApiResponse<object>.Fail(500, "服务器内部错误"));
        }
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequestDto request)
    {
        try
        {
            var user = await _authService.RegisterAsync(request);
            return Ok(UserHttpResponseDto.Success(user, "注册成功"));
        }
        catch (AuthException ex)
        {
            return ToErrorResult(ex);
        }
        catch
        {
            return StatusCode(500, ApiResponse<object>.Fail(500, "服务器内部错误"));
        }
    }

    [HttpPost("student-auth")]
    public async Task<IActionResult> SubmitStudentAuth([FromBody] StudentAuthRequestDto request)
    {
        try
        {
            var auth = await _authService.SubmitStudentAuthAsync(request, ResolveCurrentUserId());
            return Ok(StudentAuthHttpResponseDto.Success(auth, "认证信息已提交"));
        }
        catch (AuthException ex)
        {
            return ToErrorResult(ex);
        }
        catch
        {
            return StatusCode(500, ApiResponse<object>.Fail(500, "服务器内部错误"));
        }
    }

    [HttpGet("student-auth/{userId:int}")]
    public async Task<IActionResult> GetStudentAuth(int userId)
    {
        try
        {
            var auth = await _authService.GetStudentAuthByUserIdAsync(userId);
            if (auth == null)
            {
                return NotFound(ApiResponse<object>.Fail(404, "认证记录不存在"));
            }

            return Ok(StudentAuthHttpResponseDto.Success(auth));
        }
        catch (AuthException ex)
        {
            return ToErrorResult(ex);
        }
        catch
        {
            return StatusCode(500, ApiResponse<object>.Fail(500, "服务器内部错误"));
        }
    }

    [HttpPut("student-auth/{authId:int}")]
    public async Task<IActionResult> UpdateStudentAuth(int authId, [FromBody] StudentAuthRequestDto request)
    {
        try
        {
            var auth = await _authService.UpdateStudentAuthAsync(authId, request);
            return Ok(StudentAuthHttpResponseDto.Success(auth, "认证信息已更新"));
        }
        catch (AuthException ex)
        {
            return ToErrorResult(ex);
        }
        catch
        {
            return StatusCode(500, ApiResponse<object>.Fail(500, "服务器内部错误"));
        }
    }

    [HttpPost("avatar")]
    public async Task<IActionResult> UploadAvatar(IFormFile file)
    {
        try
        {
            var fileName = await _uploadService.UploadImageAsync(file);
            var avatarUrl = _uploadService.GetImageUrl(fileName);

            var user = await _authService.UpdateAvatarAsync(ReadBearerToken(), avatarUrl);
            return Ok(UserHttpResponseDto.Success(user, "头像上传成功"));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ApiResponse<object>.Fail(400, ex.Message));
        }
        catch (AuthException ex)
        {
            return ToErrorResult(ex);
        }
        catch
        {
            return StatusCode(500, ApiResponse<object>.Fail(500, "头像上传失败"));
        }
    }

    private int? ResolveCurrentUserId()
    {
        var token = ReadBearerToken();
        return _authService.TryGetUserIdFromToken(token);
    }

    private string? ReadBearerToken()
    {
        var value = Request.Headers.Authorization.ToString();
        const string prefix = "Bearer ";
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        return value.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)
            ? value[prefix.Length..].Trim()
            : value.Trim();
    }

    private IActionResult ToErrorResult(AuthException ex)
    {
        var response = ApiResponse<object>.Fail(ex.Code, ex.Message);
        return ex.Code switch
        {
            401 => Unauthorized(response),
            403 => StatusCode(403, response),
            404 => NotFound(response),
            409 => StatusCode(409, response),
            _ => BadRequest(response)
        };
    }
}
