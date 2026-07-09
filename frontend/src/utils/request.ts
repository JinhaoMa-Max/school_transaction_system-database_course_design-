import axios, { type AxiosInstance, type InternalAxiosRequestConfig, type AxiosResponse } from 'axios'
import { Message } from '@arco-design/web-vue'
import {
  mockUsers,
  mockCategories,
  mockGoods,
  mockGoodsImages,
  mockFavorites,
  mockBargains,
  mockOrders,
  mockAppointments,
  mockReviews,
  mockReports,
  mockStudentAuths,
  getMockResponse
} from './mock'
import type {
  User,
  Goods,
  Category,
  GoodsImage,
  Favorite,
  BargainOffer,
  TradeOrder,
  Appointment,
  Review,
  Report,
  StudentAuth,
  PageResult
} from '@/types'

export interface ApiResponse<T = unknown> {
  code: number
  message: string
  data: T
}

const USE_MOCK_FALLBACK = import.meta.env.DEV && (import.meta.env.VITE_USE_MOCK_FALLBACK === 'true' || import.meta.env.VITE_USE_MOCK_FALLBACK === undefined)

const service: AxiosInstance = axios.create({
  baseURL: '/api',
  timeout: 15000,
  headers: {
    'Content-Type': 'application/json'
  }
})

const getMockData = (url: string, method: string, params: any, data: any): any => {
  const path = url.replace('/api', '')

  if (path === '/categories' && method === 'get') {
    return getMockResponse<Category[]>(mockCategories)
  }

  if (path.match(/^\/goods\/\d+\/images$/) && method === 'get') {
    const goodsId = Number(path.split('/')[2])
    const images = mockGoodsImages.filter(img => img.goodsId === goodsId)
    return getMockResponse<GoodsImage[]>(images)
  }

  if (path.match(/^\/goods\/\d+$/) && method === 'get') {
    const id = Number(path.split('/')[2])
    const goods = mockGoods.find(g => g.goodsId === id)
    return getMockResponse<Goods>(goods || mockGoods[0])
  }

  if (path === '/goods' && method === 'get') {
    let list = [...mockGoods]
    if (params?.status) {
      list = list.filter(g => g.status === params.status)
    }
    if (params?.categoryId) {
      list = list.filter(g => g.categoryId === params.categoryId)
    }
    if (params?.keyword) {
      const kw = params.keyword.toLowerCase()
      list = list.filter(g => g.title.toLowerCase().includes(kw))
    }
    if (params?.sellerId) {
      list = list.filter(g => g.sellerId === params.sellerId)
    }
    if (params?.minPrice) {
      list = list.filter(g => g.price >= params.minPrice)
    }
    if (params?.maxPrice) {
      list = list.filter(g => g.price <= params.maxPrice)
    }
    const page = params?.page || 1
    const size = params?.size || 12
    const start = (page - 1) * size
    const end = start + size
    const pageList = list.slice(start, end)
    return getMockResponse<PageResult<Goods>>({
      list: pageList,
      total: list.length,
      page,
      size
    })
  }

  if (path === '/goods' && method === 'post') {
    const newGoods: Goods = {
      goodsId: mockGoods.length + 1,
      sellerId: 2,
      categoryId: data?.categoryId || 1,
      title: data?.title || '新商品',
      description: data?.description || '',
      price: data?.price || 0,
      condition: data?.condition || 'like_new',
      status: 'pending',
      viewCount: 0,
      publishTime: new Date().toISOString().replace('T', ' ').substring(0, 19)
    }
    mockGoods.push(newGoods)
    return getMockResponse<Goods>(newGoods)
  }

  if (path.match(/^\/goods\/\d+$/) && method === 'put') {
    const id = Number(path.split('/')[2])
    const idx = mockGoods.findIndex(g => g.goodsId === id)
    if (idx >= 0) {
      mockGoods[idx] = { ...mockGoods[idx], ...data }
      return getMockResponse<Goods>(mockGoods[idx])
    }
    return getMockResponse<Goods>(mockGoods[0])
  }

  if (path.match(/^\/goods\/\d+\/offline$/) && method === 'put') {
    const id = Number(path.split('/')[2])
    const idx = mockGoods.findIndex(g => g.goodsId === id)
    if (idx >= 0) {
      mockGoods[idx].status = 'offline'
      return getMockResponse({ success: true })
    }
    return getMockResponse({ success: true })
  }

  if (path.match(/^\/goods\/\d+\/view$/) && method === 'put') {
    const id = Number(path.split('/')[2])
    const goods = mockGoods.find(g => g.goodsId === id)
    if (goods) {
      goods.viewCount += 1
    }
    return getMockResponse({ success: true })
  }

  if (path.match(/^\/goods\/\d+\/images$/) && method === 'post') {
    const goodsId = Number(path.split('/')[2])
    const newImage: GoodsImage = {
      imageId: mockGoodsImages.length + 1,
      goodsId,
      imageUrl: data?.imageUrl || '',
      sortOrder: data?.sortOrder || 1
    }
    mockGoodsImages.push(newImage)
    return getMockResponse<GoodsImage>(newImage)
  }

  if (path.match(/^\/goods\/images\/\d+$/) && method === 'delete') {
    const imageId = Number(path.split('/')[3])
    const idx = mockGoodsImages.findIndex(img => img.imageId === imageId)
    if (idx >= 0) {
      mockGoodsImages.splice(idx, 1)
    }
    return getMockResponse({ success: true })
  }

  if (path === '/favorites' && method === 'get') {
    const page = params?.page || 1
    const size = params?.size || 10
    return getMockResponse<PageResult<Favorite>>({
      list: mockFavorites,
      total: mockFavorites.length,
      page,
      size
    })
  }

  if (path === '/favorites' && method === 'post') {
    const newFav: Favorite = {
      favoriteId: mockFavorites.length + 1,
      userId: 3,
      goodsId: data?.goodsId || 1,
      favoriteTime: new Date().toISOString().replace('T', ' ').substring(0, 19)
    }
    mockFavorites.push(newFav)
    return getMockResponse<Favorite>(newFav)
  }

  if (path.match(/^\/favorites\/\d+$/) && method === 'delete') {
    const id = Number(path.split('/')[2])
    const idx = mockFavorites.findIndex(f => f.favoriteId === id)
    if (idx >= 0) {
      mockFavorites.splice(idx, 1)
    }
    return getMockResponse({ success: true })
  }

  if (path.match(/^\/favorites\/check/) && method === 'get') {
    const goodsId = params?.goodsId
    const fav = mockFavorites.find(f => f.goodsId === goodsId)
    return getMockResponse({
      isFavorite: !!fav,
      favoriteId: fav?.favoriteId || null
    })
  }

  if (path === '/bargains' && method === 'get') {
    let list = [...mockBargains]
    if (params?.buyerId) {
      list = list.filter(b => b.buyerId === params.buyerId)
    }
    if (params?.sellerId) {
      list = list.filter(b => {
        const goods = mockGoods.find(g => g.goodsId === b.goodsId)
        return goods?.sellerId === params.sellerId
      })
    }
    if (params?.status) {
      list = list.filter(b => b.status === params.status)
    }
    const page = params?.page || 1
    const size = params?.size || 10
    const start = (page - 1) * size
    const end = start + size
    const pageList = list.slice(start, end)
    return getMockResponse<PageResult<BargainOffer>>({
      list: pageList,
      total: list.length,
      page,
      size
    })
  }

  if (path === '/bargains' && method === 'post') {
    const newBargain: BargainOffer = {
      bargainId: mockBargains.length + 1,
      goodsId: data?.goodsId || 1,
      buyerId: 3,
      offerPrice: data?.offerPrice || 0,
      sellerResult: 'pending',
      counterPrice: null,
      status: 'active',
      createTime: new Date().toISOString().replace('T', ' ').substring(0, 19)
    }
    mockBargains.push(newBargain)
    return getMockResponse<BargainOffer>(newBargain)
  }

  if (path.match(/^\/bargains\/\d+\/handle$/) && method === 'put') {
    const id = Number(path.split('/')[2])
    const bargain = mockBargains.find(b => b.bargainId === id)
    if (bargain) {
      bargain.sellerResult = data?.sellerResult || 'pending'
      bargain.counterPrice = data?.counterPrice || null
      if (data?.sellerResult === 'accepted') {
        bargain.status = 'accepted'
      } else if (data?.sellerResult === 'rejected') {
        bargain.status = 'rejected'
      }
    }
    return getMockResponse<BargainOffer>(bargain || mockBargains[0])
  }

  if (path.match(/^\/bargains\/\d+\/close$/) && method === 'put') {
    const id = Number(path.split('/')[2])
    const bargain = mockBargains.find(b => b.bargainId === id)
    if (bargain) {
      bargain.status = 'closed'
    }
    return getMockResponse({ success: true })
  }

  if (path === '/orders' && method === 'get') {
    let list = [...mockOrders]
    if (params?.status) {
      list = list.filter(o => o.status === params.status)
    }
    const page = params?.page || 1
    const size = params?.size || 10
    const start = (page - 1) * size
    const end = start + size
    const pageList = list.slice(start, end)
    return getMockResponse<PageResult<TradeOrder>>({
      list: pageList,
      total: list.length,
      page,
      size
    })
  }

  if (path.match(/^\/orders\/\d+$/) && method === 'get') {
    const id = Number(path.split('/')[2])
    const order = mockOrders.find(o => o.orderId === id)
    return getMockResponse<TradeOrder>(order || mockOrders[0])
  }

  if (path === '/orders' && method === 'post') {
    const newOrder: TradeOrder = {
      orderId: mockOrders.length + 1,
      goodsId: data?.goodsId || 1,
      buyerId: 3,
      sellerId: 2,
      dealPrice: data?.dealPrice || 0,
      status: 'pending_meet',
      createTime: new Date().toISOString().replace('T', ' ').substring(0, 19)
    }
    mockOrders.push(newOrder)
    return getMockResponse<TradeOrder>(newOrder)
  }

  if (path.match(/^\/orders\/\d+\/cancel$/) && method === 'put') {
    const id = Number(path.split('/')[2])
    const order = mockOrders.find(o => o.orderId === id)
    if (order) {
      order.status = 'cancelled'
    }
    return getMockResponse({ success: true })
  }

  if (path.match(/^\/orders\/\d+\/complete$/) && method === 'put') {
    const id = Number(path.split('/')[2])
    const order = mockOrders.find(o => o.orderId === id)
    if (order) {
      order.status = 'completed'
    }
    return getMockResponse({ success: true })
  }

  if (path.match(/^\/orders\/\d+\/start-meet$/) && method === 'put') {
    const id = Number(path.split('/')[2])
    const order = mockOrders.find(o => o.orderId === id)
    if (order) {
      order.status = 'in_meet'
    }
    return getMockResponse({ success: true })
  }

  if (path.match(/^\/appointments\/order\/\d+$/) && method === 'get') {
    const orderId = Number(path.split('/')[3])
    const appt = mockAppointments.find(a => a.orderId === orderId)
    return getMockResponse<Appointment>(appt || mockAppointments[0])
  }

  if (path === '/appointments' && method === 'post') {
    const newAppt: Appointment = {
      appointmentId: mockAppointments.length + 1,
      orderId: data?.orderId || 1,
      meetTime: data?.meetTime || '',
      meetLocation: data?.meetLocation || '',
      confirmCode: Math.random().toString(36).substring(2, 8).toUpperCase(),
      status: 'pending',
      createTime: new Date().toISOString().replace('T', ' ').substring(0, 19)
    }
    mockAppointments.push(newAppt)
    const order = mockOrders.find(o => o.orderId === data?.orderId)
    if (order) {
      order.status = 'in_meet'
    }
    return getMockResponse<Appointment>(newAppt)
  }

  if (path === '/appointments/verify' && method === 'post') {
    const appt = mockAppointments.find(a => a.confirmCode === data?.confirmCode)
    if (appt) {
      appt.status = 'confirmed'
      return getMockResponse({ success: true })
    }
    return getMockResponse({ success: false, message: '确认码错误' }, 400, '确认码错误')
  }

  if (path.match(/^\/appointments\/\d+\/confirm$/) && method === 'put') {
    const id = Number(path.split('/')[2])
    const appt = mockAppointments.find(a => a.appointmentId === id)
    if (appt) {
      appt.status = 'confirmed'
    }
    return getMockResponse({ success: true })
  }

  if (path.match(/^\/appointments\/\d+\/complete$/) && method === 'put') {
    const id = Number(path.split('/')[2])
    const appt = mockAppointments.find(a => a.appointmentId === id)
    if (appt) {
      appt.status = 'completed'
    }
    return getMockResponse({ success: true })
  }

  if (path === '/reviews' && method === 'post') {
    const newReview: Review = {
      reviewId: mockReviews.length + 1,
      orderId: data?.orderId || 1,
      reviewerId: 3,
      reviewedUserId: data?.reviewedUserId || 2,
      rating: data?.rating || 5,
      content: data?.content || null,
      reviewTime: new Date().toISOString().replace('T', ' ').substring(0, 19)
    }
    mockReviews.push(newReview)
    return getMockResponse<Review>(newReview)
  }

  if (path === '/reports' && method === 'post') {
    const newReport: Report = {
      reportId: mockReports.length + 1,
      reporterId: 3,
      reportType: data?.reportType || 'goods',
      reportedGoodsId: data?.reportedGoodsId || null,
      reportedUserId: data?.reportedUserId || null,
      reportedOrderId: data?.reportedOrderId || null,
      reason: data?.reason || '',
      status: 'pending',
      reportTime: new Date().toISOString().replace('T', ' ').substring(0, 19)
    }
    mockReports.push(newReport)
    return getMockResponse<Report>(newReport)
  }

  if (path.match(/^\/users\/\d+$/) && method === 'get') {
    const id = Number(path.split('/')[2])
    const user = mockUsers.find(u => u.userId === id)
    return getMockResponse<User>(user || mockUsers[0])
  }

  if (path === '/auth/current' && method === 'get') {
    return getMockResponse<User>(mockUsers[1])
  }

  if (path === '/auth/login' && method === 'post') {
    return getMockResponse({
      token: 'mock-token',
      user: mockUsers[1]
    })
  }

  if (path === '/auth/logout' && method === 'post') {
    return getMockResponse({ success: true })
  }

  if (path.match(/^\/student-auth\/current/) && method === 'get') {
    return getMockResponse<StudentAuth>(mockStudentAuths[0])
  }

  return getMockResponse(null, 404, '接口未找到')
}

service.interceptors.request.use(
  (config: InternalAxiosRequestConfig) => {
    const token = localStorage.getItem('accessToken')
    if (token) {
      config.headers = config.headers || {}
      config.headers.Authorization = `Bearer ${token}`
    }
    return config
  },
  (error) => {
    return Promise.reject(error)
  }
)

service.interceptors.response.use(
  (response: AxiosResponse<ApiResponse>) => {
    const res = response.data
    
    if (typeof res === 'string') {
      if (USE_MOCK_FALLBACK) {
        const config = response.config
        const url = config.url || ''
        const method = (config.method || 'get').toLowerCase()
        const params = config.params
        const data = config.data ? JSON.parse(config.data) : null
        try {
          const mockData = getMockData(url, method, params, data)
          if (mockData && mockData.code === 200) {
            console.log('[Mock Fallback] Using mock data for:', method, url)
            return Promise.resolve({
              data: mockData.data,
              status: 200,
              statusText: 'OK',
              headers: {},
              config: config
            })
          }
        } catch (e) {
          console.warn('Mock data fallback failed:', e)
        }
      }
      return Promise.reject(new Error('请求失败'))
    }
    
    if (res.code !== 200) {
      Message.error(res.message || '请求失败')
      if (res.code === 401) {
        localStorage.removeItem('accessToken')
        window.location.href = '/login'
      }
      return Promise.reject(new Error(res.message || '请求失败'))
    }
    return {
      ...response,
      data: res.data
    } as AxiosResponse<any, any, {}>
  },
  (error) => {
    const isNetworkError = error.code === 'ECONNREFUSED' ||
      error.message?.includes('Network Error') ||
      error.message?.includes('ERR_NETWORK') ||
      error.code === 'ERR_NETWORK'

    const isServerError = error.response?.status >= 400

    if (USE_MOCK_FALLBACK && (isNetworkError || isServerError)) {
      const config = error.config
      const url = config.url || ''
      const method = (config.method || 'get').toLowerCase()
      const params = config.params
      const data = config.data ? JSON.parse(config.data) : null

      try {
        const mockData = getMockData(url, method, params, data)
        if (mockData && mockData.code === 200) {
          console.log('[Mock Fallback] Using mock data for:', method, url)
          return Promise.resolve({
            data: mockData.data,
            status: 200,
            statusText: 'OK',
            headers: {},
            config: config
          })
        }
      } catch (e) {
        console.warn('Mock data fallback failed:', e)
      }
    }

    Message.error(error.message || '网络错误')
    return Promise.reject(error)
  }
)

export default service