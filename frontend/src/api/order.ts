// 导入封装好的请求工具函数
import request from '@/utils/request'
// 导入交易订单、分页结果和订单查询的类型定义
import type { TradeOrder, PageResult, OrderQuery } from '@/types'

/**
 * 获取订单列表（分页）
 * @param params 查询参数
 * @returns 订单分页结果
 */
export const getOrderList = (params?: OrderQuery) => {
  // 发起GET请求，请求订单列表接口，携带查询参数
  return request.get<PageResult<TradeOrder>>('/orders', { params })
}

/**
 * 根据ID获取订单详情
 * @param orderId 订单ID
 * @returns 订单详情
 */
export const getOrderById = (orderId: number) => {
  // 发起GET请求，请求指定ID的订单详情
  return request.get<TradeOrder>(`/orders/${orderId}`)
}

/**
 * 创建订单
 * @param params 订单参数
 * @param params.goodsId 商品ID
 * @param params.dealPrice 成交价格
 * @returns 创建后的订单
 */
export const createOrder = (params: { goodsId: number; dealPrice: number }) => {
  // 发起POST请求，创建新的订单
  return request.post<TradeOrder>('/orders', params)
}

/**
 * 更新订单信息
 * @param orderId 订单ID
 * @param params 更新参数（部分字段）
 * @returns 更新后的订单
 */
export const updateOrder = (orderId: number, params: Partial<TradeOrder>) => {
  // 发起PUT请求，更新指定ID的订单信息
  return request.put<TradeOrder>(`/orders/${orderId}`, params)
}

/**
 * 取消订单
 * @param orderId 订单ID
 * @returns 取消结果
 */
export const cancelOrder = (orderId: number) => {
  // 发起PUT请求，取消指定订单
  return request.put(`/orders/${orderId}/cancel`)
}

/**
 * 完成订单
 * @param orderId 订单ID
 * @returns 完成结果
 */
export const completeOrder = (orderId: number) => {
  // 发起PUT请求，完成指定订单
  return request.put(`/orders/${orderId}/complete`)
}

/**
 * 开始面交
 * @param orderId 订单ID
 * @returns 操作结果
 */
export const startMeet = (orderId: number) => {
  // 发起PUT请求，开始指定订单的面交流程
  return request.put(`/orders/${orderId}/start-meet`)
}
