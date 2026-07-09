// 导入Pinia的状态管理定义函数
import { defineStore } from 'pinia'
// 导入Vue的响应式和计算属性API
import { ref, computed } from 'vue'
// 导入用户类型定义
import type { User } from '@/types'
// 导入认证相关的API接口
import { login as loginApi, logout as logoutApi, getCurrentUser } from '@/api'

/**
 * 用户状态管理Store
 * 使用Pinia的setup模式定义，管理用户登录状态、个人信息等
 */
export const useUserStore = defineStore('user', () => {
  // 用户信息，初始值为null
  const user = ref<User | null>(null)

 // token初始值现在由保留状态决定
const token = ref(
  localStorage.getItem('accessToken') || sessionStorage.getItem('accessToken') || ''
)

  // 计算属性：判断用户是否已登录（同时存在token和user）
  const isLoggedIn = computed(() => !!token.value && !!user.value)
  // 计算属性：判断当前用户是否为管理员
  const isAdmin = computed(() => user.value?.role === 'admin')
  // 计算属性：判断当前用户是否为普通用户
  const isUser = computed(() => user.value?.role === 'user')

  // 登录方法，增加了一个rememberMe参数
const login = async (account: string, password: string, rememberMe = true) => {
  const res = await loginApi({ account, password })
  token.value = res.data.token
  user.value = res.data.user
  const storage = rememberMe ? localStorage : sessionStorage
  storage.setItem('accessToken', res.data.token)
}

// 登出方法，同上
  const logout = async () => {
  await logoutApi()
  token.value = ''
  user.value = null
  localStorage.removeItem('accessToken')
  sessionStorage.removeItem('accessToken')
}

  /**
   * 获取当前登录用户信息
   */
  const fetchCurrentUser = async () => {
    // 调用获取当前用户API
    const res = await getCurrentUser()
    // 更新用户信息状态
    user.value = res.data
  }

  /**
   * 设置用户信息
   * @param newUser 用户对象
   */
  const setUser = (newUser: User) => {
    // 直接更新用户信息状态
    user.value = newUser
  }

  // 返回store的状态和方法，供组件调用
  return {
    user,
    token,
    isLoggedIn,
    isAdmin,
    isUser,
    login,
    logout,
    fetchCurrentUser,
    setUser
  }
})