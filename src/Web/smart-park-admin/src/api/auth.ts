import http from './request'
import type { ApiResponse, LoginRequest, LoginResponse, UserInfo } from '@/types'

export function loginApi(data: LoginRequest) {
  return http.post<any, ApiResponse<LoginResponse>>('/auth/login', data)
}

export function logoutApi() {
  return http.post<any, ApiResponse<null>>('/auth/logout')
}

export function getUserInfoApi() {
  return http.get<any, ApiResponse<UserInfo>>('/auth/userinfo')
}
