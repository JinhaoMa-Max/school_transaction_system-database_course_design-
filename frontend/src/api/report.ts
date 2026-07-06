import request from '@/utils/request'
import type { Report, PageResult } from '@/types'

export const getReportList = (params?: { reportType?: string; status?: string; page?: number; size?: number }) => {
  return request.get<PageResult<Report>>('/reports', { params })
}

export const getReportById = (reportId: number) => {
  return request.get<Report>(`/reports/${reportId}`)
}

export const createReport = (params: { reportType: 'goods' | 'user' | 'order'; reportedGoodsId?: number; reportedUserId?: number; reportedOrderId?: number; reason: string }) => {
  return request.post<Report>('/reports', params)
}

export const handleReport = (reportId: number, params: { status: 'processing' | 'resolved' | 'rejected'; remark?: string }) => {
  return request.put<Report>(`/reports/${reportId}/handle`, params)
}
