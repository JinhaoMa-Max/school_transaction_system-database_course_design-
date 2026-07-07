// 导入封装好的请求工具函数
import request from '@/utils/request'
// 导入议价记录和分页结果的类型定义
import type { BargainOffer, PageResult } from '@/types'

/**
 * 获取议价列表（分页）
 * @param params 查询参数
 * @param params.goodsId 商品ID（可选）
 * @param params.buyerId 买家ID（可选）
 * @param params.status 议价状态（可选）
 * @param params.page 页码（可选，默认1）
 * @param params.size 每页数量（可选，默认10）
 * @returns 议价记录分页结果
 */
export const getBargainList = (params?: { goodsId?: number; buyerId?: number; status?: string; page?: number; size?: number }) => {
  // 发起GET请求，请求议价列表接口，携带查询参数
  return request.get<PageResult<BargainOffer>>('/bargains', { params })
}

/**
 * 根据ID获取议价详情
 * @param bargainId 议价记录ID
 * @returns 议价详情
 */
export const getBargainById = (bargainId: number) => {
  // 发起GET请求，请求指定ID的议价详情
  return request.get<BargainOffer>(`/bargains/${bargainId}`)
}

/**
 * 创建议价申请
 * @param params 议价参数
 * @param params.goodsId 商品ID
 * @param params.offerPrice 议价价格
 * @returns 创建后的议价记录
 */
export const createBargain = (params: { goodsId: number; offerPrice: number }) => {
  // 发起POST请求，创建新的议价申请
  return request.post<BargainOffer>('/bargains', params)
}

/**
 * 处理议价申请
 * @param bargainId 议价记录ID
 * @param params 处理参数
 * @param params.sellerResult 卖家处理结果：accepted(接受)、rejected(拒绝)、countered(还价)
 * @param params.counterPrice 还价价格（当sellerResult为countered时必填）
 * @returns 处理后的议价记录
 */
export const handleBargain = (bargainId: number, params: { sellerResult: 'accepted' | 'rejected' | 'countered'; counterPrice?: number }) => {
  // 发起PUT请求，处理指定的议价申请
  return request.put<BargainOffer>(`/bargains/${bargainId}/handle`, params)
}

/**
 * 关闭议价
 * @param bargainId 议价记录ID
 * @returns 关闭结果
 */
export const closeBargain = (bargainId: number) => {
  // 发起PUT请求，关闭指定的议价记录
  return request.put(`/bargains/${bargainId}/close`)
}