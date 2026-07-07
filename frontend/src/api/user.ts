// 导入请求工具
import request from '@/utils/request'
// 导入用户和分页结果的类型定义
import type { User, PageResult } from '@/types'

/**
 * 获取用户列表
 * @param params 查询参数（可选）
 * @param params.page 页码（可选）
 * @param params.size 每页数量（可选）
 * @param params.role 用户角色（可选）
 * @returns 用户分页结果
 */
export const getUserList = (params?: { page?: number; size?: number; role?: string }) => {
  // 发送GET请求获取用户列表
  return request.get<PageResult<User>>('/users', { params })
}

/**
 * 根据ID获取用户详情
 * @param userId 用户ID
 * @returns 用户详情
 */
export const getUserById = (userId: number) => {
  // 发送GET请求获取单个用户
  return request.get<User>(`/users/${userId}`)
}

/**
 * 更新用户信息
 * @param userId 用户ID
 * @param params 更新参数
 * @returns 更新后的用户信息
 */
export const updateUser = (userId: number, params: Partial<User>) => {
  // 发送PUT请求更新用户
  return request.put<User>(`/users/${userId}`, params)
}

/**
 * 删除用户
 * @param userId 用户ID
 * @returns 删除结果
 */
export const deleteUser = (userId: number) => {
  // 发送DELETE请求删除用户
  return request.delete(`/users/${userId}`)
}

/**
 * 封禁用户
 * @param userId 用户ID
 * @returns 封禁结果
 */
export const banUser = (userId: number) => {
  // 发送PUT请求封禁用户
  return request.put(`/users/${userId}/ban`)
}

/**
 * 解封用户
 * @param userId 用户ID
 * @returns 解封结果
 */
export const unbanUser = (userId: number) => {
  // 发送PUT请求解封用户
  return request.put(`/users/${userId}/unban`)
}

/**
 * 更新用户信用分
 * @param userId 用户ID
 * @param score 信用分数值
 * @returns 更新结果
 */
export const updateCreditScore = (userId: number, score: number) => {
  // 发送PUT请求更新信用分
  return request.put(`/users/${userId}/credit`, { score })
}