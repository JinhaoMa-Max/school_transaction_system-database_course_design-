<script setup lang="ts">
import { ref, reactive, computed, onMounted } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { Message } from '@arco-design/web-vue'
import { useUserStore } from '@/stores'
import { getOrderById, createReview, getGoodsById, getUserById } from '@/api'
import type { TradeOrder, Goods, User } from '@/types'

const route = useRoute()
const router = useRouter()
const userStore = useUserStore()

const orderId = Number(route.params.orderId)

const order = ref<TradeOrder | null>(null)
const goods = ref<Goods | null>(null)
const reviewedUser = ref<User | null>(null)
const loading = ref(false)
const submitLoading = ref(false)

const form = reactive({
  rating: 5,
  content: ''
})

const formRef = ref()

const reviewedUserId = computed(() => {
  if (!order.value || !userStore.user) return 0
  return userStore.user.userId === order.value.buyerId ? order.value.sellerId : order.value.buyerId
})

const fetchData = async () => {
  loading.value = true
  try {
    const orderRes = await getOrderById(orderId)
    order.value = orderRes.data

    const [goodsRes, userRes] = await Promise.all([
      getGoodsById(order.value.goodsId).catch(() => ({ data: null })),
      getUserById(reviewedUserId.value).catch(() => ({ data: null }))
    ])

    goods.value = goodsRes.data
    reviewedUser.value = userRes.data
  } catch {
  } finally {
    loading.value = false
  }
}

const handleSubmit = async ({ values }: { values: Record<string, any> }) => {
  if (values.rating < 1) {
    Message.warning('请选择评分')
    return
  }
  submitLoading.value = true
  try {
    await createReview({
      orderId,
      reviewedUserId: reviewedUserId.value,
      rating: values.rating,
      content: values.content || undefined
    })
    Message.success('评价提交成功')
    router.push('/orders')
  } catch {
  } finally {
    submitLoading.value = false
  }
}

const handleBack = () => {
  router.push(`/orders/${orderId}`)
}

onMounted(fetchData)
</script>

<template>
  <div class="review-page">
    <a-spin :loading="loading" dot>
      <div class="review-container">
        <div class="page-header">
          <a-button type="text" @click="handleBack">
            <icon-left />
            返回订单详情
          </a-button>
          <h2 class="page-title">发表评价</h2>
        </div>

        <a-card>
          <div class="order-info">
            <div v-if="goods" class="goods-info">
              <div class="goods-image">
                <img :src="goods.imageUrl || 'https://via.placeholder.com/80x80?text=No+Image'" :alt="goods.title" />
              </div>
              <div class="goods-detail">
                <div class="goods-title">{{ goods.title }}</div>
                <div class="goods-price">¥{{ order?.dealPrice || goods.price }}</div>
              </div>
            </div>
          </div>

          <a-divider />

          <div class="user-section">
            <div class="section-title">评价对象</div>
            <div class="user-info">
              <a-avatar :size="48">
                <img v-if="reviewedUser?.avatarUrl" :src="reviewedUser.avatarUrl" />
                <icon-user v-else />
              </a-avatar>
              <div class="user-detail">
                <div class="user-name">{{ reviewedUser?.nickname || '用户' }}</div>
                <a-tag color="green" size="small">信用分 {{ reviewedUser?.creditScore || '--' }}</a-tag>
              </div>
            </div>
          </div>

          <a-divider />

          <a-form
            ref="formRef"
            :model="form"
            layout="vertical"
            @submit="handleSubmit"
          >
            <a-form-item
              field="rating"
              label="评分"
              :rules="[{ required: true, message: '请选择评分' }]"
            >
              <a-rate v-model="form.rating" :count="5" size="large" />
            </a-form-item>

            <a-form-item
              field="content"
              label="评价内容"
            >
              <a-textarea
                v-model="form.content"
                placeholder="请输入您的评价内容（选填，最多500字）"
                :max-length="500"
                show-word-limit
                :auto-size="{ minRows: 4, maxRows: 8 }"
              />
            </a-form-item>

            <div class="form-actions">
              <a-button size="large" @click="handleBack">
                取消
              </a-button>
              <a-button type="primary" html-type="submit" size="large" :loading="submitLoading">
                提交评价
              </a-button>
            </div>
          </a-form>
        </a-card>
      </div>
    </a-spin>
  </div>
</template>

<style scoped>
.review-page {
  max-width: 640px;
  margin: 0 auto;
  padding: 24px;
}

.page-header {
  margin-bottom: 24px;
}

.page-title {
  margin: 16px 0 0 0;
  font-size: 24px;
  font-weight: 600;
}

.goods-info {
  display: flex;
  gap: 16px;
  padding: 8px 0;
}

.goods-image {
  width: 80px;
  height: 80px;
  border-radius: 8px;
  overflow: hidden;
  background: #f5f5f5;
  flex-shrink: 0;
}

.goods-image img {
  width: 100%;
  height: 100%;
  object-fit: cover;
}

.goods-detail {
  flex: 1;
  display: flex;
  flex-direction: column;
  justify-content: center;
}

.goods-title {
  font-size: 15px;
  font-weight: 500;
  margin-bottom: 8px;
  color: #1d2129;
  display: -webkit-box;
  -webkit-line-clamp: 2;
  -webkit-box-orient: vertical;
  overflow: hidden;
}

.goods-price {
  font-size: 20px;
  font-weight: bold;
  color: #f53f3f;
}

.section-title {
  font-size: 15px;
  font-weight: 500;
  color: #1d2129;
  margin-bottom: 16px;
}

.user-info {
  display: flex;
  align-items: center;
  gap: 16px;
}

.user-detail {
  display: flex;
  flex-direction: column;
  gap: 4px;
}

.user-name {
  font-size: 16px;
  font-weight: 500;
  color: #1d2129;
}

.form-actions {
  display: flex;
  justify-content: flex-end;
  gap: 12px;
  margin-top: 24px;
}
</style>
