import request from '@/utils/request'
import type { BargainOffer, PageResult } from '@/types'

export const getBargainList = (params?: { goodsId?: number; buyerId?: number; status?: string; page?: number; size?: number }) => {
  return request.get<PageResult<BargainOffer>>('/bargains', { params })
}

export const getBargainById = (bargainId: number) => {
  return request.get<BargainOffer>(`/bargains/${bargainId}`)
}

export const createBargain = (params: { goodsId: number; offerPrice: number }) => {
  return request.post<BargainOffer>('/bargains', params)
}

export const handleBargain = (bargainId: number, params: { sellerResult: 'accepted' | 'rejected' | 'countered'; counterPrice?: number }) => {
  return request.put<BargainOffer>(`/bargains/${bargainId}/handle`, params)
}

export const closeBargain = (bargainId: number) => {
  return request.put(`/bargains/${bargainId}/close`)
}
