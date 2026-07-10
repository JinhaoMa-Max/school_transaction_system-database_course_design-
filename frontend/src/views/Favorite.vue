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
    <div class="page-header">
      <h2>我的收藏</h2>
      <span class="count">共 {{ favorites.length }} 件商品</span>
    </div>
    
    <div class="favorite-list" v-if="favorites.length > 0">
      <div 
        v-for="item in favorites" 
        :key="item.favoriteId" 
        class="favorite-card"
      >
        <div class="goods-image" @click="goToDetail(item.goodsId)">
          <img 
            :src="item.imageUrl || 'https://trae-api-cn.mchost.guru/api/ide/v1/text_to_image?prompt=secondhand%20goods%20placeholder&image_size=square'" 
            :alt="item.title || '商品图片'"
          />
        </div>
        
        <div class="goods-info" @click="goToDetail(item.goodsId)">
          <h3 class="goods-title">{{ item.title || `商品 ${item.goodsId}` }}</h3>
          <p class="goods-price">¥{{ item.price || '0.00' }}</p>
          <p class="favorite-time">收藏时间：{{ item.favoriteTime }}</p>
        </div>
        
        <div class="actions">
          <button class="delete-btn" @click="handleDelete(item.favoriteId)">
            取消收藏
          </button>
        </div>
      </div>
    </div>
    
    <div v-else class="empty-state">
      <div class="empty-icon">♥</div>
      <p class="empty-text">暂无收藏商品</p>
      <button class="go-shopping-btn" @click="router.push('/goods')">
        去逛逛
      </button>
    </div>
  </div>
</template>

<style scoped>
.favorite-page {
  max-width: 1200px;
  margin: 0 auto;
  padding: 24px;
}

.page-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  margin-bottom: 24px;
  padding-bottom: 16px;
  border-bottom: 1px solid #eee;
}

.page-header h2 {
  margin: 0;
  font-size: 24px;
  color: #333;
}

.count {
  font-size: 14px;
  color: #999;
}

.favorite-list {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(280px, 1fr));
  gap: 20px;
}

.favorite-card {
  display: flex;
  flex-direction: column;
  background: white;
  border-radius: 12px;
  box-shadow: 0 2px 12px rgba(0, 0, 0, 0.08);
  overflow: hidden;
  transition: transform 0.2s, box-shadow 0.2s;
}

.favorite-card:hover {
  transform: translateY(-4px);
  box-shadow: 0 4px 20px rgba(0, 0, 0, 0.12);
}

.goods-image {
  width: 100%;
  height: 200px;
  overflow: hidden;
  cursor: pointer;
  background: #f8f8f8;
}

.goods-image img {
  width: 100%;
  height: 100%;
  object-fit: cover;
  transition: transform 0.3s;
}

.goods-image:hover img {
  transform: scale(1.05);
}

.goods-info {
  flex: 1;
  padding: 16px;
  cursor: pointer;
  display: flex;
  flex-direction: column;
}

.goods-title {
  margin: 0 0 8px 0;
  font-size: 16px;
  font-weight: 500;
  color: #333;
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
}

.goods-price {
  margin: 0 0 8px 0;
  font-size: 20px;
  font-weight: 600;
  color: #ff4d4f;
}

.favorite-time {
  margin: auto 0 0 0;
  font-size: 12px;
  color: #bbb;
}

.actions {
  padding: 0 16px 16px;
}

.delete-btn {
  width: 100%;
  padding: 10px;
  background: #fff;
  color: #666;
  border: 1px solid #ddd;
  border-radius: 6px;
  cursor: pointer;
  font-size: 14px;
  transition: all 0.2s;
}

.delete-btn:hover {
  background: #fff1f0;
  border-color: #ffccc7;
  color: #ff4d4f;
}

.empty-state {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  padding: 80px 20px;
}

.empty-icon {
  font-size: 64px;
  color: #ffccc7;
  margin-bottom: 16px;
}

.empty-text {
  font-size: 16px;
  color: #999;
  margin: 0 0 24px 0;
}

.go-shopping-btn {
  padding: 12px 32px;
  background: #52c41a;
  color: white;
  border: none;
  border-radius: 24px;
  cursor: pointer;
  font-size: 14px;
  transition: background 0.2s;
}

.go-shopping-btn:hover {
  background: #389e0d;
}
</style>