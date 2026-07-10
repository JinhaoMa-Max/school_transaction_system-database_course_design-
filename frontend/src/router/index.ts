import { createRouter, createWebHistory } from 'vue-router'
import type { RouteRecordRaw } from 'vue-router'
import { useUserStore } from '@/stores'

//为了开发前端页面的神秘小代码（绕过登录验证），在开发环境下可以设置 VITE_BYPASS_AUTH=true 来启用，常态关闭！
import { mockUsers} from '@/utils/mock'

const DEV_BYPASS_AUTH =
  import.meta.env.DEV && import.meta.env.VITE_BYPASS_AUTH === 'true'

const USE_MOCK_USER =
  import.meta.env.DEV && import.meta.env.VITE_USE_MOCK_USER === 'true'


const routes: RouteRecordRaw[] = [
  {
    path: '/login',
    name: 'Login',
    component: () => import('@/views/Login.vue'),
    meta: { requiresAuth: false }
  },
  {
    path: '/register',
    name: 'Register',
    component: () => import('@/views/Register.vue'),
    meta: { requiresAuth: false }
  },
  {
    path: '/',
    name: 'Home',
    component: () => import('@/views/Home.vue'),
    meta: { requiresAuth: false }
  },
  {
    path: '/goods',
    name: 'GoodsList',
    component: () => import('@/views/goods/List.vue'),
    meta: { requiresAuth: false }
  },
  {
    path: '/goods/:id',
    name: 'GoodsDetail',
    component: () => import('@/views/goods/Detail.vue'),
    meta: { requiresAuth: false }
  },
  {
    path: '/goods/publish',
    name: 'GoodsPublish',
    component: () => import('@/views/goods/Publish.vue'),
    meta: { requiresAuth: true }
  },
  {
    path: '/goods/:id/edit',
    name: 'GoodsEdit',
    component: () => import('@/views/goods/Edit.vue'),
    meta: { requiresAuth: true }
  },
  {
    path: '/favorites',
    name: 'FavoriteList',
    component: () => import('@/views/Favorite.vue'),
    meta: { requiresAuth: true }
  },
  {
    path: '/bargains',
    name: 'BargainList',
    component: () => import('@/views/Bargain.vue'),
    meta: { requiresAuth: true }
  },
  {
    path: '/orders',
    name: 'OrderList',
    component: () => import('@/views/order/List.vue'),
    meta: { requiresAuth: true }
  },
  {
    path: '/orders/:id',
    name: 'OrderDetail',
    component: () => import('@/views/order/Detail.vue'),
    meta: { requiresAuth: true }
  },
  {
    path: '/appointments',
    name: 'AppointmentList',
    component: () => import('@/views/Appointment.vue'),
    meta: { requiresAuth: true }
  },
  {
    path: '/appointment/:orderId',
    name: 'AppointmentCreate',
    component: () => import('@/views/appointment/Index.vue'),
    meta: { requiresAuth: true }
  },
  {
    path: '/chat',
    name: 'Chat',
    component: () => import('@/views/Chat.vue'),
    meta: { requiresAuth: true }
  },
  {
    path: '/reviews',
    name: 'ReviewList',
    component: () => import('@/views/Review.vue'),
    meta: { requiresAuth: true }
  },
  {
    path: '/review/:orderId',
    name: 'ReviewCreate',
    component: () => import('@/views/review/Index.vue'),
    meta: { requiresAuth: true }
  },
  {
    path: '/report',
    name: 'ReportCreate',
    component: () => import('@/views/Report.vue'),
    meta: { requiresAuth: true }
  },
  {
    path: '/my/goods',
    name: 'MyGoods',
    component: () => import('@/views/my/Goods.vue'),
    meta: { requiresAuth: true }
  },
  {
    path: '/profile',
    name: 'Profile',
    component: () => import('@/views/Profile.vue'),
    meta: { requiresAuth: true }
  },
  {
    path: '/profile/edit',
    name: 'ProfileEdit',
    component: () => import('@/views/EditProfile.vue'),
    meta: { requiresAuth: true }
  },
  {
    path: '/student-auth',
    name: 'StudentAuth',
    component: () => import('@/views/StudentAuth.vue'),
    meta: { requiresAuth: true }
  },
  {
    path: '/admin',
    name: 'Admin',
    component: () => import('@/views/admin/Layout.vue'),
    meta: { requiresAuth: true, roles: ['admin'] },
    children: [
      {
        path: 'dashboard',
        name: 'AdminDashboard',
        component: () => import('@/views/admin/Dashboard.vue')
      },
      {
        path: 'users',
        name: 'AdminUserList',
        component: () => import('@/views/admin/UserList.vue')
      },
      {
        path: 'goods',
        name: 'AdminGoodsList',
        component: () => import('@/views/admin/GoodsList.vue')
      },
      {
        path: 'orders',
        name: 'AdminOrderList',
        component: () => import('@/views/admin/OrderList.vue')
      },
      {
        path: 'reports',
        name: 'AdminReportList',
        component: () => import('@/views/admin/ReportList.vue')
      },
      {
        path: 'notices',
        name: 'AdminNoticeList',
        component: () => import('@/views/admin/NoticeList.vue')
      },
      {
        path: 'audit-logs',
        name: 'AdminAuditLogs',
        component: () => import('@/views/admin/AuditLogs.vue')
      }
    ]
  }
]

const router = createRouter({
  history: createWebHistory(),
  routes
})

router.beforeEach(async (to, _from, next) => {
  const userStore = useUserStore()

  // 本地开发临时绕过登录校验
  if (DEV_BYPASS_AUTH) {
    if (USE_MOCK_USER && !userStore.user) {
      userStore.setUser(mockUsers[2])

      // 如果store 里 token 是可以直接赋值(还没看)
      userStore.token = 'mock-token'

      localStorage.setItem('accessToken', 'mock-token')
    }

    next()
    return
  }

    // 新增：已登录用户访问登录/注册页 → 跳首页
  if (userStore.isLoggedIn && (to.path === '/login' || to.path === '/register')) {
    next('/')
    return
  }

  const requiresAuth = to.meta.requiresAuth

  if (requiresAuth && !userStore.isLoggedIn) {
     next(`/login?redirect=${encodeURIComponent(to.fullPath)}`)
    return
  }

  if (requiresAuth && userStore.isLoggedIn && !userStore.user) {
    try {
      await userStore.fetchCurrentUser()
    } catch {
      userStore.logout()
      next('/login')
      return
    }
  }

  if (to.meta.roles && userStore.user) {
    const roles = to.meta.roles as string[]
    if (!roles.includes(userStore.user.role)) {
      next('/')
      return
    }
  }

  next()
})

export default router
