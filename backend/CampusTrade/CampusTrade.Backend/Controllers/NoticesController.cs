using CampusTrade.Backend.Models;
using CampusTrade.Backend.Models.DTOs;
using CampusTrade.Backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CampusTrade.Backend.Controllers;

[ApiController]
[Route("api/notices")]
[AllowAnonymous]
public class NoticesController(IAdminService adminService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetList([FromQuery] int page = 1, [FromQuery] int size = 10,
        [FromQuery] string? noticeType = null)
    {
        try
        {
            return Ok(ApiResponse<NoticeListResult>.Success(
                await adminService.GetPublicNoticesAsync(page, size, noticeType)));
        }
        catch (ArgumentException)
        {
            return BadRequest(ApiResponse<object>.Fail(400, "公告类型无效"));
        }
    }

    [HttpGet("{noticeId:int}")]
    public async Task<IActionResult> GetById(int noticeId)
    {
        var notice = await adminService.GetPublicNoticeByIdAsync(noticeId);
        return notice == null
            ? NotFound(ApiResponse<object>.Fail(404, "公告不存在或已删除"))
            : Ok(ApiResponse<NoticeDto>.Success(notice));
    }
}
