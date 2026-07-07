// 导入Vue Router的核心函数
import { createRouter, createWebHistory } from 'vue-router'
// 导入Vue Router的路由记录类型定义
import type { RouteRecordRaw } from 'vue-router'
// 导入用户状态管理store，用于路由守卫中的权限判断
import { useUserStore } from '@/stores'

// 路由配置数组，定义了所有页面路由及其权限要求
const routes: RouteRecordRaw[] = [
  // 登录页面 - 无需认证
  {
    path: '/login',
    name: 'Login',
    component: () => import('@/views/Login.vue'),
    meta: { requiresAuth: false }
  },
  // 注册页面 - 无需认证
  {
    path: '/register',
    name: 'Register',
    component: () => import('@/views/Register.vue'),
    meta: { requiresAuth: false }
  },
  // 首页 - 无需认证
  {
    path: '/',
    name: 'Home',
    component: () => import('@/views/Home.vue'),
    meta: { requiresAuth: false }
  },
  // 商品列表页面 - 无需认证
  {
    path: '/goods',
    name: 'GoodsList',
    component: () => import('@/views/goods/List.vue'),
    meta: { requiresAuth: false }
  },
  // 商品详情页面 - 无需认证
  {
    path: '/goods/:id',
    name: 'GoodsDetail',
    component: () => import('@/views/goods/Detail.vue'),
    meta: { requiresAuth: false }
  },
  // 发布商品页面 - 需要登录，仅限卖家角色
  {
    path: '/goods/publish',
    name: 'GoodsPublish',
    component: () => import('@/views/goods/Publish.vue'),
    meta: { requiresAuth: true, roles: ['seller'] }
  },
  // 编辑商品页面 - 需要登录，仅限卖家角色
  {
    path: '/goods/:id/edit',
    name: 'GoodsEdit',
    component: () => import('@/views/goods/Edit.vue'),
    meta: { requiresAuth: true, roles: ['seller'] }
  },
  // 我的收藏页面 - 需要登录
  {
    path: '/favorites',
    name: 'FavoriteList',
    component: () => import('@/views/Favorite.vue'),
    meta: { requiresAuth: true }
  },
  // 议价列表页面 - 需要登录
  {
    path: '/bargains',
    name: 'BargainList',
    component: () => import('@/views/Bargain.vue'),
    meta: { requiresAuth: true }
  },
  // 订单列表页面 - 需要登录
  {
    path: '/orders',
    name: 'OrderList',
    component: () => import('@/views/order/List.vue'),
    meta: { requiresAuth: true }
  },
  // 订单详情页面 - 需要登录
  {
    path: '/orders/:id',
    name: 'OrderDetail',
    component: () => import('@/views/order/Detail.vue'),
    meta: { requiresAuth: true }
  },
  // 预约列表页面 - 需要登录
  {
    path: '/appointments',
    name: 'AppointmentList',
    component: () => import('@/views/Appointment.vue'),
    meta: { requiresAuth: true }
  },
  // 聊天页面 - 需要登录
  {
    path: '/chat',
    name: 'Chat',
    component: () => import('@/views/Chat.vue'),
    meta: { requiresAuth: true }
  },
  // 评价列表页面 - 需要登录
  {
    path: '/reviews',
    name: 'ReviewList',
    component: () => import('@/views/Review.vue'),
    meta: { requiresAuth: true }
  },
  // 举报列表页面 - 需要登录
  {
    path: '/reports',
    name: 'ReportList',
    component: () => import('@/views/Report.vue'),
    meta: { requiresAuth: true }
  },
  // 个人中心页面 - 需要登录
  {
    path: '/profile',
    name: 'Profile',
    component: () => import('@/views/Profile.vue'),
    meta: { requiresAuth: true }
  },
  // 学生认证页面 - 需要登录
  {
    path: '/student-auth',
    name: 'StudentAuth',
    component: () => import('@/views/StudentAuth.vue'),
    meta: { requiresAuth: true }
  },
  // 管理员后台路由 - 需要登录，仅限管理员角色
  {
    path: '/admin',
    name: 'Admin',
    component: () => import('@/views/admin/Layout.vue'),
    meta: { requiresAuth: true, roles: ['admin'] },
    children: [
      // 管理员仪表盘 - 管理员后台首页
      {
        path: 'dashboard',
        name: 'AdminDashboard',
        component: () => import('@/views/admin/Dashboard.vue')
      },
      // 用户管理 - 管理员后台用户管理页面
      {
        path: 'users',
        name: 'AdminUserList',
        component: () => import('@/views/admin/UserList.vue')
      },
      // 商品管理 - 管理员后台商品管理页面
      {
        path: 'goods',
        name: 'AdminGoodsList',
        component: () => import('@/views/admin/GoodsList.vue')
      },
      // 订单管理 - 管理员后台订单管理页面
      {
        path: 'orders',
        name: 'AdminOrderList',
        component: () => import('@/views/admin/OrderList.vue')
      },
      // 举报管理 - 管理员后台举报管理页面
      {
        path: 'reports',
        name: 'AdminReportList',
        component: () => import('@/views/admin/ReportList.vue')
      },
      // 通知管理 - 管理员后台通知管理页面
      {
        path: 'notices',
        name: 'AdminNoticeList',
        component: () => import('@/views/admin/NoticeList.vue')
      },
      // 审计日志 - 管理员后台审计日志页面
      {
        path: 'audit-logs',
        name: 'AdminAuditLogs',
        component: () => import('@/views/admin/AuditLogs.vue')
      }
    ]
  }
]

// 创建路由实例，使用HTML5 History模式
const router = createRouter({
  // 使用HTML5 History模式，不带#前缀
  history: createWebHistory(),
  // 应用路由配置
  routes
})

// 全局路由前置守卫，用于权限控制和导航拦截
router.beforeEach(async (to, _from, next) => {
  // 获取用户状态管理store实例
  const userStore = useUserStore()
  // 获取目标路由是否需要认证
  const requiresAuth = to.meta.requiresAuth

  // 如果目标路由需要认证但用户未登录，重定向到登录页
  if (requiresAuth && !userStore.isLoggedIn) {
    next('/login')
    return
  }

  // 如果目标路由需要认证且用户已登录但用户信息为空，尝试获取当前用户信息
  if (requiresAuth && userStore.isLoggedIn && !userStore.user) {
    try {
      // 请求后端获取当前用户信息
      await userStore.fetchCurrentUser()
    } catch {
      // 获取失败，退出登录并重定向到登录页
      userStore.logout()
      next('/login')
      return
    }
  }

  // 如果路由配置了角色权限，检查当前用户角色是否匹配
  if (to.meta.roles && userStore.user) {
    // 获取路由要求的角色列表
    const roles = to.meta.roles as string[]
    // 如果用户角色不在允许的角色列表中，重定向到首页
    if (!roles.includes(userStore.user.role)) {
      next('/')
      return
    }
  }

  // 权限检查通过，继续导航到目标路由
  next()
})

// 导出路由实例，供main.ts使用
export default router