import { defineStore } from 'pinia'
import { ref, computed } from 'vue'
import type { User } from '@/types'
import { login as loginApi, logout as logoutApi, getCurrentUser } from '@/api'

export const useUserStore = defineStore('user', () => {
  const user = ref<User | null>(null)
  const token = ref(localStorage.getItem('accessToken') || '')

  const isLoggedIn = computed(() => !!token.value && !!user.value)
  const isAdmin = computed(() => user.value?.role === 'admin')
  const isSeller = computed(() => user.value?.role === 'seller')
  const isBuyer = computed(() => user.value?.role === 'buyer')

  const login = async (username: string, password: string) => {
    const res = await loginApi({ username, password })
    token.value = res.data.token
    user.value = res.data.user
    localStorage.setItem('accessToken', res.data.token)
  }

  const logout = async () => {
    await logoutApi()
    token.value = ''
    user.value = null
    localStorage.removeItem('accessToken')
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
