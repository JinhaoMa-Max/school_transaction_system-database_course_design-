// 导入封装好的请求工具函数
import request from '@/utils/request'
// 导入审计日志、通知和分页结果的类型定义
import type { AuditLog, Notice, PageResult } from '@/types'

/**
 * 获取审计日志列表（分页）
 * @param params 查询参数
 * @param params.adminId 管理员ID（可选）
 * @param params.auditType 审计类型（可选）
 * @param params.page 页码（可选，默认1）
 * @param params.size 每页数量（可选，默认10）
 * @returns 审计日志分页结果
 */
export const getAuditLogList = (params?: { adminId?: number; auditType?: string; page?: number; size?: number }) => {
  // 发起GET请求，请求审计日志列表接口，携带查询参数
  return request.get<PageResult<AuditLog>>('/admin/audit-logs', { params })
}

/**
 * 根据ID获取审计日志详情
 * @param logId 审计日志ID
 * @returns 审计日志详情
 */
export const getAuditLogById = (logId: number) => {
  // 发起GET请求，请求指定ID的审计日志详情
  return request.get<AuditLog>(`/admin/audit-logs/${logId}`)
}

/**
 * 创建审计日志
 * @param params 审计日志参数（部分字段）
 * @returns 创建后的审计日志
 */
export const createAuditLog = (params: Partial<AuditLog>) => {
  // 发起POST请求，创建新的审计日志
  return request.post<AuditLog>('/admin/audit-logs', params)
}

/**
 * 获取通知列表（分页）
 * @param params 查询参数
 * @param params.noticeType 通知类型（可选）
 * @param params.page 页码（可选，默认1）
 * @param params.size 每页数量（可选，默认10）
 * @returns 通知分页结果
 */
export const getNoticeList = (params?: { noticeType?: string; page?: number; size?: number }) => {
  // 发起GET请求，请求通知列表接口，携带查询参数
  return request.get<PageResult<Notice>>('/admin/notices', { params })
}

/**
 * 根据ID获取通知详情
 * @param noticeId 通知ID
 * @returns 通知详情
 */
export const getNoticeById = (noticeId: number) => {
  // 发起GET请求，请求指定ID的通知详情
  return request.get<Notice>(`/admin/notices/${noticeId}`)
}

/**
 * 创建通知
 * @param params 通知参数
 * @param params.title 通知标题
 * @param params.content 通知内容
 * @param params.noticeType 通知类型：system(系统)、transaction(交易)、violation(违规)
 * @returns 创建后的通知
 */
export const createNotice = (params: { title: string; content: string; noticeType: 'system' | 'transaction' | 'violation' }) => {
  // 发起POST请求，创建新的通知
  return request.post<Notice>('/admin/notices', params)
}

/**
 * 更新通知
 * @param noticeId 通知ID
 * @param params 更新参数（部分字段）
 * @returns 更新后的通知
 */
export const updateNotice = (noticeId: number, params: Partial<Notice>) => {
  // 发起PUT请求，更新指定ID的通知
  return request.put<Notice>(`/admin/notices/${noticeId}`, params)
}

/**
 * 删除通知
 * @param noticeId 通知ID
 * @returns 删除结果
 */
export const deleteNotice = (noticeId: number) => {
  // 发起DELETE请求，删除指定ID的通知
  return request.delete(`/admin/notices/${noticeId}`)
}
