// 导入请求工具
import request from '@/utils/request'
// 导入议价和分页结果的类型定义
import type { BargainOffer, PageResult } from '@/types'

/**
 * 获取议价列表
 * @param params 查询参数（可选）
 * @param params.goodsId 商品ID（可选）
 * @param params.buyerId 买家ID（可选）
 * @param params.sellerId 卖家ID（可选）
 * @param params.status 议价状态（可选）
 * @param params.page 页码（可选）
 * @param params.size 每页数量（可选）
 * @returns 议价分页结果
 */
export const getBargainList = (params?: { goodsId?: number; buyerId?: number; sellerId?: number; status?: string; page?: number; size?: number }) => {
  // 发送GET请求获取议价列表
  return request.get<PageResult<BargainOffer>>('/bargains', { params })
}

/**
 * 根据ID获取议价详情
 * @param bargainId 议价ID
 * @returns 议价详情
 */
export const getBargainById = (bargainId: number) => {
  // 发送GET请求获取单个议价
  return request.get<BargainOffer>(`/bargains/${bargainId}`)
}

/**
 * 创建议价申请
 * @param params 议价参数
 * @param params.goodsId 商品ID
 * @param params.offerPrice 议价价格
 * @returns 创建的议价申请
 */
export const createBargain = (params: { goodsId: number; offerPrice: number }) => {
  // 发送POST请求创建议价
  return request.post<BargainOffer>('/bargains', params)
}

/**
 * 处理议价申请
 * @param bargainId 议价ID
 * @param params 处理参数
 * @param params.sellerResult 卖家处理结果（accepted/rejected/countered）
 * @param params.counterPrice 还价价格（可选，当sellerResult为countered时必填）
 * @returns 处理后的议价信息
 */
export const handleBargain = (bargainId: number, params: { sellerResult: 'accepted' | 'rejected' | 'countered'; counterPrice?: number }) => {
  // 发送PUT请求处理议价
  return request.put<BargainOffer>(`/bargains/${bargainId}/handle`, params)
}

/**
 * 买家处理议价（买家对卖家还价做出回应）
 * @param bargainId 议价ID
 * @param params 处理参数
 * @param params.buyerResult 买家处理结果（accepted/rejected/countered）— 中间传递值，对称于 sellerResult，不存入数据库
 * @param params.offerPrice 买家新出价（可选，当 buyerResult 为 countered 时必填）
 * @returns 处理后的议价信息
 */
export const handleBargainByBuyer = (bargainId: number, params: { buyerResult: 'accepted' | 'rejected' | 'countered'; offerPrice?: number }) => {
  // 发送PUT请求，买家处理议价
  return request.put<BargainOffer>(`/bargains/${bargainId}/buyer-handle`, params)
}

/**
 * 关闭议价
 * @param bargainId 议价ID
 * @returns 关闭结果
 */
export const closeBargain = (bargainId: number) => {
  // 发送PUT请求关闭议价
  return request.put(`/bargains/${bargainId}/close`)
}