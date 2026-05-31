import http from './request'
import type { CurrentUserResponse, LoginRequest, LoginTokenResponse } from '@/types'

export function loginApi(data: LoginRequest) {
  return http.post<any, LoginTokenResponse>('/auth/login', data)
}

export function logoutApi() {
  return http.post<any, null>('/auth/logout')
}

export function getUserInfoApi() {
  return http.get<any, CurrentUserResponse>('/auth/me')
}
