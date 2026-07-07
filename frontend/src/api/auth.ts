// 导入封装好的请求工具函数
import request from '@/utils/request'
// 导入登录参数、登录结果、用户和学生认证的类型定义
import type { LoginParams, LoginResult, User, StudentAuth } from '@/types'

/**
 * 用户登录
 * @param params 登录参数
 * @returns 登录结果（包含token和用户信息）
 */
export const login = (params: LoginParams) => {
  // 发起POST请求，提交登录表单数据
  return request.post<LoginResult>('/auth/login', params)
}

/**
 * 用户登出
 * @returns 登出结果
 */
export const logout = () => {
  // 发起POST请求，执行登出操作
  return request.post('/auth/logout')
}

/**
 * 获取当前登录用户信息
 * @returns 当前用户信息
 */
export const getCurrentUser = () => {
  // 发起GET请求，获取当前登录用户的详细信息
  return request.get<User>('/auth/current')
}

/**
 * 用户注册
 * @param params 用户注册参数（部分字段）
 * @returns 注册后的用户信息
 */
export const register = (params: Partial<User>) => {
  // 发起POST请求，提交用户注册数据
  return request.post<User>('/auth/register', params)
}

/**
 * 提交学生认证申请
 * @param params 学生认证参数（部分字段）
 * @returns 学生认证信息
 */
export const submitStudentAuth = (params: Partial<StudentAuth>) => {
  // 发起POST请求，提交学生认证数据
  return request.post<StudentAuth>('/auth/student-auth', params)
}

/**
 * 获取学生认证信息
 * @param userId 用户ID
 * @returns 学生认证信息
 */
export const getStudentAuth = (userId: number) => {
  // 发起GET请求，获取指定用户的学生认证信息
  return request.get<StudentAuth>(`/auth/student-auth/${userId}`)
}

/**
 * 更新学生认证信息
 * @param authId 认证记录ID
 * @param params 更新参数（部分字段）
 * @returns 更新后的学生认证信息
 */
export const updateStudentAuth = (authId: number, params: Partial<StudentAuth>) => {
  // 发起PUT请求，更新指定认证记录
  return request.put<StudentAuth>(`/auth/student-auth/${authId}`, params)
}