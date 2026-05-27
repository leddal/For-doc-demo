/** 统一 API 响应结构 */
export interface ApiResponse<T = unknown> {
  code: number
  message: string
  data: T
}

/** 分页参数 */
export interface PageParams {
  page: number
  pageSize: number
}

/** 分页响应 */
export interface PageResult<T> {
  items: T[]
  total: number
  page: number
  pageSize: number
}

/** 登录请求 */
export interface LoginRequest {
  username: string
  password: string
}

/** 登录响应 */
export interface LoginResponse {
  token: string
  user: UserInfo
}

/** 用户信息 */
export interface UserInfo {
  id: number
  username: string
  displayName: string
  role: string
}

/** 工单 */
export interface WorkOrder {
  id: number
  title: string
  description: string
  status: 'pending' | 'processing' | 'completed' | 'cancelled'
  priority: 'low' | 'medium' | 'high' | 'urgent'
  assignee?: string
  createdAt: string
  updatedAt: string
}

/** 资产 */
export interface Asset {
  id: number
  name: string
  type: string
  location: string
  status: 'normal' | 'warning' | 'fault' | 'offline'
  department: string
  lastMaintenance: string
}

/** 事件 */
export interface ParkEvent {
  id: number
  title: string
  type: 'fire' | 'intrusion' | 'device' | 'environment' | 'other'
  level: 'info' | 'warning' | 'critical'
  source: string
  description: string
  happenedAt: string
  status: 'unprocessed' | 'processing' | 'resolved'
}

/** 公告 */
export interface Announcement {
  id: number
  title: string
  content: string
  publisher: string
  publishedAt: string
  priority: 'low' | 'normal' | 'high'
}
