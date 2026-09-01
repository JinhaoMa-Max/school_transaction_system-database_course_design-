using CampusTrade.Backend.Models;
using CampusTrade.Backend.Services;
using Microsoft.AspNetCore.Mvc;

namespace CampusTrade.Backend.Controllers;

[ApiController]
[Route("api/upload")]
public class UploadController : ControllerBase
{
    private readonly IUploadService _uploadService;
    private readonly IAuthService _authService;

    public UploadController(IUploadService uploadService, IAuthService authService)
    {
        _uploadService = uploadService;
        _authService = authService;
    }

    [HttpPost("image")]
    public async Task<IActionResult> UploadImage(IFormFile file)
    {
        try
        {
            var token = Request.Headers.Authorization.ToString();
            if (!_authService.TryGetUserIdFromToken(token.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase)
                    ? token[7..].Trim()
                    : token.Trim()).HasValue)
            {
                return Unauthorized(ApiResponse<object>.Fail(401, "login required"));
            }

            var fileName = await _uploadService.UploadImageAsync(file);
            var url = _uploadService.GetImageUrl(fileName);
            return Ok(ApiResponse<object>.Success(new { fileName, url }, "图片上传成功"));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ApiResponse<object>.Fail(400, ex.Message));
        }
        catch (Exception)
        {
            return StatusCode(500, ApiResponse<object>.Fail(500, "图片上传失败"));
        }
    }
}
