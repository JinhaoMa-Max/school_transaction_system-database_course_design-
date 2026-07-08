// 导入请求工具
import request from '@/utils/request'
// 导入评价和分页结果的类型定义
import type { Review, PageResult } from '@/types'

/**
 * 获取评价列表
 * @param params 查询参数（可选）
 * @param params.orderId 订单ID（可选）
 * @param params.reviewerId 评价者ID（可选）
 * @param params.reviewedUserId 被评价者ID（可选）
 * @param params.page 页码（可选）
 * @param params.size 每页数量（可选）
 * @returns 评价分页结果
 */
export const getReviewList = (params?: { orderId?: number; reviewerId?: number; reviewedUserId?: number; page?: number; size?: number }) => {
  // 发送GET请求获取评价列表
  return request.get<PageResult<Review>>('/reviews', { params })
}

/**
 * 根据ID获取评价详情
 * @param reviewId 评价ID
 * @returns 评价详情
 */
export const getReviewById = (reviewId: number) => {
  // 发送GET请求获取单个评价
  return request.get<Review>(`/reviews/${reviewId}`)
}

/**
 * 创建评价
 * @param params 评价参数
 * @param params.orderId 订单ID
 * @param params.reviewedUserId 被评价者ID
 * @param params.rating 评分
 * @param params.content 评价内容（可选）
 * @returns 创建的评价
 */
export const createReview = (params: { orderId: number; reviewedUserId: number; rating: number; content?: string }) => {
  // 发送POST请求创建评价
  return request.post<Review>('/reviews', params)
}

/**
 * 更新评价
 * @param reviewId 评价ID
 * @param params 更新参数
 * @returns 更新后的评价
 */
export const updateReview = (reviewId: number, params: Partial<Review>) => {
  // 发送PUT请求更新评价
  return request.put<Review>(`/reviews/${reviewId}`, params)
}

/**
 * 删除评价
 * @param reviewId 评价ID
 * @returns 删除结果
 */
export const deleteReview = (reviewId: number) => {
  // 发送DELETE请求删除评价
  return request.delete(`/reviews/${reviewId}`)
}