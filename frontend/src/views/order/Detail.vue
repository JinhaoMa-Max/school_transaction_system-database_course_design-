<script setup lang="ts">
import { ref, onMounted, computed } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { Message, Modal } from '@arco-design/web-vue'
import { useUserStore } from '@/stores'
import {
  getOrderById,
  cancelOrder,
  completeOrder
} from '@/api'
import {
  getAppointmentByOrderId,
  createAppointment,
  verifyConfirmCode
} from '@/api'
import type { TradeOrder, Appointment } from '@/types'

const route = useRoute()
const router = useRouter()
const userStore = useUserStore()

const orderId = Number(route.params.id)
const loading = ref(false)
const order = ref<TradeOrder | null>(null)
const appointment = ref<Appointment | null>(null)

const verifyVisible = ref(false)
const verifyCode = ref('')
const verifyLoading = ref(false)

const orderStatusMap: Record<string, { text: string; color: string }> = {
  pending_meet: { text: '待面交', color: 'orange' },
  in_meet: { text: '面交中', color: 'blue' },
  completed: { text: '已完成', color: 'green' },
  cancelled: { text: '已取消', color: 'gray' }
}

const appointmentStatusMap: Record<string, { text: string; color: string }> = {
  pending: { text: '待确认', color: 'orange' },
  confirmed: { text: '已确认', color: 'blue' },
  completed: { text: '已完成', color: 'green' },
  cancelled: { text: '已取消', color: 'gray' }
}

const isBuyer = computed(() => {
  return userStore.user?.userId === order.value?.buyerId
})

const isSeller = computed(() => {
  return userStore.user?.userId === order.value?.sellerId
})

const fetchData = async () => {
  loading.value = true
  try {
    const [orderRes, appRes] = await Promise.all([
      getOrderById(orderId),
      getAppointmentByOrderId(orderId).catch(() => ({ data: null }))
    ])
    order.value = orderRes.data
    appointment.value = appRes.data || null
  } catch {
    // 错误已由全局拦截器处理
  } finally {
    loading.value = false
  }
}

const goToGoods = () => {
  if (order.value) {
    router.push(`/goods/${order.value.goodsId}`)
  }
}

const goToAppointment = () => {
  router.push(`/appointment/${orderId}`)
}

const goToReview = () => {
  router.push(`/review/${orderId}`)
}

const handleCancel = () => {
  Modal.confirm({
    title: '取消订单',
    content: '确定要取消该订单吗？取消后无法恢复。',
    okText: '确认取消',
    cancelText: '再想想',
    status: 'warning',
    onOk: async () => {
      try {
        await cancelOrder(orderId)
        Message.success('订单已取消')
        fetchData()
      } catch {
        // 错误已由全局拦截器处理
      }
    }
  })
}

const openVerify = () => {
  verifyCode.value = ''
  verifyVisible.value = true
}

const handleVerify = async () => {
  if (!verifyCode.value) {
    Message.warning('请输入确认码')
    return
  }
  verifyLoading.value = true
  try {
    await verifyConfirmCode({ orderId, confirmCode: verifyCode.value })
    Message.success('确认码验证成功')
    verifyVisible.value = false
    fetchData()
  } catch {
    // 错误已由全局拦截器处理
  } finally {
    verifyLoading.value = false
  }
}

const handleComplete = () => {
  Modal.confirm({
    title: '确认完成',
    content: '确认交易已完成吗？确认后将无法撤销。',
    okText: '确认完成',
    cancelText: '取消',
    onOk: async () => {
      try {
        await completeOrder(orderId)
        Message.success('订单已完成')
        fetchData()
      } catch {
        // 错误已由全局拦截器处理
      }
    }
  })
}

onMounted(fetchData)
</script>

<template>
  <div class="order-detail-page">
    <a-spin :loading="loading" style="width: 100%">
      <div v-if="order" class="detail-container">
        <div class="main-section">
          <a-card class="status-card">
            <div class="status-header">
              <a-tag :color="orderStatusMap[order.status]?.color || 'gray'" size="large">
                {{ orderStatusMap[order.status]?.text || order.status }}
              </a-tag>
              <span class="order-id">订单号：{{ order.orderId }}</span>
            </div>
          </a-card>

          <a-card class="goods-card" @click="goToGoods">
            <div class="goods-info">
              <div class="goods-image">
                <img src="https://via.placeholder.com/120x120?text=Goods" alt="商品图片" />
              </div>
              <div class="goods-detail">
                <h3 class="goods-title">商品 #{{ order.goodsId }}</h3>
                <p class="goods-price">成交价：<span class="price">¥{{ order.dealPrice.toFixed(2) }}</span></p>
                <p class="click-hint">点击查看商品详情</p>
              </div>
            </div>
          </a-card>

          <a-card title="订单信息" class="info-card">
            <a-descriptions :column="2" bordered size="medium">
              <a-descriptions-item label="订单编号">{{ order.orderId }}</a-descriptions-item>
              <a-descriptions-item label="订单状态">
                <a-tag :color="orderStatusMap[order.status]?.color || 'gray'">
                  {{ orderStatusMap[order.status]?.text || order.status }}
                </a-tag>
              </a-descriptions-item>
              <a-descriptions-item label="商品ID">{{ order.goodsId }}</a-descriptions-item>
              <a-descriptions-item label="成交价格">
                <span class="price-text">¥{{ order.dealPrice.toFixed(2) }}</span>
              </a-descriptions-item>
              <a-descriptions-item label="买家ID">{{ order.buyerId }}</a-descriptions-item>
              <a-descriptions-item label="卖家ID">{{ order.sellerId }}</a-descriptions-item>
              <a-descriptions-item label="下单时间" :span="2">{{ order.createTime }}</a-descriptions-item>
            </a-descriptions>
          </a-card>

          <a-card v-if="appointment" title="面交预约" class="info-card">
            <a-descriptions :column="2" bordered size="medium">
              <a-descriptions-item label="预约状态">
                <a-tag :color="appointmentStatusMap[appointment.status]?.color || 'gray'">
                  {{ appointmentStatusMap[appointment.status]?.text || appointment.status }}
                </a-tag>
              </a-descriptions-item>
              <a-descriptions-item label="确认码">
                <span class="confirm-code">{{ appointment.confirmCode }}</span>
              </a-descriptions-item>
              <a-descriptions-item label="面交时间">{{ appointment.meetTime }}</a-descriptions-item>
              <a-descriptions-item label="面交地点">{{ appointment.meetLocation }}</a-descriptions-item>
              <a-descriptions-item label="创建时间" :span="2">{{ appointment.createTime }}</a-descriptions-item>
            </a-descriptions>
          </a-card>
        </div>

        <div class="side-section">
          <a-card title="操作" class="action-card">
            <div class="action-buttons">
              <template v-if="order.status === 'pending_meet'">
                <a-button type="primary" long @click="goToAppointment">
                  预约面交
                </a-button>
                <a-button type="outline" status="warning" long @click="handleCancel">
                  取消订单
                </a-button>
              </template>

              <template v-else-if="order.status === 'in_meet'">
                <a-button type="primary" long @click="openVerify">
                  核销确认码
                </a-button>
                <a-button type="outline" long @click="handleComplete">
                  确认完成
                </a-button>
              </template>

              <template v-else-if="order.status === 'completed'">
                <a-button type="primary" long @click="goToReview">
                  去评价
                </a-button>
              </template>

              <a-button type="text" long @click="router.push('/orders')">
                返回订单列表
              </a-button>
            </div>
          </a-card>

          <a-card title="对方信息" class="info-card">
            <div class="user-info">
              <a-avatar :size="48">
                <template #icon>
                  <icon-user />
                </template>
              </a-avatar>
              <div class="user-detail">
                <div class="user-name">
                  {{ isBuyer ? '卖家' : '买家' }} #{{ isBuyer ? order.sellerId : order.buyerId }}
                </div>
                <a-tag color="green" size="small">信用分 100</a-tag>
              </div>
            </div>
          </a-card>
        </div>
      </div>
    </a-spin>

    <a-modal
      v-model:visible="verifyVisible"
      title="核销确认码"
      @ok="handleVerify"
      @cancel="verifyVisible = false"
      :confirm-loading="verifyLoading"
      ok-text="确认核销"
    >
      <div class="verify-form">
        <p>请输入对方提供的确认码进行核销：</p>
        <a-input
          v-model="verifyCode"
          placeholder="请输入确认码"
          style="width: 100%"
        />
      </div>
    </a-modal>
  </div>
</template>

<style scoped>
.order-detail-page {
  max-width: 1200px;
  margin: 0 auto;
  padding: 24px;
}

.detail-container {
  display: flex;
  gap: 24px;
  align-items: flex-start;
}

.main-section {
  flex: 1;
  display: flex;
  flex-direction: column;
  gap: 16px;
}

.side-section {
  flex: 0 0 320px;
  display: flex;
  flex-direction: column;
  gap: 16px;
}

.status-card {
  border-radius: 8px;
}

.status-header {
  display: flex;
  align-items: center;
  gap: 16px;
}

.order-id {
  font-size: 14px;
  color: #86909c;
}

.goods-card {
  border-radius: 8px;
  cursor: pointer;
  transition: box-shadow 0.2s;
}

.goods-card:hover {
  box-shadow: 0 4px 16px rgba(0, 0, 0, 0.08);
}

.goods-info {
  display: flex;
  gap: 16px;
  align-items: center;
}

.goods-image {
  width: 100px;
  height: 100px;
  border-radius: 8px;
  overflow: hidden;
  flex-shrink: 0;
}

.goods-image img {
  width: 100%;
  height: 100%;
  object-fit: cover;
}

.goods-detail {
  flex: 1;
}

.goods-title {
  margin: 0 0 8px 0;
  font-size: 16px;
  font-weight: 500;
}

.goods-price {
  margin: 0 0 8px 0;
  font-size: 14px;
  color: #4e5969;
}

.goods-price .price {
  color: #f53f3f;
  font-weight: 600;
  font-size: 18px;
}

.click-hint {
  margin: 0;
  font-size: 12px;
  color: #86909c;
}

.info-card {
  border-radius: 8px;
}

.price-text {
  color: #f53f3f;
  font-weight: 600;
}

.confirm-code {
  font-family: monospace;
  font-size: 16px;
  font-weight: 600;
  color: #165dff;
  letter-spacing: 2px;
}

.action-card {
  border-radius: 8px;
}

.action-buttons {
  display: flex;
  flex-direction: column;
  gap: 12px;
}

.user-info {
  display: flex;
  gap: 12px;
  align-items: center;
}

.user-detail {
  flex: 1;
}

.user-name {
  font-size: 14px;
  font-weight: 500;
  margin-bottom: 6px;
}

.verify-form {
  padding: 8px 0;
}

.verify-form p {
  margin: 0 0 12px 0;
  color: #4e5969;
}

@media (max-width: 1024px) {
  .detail-container {
    flex-direction: column;
  }

  .side-section {
    flex: none;
    width: 100%;
  }
}
</style>
