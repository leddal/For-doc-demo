<template>
  <div class="page-container">
    <el-card shadow="never">
      <template #header>公告列表</template>

      <el-table :data="list" v-loading="loading" stripe style="width: 100%">
        <el-table-column prop="id" label="ID" width="70" />
        <el-table-column prop="title" label="标题" min-width="200" />
        <el-table-column label="优先级" width="90">
          <template #default="{ row }">
            <el-tag :type="priorityTag(row.priority)" size="small">{{ row.priority }}</el-tag>
          </template>
        </el-table-column>
        <el-table-column prop="publisher" label="发布人" width="120" />
        <el-table-column prop="publishedAt" label="发布时间" width="180" />
        <el-table-column label="操作" width="80" fixed="right">
          <template #default="{ row }">
            <el-button type="primary" size="small" link @click="handleView(row)">查看</el-button>
          </template>
        </el-table-column>
      </el-table>

      <div v-if="!loading && list.length === 0" class="empty-state">
        <el-empty description="暂无公告数据" />
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

    <!-- 查看公告弹窗 -->
    <el-dialog v-model="dialogVisible" title="公告详情" width="640px">
      <h3 style="margin-top: 0">{{ current?.title }}</h3>
      <div class="meta-row">
        <span>发布人：{{ current?.publisher }}</span>
        <span>时间：{{ current?.publishedAt }}</span>
      </div>
      <el-divider />
      <div class="content-text">{{ current?.content }}</div>
    </el-dialog>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { getAnnouncementsApi } from '@/api/announcement'
import type { Announcement } from '@/types'

const list = ref<Announcement[]>([])
const loading = ref(false)
const page = ref(1)
const pageSize = ref(10)
const total = ref(0)

const dialogVisible = ref(false)
const current = ref<Announcement>()

onMounted(() => fetchData())

async function fetchData() {
  loading.value = true
  try {
    const res = await getAnnouncementsApi({ page: page.value, pageSize: pageSize.value })
    list.value = res.data.items
    total.value = res.data.total
  } catch {
    list.value = []
  } finally {
    loading.value = false
  }
}

function handleView(row: Announcement) {
  current.value = row
  dialogVisible.value = true
}

function priorityTag(p: string): 'primary' | 'success' | 'warning' | 'info' | 'danger' | undefined {
  const map = { low: 'info', normal: undefined, high: 'danger' } as const
  return map[p as keyof typeof map] ?? 'info'
}
</script>

<style scoped>
.page-container {
  display: flex;
  flex-direction: column;
  gap: 16px;
}
.pagination-wrap {
  margin-top: 16px;
  display: flex;
  justify-content: flex-end;
}
.empty-state {
  padding: 40px 0;
}
.meta-row {
  display: flex;
  gap: 24px;
  color: #909399;
  font-size: 13px;
}
.content-text {
  white-space: pre-wrap;
  line-height: 1.8;
  color: #303133;
}
</style>
