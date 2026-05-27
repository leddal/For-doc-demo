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
        <el-table-column prop="id" label="ID" width="70" />
        <el-table-column prop="title" label="标题" min-width="180" />
        <el-table-column label="优先级" width="90">
          <template #default="{ row }">
            <el-tag :type="priorityType(row.priority)" size="small">
              {{ row.priority }}
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
        <el-table-column prop="assignee" label="负责人" width="120" />
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
    list.value = res.data.items
    total.value = res.data.total
  } catch {
    list.value = []
  } finally {
    loading.value = false
  }
}

function handleCreate() {
  ElMessage.info('新建工单功能待对接后端接口')
}

function priorityType(p: string): 'primary' | 'success' | 'warning' | 'info' | 'danger' | undefined {
  const map = { low: 'info', medium: undefined, high: 'warning', urgent: 'danger' } as const
  return map[p as keyof typeof map] ?? 'info'
}

function statusType(s: string): 'primary' | 'success' | 'warning' | 'info' | 'danger' | undefined {
  const map = { pending: 'info', processing: 'warning', completed: 'success', cancelled: 'danger' } as const
  return map[s as keyof typeof map] ?? 'info'
}

function statusLabel(s: string) {
  const map: Record<string, string> = { pending: '待处理', processing: '处理中', completed: '已完成', cancelled: '已取消' }
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
</style>
