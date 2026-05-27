import http from './request'
import type { ApiResponse, PageParams, PageResult, Asset } from '@/types'

export function getAssetsApi(params: PageParams & { keyword?: string }) {
  return http.get<any, ApiResponse<PageResult<Asset>>>('/asset/list', { params })
}

export function getAssetDetailApi(id: number) {
  return http.get<any, ApiResponse<Asset>>(`/asset/${id}`)
}
