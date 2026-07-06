import request from '@/utils/request'
import type { AuditLog, Notice, PageResult } from '@/types'

export const getAuditLogList = (params?: { adminId?: number; auditType?: string; page?: number; size?: number }) => {
  return request.get<PageResult<AuditLog>>('/admin/audit-logs', { params })
}

export const getAuditLogById = (logId: number) => {
  return request.get<AuditLog>(`/admin/audit-logs/${logId}`)
}

export const createAuditLog = (params: Partial<AuditLog>) => {
  return request.post<AuditLog>('/admin/audit-logs', params)
}

export const getNoticeList = (params?: { noticeType?: string; page?: number; size?: number }) => {
  return request.get<PageResult<Notice>>('/admin/notices', { params })
}

export const getNoticeById = (noticeId: number) => {
  return request.get<Notice>(`/admin/notices/${noticeId}`)
}

export const createNotice = (params: { title: string; content: string; noticeType: 'system' | 'transaction' | 'violation' }) => {
  return request.post<Notice>('/admin/notices', params)
}

export const updateNotice = (noticeId: number, params: Partial<Notice>) => {
  return request.put<Notice>(`/admin/notices/${noticeId}`, params)
}

export const deleteNotice = (noticeId: number) => {
  return request.delete(`/admin/notices/${noticeId}`)
}
