// 导入封装好的请求工具函数
import request from '@/utils/request'
// 导入用户和分页结果的类型定义
import type { User, PageResult } from '@/types'

/**
 * 获取用户列表（分页）
 * @param params 查询参数
 * @param params.page 页码（可选，默认1）
 * @param params.size 每页数量（可选，默认10）
 * @param params.role 用户角色（可选）
 * @returns 用户分页结果
 */
export const getUserList = (params?: { page?: number; size?: number; role?: string }) => {
  // 发起GET请求，请求用户列表接口，携带查询参数
  return request.get<PageResult<User>>('/users', { params })
}

/**
 * 根据ID获取用户详情
 * @param userId 用户ID
 * @returns 用户详情
 */
export const getUserById = (userId: number) => {
  // 发起GET请求，请求指定ID的用户详情
  return request.get<User>(`/users/${userId}`)
}

/**
 * 更新用户信息
 * @param userId 用户ID
 * @param params 更新参数（部分字段）
 * @returns 更新后的用户信息
 */
export const updateUser = (userId: number, params: Partial<User>) => {
  // 发起PUT请求，更新指定ID的用户信息
  return request.put<User>(`/users/${userId}`, params)
}

/**
 * 删除用户
 * @param userId 用户ID
 * @returns 删除结果
 */
export const deleteUser = (userId: number) => {
  // 发起DELETE请求，删除指定ID的用户
  return request.delete(`/users/${userId}`)
}

/**
 * 封禁用户
 * @param userId 用户ID
 * @returns 封禁结果
 */
export const banUser = (userId: number) => {
  // 发起PUT请求，封禁指定用户
  return request.put(`/users/${userId}/ban`)
}

/**
 * 解封用户
 * @param userId 用户ID
 * @returns 解封结果
 */
export const unbanUser = (userId: number) => {
  // 发起PUT请求，解封指定用户
  return request.put(`/users/${userId}/unban`)
}

/**
 * 更新用户信用分
 * @param userId 用户ID
 * @param score 信用分数
 * @returns 更新结果
 */
export const updateCreditScore = (userId: number, score: number) => {
  // 发起PUT请求，更新指定用户的信用分数
  return request.put(`/users/${userId}/credit`, { score })
}