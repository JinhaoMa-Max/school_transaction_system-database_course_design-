import request from '@/utils/request'
import type { LoginParams,RegisterParams, LoginResult, User, StudentAuth } from '@/types'

export const login = (params: LoginParams) => {
  return request.post<LoginResult>('/auth/login', params)
}

export const uploadAvatar = (file: File) => {
  const formData = new FormData()
  formData.append('file', file)
  return request.post<User>('/auth/avatar', formData, {
    headers: { 'Content-Type': 'multipart/form-data' }
  })
}

export const logout = () => {
  return request.post('/auth/logout')
}

export const getCurrentUser = () => {
  return request.get<User>('/auth/current')
}

export const register = (params: RegisterParams) => {
  return request.post<User>('/auth/register', params)
}

export const submitStudentAuth = (params: Partial<StudentAuth>) => {
  return request.post<StudentAuth>('/auth/student-auth', params)
}

export const getStudentAuth = (userId: number) => {
  return request.get<StudentAuth>(`/auth/student-auth/${userId}`)
}

export const updateStudentAuth = (authId: number, params: Partial<StudentAuth>) => {
  return request.put<StudentAuth>(`/auth/student-auth/${authId}`, params)
}
