<script setup lang="ts">
import { computed } from 'vue'
import { RouterView, useRoute } from 'vue-router'
import AppHeader from '@/components/common/AppHeader.vue'

const route = useRoute()

// 登录注册页保持简洁；后台管理已经有自己的侧边导航。
const showGlobalHeader = computed(() =>
  route.path !== '/login' &&
  route.path !== '/register' &&
  !route.path.startsWith('/admin')
)
</script>

<template>
  <AppHeader v-if="showGlobalHeader" />
  <RouterView />
</template>
