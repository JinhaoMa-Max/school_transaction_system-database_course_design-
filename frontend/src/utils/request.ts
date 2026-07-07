// 导入axios及其类型定义
import axios, { type AxiosInstance, type InternalAxiosRequestConfig, type AxiosResponse } from 'axios'
// 导入Arco Design的Message组件，用于显示错误提示
import { Message } from '@arco-design/web-vue'

/**
 * API响应类型定义
 * @template T 响应数据的类型
 */
export interface ApiResponse<T = unknown> {
  // 响应码，200表示成功
  code: number
  // 响应消息
  message: string
  // 响应数据
  data: T
}

// 创建axios实例，配置基础请求参数
const service: AxiosInstance = axios.create({
  // 基础URL，所有请求都会在此基础上拼接路径
  baseURL: '/api',
  // 请求超时时间，15秒
  timeout: 15000,
  // 默认请求头，设置为JSON格式
  headers: {
    'Content-Type': 'application/json'
  }
})

// 请求拦截器：在发送请求前做一些处理
service.interceptors.request.use(
  (config: InternalAxiosRequestConfig) => {
    // 从localStorage中获取用户token
    const token = localStorage.getItem('accessToken')
    // 如果token存在，将其添加到请求头的Authorization字段中
    if (token) {
      config.headers = config.headers || {}
      config.headers.Authorization = `Bearer ${token}`
    }
    // 返回修改后的配置
    return config
  },
  // 请求错误处理
  (error) => {
    return Promise.reject(error)
  }
)

// 响应拦截器：在接收到响应后做一些处理
service.interceptors.response.use(
  (response: AxiosResponse<ApiResponse>) => {
    // 获取响应体数据
    const res = response.data
    // 如果响应码不是200，表示请求失败
    if (res.code !== 200) {
      // 显示错误提示
      Message.error(res.message || '请求失败')
      // 如果是401未授权错误，清除token并跳转到登录页
      if (res.code === 401) {
        localStorage.removeItem('accessToken')
        window.location.href = '/login'
      }
      // 抛出错误
      return Promise.reject(new Error(res.message || '请求失败'))
    }
    // 请求成功，返回响应对象，但将response.data替换为业务数据res.data
    return {
      ...response,
      data: res.data
    }
  },
  // 响应错误处理（网络错误等）
  (error) => {
    // 显示错误提示
    Message.error(error.message || '网络错误')
    return Promise.reject(error)
  }
)

// 导出axios实例
export default service