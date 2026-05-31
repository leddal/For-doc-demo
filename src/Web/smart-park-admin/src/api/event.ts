import http from './request'
import type { PageParams, PageResult, ParkEvent } from '@/types'

export function getEventsApi(params: PageParams & { status?: string | number; keyword?: string }) {
  const { page, pageSize, ...rest } = params
  return http.get<any, PageResult<ParkEvent>>('/events', {
    params: {
      ...rest,
      pageNumber: page,
      pageSize,
    },
  })
}

export function resolveEventApi(id: string) {
  return http.post<any, ParkEvent>(`/events/${id}/close`, { note: '前端标记处理' })
}
