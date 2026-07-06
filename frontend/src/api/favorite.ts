import request from '@/utils/request'
import type { Favorite, PageResult } from '@/types'

export const getFavoriteList = (params?: { page?: number; size?: number }) => {
  return request.get<PageResult<Favorite>>('/favorites', { params })
}

export const getFavoriteById = (favoriteId: number) => {
  return request.get<Favorite>(`/favorites/${favoriteId}`)
}

export const createFavorite = (params: { goodsId: number }) => {
  return request.post<Favorite>('/favorites', params)
}

export const deleteFavorite = (favoriteId: number) => {
  return request.delete(`/favorites/${favoriteId}`)
}

export const checkFavorite = (goodsId: number) => {
  return request.get(`/favorites/check?goodsId=${goodsId}`)
}
