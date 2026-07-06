export { default as request, type ApiResponse } from './request'
export { handleApiError, handleNetworkError, getErrorMessage, ApiError } from './errorHandler'
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
