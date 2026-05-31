import { defineStore } from 'pinia'
import { ref } from 'vue'
import { loginApi, logoutApi, getUserInfoApi } from '@/api/auth'
import type { CurrentUserResponse, LoginRequest, LoginTokenResponse, UserInfo } from '@/types'
import { ElMessage } from 'element-plus'

function mapUser(payload: CurrentUserResponse | LoginTokenResponse): UserInfo {
  return {
    id: payload.userId,
    username: payload.userName,
    displayName: payload.displayName,
    roles: payload.roles,
    permissions: payload.permissions,
  }
}

export const useAuthStore = defineStore('auth', () => {
  const token = ref(localStorage.getItem('token') || '')
  const user = ref<UserInfo | null>(null)

  async function login(data: LoginRequest) {
    const res = await loginApi(data)
    token.value = res.accessToken
    user.value = mapUser(res)
    localStorage.setItem('token', res.accessToken)
    return user.value
  }

  async function fetchUserInfo() {
    try {
      const res = await getUserInfoApi()
      user.value = mapUser(res)
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
