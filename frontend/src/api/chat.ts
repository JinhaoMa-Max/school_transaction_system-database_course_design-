import request from '@/utils/request'
import type { ChatSession, ChatMessage, PageResult } from '@/types'

export const getSessionList = () => {
  return request.get<ChatSession[]>('/chat/sessions')
}

export const getSessionById = (sessionId: number) => {
  return request.get<ChatSession>(`/chat/sessions/${sessionId}`)
}

export const createSession = (params: { goodsId: number; sellerId: number }) => {
  return request.post<ChatSession>('/chat/sessions', params)
}

export const getMessages = (sessionId: number, params?: { page?: number; size?: number }) => {
  return request.get<PageResult<ChatMessage>>(`/chat/sessions/${sessionId}/messages`, { params })
}

export const sendMessage = (params: { sessionId: number; content: string }) => {
  return request.post<ChatMessage>('/chat/messages', params)
}

export const markAsRead = (sessionId: number) => {
  return request.put(`/chat/sessions/${sessionId}/read`)
}

export const getUnreadCount = () => {
  return request.get('/chat/unread-count')
}
