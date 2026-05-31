<template>
  <div class="page-container">
    <el-card shadow="never">
      <template #header>
        <div class="card-header">
          <span>事件列表</span>
          <el-select v-model="statusFilter" placeholder="事件状态" clearable style="width: 160px" @change="filterChange">
            <el-option label="待处理" :value="1" />
            <el-option label="已转工单" :value="2" />
            <el-option label="已关闭" :value="3" />
          </el-select>
        </div>
      </template>

      <el-table :data="list" v-loading="loading" stripe style="width: 100%">
        <el-table-column prop="code" label="事件编码" width="180" />
        <el-table-column prop="title" label="事件标题" min-width="220" />
        <el-table-column label="严重级别" width="110">
          <template #default="{ row }">
            <el-tag :type="severityTag(row.severity)" size="small">{{ severityLabel(row.severity) }}</el-tag>
          </template>
        </el-table-column>
        <el-table-column prop="area" label="区域" min-width="180" />
        <el-table-column label="状态" width="100">
          <template #default="{ row }">
            <el-tag :type="evtStatusType(row.status)" size="small">{{ evtStatusLabel(row.status) }}</el-tag>
          </template>
        </el-table-column>
        <el-table-column prop="createdAt" label="创建时间" width="180" />
        <el-table-column label="操作" width="110" fixed="right">
          <template #default="{ row }">
            <el-button v-if="!isResolved(row.status)" type="primary" size="small" @click="handleResolve(row.id)">
              处理
            </el-button>
            <span v-else class="resolved-text">已处理</span>
          </template>
        </el-table-column>
      </el-table>

      <div v-if="!loading && list.length === 0" class="empty-state">
        <el-empty description="暂无事件数据" />
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
import { getEventsApi, resolveEventApi } from '@/api/event'
import type { ParkEvent } from '@/types'
import { ElMessage } from 'element-plus'

const list = ref<ParkEvent[]>([])
const loading = ref(false)
const page = ref(1)
const pageSize = ref(10)
const total = ref(0)
const statusFilter = ref<number | undefined>()

onMounted(() => fetchData())

async function fetchData() {
  loading.value = true
  try {
    const res = await getEventsApi({ page: page.value, pageSize: pageSize.value, status: statusFilter.value })
    list.value = res.items ?? []
    total.value = res.total ?? res.totalCount ?? list.value.length
  } catch {
    list.value = []
    total.value = 0
  } finally {
    loading.value = false
  }
}

function filterChange() {
  page.value = 1
  fetchData()
}

async function handleResolve(id: string) {
  try {
    await resolveEventApi(id)
    ElMessage.success('事件已标记处理')
    fetchData()
  } catch {
    // handled by interceptor
  }
}

function normalize(value: unknown) {
  return String(value ?? '').toLowerCase()
}

function isResolved(status: unknown) {
  return normalize(status) !== '1' && normalize(status) !== 'open'
}

function severityTag(level: unknown): 'primary' | 'success' | 'warning' | 'info' | 'danger' | undefined {
  const map = {
    '1': 'info',
    '2': 'warning',
    '3': 'warning',
    '4': 'danger',
    low: 'info',
    medium: 'warning',
    high: 'warning',
    critical: 'danger',
  } as const
  return map[normalize(level) as keyof typeof map] ?? 'info'
}

function severityLabel(level: unknown) {
  const map: Record<string, string> = {
    '1': '低',
    '2': '中',
    '3': '高',
    '4': '严重',
    low: '低',
    medium: '中',
    high: '高',
    critical: '严重',
  }
  return map[normalize(level)] || String(level ?? '-')
}

function evtStatusType(status: unknown): 'primary' | 'success' | 'warning' | 'info' | 'danger' | undefined {
  const map = {
    '1': 'danger',
    '2': 'warning',
    '3': 'success',
    open: 'danger',
    workordercreated: 'warning',
    closed: 'success',
  } as const
  return map[normalize(status) as keyof typeof map] ?? 'info'
}

function evtStatusLabel(status: unknown) {
  const map: Record<string, string> = {
    '1': '待处理',
    '2': '已转工单',
    '3': '已关闭',
    open: '待处理',
    workordercreated: '已转工单',
    closed: '已关闭',
  }
  return map[normalize(status)] || String(status ?? '-')
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
.resolved-text {
  color: #67c23a;
  font-size: 13px;
}
</style>
