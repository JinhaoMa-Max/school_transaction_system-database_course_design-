<script setup lang="ts">
import { useRoute, useRouter } from 'vue-router'
import { useUserStore } from '@/stores'
import { IconHome } from '@arco-design/web-vue/es/icon'

const route = useRoute()
const router = useRouter()
const userStore = useUserStore()

const navItems = [
  { label: '商品广场', path: '/goods' },
  { label: '发布商品', path: '/goods/publish', requiresAuth: true },
  { label: '我的商品', path: '/my/goods', requiresAuth: true },
  { label: '我的订单', path: '/orders', requiresAuth: true },
  { label: '我的收藏', path: '/favorites', requiresAuth: true },
  { label: '议价管理', path: '/bargains', requiresAuth: true },
  { label: '聊天', path: '/chat', requiresAuth: true },
  { label: '个人中心', path: '/profile', requiresAuth: true }
]

const isActive = (path: string) => {
  if (path === '/goods') {
    return route.path === '/goods' || /^\/goods\/\d+$/.test(route.path)
  }

  if (path === '/orders') {
    return route.path === '/orders' || route.path.startsWith('/orders/')
  }

  if (path === '/profile') {
    return route.path === '/profile' || route.path === '/profile/edit' || route.path === '/student-auth'
  }

  return route.path === path
}

const handleLogout = async () => {
  await userStore.logout()
  router.push('/login')
}
</script>

<template>
  <header class="app-header-wrap">
    <a-card class="app-header-card" :bordered="false">
      <div class="app-header">
        <button
          class="brand"
          type="button"
          title="返回首页"
          aria-label="校园二手交易平台，返回首页"
          @click="router.push('/')"
        >
          <span>校园二手交易平台</span>
          <IconHome class="brand-home-icon" />
        </button>

        <nav class="app-nav" aria-label="主要导航">
          <a-button
            v-for="item in navItems"
            v-show="!item.requiresAuth || userStore.isLoggedIn"
            :key="item.path"
            type="text"
            :class="{ active: isActive(item.path) }"
            @click="router.push(item.path)"
          >
            {{ item.label }}
          </a-button>

          <a-button
            v-if="userStore.isAdmin"
            type="text"
            :class="{ active: route.path.startsWith('/admin') }"
            @click="router.push('/admin')"
          >
            后台管理
          </a-button>

          <a-button
            v-if="userStore.isLoggedIn"
            type="text"
            status="danger"
            @click="handleLogout"
          >
            退出登录
          </a-button>

          <a-button v-else type="primary" @click="router.push('/login')">
            登录
          </a-button>
        </nav>
      </div>
    </a-card>
  </header>
</template>

<style scoped>
.app-header-wrap {
  position: sticky;
  top: 0;
  z-index: 100;
  padding: 20px 20px 0;
  background: rgba(255, 255, 255, 0.9);
  backdrop-filter: blur(12px);
}

.app-header-card {
  max-width: 1180px;
  margin: 0 auto;
  border-radius: 20px;
  box-shadow: 0 8px 24px rgba(45, 54, 142, 0.08);
}

.app-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 24px;
}

.brand {
  flex: none;
  display: inline-flex;
  align-items: center;
  gap: 10px;
  padding: 0;
  border: 0;
  background: transparent;
  color: #253554;
  font-family: "PingFang SC", "Microsoft YaHei", sans-serif;
  font-size: 35px;
  font-weight: 800;
  letter-spacing: 1.5px;
  cursor: pointer;
}

.brand-home-icon {
  flex: none;
  font-size: 25px;
  transition: transform 0.2s ease;
}

.brand:hover .brand-home-icon {
  transform: translateY(-2px) scale(1.08);
}

.app-nav {
  display: flex;
  align-items: center;
  justify-content: flex-end;
  gap: 4px;
}

.app-nav :deep(.arco-btn) {
  padding: 6px 10px;
  border-radius: 999px;
  color: #253554;
  font-size: 15px;
  font-weight: 650;
  letter-spacing: 0.02em;
  white-space: nowrap;
  transition: color 0.2s ease, background-color 0.2s ease, transform 0.2s ease;
}

.app-nav :deep(.arco-btn-text:hover),
.app-nav :deep(.arco-btn-text.active) {
  color: #684993;
  background-color: rgba(104, 73, 147, 0.1);
  transform: translateY(-1px);
}

.app-nav :deep(.arco-btn-status-danger) {
  color: #cc5c5d;
}

.app-nav :deep(.arco-btn-status-danger:hover) {
  color: #b83f48;
  background-color: rgba(204, 92, 93, 0.1);
}

.app-nav :deep(.arco-btn-primary) {
  color: #fff;
  background: linear-gradient(135deg, #253554 0%, #684993 100%);
  border: none;
  box-shadow: 0 6px 14px rgba(104, 73, 147, 0.22);
}

@media (max-width: 1100px) {
  .app-header {
    align-items: flex-start;
    flex-direction: column;
    gap: 12px;
  }

  .app-nav {
    width: 100%;
    justify-content: flex-start;
    overflow-x: auto;
    padding-bottom: 4px;
  }
}

@media (max-width: 640px) {
  .app-header-wrap {
    padding: 10px 10px 0;
  }

  .brand {
    font-size: 25px;
  }
}
</style>
