import { createRouter, createWebHashHistory } from 'vue-router'
import type { RouteRecordRaw } from 'vue-router'

const routes: RouteRecordRaw[] = [
  {
    path: '/login',
    name: 'Login',
    component: () => import('@/views/login/Index.vue'),
  },
  {
    path: '/',
    component: () => import('@/layouts/MainLayout.vue'),
    redirect: '/home',
    children: [
      {
        path: 'home',
        name: 'Home',
        component: () => import('@/views/home/Index.vue'),
        meta: { title: '首页' },
      },
      {
        path: 'work-order',
        name: 'WorkOrder',
        component: () => import('@/views/work-order/Index.vue'),
        meta: { title: '工单管理' },
      },
      {
        path: 'asset',
        name: 'Asset',
        component: () => import('@/views/asset/Index.vue'),
        meta: { title: '资产管理' },
      },
      {
        path: 'event',
        name: 'Event',
        component: () => import('@/views/event/Index.vue'),
        meta: { title: '事件管理' },
      },
      {
        path: 'announcement',
        name: 'Announcement',
        component: () => import('@/views/announcement/Index.vue'),
        meta: { title: '公告管理' },
      },
    ],
  },
]

const router = createRouter({
  history: createWebHashHistory(),
  routes,
})

router.beforeEach((to) => {
  const token = localStorage.getItem('token')
  if (to.name !== 'Login' && !token) {
    return { name: 'Login' }
  }
})

export default router
