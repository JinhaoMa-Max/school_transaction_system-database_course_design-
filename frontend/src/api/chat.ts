// 导入请求工具
import request, {type ApiResponse} from '@/utils/request'
// 导入聊天会话、聊天消息和分页结果的类型定义
import type { ChatSession, ChatMessage, PageResult } from '@/types'

//开发时期调用会话数据的妙妙工具#2
import{ mockChatSessions,mockChatMessages,getMockResponse}from '@/utils/mock'

const USE_MOCK_CHAT = import.meta.env.DEV && import.meta.env.VITE_USE_MOCK_CHAT ==='true'

/**
 * 获取聊天会话列表
 * @returns 聊天会话列表
 */
export const getSessionList = () => {

  //开发数据
  if(USE_MOCK_CHAT){
    return Promise.resolve(getMockResponse<ChatSession[]>(mockChatSessions))
  }

  // 发送GET请求获取聊天会话列表
 return request.get<ApiResponse<ChatSession[]>, ApiResponse<ChatSession[]>>('/chat/sessions')
}

/**
 * 根据ID获取聊天会话详情
 * @param sessionId 会话ID
 * @returns 聊天会话详情
 */
export const getSessionById = (sessionId: number) => {
  // 发送GET请求获取单个会话
  return request.get<ApiResponse<ChatSession[]>, ApiResponse<ChatSession[]>>(`/chat/sessions/${sessionId}`)
}

/**
 * 创建聊天会话
 * @param params 会话参数
 * @param params.goodsId 商品ID
 * @param params.sellerId 卖家ID
 * @returns 创建的会话信息
 */
export const createSession = (params: { goodsId: number; sellerId: number }) => {
  // 发送POST请求创建会话
  return request.post<ApiResponse<ChatSession[]>, ApiResponse<ChatSession[]>>('/chat/sessions', params)
}

/**
 * 获取会话消息列表
 * @param sessionId 会话ID
 * @param params 查询参数（可选）
 * @param params.page 页码（可选）
 * @param params.size 每页数量（可选）
 * @returns 消息分页结果
 */
export const getMessages = (sessionId: number, params?: { page?: number; size?: number }): Promise<ApiResponse<PageResult<ChatMessage>>> => {


  //测试数据
  if(USE_MOCK_CHAT){
    
    const page = params?.page ?? 1
    const size = params?.size ?? 20

    const list = mockChatMessages
    .filter(
      (message)=>message.sessionId == sessionId
    )
    .sort(
      (a, b) => {return new Date(a.sendTime).getTime() - new Date(b.sendTime).getTime()}
    )

    const start = (page-1)*size
    const pageList = list.slice(start,start+size)

    return Promise.resolve(getMockResponse<PageResult<ChatMessage>>({
      list: pageList,
      total: list.length,
      page,
      size
  }))
  }
  // 发送GET请求获取会话消息
  return request.get<ApiResponse<PageResult<ChatMessage>>, ApiResponse<PageResult<ChatMessage>>>(`/chat/sessions/${sessionId}/messages`, { params })
}

/**
 * 发送消息
 * @param params 消息参数
 * @param params.sessionId 会话ID
 * @param params.content 消息内容
 * @returns 发送的消息信息
 */
export const sendMessage = (params: { sessionId: number; content: string;senderId?: number }) :Promise<ApiResponse<ChatMessage>>=> {
 
  //测试数据
  if (USE_MOCK_CHAT) {
    const message: ChatMessage = {
      messageId: Date.now(),
      sessionId: params.sessionId,
      senderId: params.senderId ?? 0,
      content: params.content,
      readStatus: 0,
      sendTime: new Date().toLocaleString()
    }
    mockChatMessages.push(message)
    return Promise.resolve(getMockResponse<ChatMessage>(message))
  }

 // 发送POST请求发送消息
  return request.post<ApiResponse<ChatMessage>, ApiResponse<ChatMessage>>(
    '/chat/messages', {
      sessionId: params.sessionId,
      content: params.content
    }
  )
}

/**
 * 标记会话为已读
 * @param sessionId 会话ID
 * @returns 更新结果
 */
export const markAsRead = (sessionId: number) => {

  //测试数据
  if(USE_MOCK_CHAT){
    mockChatMessages.forEach((message)=>{
      if(message.sessionId === sessionId){
        message.readStatus = 1
      }
    })
    return Promise.resolve(getMockResponse<boolean>(true))
  }
  // 发送PUT请求标记已读
   return request.put<ApiResponse<boolean>, ApiResponse<boolean>>(
    `/chat/sessions/${sessionId}/read`
  )
}

/**
 * 获取未读消息数量
 * @returns 未读消息数量
 */
export const getUnreadCount = () => {

  //测试数据
  if(USE_MOCK_CHAT){
    const count = mockChatMessages.filter((message)=>message.readStatus === 0
    ).length

   return Promise.resolve(getMockResponse<number>(count))
  }
  // 发送GET请求获取未读消息数
  return request.get<ApiResponse<number>, ApiResponse<number>>(
    '/chat/unread-count'
  )
}