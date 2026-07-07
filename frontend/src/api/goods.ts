// 导入封装好的请求工具函数
import request from '@/utils/request'
// 导入商品、商品图片、分页结果和商品查询的类型定义
import type { Goods, GoodsImage, PageResult, GoodsQuery } from '@/types'

/**
 * 获取商品列表（分页）
 * @param params 查询参数
 * @returns 商品分页结果
 */
export const getGoodsList = (params?: GoodsQuery) => {
  // 发起GET请求，请求商品列表接口，携带查询参数
  return request.get<PageResult<Goods>>('/goods', { params })
}

/**
 * 根据ID获取商品详情
 * @param goodsId 商品ID
 * @returns 商品详情
 */
export const getGoodsById = (goodsId: number) => {
  // 发起GET请求，请求指定ID的商品详情
  return request.get<Goods>(`/goods/${goodsId}`)
}

/**
 * 创建商品
 * @param params 商品参数（部分字段）
 * @returns 创建后的商品
 */
export const createGoods = (params: Partial<Goods>) => {
  // 发起POST请求，创建新的商品
  return request.post<Goods>('/goods', params)
}

/**
 * 更新商品信息
 * @param goodsId 商品ID
 * @param params 更新参数（部分字段）
 * @returns 更新后的商品
 */
export const updateGoods = (goodsId: number, params: Partial<Goods>) => {
  // 发起PUT请求，更新指定ID的商品信息
  return request.put<Goods>(`/goods/${goodsId}`, params)
}

/**
 * 删除商品
 * @param goodsId 商品ID
 * @returns 删除结果
 */
export const deleteGoods = (goodsId: number) => {
  // 发起DELETE请求，删除指定ID的商品
  return request.delete(`/goods/${goodsId}`)
}

/**
 * 获取商品图片列表
 * @param goodsId 商品ID
 * @returns 商品图片数组
 */
export const getGoodsImages = (goodsId: number) => {
  // 发起GET请求，获取指定商品的所有图片
  return request.get<GoodsImage[]>(`/goods/${goodsId}/images`)
}

/**
 * 上传商品图片
 * @param goodsId 商品ID
 * @param params 图片参数
 * @param params.imageUrl 图片URL
 * @param params.sortOrder 排序序号
 * @returns 上传后的图片信息
 */
export const uploadGoodsImage = (goodsId: number, params: { imageUrl: string; sortOrder: number }) => {
  // 发起POST请求，上传商品图片
  return request.post<GoodsImage>(`/goods/${goodsId}/images`, params)
}

/**
 * 删除商品图片
 * @param imageId 图片ID
 * @returns 删除结果
 */
export const deleteGoodsImage = (imageId: number) => {
  // 发起DELETE请求，删除指定ID的商品图片
  return request.delete(`/goods/images/${imageId}`)
}

/**
 * 审核商品
 * @param goodsId 商品ID
 * @param params 审核参数
 * @param params.status 审核状态：approved(通过)、rejected(拒绝)
 * @param params.remark 审核备注（可选）
 * @returns 审核结果
 */
export const auditGoods = (goodsId: number, params: { status: 'approved' | 'rejected'; remark?: string }) => {
  // 发起PUT请求，审核指定商品
  return request.put(`/goods/${goodsId}/audit`, params)
}

/**
 * 下架商品
 * @param goodsId 商品ID
 * @returns 下架结果
 */
export const offlineGoods = (goodsId: number) => {
  // 发起PUT请求，下架指定商品
  return request.put(`/goods/${goodsId}/offline`)
}

/**
 * 增加商品浏览次数
 * @param goodsId 商品ID
 * @returns 更新结果
 */
export const incrementViewCount = (goodsId: number) => {
  // 发起PUT请求，增加商品浏览量
  return request.put(`/goods/${goodsId}/view`)
}