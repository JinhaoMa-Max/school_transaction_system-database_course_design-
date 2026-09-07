using CampusTrade.Backend.Models;
using CampusTrade.Backend.Services;
using Microsoft.AspNetCore.Mvc;

namespace CampusTrade.Backend.Controllers;

[ApiController]
[Route("api/help")]
public class HelpController : ControllerBase
{
    private readonly IAiKnowledgeService _knowledgeService;

    public HelpController(IAiKnowledgeService knowledgeService)
    {
        _knowledgeService = knowledgeService;
    }

    [HttpGet("manual")]
    public IActionResult GetManual()
    {
        var text = _knowledgeService.GetManualText();
        if (text == null)
        {
            return NotFound(ApiResponse<object>.Fail(404, "帮助文档不存在"));
        }

        return Ok(ApiResponse<string>.Success(text));
    }
}
