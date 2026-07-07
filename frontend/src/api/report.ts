// 导入请求工具
import request from '@/utils/request'
// 导入举报和分页结果的类型定义
import type { Report, PageResult } from '@/types'

/**
 * 获取举报列表
 * @param params 查询参数（可选）
 * @param params.reportType 举报类型（可选）
 * @param params.status 举报状态（可选）
 * @param params.page 页码（可选）
 * @param params.size 每页数量（可选）
 * @returns 举报分页结果
 */
export const getReportList = (params?: { reportType?: string; status?: string; page?: number; size?: number }) => {
  // 发送GET请求获取举报列表
  return request.get<PageResult<Report>>('/reports', { params })
}

/**
 * 根据ID获取举报详情
 * @param reportId 举报ID
 * @returns 举报详情
 */
export const getReportById = (reportId: number) => {
  // 发送GET请求获取单个举报
  return request.get<Report>(`/reports/${reportId}`)
}

/**
 * 创建举报
 * @param params 举报参数
 * @param params.reportType 举报类型（goods/user/order）
 * @param params.reportedGoodsId 被举报商品ID（可选）
 * @param params.reportedUserId 被举报用户ID（可选）
 * @param params.reportedOrderId 被举报订单ID（可选）
 * @param params.reason 举报原因
 * @returns 创建的举报
 */
export const createReport = (params: { reportType: 'goods' | 'user' | 'order'; reportedGoodsId?: number; reportedUserId?: number; reportedOrderId?: number; reason: string }) => {
  // 发送POST请求创建举报
  return request.post<Report>('/reports', params)
}

/**
 * 处理举报
 * @param reportId 举报ID
 * @param params 处理参数
 * @param params.status 处理状态（processing/resolved/rejected）
 * @param params.remark 处理备注（可选）
 * @returns 处理后的举报信息
 */
export const handleReport = (reportId: number, params: { status: 'processing' | 'resolved' | 'rejected'; remark?: string }) => {
  // 发送PUT请求处理举报
  return request.put<Report>(`/reports/${reportId}/handle`, params)
}