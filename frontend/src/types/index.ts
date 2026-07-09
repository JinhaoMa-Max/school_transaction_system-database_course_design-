export interface User {
  userId: number
  username: string
  password: string
  nickname: string
  avatarUrl: string
  phone: string
  email: string
  role: 'buyer' | 'seller' | 'admin'
  status: 'normal' | 'banned'
  creditScore: number
  registerTime: string
}

export interface StudentAuth {
  authId: number
  userId: number
  studentId: string
  realName: string
  college: string
  authStatus: 'pending' | 'approved' | 'rejected'
  authTime: string
}

export interface Category {
  categoryId: number
  categoryName: string
  parentId: number | null
  sortOrder: number
  children?: Category[]
}

export interface Goods {
  goodsId: number
  sellerId: number
  categoryId: number
  title: string
  description: string
  price: number
  condition: 'new' | 'like_new' | 'slight_use' | 'obvious_trace'
  status: 'pending' | 'approved' | 'rejected' | 'locked' | 'sold' | 'offline'
  viewCount: number
  publishTime: string
  imageUrl?: string
  sellerNickname?: string
}

export interface GoodsImage {
  imageId: number
  goodsId: number
  imageUrl: string
  sortOrder: number
}

export interface Favorite {
  favoriteId: number
  userId: number
  goodsId: number
  favoriteTime: string
}

export interface BargainOffer {
  bargainId: number
  goodsId: number
  buyerId: number
  offerPrice: number
  sellerResult: 'pending' | 'accepted' | 'rejected' | 'countered'
  counterPrice: number | null
  status: 'active' | 'accepted' | 'rejected' | 'closed'
  createTime: string
  /** 买家处理结果（中间传递值，对称于 sellerResult，不存入数据库表） */
  buyerResult?: 'pending' | 'accepted' | 'rejected' | 'countered'
}

export interface TradeOrder {
  orderId: number
  goodsId: number
  buyerId: number
  sellerId: number
  dealPrice: number
  status: 'pending_meet' | 'in_meet' | 'completed' | 'cancelled'
  createTime: string
}

export interface Appointment {
  appointmentId: number
  orderId: number
  meetTime: string
  meetLocation: string
  confirmCode: string
  status: 'pending' | 'confirmed' | 'completed' | 'cancelled'
  createTime: string
}

export interface ChatSession {
  sessionId: number
  goodsId: number
  buyerId: number
  sellerId: number
  createTime: string
}

export interface ChatMessage {
  messageId: number
  sessionId: number
  senderId: number
  content: string
  readStatus: 0 | 1
  sendTime: string
}

export interface Review {
  reviewId: number
  orderId: number
  reviewerId: number
  reviewedUserId: number
  rating: number
  content: string | null
  reviewTime: string
}

export interface Report {
  reportId: number
  reporterId: number
  reportType: 'goods' | 'user' | 'order'
  reportedGoodsId: number | null
  reportedUserId: number | null
  reportedOrderId: number | null
  reason: string
  status: 'pending' | 'processing' | 'resolved' | 'rejected'
  reportTime: string
}

export interface AuditLog {
  logId: number
  adminId: number
  auditType: 'goods_audit' | 'report_handle' | 'user_ban' | 'goods_offline'
  targetId: number
  action: 'approve' | 'reject' | 'ban' | 'offline'
  result: string
  remark: string | null
  handleTime: string
}

export interface Notice {
  noticeId: number
  title: string
  content: string
  noticeType: 'system' | 'transaction' | 'violation'
  publisherId: number
  publishTime: string
}

export interface LoginParams {
  //修改：把username改为account，方便登录时使用学号或账号名登录
  account: string
  password: string
}

//新增：增加了一个接口，用于注册时把学号也搞进去
export interface RegisterParams {
  username: string
  password: string
  studentId: string
  nickname: string
  phone: string
  email: string
}


export interface LoginResult {
  token: string
  user: User
}

export interface PageResult<T> {
  list: T[]
  total: number
  page: number
  size: number
}

export interface GoodsQuery {
  keyword?: string
  categoryId?: number
  minPrice?: number
  maxPrice?: number
  condition?: string
  status?: string
  page?: number
  size?: number
  sortBy?: string
  ascending?: boolean
}

export interface OrderQuery {
  status?: string
  page?: number
  size?: number
}

//新增：增加了一个接口，用于更新用户信息时的参数(安全性保证)
export interface UpdateUserParams {
  nickname?: string
  avatarUrl?: string
  phone?: string
  email?: string
}
