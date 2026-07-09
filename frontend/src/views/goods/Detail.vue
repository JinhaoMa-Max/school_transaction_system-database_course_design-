<script setup lang="ts">
import { ref, onMounted, computed } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { Message } from '@arco-design/web-vue'
import { useUserStore } from '@/stores'
import {
  getGoodsById,
  getGoodsImages,
  incrementViewCount,
  createFavorite,
  deleteFavorite,
  checkFavorite,
  createBargain,
  createOrder
} from '@/api'
import { createSession } from '@/api/chat'
import type { Goods, GoodsImage } from '@/types'

const route = useRoute()
const router = useRouter()
const userStore = useUserStore()

const goodsId = Number(route.params.id)
const goods = ref<Goods | null>(null)
const images = ref<GoodsImage[]>([])
const loading = ref(false)
const isFavorite = ref(false)
const favoriteId = ref<number | null>(null)

const bargainVisible = ref(false)
const bargainPrice = ref(0)
const bargainLoading = ref(false)

const buyVisible = ref(false)
const buyLoading = ref(false)

const conditionMap: Record<string, string> = {
  new: '全新',
  like_new: '几乎全新',
  slight_use: '轻微使用',
  obvious_trace: '明显痕迹'
}

const statusMap: Record<string, { text: string; color: string }> = {
  pending: { text: '待审核', color: 'orange' },
  approved: { text: '已上架', color: 'green' },
  rejected: { text: '已驳回', color: 'red' },
  locked: { text: '已锁定', color: 'gold' },
  sold: { text: '已售出', color: 'cyan' },
  offline: { text: '已下架', color: 'gray' }
}

const mainImage = computed(() => {
  if (images.value.length > 0) {
    return images.value[0].imageUrl
  }
  return goods.value?.imageUrl || 'https://via.placeholder.com/600x600?text=No+Image'
})

const imageList = computed(() => {
  if (images.value.length > 0) {
    return images.value.map(img => img.imageUrl)
  }
  return [goods.value?.imageUrl || 'https://via.placeholder.com/600x600?text=No+Image']
})

const isOwner = computed(() => {
  return userStore.user?.userId === goods.value?.sellerId
})

const canBuy = computed(() => {
  return goods.value?.status === 'approved' && !isOwner.value
})

const fetchData = async () => {
  loading.value = true
  try {
    const [goodsRes, imagesRes] = await Promise.all([
      getGoodsById(goodsId),
      getGoodsImages(goodsId).catch(() => ({ data: [] }))
    ])
    goods.value = goodsRes.data
    images.value = imagesRes.data || []

    incrementViewCount(goodsId).catch(() => {})

    if (userStore.isLoggedIn) {
      try {
        const favRes = await checkFavorite(goodsId)
        isFavorite.value = favRes.data?.isFavorite || false
        favoriteId.value = favRes.data?.favoriteId || null
      } catch {
        isFavorite.value = false
      }
    }
  } catch {
    // 错误已由全局拦截器处理
  } finally {
    loading.value = false
  }
}

const handleFavorite = async () => {
  if (!userStore.isLoggedIn) {
    router.push(`/login?redirect=${encodeURIComponent(route.fullPath)}`)
    return
  }

  try {
    if (isFavorite.value && favoriteId.value) {
      await deleteFavorite(favoriteId.value)
      isFavorite.value = false
      favoriteId.value = null
      Message.success('已取消收藏')
    } else {
      const res = await createFavorite({ goodsId })
      isFavorite.value = true
      favoriteId.value = res.data.favoriteId
      Message.success('收藏成功')
    }
  } catch {
    // 错误已由全局拦截器处理
  }
}

const openBargain = () => {
  if (!userStore.isLoggedIn) {
    router.push(`/login?redirect=${encodeURIComponent(route.fullPath)}`)
    return
  }
  bargainPrice.value = goods.value?.price || 0
  bargainVisible.value = true
}

const handleBargain = async () => {
  if (bargainPrice.value <= 0) {
    Message.warning('请输入有效的议价金额')
    return
  }
  bargainLoading.value = true
  try {
    await createBargain({ goodsId, offerPrice: bargainPrice.value })
    Message.success('议价申请已发送')
    bargainVisible.value = false
  } catch {
    // 错误已由全局拦截器处理
  } finally {
    bargainLoading.value = false
  }
}

const openBuy = () => {
  if (!userStore.isLoggedIn) {
    router.push(`/login?redirect=${encodeURIComponent(route.fullPath)}`)
    return
  }
  buyVisible.value = true
}

const handleBuy = async () => {
  buyLoading.value = true
  try {
    const res = await createOrder({
      goodsId,
      dealPrice: goods.value?.price || 0
    })
    Message.success('订单创建成功')
    buyVisible.value = false
    router.push(`/orders/${res.data.orderId}`)
  } catch {
    // 错误已由全局拦截器处理
  } finally {
    buyLoading.value = false
  }
}

const handleContact = async () => {
  if (!userStore.isLoggedIn) {
    router.push(`/login?redirect=${encodeURIComponent(route.fullPath)}`)
    return
  }
  if (!goods.value) return

  try {
    const res = await createSession({
      goodsId: goods.value.goodsId,
      sellerId: goods.value.sellerId
    })
    Message.success('已进入私聊')
    router.push({ path: '/chat', query: { sessionId: String(res.data.sessionId) } })
  } catch {
    Message.error('发起私聊失败')
  }
}

const goToReport = () => {
  if (!userStore.isLoggedIn) {
    router.push(`/login?redirect=${encodeURIComponent(route.fullPath)}`)
    return
  }
  router.push(`/report?type=goods&id=${goodsId}`)
}

onMounted(fetchData)
</script>

<template>
  <div class="goods-detail-page">
    <a-spin :loading="loading" dot>
      <div v-if="goods" class="detail-container">
        <div class="image-section">
          <div class="main-image">
            <img :src="mainImage" :alt="goods.title" />
          </div>
          <div v-if="imageList.length > 1" class="thumbnail-list">
            <div
              v-for="(img, index) in imageList"
              :key="index"
              class="thumbnail"
              :class="{ active: index === 0 }"
            >
              <img :src="img" :alt="`缩略图${index + 1}`" />
            </div>
          </div>
        </div>

        <div class="info-section">
          <div class="goods-header">
            <h1 class="goods-title">{{ goods.title }}</h1>
            <div class="goods-header-right">
              <a-button
                v-if="!isOwner && canBuy"
                type="primary"
                size="small"
                @click="handleContact"
              >
                <icon-message />
                私聊卖家
              </a-button>
              <a-tag :color="statusMap[goods.status]?.color || 'default'">
                {{ statusMap[goods.status]?.text || goods.status }}
              </a-tag>
            </div>
          </div>

          <div class="goods-price">
            <span class="price-label">售价</span>
            <span class="price-value">¥{{ goods.price }}</span>
          </div>

          <a-descriptions bordered size="medium" :column="2" class="goods-desc">
            <a-descriptions-item label="成色">
              <a-tag :color="goods.condition === 'new' ? 'green' : 'blue'">
                {{ conditionMap[goods.condition] || goods.condition }}
              </a-tag>
            </a-descriptions-item>
            <a-descriptions-item label="浏览量">{{ goods.viewCount }}</a-descriptions-item>
            <a-descriptions-item label="发布时间">{{ goods.publishTime }}</a-descriptions-item>
            <a-descriptions-item label="分类ID">{{ goods.categoryId }}</a-descriptions-item>
          </a-descriptions>

          <div class="seller-card">
            <a-avatar :size="48">
              <template #icon>
                <icon-user />
              </template>
            </a-avatar>
            <div class="seller-info">
              <div class="seller-name">{{ goods.sellerNickname || '卖家' }}</div>
              <div class="seller-meta">
                <a-tag color="green" size="small">信用分 {{ userStore.user?.creditScore || 100 }}</a-tag>
              </div>
            </div>
            <a-button type="text" @click="handleContact">
              <icon-message />
              私信卖家
            </a-button>
          </div>

          <div class="action-buttons">
            <a-button
              type="secondary"
              size="large"
              @click="handleFavorite"
              :icon="isFavorite ? 'icon-star-fill' : 'icon-star'"
            >
              {{ isFavorite ? '已收藏' : '收藏' }}
            </a-button>
            <a-button
              v-if="canBuy"
              type="primary"
              status="warning"
              size="large"
              @click="openBargain"
            >
              我要议价
            </a-button>
            <a-button
              v-if="canBuy"
              type="primary"
              size="large"
              status="danger"
              @click="openBuy"
            >
              立即购买
            </a-button>
            <a-button
              v-if="!canBuy && isOwner"
              type="secondary"
              size="large"
              @click="router.push('/my/goods')"
            >
              查看我的商品
            </a-button>
          </div>

          <div class="report-link">
            <a type="link" @click="goToReport">
              <icon-exclamation-circle />
              举报商品
            </a>
          </div>
        </div>
      </div>

      <div v-if="goods" class="description-section">
        <a-card title="商品描述">
          <div class="description-content">{{ goods.description }}</div>
        </a-card>
      </div>
    </a-spin>

    <a-modal
      v-model:visible="bargainVisible"
      title="发起议价"
      @ok="handleBargain"
      @cancel="bargainVisible = false"
      :confirm-loading="bargainLoading"
    >
      <div class="bargain-form">
        <p>商品原价：<strong>¥{{ goods?.price }}</strong></p>
        <a-form :model="{ price: bargainPrice }" layout="vertical">
          <a-form-item label="您的出价">
            <a-input-number
              v-model="bargainPrice"
              :min="0.01"
              :max="goods?.price || 999999"
              :precision="2"
              placeholder="请输入您的出价"
              style="width: 100%"
            />
          </a-form-item>
        </a-form>
      </div>
    </a-modal>

    <a-modal
      v-model:visible="buyVisible"
      title="确认购买"
      @ok="handleBuy"
      @cancel="buyVisible = false"
      :confirm-loading="buyLoading"
      ok-text="确认购买"
    >
      <div class="buy-confirm">
        <p>您确定要以 <strong class="price">¥{{ goods?.price }}</strong> 的价格购买该商品吗？</p>
        <a-alert type="info">
          <p>购买后将生成订单，请及时与卖家联系完成面交。</p>
        </a-alert>
      </div>
    </a-modal>
  </div>
</template>

<style scoped>
.goods-detail-page {
  max-width: 1200px;
  margin: 0 auto;
  padding: 24px;
}

.detail-container {
  display: flex;
  gap: 32px;
  margin-bottom: 24px;
}

.image-section {
  flex: 0 0 480px;
}

.main-image {
  width: 480px;
  height: 480px;
  border-radius: 8px;
  overflow: hidden;
  background: #f5f5f5;
}

.main-image img {
  width: 100%;
  height: 100%;
  object-fit: cover;
}

.thumbnail-list {
  display: flex;
  gap: 12px;
  margin-top: 16px;
}

.thumbnail {
  width: 80px;
  height: 80px;
  border-radius: 4px;
  overflow: hidden;
  cursor: pointer;
  border: 2px solid transparent;
  transition: border-color 0.2s;
}

.thumbnail.active {
  border-color: #165dff;
}

.thumbnail img {
  width: 100%;
  height: 100%;
  object-fit: cover;
}

.info-section {
  flex: 1;
  display: flex;
  flex-direction: column;
}

.goods-header {
  display: flex;
  align-items: flex-start;
  justify-content: space-between;
  margin-bottom: 16px;
}

.goods-title {
  margin: 0;
  font-size: 24px;
  font-weight: 600;
  line-height: 1.4;
  flex: 1;
}

.goods-header-right {
  display: flex;
  align-items: center;
  gap: 10px;
  flex-shrink: 0;
  margin-left: 16px;
}

.goods-price {
  background: linear-gradient(135deg, #fff1f0 0%, #ffe7e6 100%);
  padding: 16px 20px;
  border-radius: 8px;
  margin-bottom: 20px;
  display: flex;
  align-items: baseline;
  gap: 12px;
}

.price-label {
  font-size: 14px;
  color: #86909c;
}

.price-value {
  font-size: 36px;
  font-weight: bold;
  color: #f53f3f;
}

.goods-desc {
  margin-bottom: 20px;
}

.seller-card {
  display: flex;
  align-items: center;
  gap: 16px;
  padding: 16px;
  background: #f7f8fa;
  border-radius: 8px;
  margin-bottom: 20px;
}

.seller-info {
  flex: 1;
}

.seller-name {
  font-size: 16px;
  font-weight: 500;
  margin-bottom: 4px;
}

.seller-meta {
  font-size: 12px;
}

.action-buttons {
  display: flex;
  gap: 12px;
  margin-bottom: 16px;
}

.action-buttons .arco-btn {
  flex: 1;
}

.report-link {
  text-align: center;
}

.description-section {
  margin-top: 24px;
}

.description-content {
  line-height: 1.8;
  font-size: 14px;
  color: #1d2129;
  white-space: pre-wrap;
}

.bargain-form p {
  margin: 0 0 16px 0;
}

.buy-confirm p {
  margin: 0 0 16px 0;
}

.buy-confirm .price {
  color: #f53f3f;
  font-size: 18px;
}

@media (max-width: 1024px) {
  .detail-container {
    flex-direction: column;
  }

  .image-section {
    flex: none;
    width: 100%;
  }

  .main-image {
    width: 100%;
    height: auto;
    aspect-ratio: 1 / 1;
  }
}
</style>
