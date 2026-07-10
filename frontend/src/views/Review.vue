<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { Message } from '@arco-design/web-vue'
import { useUserStore } from '@/stores'
import { getReviewList } from '@/api'
import type { Review } from '@/types'

const router = useRouter()
const userStore = useUserStore()

const currentUserId = computed(() => userStore.user?.userId ?? 0)

// 评价列表
const reviews = ref<Review[]>([])
const total = ref(0)
const page = ref(1)
const pageSize = ref(10)
const loading = ref(false)

// 标签页：我发出的 | 我收到的
const activeTab = ref<'sent' | 'received'>('received')

// 计算平均评分
const avgRating = computed(() => {
  if (reviews.value.length === 0) return 0
  const sum = reviews.value.reduce((acc, r) => acc + r.rating, 0)
  return (sum / reviews.value.length).toFixed(1)
})

// 评分分布统计
const ratingDistribution = computed(() => {
  const dist: Record<number, number> = { 5: 0, 4: 0, 3: 0, 2: 0, 1: 0 }
  reviews.value.forEach(r => {
    if (dist[r.rating] !== undefined) dist[r.rating]++
  })
  return dist
})

const fetchReviews = async () => {
  if (!currentUserId.value) return

  loading.value = true
  try {
    const params: Record<string, any> = {
      page: page.value,
      size: pageSize.value
    }
    if (activeTab.value === 'sent') {
      params.reviewerId = currentUserId.value
    } else {
      params.reviewedUserId = currentUserId.value
    }
    const res = await getReviewList(params)
    reviews.value = res.data.list
    total.value = res.data.total
  } catch {
    Message.error('获取评价列表失败')
  } finally {
    loading.value = false
  }
}

const handleTabChange = (key: string | number) => {
  activeTab.value = key as 'sent' | 'received'
  page.value = 1
  fetchReviews()
}

const handlePageChange = (pageNum: number) => {
  page.value = pageNum
  fetchReviews()
}

const handlePageSizeChange = (size: number) => {
  pageSize.value = size
  page.value = 1
  fetchReviews()
}

const goToGoods = (goodsId?: number) => {
  if (goodsId) router.push(`/goods/${goodsId}`)
}

const goToOrder = (orderId: number) => {
  router.push(`/orders/${orderId}`)
}

// 评分星星颜色
const getRatingColor = (rating: number) => {
  if (rating >= 4) return '#f7ba1e'
  if (rating >= 3) return '#f7ba1e'
  if (rating >= 2) return '#e69a2e'
  return '#f53f3f'
}

onMounted(fetchReviews)
</script>

<template>
  <div class="review-page">
    <div class="page-container">
      <!-- 页面标题 -->
      <div class="page-header">
        <h2>我的评价</h2>
        <p class="page-desc">查看您发出和收到的所有评价</p>
      </div>

      <!-- 标签页 -->
      <a-card class="content-card" :bordered="false">
        <a-tabs
          v-model:active-key="activeTab"
          type="rounded"
          @change="handleTabChange"
        >
          <a-tab-pane key="received" title="我收到的评价" />
          <a-tab-pane key="sent" title="我发出的评价" />
        </a-tabs>

        <!-- 统计概览（仅"收到的评价"显示） -->
        <div v-if="activeTab === 'received' && reviews.length > 0" class="rating-summary">
          <div class="summary-score">
            <span class="score-number">{{ avgRating }}</span>
            <span class="score-unit">/5</span>
          </div>
          <div class="summary-stars">
            <a-rate :model-value="Number(avgRating)" :count="5" disabled allow-half />
          </div>
          <div class="summary-total">
            共 {{ total }} 条评价
          </div>
          <!-- 评分分布条 -->
          <div class="rating-bars">
            <div v-for="star in 5" :key="star" class="rating-bar-row">
              <span class="bar-label">{{ star }}星</span>
              <div class="bar-track">
                <div
                  class="bar-fill"
                  :style="{ width: total > 0 ? (ratingDistribution[star] / total * 100) + '%' : '0%' }"
                />
              </div>
              <span class="bar-count">{{ ratingDistribution[star] }}</span>
            </div>
          </div>
        </div>

        <!-- 评价列表 -->
        <a-spin :loading="loading" style="width: 100%">
          <a-empty v-if="!loading && reviews.length === 0" description="暂无评价记录" />

          <div v-else class="review-list">
            <div
              v-for="item in reviews"
              :key="item.reviewId"
              class="review-item"
            >
              <div class="review-header">
                <div class="review-user">
                  <a-avatar :size="44" class="review-avatar">
                    <span>{{ (activeTab === 'sent' ? (item.reviewedUserName || '用户') : (item.reviewerName || '用户')).slice(0, 1).toUpperCase() }}</span>
                  </a-avatar>
                  <div class="user-meta">
                    <div class="user-name">
                      {{ activeTab === 'sent' ? '评价 ' + (item.reviewedUserName || '用户') : (item.reviewerName || '用户') + ' 的评价' }}
                      <a-tag v-if="item.isFollowUp" color="orange" size="small" class="followup-tag">追评</a-tag>
                    </div>
                    <div class="review-time">{{ item.createTime }}</div>
                  </div>
                </div>
                <div class="review-rating">
                  <a-rate
                    :model-value="item.rating"
                    :count="5"
                    disabled
                    :color="getRatingColor(item.rating)"
                    size="small"
                  />
                </div>
              </div>

              <div class="review-content" v-if="item.content">
                <p>{{ item.content }}</p>
              </div>

              <div class="review-footer">
                <div
                  v-if="item.goodsTitle"
                  class="goods-link"
                  @click="goToGoods(item.goodsId)"
                >
                  <icon-gift />
                  <span>{{ item.goodsTitle }}</span>
                </div>
                <a-button
                  type="text"
                  size="small"
                  @click="goToOrder(item.orderId)"
                >
                  查看订单 #{{ item.orderId }}
                </a-button>
              </div>
            </div>
          </div>

          <!-- 分页 -->
          <div v-if="total > pageSize" class="pagination-container">
            <a-pagination
              :total="total"
              :current="page"
              :page-size="pageSize"
              :page-size-options="[10, 20, 50]"
              show-total
              show-jumper
              show-page-size
              @change="handlePageChange"
              @page-size-change="handlePageSizeChange"
            />
          </div>
        </a-spin>
      </a-card>
    </div>
  </div>
</template>

<style scoped>
.review-page {
  min-height: calc(100vh - 64px);
  padding: 24px;
  background: #f5f7fa;
}

.page-container {
  max-width: 800px;
  margin: 0 auto;
}

.page-header {
  margin-bottom: 24px;
}

.page-header h2 {
  margin: 0 0 4px 0;
  font-size: 24px;
  font-weight: 600;
  color: #1d2129;
}

.page-desc {
  margin: 0;
  font-size: 14px;
  color: #86909c;
}

.content-card {
  border-radius: 12px;
  box-shadow: 0 2px 12px rgba(0, 0, 0, 0.04);
}

/* 评分概览 */
.rating-summary {
  display: flex;
  flex-wrap: wrap;
  align-items: center;
  gap: 20px;
  padding: 20px 24px;
  margin-bottom: 20px;
  background: #f7f8fa;
  border-radius: 12px;
}

.summary-score {
  display: flex;
  align-items: baseline;
  gap: 2px;
}

.score-number {
  font-size: 36px;
  font-weight: 700;
  color: #1d2129;
  line-height: 1;
}

.score-unit {
  font-size: 16px;
  color: #86909c;
}

.summary-stars {
  display: flex;
  align-items: center;
}

.summary-total {
  font-size: 13px;
  color: #86909c;
}

.rating-bars {
  flex: 1;
  min-width: 200px;
  display: flex;
  flex-direction: column;
  gap: 4px;
}

.rating-bar-row {
  display: flex;
  align-items: center;
  gap: 8px;
}

.bar-label {
  width: 32px;
  font-size: 12px;
  color: #86909c;
  text-align: right;
  flex-shrink: 0;
}

.bar-track {
  flex: 1;
  height: 6px;
  background: #e5e6eb;
  border-radius: 3px;
  overflow: hidden;
}

.bar-fill {
  height: 100%;
  background: #f7ba1e;
  border-radius: 3px;
  transition: width 0.3s ease;
}

.bar-count {
  width: 24px;
  font-size: 12px;
  color: #86909c;
  flex-shrink: 0;
}

/* 评价列表 */
.review-list {
  display: flex;
  flex-direction: column;
  gap: 12px;
}

.review-item {
  padding: 20px;
  border: 1px solid #e5e6eb;
  border-radius: 10px;
  transition: box-shadow 0.2s;
}

.review-item:hover {
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.06);
}

.review-header {
  display: flex;
  justify-content: space-between;
  align-items: flex-start;
}

.review-user {
  display: flex;
  align-items: center;
  gap: 12px;
}

.review-avatar {
  background: #e8f3ff;
  color: #165dff;
  font-weight: 600;
  font-size: 16px;
  flex-shrink: 0;
}

.user-meta {
  display: flex;
  flex-direction: column;
  gap: 2px;
}

.user-name {
  font-size: 15px;
  font-weight: 500;
  color: #1d2129;
  display: flex;
  align-items: center;
  gap: 8px;
}

.followup-tag {
  font-size: 11px;
}

.review-time {
  font-size: 12px;
  color: #86909c;
}

.review-content {
  margin-top: 14px;
  padding: 12px 16px;
  background: #f7f8fa;
  border-radius: 8px;
}

.review-content p {
  margin: 0;
  font-size: 14px;
  color: #4e5969;
  line-height: 1.6;
  white-space: pre-wrap;
  word-break: break-word;
}

.review-footer {
  margin-top: 12px;
  display: flex;
  justify-content: space-between;
  align-items: center;
}

.goods-link {
  display: flex;
  align-items: center;
  gap: 6px;
  font-size: 13px;
  color: #165dff;
  cursor: pointer;
  transition: opacity 0.2s;
}

.goods-link:hover {
  opacity: 0.8;
}

.pagination-container {
  margin-top: 24px;
  display: flex;
  justify-content: flex-end;
}

@media (max-width: 640px) {
  .review-page {
    padding: 16px;
  }

  .rating-summary {
    flex-direction: column;
    align-items: flex-start;
  }

  .rating-bars {
    width: 100%;
  }
}
</style>
