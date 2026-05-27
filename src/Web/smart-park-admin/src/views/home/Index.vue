<template>
  <div class="home-page">
    <el-row :gutter="16">
      <el-col :span="6" v-for="item in stats" :key="item.label">
        <el-card shadow="hover" class="stat-card">
          <div class="stat-value">{{ item.value }}</div>
          <div class="stat-label">{{ item.label }}</div>
        </el-card>
      </el-col>
    </el-row>

    <el-card class="section-card" shadow="never">
      <template #header>快捷操作</template>
      <el-space wrap>
        <el-button type="primary" @click="$router.push('/work-order')">工单管理</el-button>
        <el-button type="success" @click="$router.push('/asset')">资产管理</el-button>
        <el-button type="warning" @click="$router.push('/event')">事件管理</el-button>
        <el-button type="info" @click="$router.push('/announcement')">公告管理</el-button>
      </el-space>
    </el-card>

    <el-card shadow="never">
      <template #header>系统信息</template>
      <el-descriptions :column="2" border>
        <el-descriptions-item label="系统版本">v1.0.0</el-descriptions-item>
        <el-descriptions-item label="运行环境">{{ envMode }}</el-descriptions-item>
        <el-descriptions-item label="API 地址">{{ apiBase }}</el-descriptions-item>
        <el-descriptions-item label="登录用户">{{ authStore.user?.displayName || authStore.user?.username || '-' }}</el-descriptions-item>
      </el-descriptions>
    </el-card>
  </div>
</template>

<script setup lang="ts">
import { useAuthStore } from '@/stores/auth'

const authStore = useAuthStore()
const envMode = import.meta.env.MODE
const apiBase = import.meta.env.VITE_API_BASE || '/gateway'

const stats = [
  { label: '待处理工单', value: 0 },
  { label: '运行中资产', value: 0 },
  { label: '未处理事件', value: 0 },
  { label: '有效公告', value: 0 },
]
</script>

<style scoped>
.home-page {
  display: flex;
  flex-direction: column;
  gap: 16px;
}
.stat-card {
  text-align: center;
}
.stat-value {
  font-size: 32px;
  font-weight: 700;
  color: #409eff;
}
.stat-label {
  font-size: 14px;
  color: #909399;
  margin-top: 4px;
}
.section-card {
  margin-top: 4px;
}
</style>
