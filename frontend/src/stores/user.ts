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
  // 用户Token，从localStorage中读取，初始为空字符串
  const token = ref(localStorage.getItem('accessToken') || '')

  // 计算属性：判断用户是否已登录（同时存在token和user）
  const isLoggedIn = computed(() => !!token.value && !!user.value)
  // 计算属性：判断当前用户是否为管理员
  const isAdmin = computed(() => user.value?.role === 'admin')
  // 计算属性：判断当前用户是否为卖家
  const isSeller = computed(() => user.value?.role === 'seller')
  // 计算属性：判断当前用户是否为买家
  const isBuyer = computed(() => user.value?.role === 'buyer')

  /**
   * 用户登录
   * @param username 用户名
   * @param password 密码
   */
  const login = async (username: string, password: string) => {
    // 调用登录API获取登录结果
    const res = await loginApi({ username, password })
    // 更新token状态
    token.value = res.data.token
    // 更新用户信息状态
    user.value = res.data.user
    // 将token存入localStorage，实现持久化存储
    localStorage.setItem('accessToken', res.data.token)
  }

  /**
   * 用户登出
   */
  const logout = async () => {
    // 调用登出API通知后端
    await logoutApi()
    // 清空token状态
    token.value = ''
    // 清空用户信息状态
    user.value = null
    // 从localStorage中移除token
    localStorage.removeItem('accessToken')
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
    isSeller,
    isBuyer,
    login,
    logout,
    fetchCurrentUser,
    setUser
  }
})