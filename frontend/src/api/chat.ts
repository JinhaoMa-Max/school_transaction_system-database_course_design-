// 导入封装好的请求工具函数
import request from '@/utils/request'
// 导入聊天会话、聊天消息和分页结果的类型定义
import type { ChatSession, ChatMessage, PageResult } from '@/types'

/**
 * 获取聊天会话列表
 * @returns 聊天会话数组
 */
export const getSessionList = () => {
  // 发起GET请求，获取当前用户的所有聊天会话
  return request.get<ChatSession[]>('/chat/sessions')
}

/**
 * 根据ID获取聊天会话详情
 * @param sessionId 会话ID
 * @returns 聊天会话详情
 */
export const getSessionById = (sessionId: number) => {
  // 发起GET请求，获取指定ID的聊天会话详情
  return request.get<ChatSession>(`/chat/sessions/${sessionId}`)
}

/**
 * 创建聊天会话
 * @param params 会话参数
 * @param params.goodsId 商品ID
 * @param params.sellerId 卖家ID
 * @returns 创建后的聊天会话
 */
export const createSession = (params: { goodsId: number; sellerId: number }) => {
  // 发起POST请求，创建新的聊天会话
  return request.post<ChatSession>('/chat/sessions', params)
}

/**
 * 获取会话消息列表（分页）
 * @param sessionId 会话ID
 * @param params 查询参数
 * @param params.page 页码（可选，默认1）
 * @param params.size 每页数量（可选，默认10）
 * @returns 消息分页结果
 */
export const getMessages = (sessionId: number, params?: { page?: number; size?: number }) => {
  // 发起GET请求，获取指定会话的消息列表
  return request.get<PageResult<ChatMessage>>(`/chat/sessions/${sessionId}/messages`, { params })
}

/**
 * 发送消息
 * @param params 消息参数
 * @param params.sessionId 会话ID
 * @param params.content 消息内容
 * @returns 发送后的消息
 */
export const sendMessage = (params: { sessionId: number; content: string }) => {
  // 发起POST请求，发送新消息
  return request.post<ChatMessage>('/chat/messages', params)
}

/**
 * 标记会话为已读
 * @param sessionId 会话ID
 * @returns 操作结果
 */
export const markAsRead = (sessionId: number) => {
  // 发起PUT请求，标记指定会话的消息为已读
  return request.put(`/chat/sessions/${sessionId}/read`)
}

/**
 * 获取未读消息总数
 * @returns 未读消息数量
 */
export const getUnreadCount = () => {
  // 发起GET请求，获取当前用户的未读消息总数
  return request.get('/chat/unread-count')
}