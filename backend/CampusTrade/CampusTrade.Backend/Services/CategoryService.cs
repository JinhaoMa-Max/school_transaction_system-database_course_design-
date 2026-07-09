using CampusTrade.Backend.Models.DTOs;
using CampusTrade.Backend.Repositories;

namespace CampusTrade.Backend.Services;

public class CategoryService : ICategoryService
{
    private readonly ICategoryRepository _categoryRepository;

    public CategoryService(ICategoryRepository categoryRepository)
    {
        _categoryRepository = categoryRepository;
    }

    public async Task<List<CategoryDto>> GetAllAsync()
    {
        var flatList = await _categoryRepository.GetAllAsync();
        return BuildTree(flatList);
    }

    public async Task<CategoryDto?> GetByIdAsync(int categoryId)
    {
        return await _categoryRepository.GetByIdAsync(categoryId);
    }

    public async Task<int> CreateAsync(CreateCategoryRequest request)
    {
        if (request.ParentId.HasValue)
        {
            var parent = await _categoryRepository.GetByIdAsync(request.ParentId.Value);
            if (parent == null)
                throw new ArgumentException("父分类不存在");
        }

        if (request.SortOrder == 0)
        {
            var max = await _categoryRepository.GetMaxSortOrderAsync(request.ParentId);
            request.SortOrder = max + 1;
        }

        return await _categoryRepository.CreateAsync(request);
    }

    public async Task<bool> UpdateAsync(int categoryId, UpdateCategoryRequest request)
    {
        var existing = await _categoryRepository.GetByIdAsync(categoryId);
        if (existing == null)
            throw new ArgumentException("分类不存在");

        if (request.ParentId.HasValue)
        {
            var newParentId = request.ParentId.Value;

            if (newParentId == categoryId)
                throw new ArgumentException("不能将自己设为父分类");

            // ★ 修复：检查新父分类是否在当前分类的子树中
            // 如果是，则把当前分类移到它的子分类下会造成循环引用
            if (await IsDescendantAsync(categoryId, newParentId))
                throw new ArgumentException("不能将父分类设为当前分类的子分类，这会形成循环引用");
        }

        return await _categoryRepository.UpdateAsync(categoryId, request);
    }

    public async Task<bool> DeleteAsync(int categoryId)
    {
        var existing = await _categoryRepository.GetByIdAsync(categoryId);
        if (existing == null)
            throw new ArgumentException("分类不存在");
 
        if (await _categoryRepository.HasChildrenAsync(categoryId))
            throw new InvalidOperationException("该分类存在子分类，无法删除");

        if (await _categoryRepository.HasGoodsAsync(categoryId))
            throw new InvalidOperationException("该分类下存在商品，无法删除");

        return await _categoryRepository.DeleteAsync(categoryId);
    }

    private List<CategoryDto> BuildTree(List<CategoryDto> flatList)
    {
        var lookup = flatList.ToDictionary(c => c.CategoryId);
        var roots = new List<CategoryDto>();

        foreach (var item in flatList)
        {
            if (item.ParentId.HasValue && lookup.TryGetValue(item.ParentId.Value, out var parent))
            {
                parent.Children.Add(item);
            }
            else
            {
                roots.Add(item);
            }
        }

        SortChildren(roots);
        return roots;
    }

    private void SortChildren(List<CategoryDto> nodes)
    {
        foreach (var node in nodes)
        {
            if (node.Children.Any())
            {
                node.Children = node.Children.OrderBy(c => c.SortOrder).ToList();
                SortChildren(node.Children);
            }
        }
    }

    /// <summary>
    /// 检查 ancestorId 是否是 descendantId 的祖先（即 descendantId 是否在 ancestorId 的子树中）
    /// </summary>
    private async Task<bool> IsDescendantAsync(int ancestorId, int descendantId)
    {
        int? currentId = descendantId;
        while (currentId.HasValue)
        {
            var parent = await _categoryRepository.GetByIdAsync(currentId.Value);
            if (parent == null)
                break;
            if (parent.ParentId == ancestorId)
                return true;
            currentId = parent.ParentId;
        }
        return false;
    }
}