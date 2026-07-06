<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { useUserStore } from '@/stores'
import { getGoodsById, createFavorite, createBargain, createOrder } from '@/api'
import type { Goods } from '@/types'

const route = useRoute()
const router = useRouter()
const userStore = useUserStore()

const goodsId = Number(route.params.id)
const goods = ref<Goods | null>(null)
const offerPrice = ref(0)
const isFavorite = ref(false)

onMounted(async () => {
  const res = await getGoodsById(goodsId)
  goods.value = res.data
})

const handleFavorite = async () => {
  if (!userStore.isLoggedIn) {
    router.push('/login')
    return
  }
  await createFavorite({ goodsId })
  isFavorite.value = true
}

const handleBargain = async () => {
  if (!userStore.isLoggedIn) {
    router.push('/login')
    return
  }
  if (offerPrice.value <= 0) return
  await createBargain({ goodsId, offerPrice: offerPrice.value })
}

const handleBuy = async () => {
  if (!userStore.isLoggedIn) {
    router.push('/login')
    return
  }
  await createOrder({ goodsId, dealPrice: goods.value?.price || 0 })
  router.push('/orders')
}
</script>

<template>
  <div class="goods-detail-page">
    <div v-if="goods" class="goods-detail">
      <div class="goods-images">
        <img :src="goods.imageUrl || 'https://via.placeholder.com/400'" alt="商品图片" />
      </div>
      <div class="goods-detail-info">
        <h1>{{ goods.title }}</h1>
        <p class="price">¥{{ goods.price }}</p>
        <p class="condition">成色：{{ goods.condition }}</p>
        <p class="status">状态：{{ goods.status }}</p>
        <p class="view-count">浏览次数：{{ goods.viewCount }}</p>
        <p class="description">{{ goods.description }}</p>
        <div class="actions">
          <button @click="handleFavorite" :disabled="isFavorite">
            {{ isFavorite ? '已收藏' : '收藏' }}
          </button>
          <button @click="handleBuy">立即购买</button>
        </div>
        <div class="bargain-section">
          <input v-model="offerPrice" type="number" placeholder="输入议价金额" />
          <button @click="handleBargain">发起议价</button>
        </div>
      </div>
    </div>
  </div>
</template>

<style scoped>
.goods-detail-page {
  padding: 20px;
}

.goods-detail {
  display: flex;
  gap: 40px;
}

.goods-images img {
  width: 400px;
  height: 400px;
  object-fit: cover;
  border-radius: 8px;
}

.goods-detail-info {
  flex: 1;
}

.goods-detail-info h1 {
  margin: 0 0 16px 0;
  font-size: 24px;
}

.price {
  color: #ff4d4f;
  font-size: 32px;
  font-weight: bold;
  margin: 0 0 16px 0;
}

.condition, .status, .view-count {
  font-size: 14px;
  color: #666;
  margin: 0 0 8px 0;
}

.description {
  margin: 24px 0;
  line-height: 1.8;
}

.actions button {
  padding: 12px 24px;
  margin-right: 12px;
  border: none;
  border-radius: 4px;
  font-size: 16px;
  cursor: pointer;
}

.actions button:first-child {
  background: #f5f5f5;
  color: #333;
}

.actions button:last-child {
  background: #165dff;
  color: white;
}

.bargain-section {
  margin-top: 24px;
}

.bargain-section input {
  padding: 10px;
  width: 200px;
  border: 1px solid #e5e5e5;
  border-radius: 4px;
}

.bargain-section button {
  padding: 10px 20px;
  background: #faad14;
  color: white;
  border: none;
  border-radius: 4px;
  cursor: pointer;
  margin-left: 12px;
}
</style>
