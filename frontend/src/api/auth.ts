// 导入请求工具
import request from '@/utils/request'
// 导入登录参数、登录结果、用户和学生认证的类型定义
import type { LoginParams, LoginResult, User, StudentAuth } from '@/types'

/**
 * 用户登录
 * @param params 登录参数
 * @param params.username 用户名
 * @param params.password 密码
 * @returns 登录结果（包含token和用户信息）
 */
export const login = (params: LoginParams) => {
  // 发送POST请求进行登录
  return request.post<LoginResult>('/auth/login', params)
}

/**
 * 用户登出
 * @returns 登出结果
 */
export const logout = () => {
  // 发送POST请求进行登出
  return request.post('/auth/logout')
}

/**
 * 获取当前登录用户信息
 * @returns 当前用户信息
 */
export const getCurrentUser = () => {
  // 发送GET请求获取当前用户
  return request.get<User>('/auth/current')
}

/**
 * 用户注册
 * @param params 用户注册参数
 * @returns 注册后的用户信息
 */
export const register = (params: Partial<User>) => {
  // 发送POST请求进行注册
  return request.post<User>('/auth/register', params)
}

/**
 * 提交学生认证
 * @param params 学生认证参数
 * @returns 学生认证信息
 */
export const submitStudentAuth = (params: Partial<StudentAuth>) => {
  // 发送POST请求提交学生认证
  return request.post<StudentAuth>('/auth/student-auth', params)
}

/**
 * 获取学生认证信息
 * @param userId 用户ID
 * @returns 学生认证信息
 */
export const getStudentAuth = (userId: number) => {
  // 发送GET请求获取学生认证
  return request.get<StudentAuth>(`/auth/student-auth/${userId}`)
}

/**
 * 更新学生认证信息
 * @param authId 认证ID
 * @param params 更新参数
 * @returns 更新后的学生认证信息
 */
export const updateStudentAuth = (authId: number, params: Partial<StudentAuth>) => {
  // 发送PUT请求更新学生认证
  return request.put<StudentAuth>(`/auth/student-auth/${authId}`, params)
}