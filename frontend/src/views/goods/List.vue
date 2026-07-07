<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { getGoodsList } from '@/api'
import type { Goods } from '@/types'

const router = useRouter()
const goods = ref<Goods[]>([])

onMounted(async () => {
  const res = await getGoodsList()
  goods.value = res.data.data.list
})

const goToDetail = (id: number) => {
  router.push(`/goods/${id}`)
}
</script>

<template>
  <div class="goods-list-page">
    <h2>商品列表</h2>
    <div class="goods-grid">
      <div 
        v-for="item in goods" 
        :key="item.goodsId" 
        class="goods-card"
        @click="goToDetail(item.goodsId)"
      >
        <div class="goods-image">
          <img :src="item.imageUrl || 'https://via.placeholder.com/200'" alt="商品图片" />
        </div>
        <div class="goods-info">
          <h3>{{ item.title }}</h3>
          <p class="price">¥{{ item.price }}</p>
          <p class="condition">{{ item.condition }}</p>
          <p class="seller">{{ item.sellerNickname }}</p>
        </div>
      </div>
    </div>
  </div>
</template>

<style scoped>
.goods-list-page {
  padding: 20px;
}

.goods-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(250px, 1fr));
  gap: 20px;
}

.goods-card {
  background: white;
  border-radius: 8px;
  overflow: hidden;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.08);
  cursor: pointer;
}

.goods-image img {
  width: 100%;
  height: 200px;
  object-fit: cover;
}

.goods-info {
  padding: 16px;
}

.goods-info h3 {
  margin: 0 0 8px 0;
  font-size: 16px;
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
}

.price {
  color: #ff4d4f;
  font-size: 18px;
  font-weight: bold;
  margin: 0 0 8px 0;
}

.condition, .seller {
  font-size: 12px;
  color: #999;
  margin: 0 0 4px 0;
}
</style>
