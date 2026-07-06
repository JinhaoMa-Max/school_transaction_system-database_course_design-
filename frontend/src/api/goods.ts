import request from '@/utils/request'
import type { Goods, GoodsImage, PageResult, GoodsQuery } from '@/types'

export const getGoodsList = (params?: GoodsQuery) => {
  return request.get<PageResult<Goods>>('/goods', { params })
}

export const getGoodsById = (goodsId: number) => {
  return request.get<Goods>(`/goods/${goodsId}`)
}

export const createGoods = (params: Partial<Goods>) => {
  return request.post<Goods>('/goods', params)
}

export const updateGoods = (goodsId: number, params: Partial<Goods>) => {
  return request.put<Goods>(`/goods/${goodsId}`, params)
}

export const deleteGoods = (goodsId: number) => {
  return request.delete(`/goods/${goodsId}`)
}

export const getGoodsImages = (goodsId: number) => {
  return request.get<GoodsImage[]>(`/goods/${goodsId}/images`)
}

export const uploadGoodsImage = (goodsId: number, params: { imageUrl: string; sortOrder: number }) => {
  return request.post<GoodsImage>(`/goods/${goodsId}/images`, params)
}

export const deleteGoodsImage = (imageId: number) => {
  return request.delete(`/goods/images/${imageId}`)
}

export const auditGoods = (goodsId: number, params: { status: 'approved' | 'rejected'; remark?: string }) => {
  return request.put(`/goods/${goodsId}/audit`, params)
}

export const offlineGoods = (goodsId: number) => {
  return request.put(`/goods/${goodsId}/offline`)
}

export const incrementViewCount = (goodsId: number) => {
  return request.put(`/goods/${goodsId}/view`)
}
