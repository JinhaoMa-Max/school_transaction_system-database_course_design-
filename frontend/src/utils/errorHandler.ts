// 导入API响应类型定义
import type { ApiResponse } from './request'

/**
 * API错误类
 * 继承自JavaScript的Error类，添加了错误码和错误数据属性
 */
export class ApiError extends Error {
  // 错误码
  code: number
  // 错误数据（可选）
  data: unknown

  /**
   * 构造函数
   * @param code 错误码
   * @param message 错误消息
   * @param data 错误数据（可选）
   */
  constructor(code: number, message: string, data?: unknown) {
    super(message)
    this.code = code
    this.data = data
    this.name = 'ApiError'
  }
}

/**
 * 处理API响应错误
 * 根据响应的code码返回对应的错误消息，并抛出ApiError异常
 * @param response API响应对象
 * @throws ApiError 抛出API错误异常
 */
export const handleApiError = (response: ApiResponse): never => {
  // 错误码与错误消息的映射表
  const errorMessages: Record<number, string> = {
    400: '请求参数错误',
    401: '未授权，请重新登录',
    403: '无权访问',
    404: '资源不存在',
    500: '服务器内部错误'
  }

  // 获取错误消息，优先使用映射表中的消息，其次使用响应中的消息，最后使用默认消息
  const message = errorMessages[response.code] || response.message || '请求失败'
  
  // 如果是401未授权错误，清除token并跳转到登录页
  if (response.code === 401) {
    localStorage.removeItem('accessToken')
    window.location.href = '/login'
  }

  // 抛出ApiError异常
  throw new ApiError(response.code, message, response.data)
}

/**
 * 处理网络错误
 * 区分网络连接错误和超时错误，返回对应的错误消息，并抛出ApiError异常
 * @param error 错误对象
 * @throws ApiError 抛出网络错误异常
 */
export const handleNetworkError = (error: unknown): never => {
  if (error instanceof Error) {
    // 判断是否为网络连接错误
    if (error.message.includes('Network Error')) {
      throw new ApiError(0, '网络连接失败，请检查网络')
    }
    // 判断是否为请求超时错误
    if (error.message.includes('timeout')) {
      throw new ApiError(0, '请求超时，请重试')
    }
    // 其他错误类型，直接抛出原错误消息
    throw new ApiError(0, error.message)
  }
  // 未知错误类型
  throw new ApiError(0, '未知错误')
}

/**
 * 获取错误消息
 * 从错误对象中提取错误消息，支持ApiError、Error和其他类型的错误
 * @param error 错误对象
 * @returns 错误消息字符串
 */
export const getErrorMessage = (error: unknown): string => {
  // 如果是ApiError类型，返回其message属性
  if (error instanceof ApiError) {
    return error.message
  }
  // 如果是Error类型，返回其message属性
  if (error instanceof Error) {
    return error.message
  }
  // 其他类型错误，返回默认消息
  return '未知错误'
}