// 导入请求工具
import request from '@/utils/request'
// 导入收藏和分页结果的类型定义
import type { Favorite, PageResult } from '@/types'

/**
 * 获取收藏列表
 * @param params 查询参数（可选）
 * @param params.page 页码（可选）
 * @param params.size 每页数量（可选）
 * @returns 收藏分页结果
 */
export const getFavoriteList = (params?: { page?: number; size?: number }) => {
  // 发送GET请求获取收藏列表
  return request.get<PageResult<Favorite>>('/favorites', { params })
}

/**
 * 根据ID获取收藏详情
 * @param favoriteId 收藏ID
 * @returns 收藏详情
 */
export const getFavoriteById = (favoriteId: number) => {
  // 发送GET请求获取单个收藏
  return request.get<Favorite>(`/favorites/${favoriteId}`)
}

/**
 * 添加收藏
 * @param params 收藏参数
 * @param params.goodsId 商品ID
 * @returns 添加的收藏信息
 */
export const createFavorite = (params: { goodsId: number }) => {
  // 发送POST请求添加收藏
  return request.post<Favorite>('/favorites', params)
}

/**
 * 删除收藏
 * @param favoriteId 收藏ID
 * @returns 删除结果
 */
export const deleteFavorite = (favoriteId: number) => {
  // 发送DELETE请求删除收藏
  return request.delete(`/favorites/${favoriteId}`)
}

/**
 * 检查商品是否已收藏
 * @param goodsId 商品ID
 * @returns 收藏状态
 */
export const checkFavorite = (goodsId: number) => {
  // 发送GET请求检查收藏状态
  return request.get(`/favorites/check?goodsId=${goodsId}`)
}