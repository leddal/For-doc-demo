<template>
  <div class="page-container">
    <el-card shadow="never">
      <template #header>
        <div class="card-header">
          <span>事件列表</span>
          <el-select v-model="typeFilter" placeholder="事件类型" clearable style="width: 140px" @change="filterChange">
            <el-option label="火灾" value="fire" />
            <el-option label="入侵" value="intrusion" />
            <el-option label="设备" value="device" />
            <el-option label="环境" value="environment" />
            <el-option label="其他" value="other" />
          </el-select>
        </div>
      </template>

      <el-table :data="list" v-loading="loading" stripe style="width: 100%">
        <el-table-column prop="id" label="ID" width="70" />
        <el-table-column prop="title" label="事件标题" min-width="180" />
        <el-table-column label="类型" width="90">
          <template #default="{ row }">
            <el-tag :type="typeTag(row.type)" size="small">{{ typeLabel(row.type) }}</el-tag>
          </template>
        </el-table-column>
        <el-table-column label="级别" width="80">
          <template #default="{ row }">
            <el-tag :type="levelTag(row.level)" size="small">{{ row.level }}</el-tag>
          </template>
        </el-table-column>
        <el-table-column prop="source" label="来源" width="120" />
        <el-table-column label="状态" width="100">
          <template #default="{ row }">
            <el-tag :type="evtStatusType(row.status)" size="small">{{ evtStatusLabel(row.status) }}</el-tag>
          </template>
        </el-table-column>
        <el-table-column prop="happenedAt" label="发生时间" width="180" />
        <el-table-column label="操作" width="110" fixed="right">
          <template #default="{ row }">
            <el-button v-if="row.status !== 'resolved'" type="primary" size="small" @click="handleResolve(row.id)">
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
const typeFilter = ref('')

onMounted(() => fetchData())

async function fetchData() {
  loading.value = true
  try {
    const res = await getEventsApi({ page: page.value, pageSize: pageSize.value, type: typeFilter.value || undefined })
    list.value = res.data.items
    total.value = res.data.total
  } catch {
    list.value = []
  } finally {
    loading.value = false
  }
}

function filterChange() {
  page.value = 1
  fetchData()
}

async function handleResolve(id: number) {
  try {
    await resolveEventApi(id)
    ElMessage.success('事件已标记处理')
    fetchData()
  } catch {
    // handled by interceptor
  }
}

function typeTag(t: string): 'primary' | 'success' | 'warning' | 'info' | 'danger' | undefined {
  const map = { fire: 'danger', intrusion: 'warning', device: undefined, environment: 'success', other: 'info' } as const
  return map[t as keyof typeof map] ?? 'info'
}
function typeLabel(t: string) {
  const map: Record<string, string> = { fire: '火灾', intrusion: '入侵', device: '设备', environment: '环境', other: '其他' }
  return map[t] || t
}
function levelTag(l: string): 'primary' | 'success' | 'warning' | 'info' | 'danger' | undefined {
  const map = { info: 'info', warning: 'warning', critical: 'danger' } as const
  return map[l as keyof typeof map] ?? 'info'
}
function evtStatusType(s: string): 'primary' | 'success' | 'warning' | 'info' | 'danger' | undefined {
  const map = { unprocessed: 'danger', processing: 'warning', resolved: 'success' } as const
  return map[s as keyof typeof map] ?? 'info'
}
function evtStatusLabel(s: string) {
  const map: Record<string, string> = { unprocessed: '未处理', processing: '处理中', resolved: '已处理' }
  return map[s] || s
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
