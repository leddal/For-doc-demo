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
  totalCount?: number
  page: number
  pageNumber?: number
  pageSize: number
}

/** 登录请求 */
export interface LoginRequest {
  username: string
  password: string
}

/** 登录响应 */
export interface LoginTokenResponse {
  accessToken: string
  expiresAt: string
  userId: string
  userName: string
  displayName: string
  roles: string[]
  permissions: string[]
}

/** 当前用户响应 */
export interface CurrentUserResponse {
  userId: string
  userName: string
  displayName: string
  roles: string[]
  permissions: string[]
}

/** 用户信息 */
export interface UserInfo {
  id: string
  username: string
  displayName: string
  roles: string[]
  permissions: string[]
}

/** 工单 */
export interface WorkOrder {
  id: string
  number?: string
  title: string
  description?: string
  status: string | number
  priority: string | number
  sourceType: string | number
  businessType?: string | number
  parkArea?: string
  relatedAssetId?: string | null
  relatedEventId?: string | null
  assetId?: string | null
  eventId?: string | null
  createdAt: string
}

/** 资产 */
export interface Asset {
  id: string
  assetCode?: string
  code?: string
  name: string
  type?: string | number
  category?: string
  area?: string
  location: string
  status: string | number
  model?: string | null
  createdAt?: string
  warrantyUntil?: string | null
}

/** 事件 */
export interface ParkEvent {
  id: string
  code?: string
  title: string
  description: string
  area?: string
  location?: string
  severity: string | number
  status: string | number
  relatedAssetId?: string | null
  relatedAlertId?: string | null
  workOrderId?: string | null
  workOrderNumber?: string | null
  createdAt: string
  closedAt?: string | null
}

/** 公告 */
export interface Announcement {
  id: string
  title: string
  content: string
  status?: string | number | boolean
  isPublished?: boolean
  createdBy?: string
  publishedAt?: string | null
  createdAt?: string
}
