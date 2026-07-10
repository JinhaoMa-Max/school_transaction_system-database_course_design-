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
const canReview = ref(true)
const blockReason = ref('')

// 是否为追评
const isFollowUp = ref(false)

const form = reactive({
  rating: 5,
  content: ''
})

const formRef = ref()

// 当前用户是否为买家
const isBuyer = computed(() => {
  return userStore.user?.userId === order.value?.buyerId
})

// 被评价者 = 交易对方
const reviewedUserId = computed(() => {
  if (!order.value || !userStore.user) return 0
  return isBuyer.value ? order.value.sellerId : order.value.buyerId
})

// 当前用户的已有评价数（0=未评, 1=已首评可追评, 2=已达上限）
const reviewCount = computed(() => {
  if (!order.value) return 0
  return isBuyer.value ? (order.value.buyerReviewed ?? 0) : (order.value.sellerReviewed ?? 0)
})

// 页面标题
const pageTitle = computed(() => isFollowUp.value ? '发表追评' : '发表评价')

// 提交按钮文字
const submitText = computed(() => isFollowUp.value ? '提交追评' : '提交评价')

const fetchData = async () => {
  loading.value = true
  try {
    const orderRes = await getOrderById(orderId)
    order.value = orderRes.data

    // ---- 校验前置条件 ----
    if (order.value.status !== 'completed') {
      canReview.value = false
      blockReason.value = '只有已完成的订单才能评价'
      loading.value = false
      return
    }

    const count = reviewCount.value
    if (count >= 2) {
      canReview.value = false
      blockReason.value = '您已达到评价次数上限（首评+追评各一次）'
      loading.value = false
      return
    }

    // count == 1 → 追评模式
    isFollowUp.value = count === 1

    const [goodsRes, userRes] = await Promise.all([
      getGoodsById(order.value.goodsId).catch(() => ({ data: null })),
      getUserById(reviewedUserId.value).catch(() => ({ data: null }))
    ])

    goods.value = goodsRes.data
    reviewedUser.value = userRes.data
  } catch {
    canReview.value = false
    blockReason.value = '获取订单信息失败'
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
    Message.success(isFollowUp.value ? '追评提交成功' : '评价提交成功')
    router.push(`/orders/${orderId}`)
  } catch {
    // 错误已由全局拦截器处理（包括超7天等后端校验）
  } finally {
    submitLoading.value = false
  }
}

const handleBack = () => {
  router.push(`/orders/${orderId}`)
}

const goToOrder = () => {
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
          <h2 class="page-title">{{ pageTitle }}</h2>
          <p v-if="isFollowUp" class="followup-hint">
            ⚠️ 追评需在首评后7天内提交，且仅可追评一次
          </p>
        </div>

        <a-card>
          <!-- 前置条件不满足时显示阻止信息 -->
          <a-result
            v-if="!canReview"
            status="warning"
            :title="blockReason"
          >
            <template #extra>
              <a-button type="primary" @click="goToOrder">返回订单详情</a-button>
            </template>
          </a-result>

          <template v-else>
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
                <div class="user-name">
                  {{ reviewedUser?.nickname || '用户' }}
                  <a-tag v-if="isFollowUp" color="orange" size="small">追评</a-tag>
                </div>
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
                {{ submitText }}
              </a-button>
            </div>
          </a-form>
          </template>
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

.followup-hint {
  margin: 8px 0 0 0;
  font-size: 13px;
  color: #e69a2e;
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
  display: flex;
  align-items: center;
  gap: 8px;
}

.form-actions {
  display: flex;
  justify-content: flex-end;
  gap: 12px;
  margin-top: 24px;
}
</style>
