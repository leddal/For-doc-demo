import http from './request'
import type { PageParams, PageResult, Announcement } from '@/types'

export async function getAnnouncementsApi(params: PageParams): Promise<PageResult<Announcement>> {
  const { page, pageSize } = params
  const items = await http.get<any, Announcement[]>('/announcements')
  const start = (page - 1) * pageSize
  return {
    items: items.slice(start, start + pageSize),
    total: items.length,
    page,
    pageSize,
  }
}
