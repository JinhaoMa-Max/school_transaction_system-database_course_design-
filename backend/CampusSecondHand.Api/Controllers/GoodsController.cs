using CampusSecondHand.Api.Common;
using CampusSecondHand.Api.Models.Dtos;
using CampusSecondHand.Api.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace CampusSecondHand.Api.Controllers;

[ApiController]
[Route("api/goods")]
public class GoodsController : ControllerBase
{
    private readonly IGoodsService _goodsService;

    public GoodsController(IGoodsService goodsService)
    {
        _goodsService = goodsService;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<GoodsListResult>>> GetList(
        [FromQuery] int page = 1,
        [FromQuery] int size = 12)
    {
        var result = await _goodsService.GetListAsync(page, size);
        return Ok(ApiResponse<GoodsListResult>.Ok(result));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ApiResponse<GoodsDto>>> GetById(int id)
    {
        var goods = await _goodsService.GetByIdAsync(id);
        if (goods is null)
            return NotFound(ApiResponse<GoodsDto>.Fail("Goods not found", 404));
        return Ok(ApiResponse<GoodsDto>.Ok(goods));
    }
}
