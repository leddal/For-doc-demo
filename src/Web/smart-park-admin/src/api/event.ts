import http from './request'
import type { ApiResponse, PageParams, PageResult, ParkEvent } from '@/types'

export function getEventsApi(params: PageParams & { type?: string }) {
  return http.get<any, ApiResponse<PageResult<ParkEvent>>>('/event/list', { params })
}

export function resolveEventApi(id: number) {
  return http.put<any, ApiResponse<null>>(`/event/${id}/resolve`)
}
