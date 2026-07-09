using System.Text.Json.Serialization;

namespace CampusTrade.Backend.Models.DTOs;

/// <summary>
/// 分类数据传输对象（用于响应和树形结构）
/// </summary>
public class CategoryDto
{
    public int CategoryId { get; set; }

    public string CategoryName { get; set; } = string.Empty;

    public int? ParentId { get; set; }

    public int SortOrder { get; set; }

    [JsonPropertyName("children")]
    public List<CategoryDto> Children { get; set; } = new();
}

/// <summary>
/// 创建分类请求
/// </summary>
public class CreateCategoryRequest
{
    public string CategoryName { get; set; } = string.Empty;
    public int? ParentId { get; set; }
    public int SortOrder { get; set; } = 0;
}

/// <summary>
/// 更新分类请求（所有字段均为可选，支持部分更新）
/// </summary>
public class UpdateCategoryRequest
{
    public string? CategoryName { get; set; }
    public int? ParentId { get; set; }
    public int? SortOrder { get; set; }
}