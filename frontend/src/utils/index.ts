// 导出请求工具函数和API响应类型定义
export { default as request, type ApiResponse } from './request'
// 导出错误处理相关函数和ApiError类
export { handleApiError, handleNetworkError, getErrorMessage, ApiError } from './errorHandler'
// 导出所有模拟数据和模拟响应工具函数
export {
  mockUsers,
  mockCategories,
  mockGoods,
  mockGoodsImages,
  mockFavorites,
  mockBargains,
  mockOrders,
  mockAppointments,
  mockChatSessions,
  mockChatMessages,
  mockReviews,
  mockReports,
  mockAuditLogs,
  mockNotices,
  mockStudentAuths,
  getMockResponse,
  getMockPageResult
} from './mock'