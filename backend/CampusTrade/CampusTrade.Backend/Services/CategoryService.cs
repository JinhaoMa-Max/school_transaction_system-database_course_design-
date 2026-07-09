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
        // 校验父分类是否存在（如果传入 ParentId）
        if (request.ParentId.HasValue)
        {
            var parent = await _categoryRepository.GetByIdAsync(request.ParentId.Value);
            if (parent == null)
                throw new ArgumentException("父分类不存在");
        }

        // 检查同父下是否有同名分类（可选）
        // 自动分配 sort_order（如果未指定，取最大值+1）
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

        // 如果修改父分类，需要检查是否会造成循环引用（不能将自己或子分类设为父分类）
        if (request.ParentId.HasValue)
        {
            if (request.ParentId.Value == categoryId)
                throw new ArgumentException("不能将自己设为父分类");

            // 简单检查：是否有循环（如果 parentId 的祖先包含自己，则禁止）
            // 这里可以通过递归检查，但为了简化，只禁止直接冲突。
            // 更好的做法：检查新父分类是否是当前分类的子分类，但需要遍历树。
            // 我们可以在业务层做简单校验，或者依赖数据库触发器/应用层。
            // 暂时只检查是否为自己。
        }

        return await _categoryRepository.UpdateAsync(categoryId, request);
    }

    public async Task<bool> DeleteAsync(int categoryId)
    {
        var existing = await _categoryRepository.GetByIdAsync(categoryId);
        if (existing == null)
            throw new ArgumentException("分类不存在");

        // 检查是否有子分类
        if (await _categoryRepository.HasChildrenAsync(categoryId))
            throw new InvalidOperationException("该分类存在子分类，无法删除");

        // 检查是否有商品引用（可选）
        // 这里可以调用商品仓储检查，暂不实现

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

        // 对每个节点的 Children 按 SortOrder 排序
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
}