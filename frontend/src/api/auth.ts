import request from '@/utils/request'
import type { LoginParams,RegisterParams, LoginResult, User, StudentAuth } from '@/types'

//开发时期调用认证数据的妙妙工具#3
import { mockStudentAuths, getMockResponse } from '@/utils/mock'

const USE_MOCK_AUTH = import.meta.env.DEV && import.meta.env.VITE_USE_MOCK_AUTH ==='true'

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

  //开发绕过
  if (USE_MOCK_AUTH) {
    const auth: StudentAuth = {
      authId: Date.now(),
      userId: params.userId!,
      studentId: params.studentId || '',
      realName: params.realName || '',
      college: params.college || '',
      authStatus: 'pending',
      authTime: new Date().toLocaleString()
    }

    mockStudentAuths.push(auth)

    return Promise.resolve(
      getMockResponse<StudentAuth>(auth)
    )
  }

  return request.post<StudentAuth>('/auth/student-auth', params)
}

export const getStudentAuth = (userId: number) => {

  //开发绕过
  if (USE_MOCK_AUTH) {
    const auth = mockStudentAuths.find(item => item.userId === userId)

    return Promise.resolve(
      getMockResponse<StudentAuth | null>(auth ?? null)
    )
  }

  return request.get<StudentAuth>(`/auth/student-auth/${userId}`)
}

export const updateStudentAuth = (authId: number, params: Partial<StudentAuth>) => {

  //开发绕过
  if (USE_MOCK_AUTH) {
    const index = mockStudentAuths.findIndex(item => item.authId === authId)

    if (index === -1) {
      return Promise.resolve(
        getMockResponse<null>(null, 404, '认证信息不存在')
      )
    }

    mockStudentAuths[index] = {
      ...mockStudentAuths[index],
      ...params,
      authStatus: 'pending',
      authTime: new Date().toLocaleString()
    }

    return Promise.resolve(
      getMockResponse<StudentAuth>(mockStudentAuths[index])
    )
  }

  return request.put<StudentAuth>(`/auth/student-auth/${authId}`, params)
}
