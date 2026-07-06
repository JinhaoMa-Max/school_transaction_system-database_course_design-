using CampusSecondHand.Api.Common;
using CampusSecondHand.Api.Models.Dtos;
using CampusSecondHand.Api.Models.Entities;
using CampusSecondHand.Api.Models.Requests;
using CampusSecondHand.Api.Repositories.Interfaces;
using CampusSecondHand.Api.Services.Interfaces;
using Oracle.ManagedDataAccess.Client;

namespace CampusSecondHand.Api.Services;

public class CategoryService : ICategoryService
{
    private const int ForeignKeyConstraintViolation = 2292;

    private readonly ICategoryRepository _categoryRepository;

    public CategoryService(ICategoryRepository categoryRepository)
    {
        _categoryRepository = categoryRepository;
    }

    public async Task<List<CategoryDto>> GetAllAsync()
    {
        var categories = await _categoryRepository.GetAllAsync();
        return categories.Select(ToDto).ToList();
    }

    public async Task<CategoryDto?> GetByIdAsync(int id)
    {
        var category = await _categoryRepository.GetByIdAsync(id);
        return category is null ? null : ToDto(category);
    }

    public async Task<CategoryDto> CreateAsync(CreateCategoryRequest request)
    {
        var categoryName = NormalizeCategoryName(request.CategoryName);

        await EnsureParentExistsAsync(request.ParentId);

        var category = new Category
        {
            CategoryName = categoryName,
            ParentId = request.ParentId,
            SortOrder = request.SortOrder
        };

        var newCategoryId = await _categoryRepository.CreateAsync(category);
        var createdCategory = await _categoryRepository.GetByIdAsync(newCategoryId);

        if (createdCategory is null)
        {
            throw new BusinessException("Created category could not be loaded.");
        }

        return ToDto(createdCategory);
    }

    public async Task<CategoryDto> UpdateAsync(int id, UpdateCategoryRequest request)
    {
        var existingCategory = await _categoryRepository.GetByIdAsync(id);

        if (existingCategory is null)
        {
            throw new BusinessException("Category does not exist.");
        }

        var categoryName = NormalizeCategoryName(request.CategoryName);

        if (request.ParentId == id)
        {
            throw new BusinessException("Parent category cannot be the category itself.");
        }

        await EnsureParentExistsAsync(request.ParentId);

        var category = new Category
        {
            CategoryId = id,
            CategoryName = categoryName,
            ParentId = request.ParentId,
            SortOrder = request.SortOrder
        };

        var updated = await _categoryRepository.UpdateAsync(category);

        if (!updated)
        {
            throw new BusinessException("Category update failed.");
        }

        var updatedCategory = await _categoryRepository.GetByIdAsync(id);

        if (updatedCategory is null)
        {
            throw new BusinessException("Updated category could not be loaded.");
        }

        return ToDto(updatedCategory);
    }

    public async Task DeleteAsync(int id)
    {
        if (!await _categoryRepository.ExistsAsync(id))
        {
            throw new BusinessException("Category does not exist.");
        }

        if (await _categoryRepository.HasChildrenAsync(id))
        {
            throw new BusinessException("Category has child categories and cannot be deleted.");
        }

        if (await _categoryRepository.HasGoodsAsync(id))
        {
            throw new BusinessException("Category is referenced by goods and cannot be deleted.");
        }

        try
        {
            var deleted = await _categoryRepository.DeleteAsync(id);

            if (!deleted)
            {
                throw new BusinessException("Category delete failed.");
            }
        }
        catch (OracleException exception) when (exception.Number == ForeignKeyConstraintViolation)
        {
            throw new BusinessException("Category is referenced and cannot be deleted.");
        }
    }

    private async Task EnsureParentExistsAsync(int? parentId)
    {
        if (parentId.HasValue && !await _categoryRepository.ExistsAsync(parentId.Value))
        {
            throw new BusinessException("Parent category does not exist.");
        }
    }

    private static string NormalizeCategoryName(string? categoryName)
    {
        if (string.IsNullOrWhiteSpace(categoryName))
        {
            throw new BusinessException("Category name is required.");
        }

        return categoryName.Trim();
    }

    private static CategoryDto ToDto(Category category)
    {
        return new CategoryDto
        {
            CategoryId = category.CategoryId,
            CategoryName = category.CategoryName,
            ParentId = category.ParentId,
            SortOrder = category.SortOrder,
            CreatedAt = category.CreatedAt
        };
    }
}
