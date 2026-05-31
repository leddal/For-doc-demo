import http from './request'
import type { PageParams, PageResult, Asset } from '@/types'

export function getAssetsApi(params: PageParams & { keyword?: string }) {
  const { page, pageSize, ...rest } = params
  return http.get<any, PageResult<Asset>>('/assets', {
    params: {
      ...rest,
      pageNumber: page,
      pageSize,
    },
  })
}

export function getAssetDetailApi(id: string) {
  return http.get<any, Asset>(`/assets/${id}`)
}
