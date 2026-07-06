import request from '@/utils/request'
import type { Appointment } from '@/types'

export const getAppointmentByOrderId = (orderId: number) => {
  return request.get<Appointment>(`/appointments/order/${orderId}`)
}

export const createAppointment = (params: { orderId: number; meetTime: string; meetLocation: string }) => {
  return request.post<Appointment>('/appointments', params)
}

export const confirmAppointment = (appointmentId: number) => {
  return request.put(`/appointments/${appointmentId}/confirm`)
}

export const completeAppointment = (appointmentId: number) => {
  return request.put(`/appointments/${appointmentId}/complete`)
}

export const cancelAppointment = (appointmentId: number) => {
  return request.put(`/appointments/${appointmentId}/cancel`)
}

export const verifyConfirmCode = (params: { orderId: number; confirmCode: string }) => {
  return request.post('/appointments/verify', params)
}
