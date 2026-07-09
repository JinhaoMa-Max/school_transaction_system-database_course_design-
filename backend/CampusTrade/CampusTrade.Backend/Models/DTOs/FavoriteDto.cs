namespace CampusTrade.Backend.Models.DTOs;

public class FavoriteDto
{
    public int FavoriteId { get; set; }
    public int UserId { get; set; }
    public int GoodsId { get; set; }
    public string? GoodsTitle { get; set; }
    public decimal? GoodsPrice { get; set; }
    public string? CoverImage { get; set; }
    public string? GoodsStatus { get; set; }
    public DateTime CreateTime { get; set; }
    public DateTime FavoriteTime { get; set; }
}

public class FavoriteListResult
{
    public List<FavoriteDto> List { get; set; } = new();
    public int Total { get; set; }
    public int Page { get; set; }
    public int Size { get; set; }
}

public class CreateFavoriteRequest
{
    public int GoodsId { get; set; }
}

public class FavoriteCheckResult
{
    public bool IsFavorite { get; set; }
    public int? FavoriteId { get; set; }
}
