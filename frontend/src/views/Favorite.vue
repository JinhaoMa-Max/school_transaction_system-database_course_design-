<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { getFavoriteList, deleteFavorite } from '@/api'
import type { Favorite } from '@/types'

const router = useRouter()
const favorites = ref<Favorite[]>([])

onMounted(async () => {
  const res = await getFavoriteList()
  favorites.value = res.data.list
})

const goToDetail = (goodsId: number) => {
  router.push(`/goods/${goodsId}`)
}

const handleDelete = async (favoriteId: number) => {
  await deleteFavorite(favoriteId)
  favorites.value = favorites.value.filter(f => f.favoriteId !== favoriteId)
}
</script>

<template>
  <div class="favorite-page">
    <h2>我的收藏</h2>
    <div class="favorite-list">
      <div 
        v-for="item in favorites" 
        :key="item.favoriteId" 
        class="favorite-item"
      >
        <div class="goods-info" @click="goToDetail(item.goodsId)">
          <h3>商品ID: {{ item.goodsId }}</h3>
          <p>收藏时间: {{ item.favoriteTime }}</p>
        </div>
        <button @click="handleDelete(item.favoriteId)">取消收藏</button>
      </div>
    </div>
    <div v-if="favorites.length === 0" class="empty">
      <p>暂无收藏商品</p>
    </div>
  </div>
</template>

<style scoped>
.favorite-page {
  padding: 20px;
}

.favorite-list {
  display: flex;
  flex-direction: column;
  gap: 12px;
}

.favorite-item {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 16px;
  background: white;
  border-radius: 8px;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.08);
}

.goods-info {
  flex: 1;
  cursor: pointer;
}

.goods-info h3 {
  margin: 0 0 8px 0;
}

.goods-info p {
  margin: 0;
  color: #999;
  font-size: 14px;
}

button {
  padding: 8px 16px;
  background: #ff4d4f;
  color: white;
  border: none;
  border-radius: 4px;
  cursor: pointer;
}

.empty {
  text-align: center;
  padding: 40px;
  color: #999;
}
</style>
