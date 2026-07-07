/**
 * 用户信息接口
 * 定义用户的基本信息和状态
 */
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

/**
 * 学生认证信息接口
 * 定义用户的学生认证相关信息
 */
export interface StudentAuth {
  authId: number
  userId: number
  studentId: string
  realName: string
  college: string
  authStatus: 'pending' | 'approved' | 'rejected'
  authTime: string
}

/**
 * 商品分类接口
 * 定义商品分类的层级结构
 */
export interface Category {
  categoryId: number
  categoryName: string
  parentId: number | null
  sortOrder: number
  children?: Category[]
}

/**
 * 商品信息接口
 * 定义商品的基本信息和状态
 */
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

/**
 * 商品图片接口
 * 定义商品图片的信息
 */
export interface GoodsImage {
  imageId: number
  goodsId: number
  imageUrl: string
  sortOrder: number
}

/**
 * 收藏记录接口
 * 定义用户收藏商品的记录
 */
export interface Favorite {
  favoriteId: number
  userId: number
  goodsId: number
  favoriteTime: string
}

/**
 * 议价申请接口
 * 定义用户议价的相关信息
 */
export interface BargainOffer {
  bargainId: number
  goodsId: number
  buyerId: number
  offerPrice: number
  sellerResult: 'pending' | 'accepted' | 'rejected' | 'countered'
  counterPrice: number | null
  status: 'active' | 'accepted' | 'rejected' | 'closed'
  createTime: string
}

/**
 * 交易订单接口
 * 定义订单的基本信息和状态
 */
export interface TradeOrder {
  orderId: number
  goodsId: number
  buyerId: number
  sellerId: number
  dealPrice: number
  status: 'pending_meet' | 'in_meet' | 'completed' | 'cancelled'
  createTime: string
}

/**
 * 面交预约接口
 * 定义订单面交预约的相关信息
 */
export interface Appointment {
  appointmentId: number
  orderId: number
  meetTime: string
  meetLocation: string
  confirmCode: string
  status: 'pending' | 'confirmed' | 'completed' | 'cancelled'
  createTime: string
}

/**
 * 聊天会话接口
 * 定义聊天会话的基本信息
 */
export interface ChatSession {
  sessionId: number
  goodsId: number
  buyerId: number
  sellerId: number
  createTime: string
}

/**
 * 聊天消息接口
 * 定义聊天消息的内容和状态
 */
export interface ChatMessage {
  messageId: number
  sessionId: number
  senderId: number
  content: string
  readStatus: 0 | 1
  sendTime: string
}

/**
 * 评价接口
 * 定义用户对订单的评价信息
 */
export interface Review {
  reviewId: number
  orderId: number
  reviewerId: number
  reviewedUserId: number
  rating: number
  content: string | null
  reviewTime: string
}

/**
 * 举报接口
 * 定义用户举报的相关信息
 */
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

/**
 * 审计日志接口
 * 定义管理员操作的审计记录
 */
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

/**
 * 通知接口
 * 定义系统通知的相关信息
 */
export interface Notice {
  noticeId: number
  title: string
  content: string
  noticeType: 'system' | 'transaction' | 'violation'
  publisherId: number
  publishTime: string
}

/**
 * 登录参数接口
 * 定义用户登录时需要的参数
 */
export interface LoginParams {
  username: string
  password: string
}

/**
 * 登录结果接口
 * 定义用户登录成功后返回的信息
 */
export interface LoginResult {
  token: string
  user: User
}

/**
 * 分页结果接口
 * 定义分页查询返回的通用结构
 * @template T 数据列表的类型
 */
export interface PageResult<T> {
  list: T[]
  total: number
  page: number
  size: number
}

/**
 * 商品查询参数接口
 * 定义商品列表查询时可用的筛选条件
 */
export interface GoodsQuery {
  keyword?: string
  categoryId?: number
  minPrice?: number
  maxPrice?: number
  condition?: string
  status?: string
  page?: number
  size?: number
}

/**
 * 订单查询参数接口
 * 定义订单列表查询时可用的筛选条件
 */
export interface OrderQuery {
  status?: string
  page?: number
  size?: number
}