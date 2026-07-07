// 导入封装好的请求工具函数
import request from '@/utils/request'
// 导入预约的类型定义
import type { Appointment } from '@/types'

/**
 * 根据订单ID获取预约信息
 * @param orderId 订单ID
 * @returns 预约信息
 */
export const getAppointmentByOrderId = (orderId: number) => {
  // 发起GET请求，获取指定订单的预约信息
  return request.get<Appointment>(`/appointments/order/${orderId}`)
}

/**
 * 创建预约
 * @param params 预约参数
 * @param params.orderId 订单ID
 * @param params.meetTime 见面时间
 * @param params.meetLocation 见面地点
 * @returns 创建后的预约
 */
export const createAppointment = (params: { orderId: number; meetTime: string; meetLocation: string }) => {
  // 发起POST请求，创建新的预约
  return request.post<Appointment>('/appointments', params)
}

/**
 * 确认预约
 * @param appointmentId 预约ID
 * @returns 确认结果
 */
export const confirmAppointment = (appointmentId: number) => {
  // 发起PUT请求，确认指定的预约
  return request.put(`/appointments/${appointmentId}/confirm`)
}

/**
 * 完成预约
 * @param appointmentId 预约ID
 * @returns 完成结果
 */
export const completeAppointment = (appointmentId: number) => {
  // 发起PUT请求，完成指定的预约
  return request.put(`/appointments/${appointmentId}/complete`)
}

/**
 * 取消预约
 * @param appointmentId 预约ID
 * @returns 取消结果
 */
export const cancelAppointment = (appointmentId: number) => {
  // 发起PUT请求，取消指定的预约
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
  // 发起POST请求，验证面交确认码
  return request.post('/appointments/verify', params)
}