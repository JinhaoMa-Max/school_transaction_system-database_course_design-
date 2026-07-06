using CampusSecondHand.Api.Models.Dtos;

namespace CampusSecondHand.Api.Services.Interfaces;

public interface IGoodsService
{
    Task<GoodsListResult> GetListAsync(int page, int size);
    Task<GoodsDto?> GetByIdAsync(int id);
}
