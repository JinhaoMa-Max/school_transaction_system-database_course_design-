// 导入请求工具
import request from '@/utils/request'
// 导入商品、商品图片、分页结果和商品查询的类型定义
import type { Goods, GoodsImage, PageResult, GoodsQuery } from '@/types'

/**
 * 上传图片到服务器
 * @param file 图片文件
 * @returns 上传结果，包含 fileName 和 url
 */
export const uploadImage = (file: File) => {
  const formData = new FormData()
  formData.append('file', file)
  return request.post<{ fileName: string; url: string }>('/upload/image', formData, {
    headers: { 'Content-Type': 'multipart/form-data' }
  })
}

/**
 * 获取商品列表
 * @param params 查询参数（可选）
 * @returns 商品分页结果
 */
export const getGoodsList = (params?: GoodsQuery) => {
  // 发送GET请求获取商品列表
  return request.get<PageResult<Goods>>('/goods', { params })
}

/**
 * 根据ID获取商品详情
 * @param goodsId 商品ID
 * @returns 商品详情
 */
export const getGoodsById = (goodsId: number) => {
  // 发送GET请求获取单个商品
  return request.get<Goods>(`/goods/${goodsId}`)
}

/**
 * 创建商品
 * @param params 商品参数
 * @returns 创建的商品
 */
export const createGoods = (params: Partial<Goods>) => {
  // 发送POST请求创建商品
  return request.post<Goods>('/goods', params)
}

/**
 * 更新商品信息
 * @param goodsId 商品ID
 * @param params 更新参数
 * @returns 更新后的商品
 */
export const updateGoods = (goodsId: number, params: Partial<Goods>) => {
  // 发送PUT请求更新商品
  return request.put<Goods>(`/goods/${goodsId}`, params)
}

/**
 * 删除商品
 * @param goodsId 商品ID
 * @returns 删除结果
 */
export const deleteGoods = (goodsId: number) => {
  // 发送DELETE请求删除商品
  return request.delete(`/goods/${goodsId}`)
}

/**
 * 获取商品图片列表
 * @param goodsId 商品ID
 * @returns 商品图片列表
 */
export const getGoodsImages = (goodsId: number) => {
  // 发送GET请求获取商品图片
  return request.get<GoodsImage[]>(`/goods/${goodsId}/images`)
}

/**
 * 上传商品图片
 * @param goodsId 商品ID
 * @param params 图片参数
 * @param params.imageUrl 图片URL
 * @param params.sortOrder 排序序号
 * @returns 上传的图片信息
 */
export const uploadGoodsImage = (goodsId: number, params: { imageUrl: string; sortOrder: number }) => {
  // 发送POST请求上传商品图片
  return request.post<GoodsImage>(`/goods/${goodsId}/images`, params)
}

/**
 * 删除商品图片
 * @param imageId 图片ID
 * @returns 删除结果
 */
export const deleteGoodsImage = (imageId: number) => {
  // 发送DELETE请求删除商品图片
  return request.delete(`/goods/images/${imageId}`)
}

/**
 * 审核商品
 * @param goodsId 商品ID
 * @param params 审核参数
 * @param params.status 审核状态（approved/rejected）
 * @param params.remark 审核备注（可选）
 * @returns 审核结果
 */
export const auditGoods = (goodsId: number, params: { status: 'approved' | 'rejected'; remark?: string }) => {
  // 发送PUT请求审核商品
  return request.put(`/goods/${goodsId}/audit`, params)
}

/**
 * 下架商品
 * @param goodsId 商品ID
 * @returns 下架结果
 */
export const offlineGoods = (goodsId: number) => {
  // 发送PUT请求下架商品
  return request.put(`/goods/${goodsId}/offline`)
}

/**
 * 增加商品浏览量
 * @param goodsId 商品ID
 * @returns 更新结果
 */
export const incrementViewCount = (goodsId: number) => {
  // 发送PUT请求增加浏览量
  return request.put(`/goods/${goodsId}/view`)
}