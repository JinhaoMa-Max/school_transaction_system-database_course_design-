<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { getReviewList, createReview } from '@/api'
import type { Review } from '@/types'

const reviews = ref<Review[]>([])
const newReview = ref({
  orderId: 0,
  reviewedUserId: 0,
  rating: 5,
  content: ''
})

onMounted(async () => {
  const res = await getReviewList()
  reviews.value = res.data.list
})

const handleCreate = async () => {
  await createReview(newReview.value)
}
</script>

<template>
  <div class="review-page">
    <h2>我的评价</h2>
    <div class="review-list">
      <div 
        v-for="item in reviews" 
        :key="item.reviewId" 
        class="review-item"
      >
        <div class="review-info">
          <h3>评价ID: {{ item.reviewId }}</h3>
          <p>订单ID: {{ item.orderId }}</p>
          <p>评价者ID: {{ item.reviewerId }}</p>
          <p>被评价者ID: {{ item.reviewedUserId }}</p>
          <p>评分: {{ item.rating }}星</p>
          <p>评价内容: {{ item.content || '-' }}</p>
          <p>评价时间: {{ item.reviewTime }}</p>
        </div>
      </div>
    </div>
    <div v-if="reviews.length === 0" class="empty">
      <p>暂无评价记录</p>
    </div>
    <div class="create-review">
      <h3>发表评价</h3>
      <div class="form-item">
        <label>订单ID</label>
        <input v-model.number="newReview.orderId" type="number" />
      </div>
      <div class="form-item">
        <label>被评价者ID</label>
        <input v-model.number="newReview.reviewedUserId" type="number" />
      </div>
      <div class="form-item">
        <label>评分</label>
        <select v-model.number="newReview.rating">
          <option :value="1">1星</option>
          <option :value="2">2星</option>
          <option :value="3">3星</option>
          <option :value="4">4星</option>
          <option :value="5">5星</option>
        </select>
      </div>
      <div class="form-item">
        <label>评价内容</label>
        <textarea v-model="newReview.content" rows="3"></textarea>
      </div>
      <button @click="handleCreate">发表评价</button>
    </div>
  </div>
</template>

<style scoped>
.review-page {
  padding: 20px;
}

.review-list {
  display: flex;
  flex-direction: column;
  gap: 12px;
}

.review-item {
  padding: 16px;
  background: white;
  border-radius: 8px;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.08);
}

.review-info h3 {
  margin: 0 0 8px 0;
}

.review-info p {
  margin: 0 0 4px 0;
  font-size: 14px;
}

.empty {
  text-align: center;
  padding: 40px;
  color: #999;
}

.create-review {
  margin-top: 24px;
  padding: 20px;
  background: white;
  border-radius: 8px;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.08);
}

.create-review h3 {
  margin: 0 0 16px 0;
}

.form-item {
  margin-bottom: 16px;
}

.form-item label {
  display: block;
  margin-bottom: 8px;
}

.form-item input,
.form-item select,
.form-item textarea {
  width: 100%;
  padding: 10px;
  border: 1px solid #e5e5e5;
  border-radius: 4px;
}

button {
  padding: 12px 24px;
  background: #165dff;
  color: white;
  border: none;
  border-radius: 4px;
  cursor: pointer;
}
</style>
