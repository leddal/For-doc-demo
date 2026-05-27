import http from './request'
import type { ApiResponse, PageParams, PageResult, WorkOrder } from '@/types'

export function getWorkOrdersApi(params: PageParams) {
  return http.get<any, ApiResponse<PageResult<WorkOrder>>>('/work-order/list', { params })
}

export function createWorkOrderApi(data: Partial<WorkOrder>) {
  return http.post<any, ApiResponse<WorkOrder>>('/work-order/create', data)
}

export function updateWorkOrderApi(id: number, data: Partial<WorkOrder>) {
  return http.put<any, ApiResponse<WorkOrder>>(`/work-order/${id}`, data)
}
