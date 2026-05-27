import { defineStore } from 'pinia'
import { ref } from 'vue'
import { loginApi, logoutApi, getUserInfoApi } from '@/api/auth'
import type { UserInfo, LoginRequest } from '@/types'
import { ElMessage } from 'element-plus'

export const useAuthStore = defineStore('auth', () => {
  const token = ref(localStorage.getItem('token') || '')
  const user = ref<UserInfo | null>(null)

  async function login(data: LoginRequest) {
    const res = await loginApi(data)
    token.value = res.data.token
    user.value = res.data.user
    localStorage.setItem('token', res.data.token)
    return res.data
  }

  async function fetchUserInfo() {
    try {
      const res = await getUserInfoApi()
      user.value = res.data
    } catch {
      user.value = null
    }
  }

  async function logout() {
    try {
      await logoutApi()
    } catch {
      // ignore
    }
    token.value = ''
    user.value = null
    localStorage.removeItem('token')
    ElMessage.success('已退出登录')
  }

  function isLoggedIn() {
    return !!token.value
  }

  return { token, user, login, fetchUserInfo, logout, isLoggedIn }
})
