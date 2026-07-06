import request from '@/utils/request'
import type { TradeOrder, PageResult, OrderQuery } from '@/types'

export const getOrderList = (params?: OrderQuery) => {
  return request.get<PageResult<TradeOrder>>('/orders', { params })
}

export const getOrderById = (orderId: number) => {
  return request.get<TradeOrder>(`/orders/${orderId}`)
}

export const createOrder = (params: { goodsId: number; dealPrice: number }) => {
  return request.post<TradeOrder>('/orders', params)
}

export const updateOrder = (orderId: number, params: Partial<TradeOrder>) => {
  return request.put<TradeOrder>(`/orders/${orderId}`, params)
}

export const cancelOrder = (orderId: number) => {
  return request.put(`/orders/${orderId}/cancel`)
}

export const completeOrder = (orderId: number) => {
  return request.put(`/orders/${orderId}/complete`)
}

export const startMeet = (orderId: number) => {
  return request.put(`/orders/${orderId}/start-meet`)
}
