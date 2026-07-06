import request from '@/utils/request'
import type { Category } from '@/types'

export const getCategoryList = () => {
  return request.get<Category[]>('/categories')
}

export const getCategoryById = (categoryId: number) => {
  return request.get<Category>(`/categories/${categoryId}`)
}

export const createCategory = (params: Partial<Category>) => {
  return request.post<Category>('/categories', params)
}

export const updateCategory = (categoryId: number, params: Partial<Category>) => {
  return request.put<Category>(`/categories/${categoryId}`, params)
}

export const deleteCategory = (categoryId: number) => {
  return request.delete(`/categories/${categoryId}`)
}
