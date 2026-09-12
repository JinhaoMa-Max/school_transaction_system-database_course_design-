import request from '@/utils/request'
import type { Notice, PageResult } from '@/types'

export const getPublicNoticeList = (params?: { page?: number; size?: number; noticeType?: string }) =>
  request.get<PageResult<Notice>>('/notices', { params })

export const getPublicNoticeById = (noticeId: number) =>
  request.get<Notice>(`/notices/${noticeId}`)
