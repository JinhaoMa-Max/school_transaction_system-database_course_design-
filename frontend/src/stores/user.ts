import { defineStore } from 'pinia'
import { ref, computed } from 'vue'
import type { User } from '@/types'
import { login as loginApi, logout as logoutApi, getCurrentUser } from '@/api'

export const useUserStore = defineStore('user', () => {
  const user = ref<User | null>(null)

 // token初始值现在由保留状态决定
const token = ref(
  localStorage.getItem('accessToken') || sessionStorage.getItem('accessToken') || ''
)

  const isLoggedIn = computed(() => !!token.value && !!user.value)
  const isAdmin = computed(() => user.value?.role === 'admin')
  const isSeller = computed(() => user.value?.role === 'seller')
  const isBuyer = computed(() => user.value?.role === 'buyer')

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

  const fetchCurrentUser = async () => {
    const res = await getCurrentUser()
    user.value = res.data
  }

  const setUser = (newUser: User) => {
    user.value = newUser
  }

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
