<template>
  <div class="page-container">
    <el-card shadow="never">
      <template #header>
        <div class="card-header">
          <span>工单列表</span>
          <el-button type="primary" size="small" @click="handleCreate">新建工单</el-button>
        </div>
      </template>

      <el-table :data="list" v-loading="loading" stripe style="width: 100%">
        <el-table-column prop="number" label="工单编号" width="180" />
        <el-table-column prop="title" label="标题" min-width="220" />
        <el-table-column label="优先级" width="100">
          <template #default="{ row }">
            <el-tag :type="priorityType(row.priority)" size="small">
              {{ priorityLabel(row.priority) }}
            </el-tag>
          </template>
        </el-table-column>
        <el-table-column label="状态" width="100">
          <template #default="{ row }">
            <el-tag :type="statusType(row.status)" size="small">
              {{ statusLabel(row.status) }}
            </el-tag>
          </template>
        </el-table-column>
        <el-table-column label="来源" width="120">
          <template #default="{ row }">
            {{ sourceLabel(row.sourceType) }}
          </template>
        </el-table-column>
        <el-table-column label="关联对象" width="140">
          <template #default="{ row }">
            {{ relationLabel(row) }}
          </template>
        </el-table-column>
        <el-table-column prop="createdAt" label="创建时间" width="180" />
      </el-table>

      <div v-if="!loading && list.length === 0" class="empty-state">
        <el-empty description="暂无工单数据" />
      </div>

      <div class="pagination-wrap" v-if="total > 0">
        <el-pagination
          v-model:current-page="page"
          :page-size="pageSize"
          :total="total"
          layout="prev, pager, next"
          @current-change="fetchData"
        />
      </div>
    </el-card>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { getWorkOrdersApi } from '@/api/workOrder'
import type { WorkOrder } from '@/types'
import { ElMessage } from 'element-plus'

const list = ref<WorkOrder[]>([])
const loading = ref(false)
const page = ref(1)
const pageSize = ref(10)
const total = ref(0)

onMounted(() => fetchData())

async function fetchData() {
  loading.value = true
  try {
    const res = await getWorkOrdersApi({ page: page.value, pageSize: pageSize.value })
    list.value = res.items ?? []
    total.value = res.total ?? res.totalCount ?? list.value.length
  } catch {
    list.value = []
    total.value = 0
  } finally {
    loading.value = false
  }
}

function handleCreate() {
  ElMessage.info('新建工单功能待对接后端接口')
}

function normalize(value: unknown) {
  return String(value ?? '').toLowerCase()
}

function priorityType(priority: unknown): 'primary' | 'success' | 'warning' | 'info' | 'danger' | undefined {
  const map = {
    '1': 'info',
    '2': undefined,
    '3': 'warning',
    '4': 'danger',
    low: 'info',
    medium: undefined,
    high: 'warning',
    critical: 'danger',
    urgent: 'danger',
  } as const
  return map[normalize(priority) as keyof typeof map] ?? 'info'
}

function priorityLabel(priority: unknown) {
  const map: Record<string, string> = {
    '1': '低',
    '2': '中',
    '3': '高',
    '4': '紧急',
    low: '低',
    medium: '中',
    high: '高',
    critical: '紧急',
    urgent: '紧急',
  }
  return map[normalize(priority)] || String(priority ?? '-')
}

function statusType(status: unknown): 'primary' | 'success' | 'warning' | 'info' | 'danger' | undefined {
  const map = {
    '1': 'info',
    '2': 'primary',
    '3': 'primary',
    '4': 'warning',
    '5': 'warning',
    '6': 'success',
    '7': 'success',
    '8': 'success',
    created: 'info',
    dispatched: 'primary',
    accepted: 'primary',
    arrived: 'warning',
    inprogress: 'warning',
    completed: 'success',
    verified: 'success',
    closed: 'success',
  } as const
  return map[normalize(status) as keyof typeof map] ?? 'info'
}

function statusLabel(status: unknown) {
  const map: Record<string, string> = {
    '1': '待派发',
    '2': '已派发',
    '3': '已接单',
    '4': '已到场',
    '5': '处理中',
    '6': '已完成',
    '7': '已核验',
    '8': '已关闭',
    created: '待派发',
    dispatched: '已派发',
    accepted: '已接单',
    arrived: '已到场',
    inprogress: '处理中',
    completed: '已完成',
    verified: '已核验',
    closed: '已关闭',
  }
  return map[normalize(status)] || String(status ?? '-')
}

function sourceLabel(sourceType: unknown) {
  const map: Record<string, string> = {
    '1': '手工创建',
    '2': '事件触发',
    '3': '告警触发',
    manual: '手工创建',
    event: '事件触发',
    alert: '告警触发',
  }
  return map[normalize(sourceType)] || String(sourceType ?? '-')
}

function relationLabel(row: WorkOrder) {
  const eventId = row.relatedEventId ?? row.eventId
  const assetId = row.relatedAssetId ?? row.assetId
  if (eventId) {
    return `事件 #${eventId}`
  }
  if (assetId) {
    return `资产 #${assetId}`
  }
  return '-'
}
</script>

<style scoped>
.page-container {
  display: flex;
  flex-direction: column;
  gap: 16px;
}
.card-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
}
.pagination-wrap {
  margin-top: 16px;
  display: flex;
  justify-content: flex-end;
}
.empty-state {
  padding: 40px 0;
}
</style>
