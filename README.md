# SmartPark 智慧公园微服务示例项目

## 1. 项目简介

这是一个基于 **.NET 10** 搭建的智慧公园微服务示例工程，重点放在后端领域拆分、工单流转闭环、事件协同和资产关联上。

项目目标：

- 演示一个适合学习的微服务分层结构
- 通过 PostgreSQL + Redis 构建基础运行环境
- 以工单流转为核心串联物联、事件、资产和协同模块
- 提供一个可运行的极简前端管理端用于查看数据

## 2. 技术栈

### 后端

- .NET 10
- ASP.NET Core Minimal API
- Entity Framework Core
- PostgreSQL
- Redis
- YARP 反向代理网关
- .NET Aspire AppHost（本地服务编排）
- JWT 鉴权

### 前端

- Vue 3
- TypeScript
- Vite
- Element Plus

## 3. 项目目录结构

```text
smart-park/
├─ src/
│  ├─ SmartPark.AppHost/                # 本地开发编排入口
│  ├─ SmartPark.ServiceDefaults/        # 服务默认配置
│  ├─ Gateway/
│  │  └─ SmartPark.ApiGateway/          # 网关
│  ├─ BuildingBlocks/
│  │  ├─ SmartPark.SharedKernel/        # 领域基类与通用异常
│  │  ├─ SmartPark.SharedContracts/     # 共享契约
│  │  └─ SmartPark.SharedInfrastructure/# 共享基础设施
│  ├─ Services/
│  │  ├─ Identity/                      # 身份与权限服务
│  │  ├─ Asset/                         # 资产台账服务
│  │  ├─ IoTPlatform/                   # 物联与总览服务
│  │  ├─ WorkOrder/                     # 工单服务
│  │  └─ Collaboration/                 # 事件与公告协同服务
│  └─ Web/
│     └─ smart-park-admin/              # 极简管理前端
├─ deploy/
│  └─ docker-compose.infrastructure.yml # PostgreSQL / Redis 基础设施编排
├─ tests/                               # 单元测试与集成测试
└─ 核心功能模块.md                      # 原始业务模块说明
```

## 4. 微服务拆分说明

### 4.1 Identity 服务

职责：

- 登录认证
- JWT 签发
- 用户、角色、权限查询
- 为其他服务提供统一身份凭证

默认内置角色：

- Admin
- Dispatcher
- Operator
- Reviewer

### 4.2 Asset 服务

职责：

- 管理设备、基础设施、植物三类资产
- 支撑工单、事件、物联点位的资产关联
- 提供分页查询、详情、创建和更新接口

### 4.3 IoTPlatform 服务

职责：

- 管理监测点
- 管理告警记录
- 提供总览数据
- 提供灌溉控制命令接口占位

### 4.4 WorkOrder 服务

职责：

- 工单创建
- 派单
- 接单
- 到场
- 开始处理
- 完工
- 核验
- 关闭
- 工单时间线追踪

这是当前项目的核心业务服务。

### 4.5 Collaboration 服务

职责：

- 事件台账管理
- 事件关闭
- 事件转工单
- 公告管理

该服务承担“事件协同中台”的角色。

## 5. 工单流转闭环

当前项目补充了原文档中未明确展开的工单处理流转模块，默认状态机如下：

```text
Created
  -> Dispatched
  -> Accepted
  -> Arrived
  -> InProgress
  -> Completed
  -> Verified
  -> Closed
```

说明：

- 每次状态变化都会记录操作日志
- 不允许跳状态流转
- 工单可关联资产、事件、告警
- 工单可挂接附件信息

## 6. 模块联动关系

核心联动链路如下：

```text
IoTPlatform 告警/监测
    -> Collaboration 事件台账
    -> WorkOrder 工单处置
    -> Asset 资产定位与关联
    -> 前端管理端展示
```

已实现的代表性联动能力：

- 事件转工单
- 工单关联资产
- 告警与事件、工单链路预留
- 公告对前端展示开放

## 7. 鉴权设计

系统采用 JWT 鉴权。

- Identity 服务负责签发 Token
- 其他微服务负责校验 Token
- 角色控制接口访问权限
- Token 中包含角色和权限码，便于前端显示菜单和按钮

默认种子账号：

- `admin / SmartPark@123`
- `dispatcher / SmartPark@123`
- `operator / SmartPark@123`
- `reviewer / SmartPark@123`

### 7.1 异常处理机制

当前版本已经补齐统一异常捕获链路，后端服务遵循以下约定：

- 所有微服务和网关统一启用全局异常处理中间件
- 统一输出 `ProblemDetails` 风格错误响应
- 错误响应中固定包含：
  - `status`
  - `title`
  - `detail`
  - `code`
  - `message`
  - `traceId`
  - `details`（仅请求校验失败等场景按需出现）
- `401 unauthorized`：Token 缺失、无效或已过期
- `401 authentication_failed`：登录用户名或密码错误
- `403 forbidden`：已认证但无权限
- `404 resource_not_found`：资源不存在
- `422 validation_failed`：请求已完成反序列化，但字段或业务输入不满足规则
- `DomainException` 用于表达业务规则错误，如工单状态流转冲突
- `IntegrationException` 用于表达跨服务调用失败、下游异常或响应不可解析
- 协同服务调用工单服务失败时，不再把所有失败伪装成 `404`

一个典型业务冲突响应示例如下：

```json
{
  "type": "https://httpstatuses.com/409",
  "title": "业务状态冲突",
  "status": 409,
  "detail": "工单当前状态为 Dispatched，不能执行期望状态 Arrived 的流转。",
  "code": "work_order_invalid_status_transition",
  "message": "工单当前状态为 Dispatched，不能执行期望状态 Arrived 的流转。",
  "traceId": "00-..."
}
```

登录失败时会返回 `401 + authentication_failed`，示例如下：

```json
{
  "type": "https://httpstatuses.com/401",
  "title": "身份验证失败",
  "status": 401,
  "detail": "用户名或密码错误。",
  "code": "authentication_failed",
  "message": "用户名或密码错误。",
  "traceId": "00-..."
}
```

资源不存在时会返回 `404 + resource_not_found`，示例如下：

```json
{
  "type": "https://httpstatuses.com/404",
  "title": "资源不存在",
  "status": 404,
  "detail": "未找到工单 00000000-0000-0000-0000-000000000000。",
  "code": "resource_not_found",
  "message": "未找到工单 00000000-0000-0000-0000-000000000000。",
  "traceId": "00-..."
}
```

请求校验失败时会返回 `422 + validation_failed`，示例如下：

```json
{
  "type": "https://httpstatuses.com/422",
  "title": "业务规则校验失败",
  "status": 422,
  "detail": "请求校验失败。",
  "code": "validation_failed",
  "message": "请求校验失败。",
  "traceId": "00-...",
  "details": {
    "userName": ["用户名不能为空。"],
    "password": ["密码不能为空。"]
  }
}
```

## 8. 本地运行方式

### 8.1 启动基础设施

先确保 Docker Desktop 已启动，然后执行：

```bash
docker compose -f deploy/docker-compose.infrastructure.yml up -d
```

### 8.2 启动整个后端

```bash
dotnet run --project src/SmartPark.AppHost
```

### 8.3 单独构建

```bash
dotnet build SmartPark.sln
```

### 8.4 运行测试

```bash
dotnet test SmartPark.sln
```

## 9. 当前实现范围

当前版本更偏向“学习型微服务样板”，重点是：

- 服务拆分
- 分层结构
- JWT 鉴权
- 工单状态机
- 事件协同
- 基础前端可视化

暂未深做的部分：

- 正式 EF Core Migration 管理
- 消息总线
- 分布式事务
- 真正的第三方平台对接
- 完整设备控制协议接入
- 复杂前端权限系统

## 10. 建议的下一步演进

建议你后续继续补这几块：

1. 把 `EnsureCreated` 切换为正式 Migration
2. 增加 Redis 缓存与分布式锁的实际使用场景
3. 引入消息总线处理告警转事件、事件转工单
4. 为工单增加图片上传和回单能力
5. 给资产和事件补地图坐标字段
6. 完善前端的角色菜单和统计图表

## 11. 注释说明

本次已对关键入口、核心领域模型和主要端点补充中文注释，便于你后续阅读和继续扩展。

如果出现编辑器乱码，请确认 IDE / 编辑器使用 **UTF-8** 打开文件。
