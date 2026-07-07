// 导入封装好的请求工具函数
import request from '@/utils/request'
// 导入评价和分页结果的类型定义
import type { Review, PageResult } from '@/types'

/**
 * 获取评价列表（分页）
 * @param params 查询参数
 * @param params.orderId 订单ID（可选）
 * @param params.reviewerId 评价人ID（可选）
 * @param params.reviewedUserId 被评价人ID（可选）
 * @param params.page 页码（可选，默认1）
 * @param params.size 每页数量（可选，默认10）
 * @returns 评价分页结果
 */
export const getReviewList = (params?: { orderId?: number; reviewerId?: number; reviewedUserId?: number; page?: number; size?: number }) => {
  // 发起GET请求，请求评价列表接口，携带查询参数
  return request.get<PageResult<Review>>('/reviews', { params })
}

/**
 * 根据ID获取评价详情
 * @param reviewId 评价ID
 * @returns 评价详情
 */
export const getReviewById = (reviewId: number) => {
  // 发起GET请求，请求指定ID的评价详情
  return request.get<Review>(`/reviews/${reviewId}`)
}

/**
 * 创建评价
 * @param params 评价参数
 * @param params.orderId 订单ID
 * @param params.reviewedUserId 被评价人ID
 * @param params.rating 评分（1-5）
 * @param params.content 评价内容（可选）
 * @returns 创建后的评价
 */
export const createReview = (params: { orderId: number; reviewedUserId: number; rating: number; content?: string }) => {
  // 发起POST请求，创建新的评价
  return request.post<Review>('/reviews', params)
}

/**
 * 更新评价
 * @param reviewId 评价ID
 * @param params 更新参数（部分字段）
 * @returns 更新后的评价
 */
export const updateReview = (reviewId: number, params: Partial<Review>) => {
  // 发起PUT请求，更新指定ID的评价
  return request.put<Review>(`/reviews/${reviewId}`, params)
}

/**
 * 删除评价
 * @param reviewId 评价ID
 * @returns 删除结果
 */
export const deleteReview = (reviewId: number) => {
  // 发起DELETE请求，删除指定ID的评价
  return request.delete(`/reviews/${reviewId}`)
}