import request from '@/utils/request'
import type { User, PageResult } from '@/types'

export const getUserList = (params?: { page?: number; size?: number; role?: string }) => {
  return request.get<PageResult<User>>('/users', { params })
}

export const getUserById = (userId: number) => {
  return request.get<User>(`/users/${userId}`)
}

export const updateUser = (userId: number, params: Partial<User>) => {
  return request.put<User>(`/users/${userId}`, params)
}

export const deleteUser = (userId: number) => {
  return request.delete(`/users/${userId}`)
}

export const banUser = (userId: number) => {
  return request.put(`/users/${userId}/ban`)
}

export const unbanUser = (userId: number) => {
  return request.put(`/users/${userId}/unban`)
}

export const updateCreditScore = (userId: number, score: number) => {
  return request.put(`/users/${userId}/credit`, { score })
}
