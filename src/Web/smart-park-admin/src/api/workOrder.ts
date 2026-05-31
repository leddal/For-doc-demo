import http from './request'
import type { PageParams, PageResult, WorkOrder } from '@/types'

export function getWorkOrdersApi(params: PageParams) {
  const { page, pageSize } = params
  return http.get<any, PageResult<WorkOrder>>('/work-orders', {
    params: {
      pageNumber: page,
      pageSize,
    },
  })
}

export function createWorkOrderApi(data: Partial<WorkOrder>) {
  return http.post<any, WorkOrder>('/work-orders', data)
}
