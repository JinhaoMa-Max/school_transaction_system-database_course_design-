import type { ApiResponse } from './request'

export class ApiError extends Error {
  code: number
  data: unknown

  constructor(code: number, message: string, data?: unknown) {
    super(message)
    this.code = code
    this.data = data
    this.name = 'ApiError'
  }
}

export const handleApiError = (response: ApiResponse): never => {
  const errorMessages: Record<number, string> = {
    400: '请求参数错误',
    401: '未授权，请重新登录',
    403: '无权访问',
    404: '资源不存在',
    500: '服务器内部错误'
  }

  const message = errorMessages[response.code] || response.message || '请求失败'
  
  if (response.code === 401) {
    localStorage.removeItem('accessToken')
    window.location.href = '/login'
  }

  throw new ApiError(response.code, message, response.data)
}

export const handleNetworkError = (error: unknown): never => {
  if (error instanceof Error) {
    if (error.message.includes('Network Error')) {
      throw new ApiError(0, '网络连接失败，请检查网络')
    }
    if (error.message.includes('timeout')) {
      throw new ApiError(0, '请求超时，请重试')
    }
    throw new ApiError(0, error.message)
  }
  throw new ApiError(0, '未知错误')
}

export const getErrorMessage = (error: unknown): string => {
  if (error instanceof ApiError) {
    return error.message
  }
  if (error instanceof Error) {
    return error.message
  }
  return '未知错误'
}
