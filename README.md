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

默认演示账号（运行 `dotnet run --project src/Tools/SmartPark.DbSeeder` 后生成）：

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

如果你本地曾使用旧版 `EnsureCreated` 生成过库结构，建议先删除对应旧库或清理 PostgreSQL volume，再执行迁移。

### 8.2 执行数据库迁移

当前 5 个服务在启动时都会自动执行 `Database.MigrateAsync()`。
如果你希望在启动服务前显式完成迁移，可以分别执行：

```bash
dotnet ef database update --project src/Services/Identity/SmartPark.Identity.Infrastructure/SmartPark.Identity.Infrastructure.csproj --context SmartPark.Identity.Infrastructure.IdentityDbContext
dotnet ef database update --project src/Services/Asset/SmartPark.Asset.Infrastructure/SmartPark.Asset.Infrastructure.csproj --context SmartPark.Asset.Infrastructure.AssetDbContext
dotnet ef database update --project src/Services/IoTPlatform/SmartPark.IoTPlatform.Infrastructure/SmartPark.IoTPlatform.Infrastructure.csproj --context SmartPark.IoTPlatform.Infrastructure.IoTPlatformDbContext
dotnet ef database update --project src/Services/Collaboration/SmartPark.Collaboration.Infrastructure/SmartPark.Collaboration.Infrastructure.csproj --context SmartPark.Collaboration.Infrastructure.CollaborationDbContext
dotnet ef database update --project src/Services/WorkOrder/SmartPark.WorkOrder.Infrastructure/SmartPark.WorkOrder.Infrastructure.csproj --context SmartPark.WorkOrder.Infrastructure.WorkOrderDbContext
```

### 8.3 生成演示数据

统一 Seeder 项目负责生成默认账号、资产、监测点、告警、事件、公告和示例工单：

```bash
dotnet run --project src/Tools/SmartPark.DbSeeder
```

如需只生成部分模块，可传入模块参数，例如：

```bash
dotnet run --project src/Tools/SmartPark.DbSeeder -- --modules=identity,asset
```

可用模块：`identity`、`asset`、`iotplatform`（或 `iot`）、`collaboration`、`workorder`。
选择 `workorder` 或 `collaboration` 时，会自动补齐依赖模块的种子数据。

### 8.4 端口约定

本地开发默认使用以下端口：

- `15000`：SmartPark.AppHost 自身
- `5100`：SmartPark.ApiGateway
- `5101`：SmartPark.Identity.Api
- `5102`：SmartPark.Asset.Api
- `5103`：SmartPark.IoTPlatform.Api
- `5104`：SmartPark.WorkOrder.Api
- `5105`：SmartPark.Collaboration.Api

### 8.5 启动方式说明

后端有两种启动方式，但**不要同时使用**：

1. **启动 AppHost**：由 Aspire 统一编排网关和各微服务。
2. **分别启动 6 个后端项目**：适合在 Rider / Visual Studio 中多启动调试。

如果你使用 Rider / Visual Studio 自定义运行配置，不要把裸端口写进 `ASPNETCORE_URLS`，例如 `ASPNETCORE_URLS=5101` 是错误的。
如需传端口，请使用完整 URL，或改用 `ASPNETCORE_HTTP_PORTS`。

### 8.6 启动整个后端（推荐）

```bash
dotnet run --project src/SmartPark.AppHost --launch-profile http
```

说明：

- AppHost 自身监听 `http://localhost:15000`
- Gateway 启动后监听 `http://localhost:5100`
- 不要再额外单独启动 `SmartPark.ApiGateway`

### 8.7 分别启动后端项目（替代方式）

如果你不使用 AppHost，可以分别启动以下 6 个项目：

```bash
dotnet run --project src/Gateway/SmartPark.ApiGateway/SmartPark.ApiGateway.csproj
dotnet run --project src/Services/Identity/SmartPark.Identity.Api/SmartPark.Identity.Api.csproj
dotnet run --project src/Services/Asset/SmartPark.Asset.Api/SmartPark.Asset.Api.csproj
dotnet run --project src/Services/IoTPlatform/SmartPark.IoTPlatform.Api/SmartPark.IoTPlatform.Api.csproj
dotnet run --project src/Services/WorkOrder/SmartPark.WorkOrder.Api/SmartPark.WorkOrder.Api.csproj
dotnet run --project src/Services/Collaboration/SmartPark.Collaboration.Api/SmartPark.Collaboration.Api.csproj
```

这 6 个项目可以同时启动，但此时不要再启动 `SmartPark.AppHost`。

### 8.8 启动前端

首次运行先安装依赖：

```bash
npm --prefix src/Web/smart-park-admin install
```

启动前端开发服务器：

```bash
npm --prefix src/Web/smart-park-admin run dev -- --host 127.0.0.1
```

说明：

- 前端默认通过 Vite 代理把 `/api` 转发到 `http://localhost:5100`
- Vite 默认端口通常是 `5173`，如果被占用会自动递增
- 启动前请先确认 Gateway 已经可访问

### 8.9 快速验证

后端启动后，可先验证 Gateway：

```powershell
Invoke-WebRequest http://localhost:5100/
```

预期返回：

```json
{"name":"SmartPark Gateway","version":"1.0.0"}
```

然后启动前端，并使用默认账号登录：

- `admin / SmartPark@123`
- `dispatcher / SmartPark@123`
- `operator / SmartPark@123`
- `reviewer / SmartPark@123`

### 8.10 常见问题排查

#### 8.10.1 `5100 already in use`

这通常是以下两种情况之一：

- 你同时启动了 `SmartPark.AppHost` 和 `SmartPark.ApiGateway`
- Aspire 残留的 `dcp.exe` 仍在占用 `5100`

可先清理残留进程：

```powershell
Get-Process dcp -ErrorAction SilentlyContinue | Stop-Process -Force
```

然后重新按“二选一”的方式启动后端。

#### 8.10.2 `Invalid url: '5101'`

这通常表示某个运行配置把裸端口写进了 `ASPNETCORE_URLS`，例如：

```text
ASPNETCORE_URLS=5101
```

这是无效配置。正确做法是：

- 使用完整 URL，例如 `http://localhost:5101`
- 或使用 `ASPNETCORE_HTTP_PORTS=5101`

#### 8.10.3 前端能打开但没有数据

请依次确认：

1. `http://localhost:5100/` 可以访问
2. 前端开发服务器已经启动
3. `src/Web/smart-park-admin/.env.development` 中的代理目标仍是 `http://localhost:5100`
4. 默认演示数据已经通过 `SmartPark.DbSeeder` 写入数据库

### 8.11 单独构建

```bash
dotnet build SmartPark.sln
```

### 8.12 运行测试

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
- 每服务独立 PostgreSQL 数据库
- EF Core Migration 管理
- 统一 .NET Seeder 演示数据入口
- 基础前端可视化

暂未深做的部分：

- Redis 业务缓存落地（当前仅接入共享缓存基础设施）
- 消息总线
- 分布式事务
- 真正的第三方平台对接
- 完整设备控制协议接入
- 复杂前端权限系统

## 10. 建议的下一步演进

建议你后续继续补这几块：

1. 为数据库迁移补充 CI/CD / 发布流程约束
2. 增加 Redis 缓存与分布式锁的实际使用场景
3. 引入消息总线处理告警转事件、事件转工单
4. 为工单增加图片上传和回单能力
5. 给资产和事件补地图坐标字段
6. 完善前端的角色菜单和统计图表

## 11. 注释说明

本次已对关键入口、核心领域模型和主要端点补充中文注释，便于你后续阅读和继续扩展。

如果出现编辑器乱码，请确认 IDE / 编辑器使用 **UTF-8** 打开文件。
