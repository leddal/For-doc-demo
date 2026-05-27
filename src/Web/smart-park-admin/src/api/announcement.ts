import http from './request'
import type { ApiResponse, PageParams, PageResult, Announcement } from '@/types'

export function getAnnouncementsApi(params: PageParams) {
  return http.get<any, ApiResponse<PageResult<Announcement>>>('/announcement/list', { params })
}
