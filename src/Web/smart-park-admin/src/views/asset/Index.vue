<template>
  <div class="page-container">
    <el-card shadow="never">
      <template #header>
        <div class="card-header">
          <span>资产列表</span>
          <el-input
            v-model="keyword"
            placeholder="搜索资产名称"
            clearable
            style="width: 240px"
            @keyup.enter="search"
          />
        </div>
      </template>

      <el-table :data="list" v-loading="loading" stripe style="width: 100%">
        <el-table-column prop="id" label="ID" width="70" />
        <el-table-column prop="name" label="资产名称" min-width="160" />
        <el-table-column prop="type" label="类型" width="100" />
        <el-table-column prop="location" label="位置" width="160" />
        <el-table-column label="状态" width="100">
          <template #default="{ row }">
            <el-tag :type="statusType(row.status)" size="small">
              {{ statusLabel(row.status) }}
            </el-tag>
          </template>
        </el-table-column>
        <el-table-column prop="department" label="所属部门" width="120" />
        <el-table-column prop="lastMaintenance" label="最近维保" width="180" />
      </el-table>

      <div v-if="!loading && list.length === 0" class="empty-state">
        <el-empty description="暂无资产数据" />
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
import { getAssetsApi } from '@/api/asset'
import type { Asset } from '@/types'

const list = ref<Asset[]>([])
const loading = ref(false)
const page = ref(1)
const pageSize = ref(10)
const total = ref(0)
const keyword = ref('')

onMounted(() => fetchData())

async function fetchData() {
  loading.value = true
  try {
    const res = await getAssetsApi({ page: page.value, pageSize: pageSize.value, keyword: keyword.value })
    list.value = res.data.items
    total.value = res.data.total
  } catch {
    list.value = []
  } finally {
    loading.value = false
  }
}

function search() {
  page.value = 1
  fetchData()
}

function statusType(s: string): 'primary' | 'success' | 'warning' | 'info' | 'danger' | undefined {
  const map = { normal: 'success', warning: 'warning', fault: 'danger', offline: 'info' } as const
  return map[s as keyof typeof map] ?? 'info'
}

function statusLabel(s: string) {
  const map: Record<string, string> = { normal: '正常', warning: '预警', fault: '故障', offline: '离线' }
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
