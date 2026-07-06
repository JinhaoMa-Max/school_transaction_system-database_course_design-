<script setup lang="ts">
import { useRouter, useRoute } from 'vue-router'
import { useUserStore } from '@/stores'

const router = useRouter()
const route = useRoute()
const userStore = useUserStore()

const menuItems = [
  { path: '/admin/dashboard', name: '仪表盘' },
  { path: '/admin/users', name: '用户管理' },
  { path: '/admin/goods', name: '商品管理' },
  { path: '/admin/orders', name: '订单管理' },
  { path: '/admin/reports', name: '举报管理' },
  { path: '/admin/notices', name: '公告管理' },
  { path: '/admin/audit-logs', name: '审核日志' }
]

const handleNav = (path: string) => {
  router.push(path)
}

const handleLogout = () => {
  userStore.logout()
  router.push('/login')
}
</script>

<template>
  <div class="admin-layout">
    <aside class="sidebar">
      <h2>后台管理</h2>
      <nav>
        <div 
          v-for="item in menuItems" 
          :key="item.path" 
          class="menu-item"
          :class="{ active: route.path === item.path }"
          @click="handleNav(item.path)"
        >
          {{ item.name }}
        </div>
      </nav>
      <button class="logout-btn" @click="handleLogout">退出登录</button>
    </aside>
    <main class="main-content">
      <RouterView />
    </main>
  </div>
</template>

<style scoped>
.admin-layout {
  display: flex;
  height: 100vh;
}

.sidebar {
  width: 200px;
  background: #1f1f1f;
  color: white;
  padding: 20px;
  display: flex;
  flex-direction: column;
}

.sidebar h2 {
  margin: 0 0 24px 0;
  font-size: 18px;
}

.menu-item {
  padding: 12px;
  margin-bottom: 8px;
  border-radius: 4px;
  cursor: pointer;
}

.menu-item:hover {
  background: #333;
}

.menu-item.active {
  background: #165dff;
}

.logout-btn {
  margin-top: auto;
  padding: 12px;
  background: #ff4d4f;
  color: white;
  border: none;
  border-radius: 4px;
  cursor: pointer;
}

.main-content {
  flex: 1;
  padding: 20px;
  overflow-y: auto;
}
</style>
