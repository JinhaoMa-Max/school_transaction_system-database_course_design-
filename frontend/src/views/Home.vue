<script setup lang="ts">
import { useRouter } from 'vue-router'
import { useUserStore } from '@/stores'

const router = useRouter()
const userStore = useUserStore()

const goToGoods = () => {
  router.push('/goods')
}

const goToPublish = () => {
  router.push('/goods/publish')
}

const goToOrders = () => {
  router.push('/orders')
}

const goToProfile = () => {
  router.push('/profile')
}

const goToFavorites = () => {
  router.push('/favorites')
}
</script>

<template>
  <div class="home-page">
    <header class="header">
      <h1>校园二手交易平台</h1>
      <nav>
        <button @click="goToGoods">商品广场</button>
        <button v-if="userStore.isLoggedIn" @click="goToPublish">发布商品</button>
        <button v-if="userStore.isLoggedIn" @click="goToOrders">我的订单</button>
        <button v-if="userStore.isLoggedIn" @click="goToFavorites">我的收藏</button>
        <button v-if="userStore.isLoggedIn" @click="goToProfile">个人中心</button>
        <button v-if="userStore.isLoggedIn" @click="userStore.logout">退出登录</button>
        <button v-else @click="router.push('/login')">登录</button>
      </nav>
    </header>
    <main class="main">
      <div class="hero">
        <h2>发现心仪的二手好物</h2>
        <p>安全、便捷、环保的校园交易体验</p>
        <button @click="goToGoods">立即浏览</button>
      </div>
    </main>
  </div>
</template>

<style scoped>
.home-page {
  min-height: 100vh;
}

.header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 20px 40px;
  background: white;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.08);
}

.header h1 {
  color: #165dff;
}

.header nav button {
  margin-left: 20px;
  padding: 8px 16px;
  border: none;
  background: transparent;
  color: #333;
  cursor: pointer;
  font-size: 14px;
}

.header nav button:hover {
  color: #165dff;
}

.main {
  padding: 40px;
}

.hero {
  text-align: center;
  padding: 80px 0;
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
  color: white;
  border-radius: 16px;
}

.hero h2 {
  font-size: 36px;
  margin-bottom: 16px;
}

.hero p {
  font-size: 18px;
  opacity: 0.9;
  margin-bottom: 32px;
}

.hero button {
  padding: 12px 32px;
  background: white;
  color: #667eea;
  border: none;
  border-radius: 24px;
  font-size: 16px;
  cursor: pointer;
}

.hero button:hover {
  opacity: 0.9;
}
</style>
