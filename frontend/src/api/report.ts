// 导入封装好的请求工具函数
import request from '@/utils/request'
// 导入举报和分页结果的类型定义
import type { Report, PageResult } from '@/types'

/**
 * 获取举报列表（分页）
 * @param params 查询参数
 * @param params.reportType 举报类型（可选）
 * @param params.status 处理状态（可选）
 * @param params.page 页码（可选，默认1）
 * @param params.size 每页数量（可选，默认10）
 * @returns 举报分页结果
 */
export const getReportList = (params?: { reportType?: string; status?: string; page?: number; size?: number }) => {
  // 发起GET请求，请求举报列表接口，携带查询参数
  return request.get<PageResult<Report>>('/reports', { params })
}

/**
 * 根据ID获取举报详情
 * @param reportId 举报ID
 * @returns 举报详情
 */
export const getReportById = (reportId: number) => {
  // 发起GET请求，请求指定ID的举报详情
  return request.get<Report>(`/reports/${reportId}`)
}

/**
 * 创建举报
 * @param params 举报参数
 * @param params.reportType 举报类型：goods(商品)、user(用户)、order(订单)
 * @param params.reportedGoodsId 被举报商品ID（reportType为goods时必填）
 * @param params.reportedUserId 被举报用户ID（reportType为user时必填）
 * @param params.reportedOrderId 被举报订单ID（reportType为order时必填）
 * @param params.reason 举报原因
 * @returns 创建后的举报
 */
export const createReport = (params: { reportType: 'goods' | 'user' | 'order'; reportedGoodsId?: number; reportedUserId?: number; reportedOrderId?: number; reason: string }) => {
  // 发起POST请求，创建新的举报
  return request.post<Report>('/reports', params)
}

/**
 * 处理举报
 * @param reportId 举报ID
 * @param params 处理参数
 * @param params.status 处理状态：processing(处理中)、resolved(已解决)、rejected(已驳回)
 * @param params.remark 处理备注（可选）
 * @returns 处理后的举报
 */
export const handleReport = (reportId: number, params: { status: 'processing' | 'resolved' | 'rejected'; remark?: string }) => {
  // 发起PUT请求，处理指定的举报
  return request.put<Report>(`/reports/${reportId}/handle`, params)
}