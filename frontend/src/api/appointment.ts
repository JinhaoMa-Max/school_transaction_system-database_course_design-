// 导入请求工具
import request from '@/utils/request'
// 导入预约的类型定义
import type { Appointment } from '@/types'

/**
 * 根据订单ID获取预约信息
 * @param orderId 订单ID
 * @returns 预约信息
 */
export const getAppointmentByOrderId = (orderId: number) => {
  // 发送GET请求获取预约信息
  return request.get<Appointment>(`/appointments/order/${orderId}`)
}

/**
 * 创建面交预约
 * @param params 预约参数
 * @param params.orderId 订单ID
 * @param params.meetTime 面交时间
 * @param params.meetLocation 面交地点
 * @returns 创建的预约信息
 */
export const createAppointment = (params: { orderId: number; meetTime: string; meetLocation: string }) => {
  // 发送POST请求创建预约
  return request.post<Appointment>('/appointments', params)
}

/**
 * 确认预约
 * @param appointmentId 预约ID
 * @returns 确认结果
 */
export const confirmAppointment = (appointmentId: number) => {
  // 发送PUT请求确认预约
  return request.put(`/appointments/${appointmentId}/confirm`)
}

/**
 * 完成预约
 * @param appointmentId 预约ID
 * @returns 完成结果
 */
export const completeAppointment = (appointmentId: number) => {
  // 发送PUT请求完成预约
  return request.put(`/appointments/${appointmentId}/complete`)
}

/**
 * 取消预约
 * @param appointmentId 预约ID
 * @returns 取消结果
 */
export const cancelAppointment = (appointmentId: number) => {
  // 发送PUT请求取消预约
  return request.put(`/appointments/${appointmentId}/cancel`)
}

/**
 * 验证确认码
 * @param params 验证参数
 * @param params.orderId 订单ID
 * @param params.confirmCode 确认码
 * @returns 验证结果
 */
export const verifyConfirmCode = (params: { orderId: number; confirmCode: string }) => {
  // 发送POST请求验证确认码
  return request.post('/appointments/verify', params)
}