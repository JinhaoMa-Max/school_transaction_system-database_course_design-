// 导入封装好的请求工具函数
import request from '@/utils/request'
// 导入收藏和分页结果的类型定义
import type { Favorite, PageResult } from '@/types'

/**
 * 获取收藏列表（分页）
 * @param params 查询参数
 * @param params.page 页码（可选，默认1）
 * @param params.size 每页数量（可选，默认10）
 * @returns 收藏分页结果
 */
export const getFavoriteList = (params?: { page?: number; size?: number }) => {
  // 发起GET请求，请求收藏列表接口，携带查询参数
  return request.get<PageResult<Favorite>>('/favorites', { params })
}

/**
 * 根据ID获取收藏详情
 * @param favoriteId 收藏记录ID
 * @returns 收藏详情
 */
export const getFavoriteById = (favoriteId: number) => {
  // 发起GET请求，请求指定ID的收藏详情
  return request.get<Favorite>(`/favorites/${favoriteId}`)
}

/**
 * 添加收藏
 * @param params 收藏参数
 * @param params.goodsId 商品ID
 * @returns 创建后的收藏记录
 */
export const createFavorite = (params: { goodsId: number }) => {
  // 发起POST请求，创建新的收藏记录
  return request.post<Favorite>('/favorites', params)
}

/**
 * 删除收藏
 * @param favoriteId 收藏记录ID
 * @returns 删除结果
 */
export const deleteFavorite = (favoriteId: number) => {
  // 发起DELETE请求，删除指定ID的收藏记录
  return request.delete(`/favorites/${favoriteId}`)
}

/**
 * 检查商品是否已收藏
 * @param goodsId 商品ID
 * @returns 是否已收藏的结果
 */
export const checkFavorite = (goodsId: number) => {
  // 发起GET请求，检查指定商品是否已被当前用户收藏
  return request.get(`/favorites/check?goodsId=${goodsId}`)
}