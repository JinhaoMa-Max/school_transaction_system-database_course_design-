import request from '@/utils/request'
import type { Review, PageResult } from '@/types'

export const getReviewList = (params?: { orderId?: number; reviewerId?: number; reviewedUserId?: number; page?: number; size?: number }) => {
  return request.get<PageResult<Review>>('/reviews', { params })
}

export const getReviewById = (reviewId: number) => {
  return request.get<Review>(`/reviews/${reviewId}`)
}

export const createReview = (params: { orderId: number; reviewedUserId: number; rating: number; content?: string }) => {
  return request.post<Review>('/reviews', params)
}

export const updateReview = (reviewId: number, params: Partial<Review>) => {
  return request.put<Review>(`/reviews/${reviewId}`, params)
}

export const deleteReview = (reviewId: number) => {
  return request.delete(`/reviews/${reviewId}`)
}
