// 导入请求工具
import request from '@/utils/request'
// 导入分类的类型定义
import type { Category } from '@/types'

//开发时期调用会话数据的妙妙工具#4
import {
  mockCategories,
  getMockResponse
} from '@/utils/mock'

const USE_MOCK_GOODS =
  import.meta.env.DEV && import.meta.env.VITE_USE_MOCK_GOODS === 'true'

/**
 * 获取分类列表
 * @returns 分类列表
 */
export const getCategoryList = () => {

  //测试数据
  if (USE_MOCK_GOODS) {
    return Promise.resolve(
      getMockResponse<Category[]>(mockCategories as Category[])
    )
  }
  // 发送GET请求获取分类列表
  return request.get<Category[]>('/categories')
}

/**
 * 根据ID获取分类详情
 * @param categoryId 分类ID
 * @returns 分类详情
 */
export const getCategoryById = (categoryId: number) => {
  // 发送GET请求获取单个分类
  return request.get<Category>(`/categories/${categoryId}`)
}

/**
 * 创建分类
 * @param params 分类参数
 * @returns 创建的分类
 */
export const createCategory = (params: Partial<Category>) => {
  // 发送POST请求创建分类
  return request.post<Category>('/categories', params)
}

/**
 * 更新分类信息
 * @param categoryId 分类ID
 * @param params 更新参数
 * @returns 更新后的分类
 */
export const updateCategory = (categoryId: number, params: Partial<Category>) => {
  // 发送PUT请求更新分类
  return request.put<Category>(`/categories/${categoryId}`, params)
}

/**
 * 删除分类
 * @param categoryId 分类ID
 * @returns 删除结果
 */
export const deleteCategory = (categoryId: number) => {
  // 发送DELETE请求删除分类
  return request.delete(`/categories/${categoryId}`)
}