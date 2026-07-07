// 导入封装好的请求工具函数
import request from '@/utils/request'
// 导入分类的类型定义
import type { Category } from '@/types'

/**
 * 获取分类列表
 * @returns 分类数组
 */
export const getCategoryList = () => {
  // 发起GET请求，获取所有分类列表
  return request.get<Category[]>('/categories')
}

/**
 * 根据ID获取分类详情
 * @param categoryId 分类ID
 * @returns 分类详情
 */
export const getCategoryById = (categoryId: number) => {
  // 发起GET请求，请求指定ID的分类详情
  return request.get<Category>(`/categories/${categoryId}`)
}

/**
 * 创建分类
 * @param params 分类参数（部分字段）
 * @returns 创建后的分类
 */
export const createCategory = (params: Partial<Category>) => {
  // 发起POST请求，创建新的分类
  return request.post<Category>('/categories', params)
}

/**
 * 更新分类信息
 * @param categoryId 分类ID
 * @param params 更新参数（部分字段）
 * @returns 更新后的分类
 */
export const updateCategory = (categoryId: number, params: Partial<Category>) => {
  // 发起PUT请求，更新指定ID的分类信息
  return request.put<Category>(`/categories/${categoryId}`, params)
}

/**
 * 删除分类
 * @param categoryId 分类ID
 * @returns 删除结果
 */
export const deleteCategory = (categoryId: number) => {
  // 发起DELETE请求，删除指定ID的分类
  return request.delete(`/categories/${categoryId}`)
}
