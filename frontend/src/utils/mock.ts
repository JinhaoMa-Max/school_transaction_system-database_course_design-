// 导入所有需要的类型定义
import type { User, Goods, Category, TradeOrder, BargainOffer, ChatSession, ChatMessage, Review, Report, Notice, AuditLog, StudentAuth, Appointment, Favorite, GoodsImage } from '@/types'

/**
 * 模拟用户数据
 * 包含管理员和普通用户两种角色的示例用户
 */
export const mockUsers: User[] = [
  {
    userId: 1,
    username: 'admin',
    password: 'admin123',
    nickname: '管理员',
    avatarUrl: '',
    phone: '13800138000',
    email: 'admin@example.com',
    role: 'admin',
    status: 'normal',
    creditScore: 100,
    registerTime: '2024-01-01 00:00:00'
  },
  {
    userId: 2,
    username: 'user001',
    password: 'user123',
    nickname: '用户小王',
    avatarUrl: '',
    phone: '13800138001',
    email: 'user001@example.com',
    role: 'user',
    status: 'normal',
    creditScore: 98,
    registerTime: '2024-01-02 00:00:00'
  },
  {
    userId: 3,
    username: 'user002',
    password: 'user123',
    nickname: '用户小李',
    avatarUrl: '',
    phone: '13800138002',
    email: 'user002@example.com',
    role: 'user',
    status: 'normal',
    creditScore: 100,
    registerTime: '2024-01-03 00:00:00'
  }
]

/**
 * 模拟分类数据
 * 包含顶级分类和子分类（如数码产品下的手机、电脑）
 */
export const mockCategories: Category[] = [
  { categoryId: 1, categoryName: '数码产品', parentId: null, sortOrder: 1 },
  { categoryId: 2, categoryName: '教材教辅', parentId: null, sortOrder: 2 },
  { categoryId: 3, categoryName: '生活用品', parentId: null, sortOrder: 3 },
  { categoryId: 4, categoryName: '服饰鞋包', parentId: null, sortOrder: 4 },
  { categoryId: 5, categoryName: '其他', parentId: null, sortOrder: 99 },
  { categoryId: 11, categoryName: '手机', parentId: 1, sortOrder: 1 },
  { categoryId: 12, categoryName: '笔记本', parentId: 1, sortOrder: 2 },
  { categoryId: 13, categoryName: '平板/配件', parentId: 1, sortOrder: 3 },
  { categoryId: 21, categoryName: '专业教材', parentId: 2, sortOrder: 1 },
  { categoryId: 22, categoryName: '考试资料', parentId: 2, sortOrder: 2 },
  { categoryId: 23, categoryName: '文学小说', parentId: 2, sortOrder: 3 },
  { categoryId: 31, categoryName: '宿舍用品', parentId: 3, sortOrder: 1 },
  { categoryId: 32, categoryName: '运动健身', parentId: 3, sortOrder: 2 }
]

/**
 * 模拟商品数据
 * 包含不同状态的商品示例（已审核通过、待审核）
 */
export const mockGoods: Goods[] = [
  {
    goodsId: 1,
    sellerId: 2,
    categoryId: 5,
    title: 'iPhone 14 Pro 256GB 深空黑',
    description: '自用iPhone 14 Pro，95新，无磕碰，电池健康度92%',
    price: 5500,
    condition: 'like_new',
    status: 'approved',
    viewCount: 120,
    publishTime: '2024-01-10 10:00:00'
  },
  {
    goodsId: 2,
    sellerId: 2,
    categoryId: 2,
    title: '高等数学第七版上下册',
    description: '同济大学高等数学第七版，几乎全新，有少量笔记',
    price: 45,
    condition: 'like_new',
    status: 'approved',
    viewCount: 80,
    publishTime: '2024-01-11 14:00:00'
  },
  {
    goodsId: 3,
    sellerId: 2,
    categoryId: 3,
    title: 'Nike Air Force 1 白色 42码',
    description: '穿过几次，成色很新，正品保证',
    price: 400,
    condition: 'slight_use',
    status: 'pending',
    viewCount: 50,
    publishTime: '2024-01-12 09:00:00'
  }
]

/**
 * 模拟商品图片数据
 */
export const mockGoodsImages: GoodsImage[] = [
  { imageId: 1, goodsId: 1, imageUrl: 'https://via.placeholder.com/400', sortOrder: 1 },
  { imageId: 2, goodsId: 2, imageUrl: 'https://via.placeholder.com/400', sortOrder: 1 },
  { imageId: 3, goodsId: 3, imageUrl: 'https://via.placeholder.com/400', sortOrder: 1 }
]

/**
 * 模拟收藏数据
 */
export const mockFavorites: Favorite[] = [
  { favoriteId: 1, userId: 3, goodsId: 1, title: 'iPhone 14 Pro 256GB 深空黑', price: 5500, imageUrl: 'https://via.placeholder.com/400', status: 'approved', favoriteTime: '2024-01-11 10:00:00' },
  { favoriteId: 2, userId: 3, goodsId: 2, title: '高等数学第七版上下册', price: 45, imageUrl: 'https://via.placeholder.com/400', status: 'approved', favoriteTime: '2024-01-12 14:00:00' }
]

/**
 * 模拟议价数据
 */
export const mockBargains: BargainOffer[] = [
  {
    bargainId: 1,
    goodsId: 1,
    buyerId: 3,
    offerPrice: 5200,
    sellerResult: 'countered',
    counterPrice: 5300,
    status: 'active',
    createTime: '2024-01-11 15:00:00'
  },
  {
    bargainId: 2,
    goodsId: 1,
    buyerId: 3,
    offerPrice: 4800,
    sellerResult: 'pending',
    counterPrice: null,
    status: 'active',
    createTime: '2024-01-11 15:05:00'
  }
]

/**
 * 模拟订单数据
 */
export const mockOrders: TradeOrder[] = [
  {
    orderId: 1,
    goodsId: 2,
    buyerId: 3,
    sellerId: 2,
    dealPrice: 45,
    status: 'pending_meet',
    createTime: '2024-01-12 16:00:00',
    buyerReviewed: 0,
    sellerReviewed: 0
  }
]

/**
 * 模拟预约数据
 */
export const mockAppointments: Appointment[] = [
  {
    appointmentId: 1,
    orderId: 1,
    meetTime: '2024-01-13 14:00:00',
    meetLocation: '图书馆门口',
    confirmCode: 'ABC123',
    status: 'pending',
    createTime: '2024-01-12 16:30:00'
  }
]

/**
 * 模拟聊天会话数据
 */
export const mockChatSessions: ChatSession[] = [
  { sessionId: 1, goodsId: 1, buyerId: 3, sellerId: 2, createTime: '2024-01-11 10:00:00' }
]

/**
 * 模拟聊天消息数据
 */
export const mockChatMessages: ChatMessage[] = [
  { messageId: 1, sessionId: 1, senderId: 3, content: '你好，这个手机最低多少钱？', readStatus: 1, sendTime: '2024-01-11 10:00:00' },
  { messageId: 2, sessionId: 1, senderId: 2, content: '最低5500元', readStatus: 1, sendTime: '2024-01-11 10:05:00' },
  { messageId: 3, sessionId: 1, senderId: 3, content: '可以便宜一点吗？', readStatus: 1, sendTime: '2024-01-11 10:10:00' }
]

/**
 * 模拟评价数据（空数组）
 */
export const mockReviews: Review[] = []

/**
 * 模拟举报数据（空数组）
 */
export const mockReports: Report[] = []

/**
 * 模拟审计日志数据
 */
export const mockAuditLogs: AuditLog[] = [
  {
    logId: 1,
    adminId: 1,
    auditType: 'goods_audit',
    targetId: 1,
    action: 'approve',
    result: '商品审核通过',
    remark: '商品信息完整，图片清晰',
    handleTime: '2024-01-10 11:00:00'
  }
]

/**
 * 模拟通知数据
 */
export const mockNotices: Notice[] = [
  {
    noticeId: 1,
    title: '平台交易规范更新通知',
    content: '为了保障交易安全，平台将于2024年2月1日起实施新的交易规范...',
    noticeType: 'system',
    publisherId: 1,
    publishTime: '2024-01-05 10:00:00'
  }
]

/**
 * 模拟学生认证数据
 */
export const mockStudentAuths: StudentAuth[] = [
  {
    authId: 1,
    userId: 3,
    studentId: '2021001001',
    realName: '李小明',
    college: '计算机学院',
    authStatus: 'rejected',
    authTime: '2024-01-04 10:00:00'
  }
]

/**
 * 生成模拟API响应对象
 * @template T 响应数据类型
 * @param data 响应数据
 * @param code 响应码（默认200）
 * @param message 响应消息（默认success）
 * @returns API响应对象
 */
export const getMockResponse = <T>(data: T, code = 200, message = 'success') => {
  return { code, message, data }
}

/**
 * 生成模拟分页响应对象
 * @template T 数据列表类型
 * @param list 数据列表
 * @param page 当前页码（默认1）
 * @param size 每页数量（默认10）
 * @returns API分页响应对象
 */
export const getMockPageResult = <T>(list: T[], page = 1, size = 10) => {
  return getMockResponse({
    list,
    total: list.length,
    page,
    size
  })
}