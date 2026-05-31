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
        <el-table-column label="资产编码" width="160">
          <template #default="{ row }">
            {{ row.assetCode || row.code || '-' }}
          </template>
        </el-table-column>
        <el-table-column prop="name" label="资产名称" min-width="180" />
        <el-table-column label="分类" width="120">
          <template #default="{ row }">
            {{ assetTypeLabel(row.type ?? row.category) }}
          </template>
        </el-table-column>
        <el-table-column prop="location" label="位置" min-width="180" />
        <el-table-column label="状态" width="110">
          <template #default="{ row }">
            <el-tag :type="statusType(row.status)" size="small">
              {{ statusLabel(row.status) }}
            </el-tag>
          </template>
        </el-table-column>
        <el-table-column label="创建时间" width="180">
          <template #default="{ row }">
            {{ row.createdAt || '-' }}
          </template>
        </el-table-column>
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
    const res = await getAssetsApi({ page: page.value, pageSize: pageSize.value, keyword: keyword.value || undefined })
    list.value = res.items ?? []
    total.value = res.total ?? res.totalCount ?? list.value.length
  } catch {
    list.value = []
    total.value = 0
  } finally {
    loading.value = false
  }
}

function search() {
  page.value = 1
  fetchData()
}

function normalize(value: unknown) {
  return String(value ?? '').toLowerCase()
}

function assetTypeLabel(type: unknown) {
  const map: Record<string, string> = {
    '1': '设备',
    '2': '基础设施',
    '3': '植物',
    device: '设备',
    infrastructure: '基础设施',
    plant: '植物',
  }
  return map[normalize(type)] || String(type ?? '-')
}

function statusType(s: unknown): 'primary' | 'success' | 'warning' | 'info' | 'danger' | undefined {
  const map = {
    '1': 'success',
    '2': 'warning',
    '3': 'info',
    active: 'success',
    maintenance: 'warning',
    undermaintenance: 'warning',
    offline: 'info',
    inactive: 'info',
    retired: 'danger',
  } as const
  return map[normalize(s) as keyof typeof map] ?? 'info'
}

function statusLabel(s: unknown) {
  const map: Record<string, string> = {
    '1': '在用',
    '2': '维护中',
    '3': '离线',
    active: '在用',
    maintenance: '维护中',
    undermaintenance: '维护中',
    offline: '离线',
    inactive: '停用',
    retired: '已报废',
  }
  return map[normalize(s)] || String(s ?? '-')
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
