<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { getOrderList } from '@/api'
import type { TradeOrder } from '@/types'

const router = useRouter()
const orders = ref<TradeOrder[]>([])

onMounted(async () => {
  const res = await getOrderList()
  orders.value = res.data.list
})

const goToDetail = (orderId: number) => {
  router.push(`/orders/${orderId}`)
}

const getStatusText = (status: string) => {
  const map: Record<string, string> = {
    pending_meet: '待面交',
    in_meet: '面交中',
    completed: '已完成',
    cancelled: '已取消'
  }
  return map[status] || status
}
</script>

<template>
  <div class="order-list-page">
    <h2>我的订单</h2>
    <div class="order-list">
      <div 
        v-for="item in orders" 
        :key="item.orderId" 
        class="order-item"
        @click="goToDetail(item.orderId)"
      >
        <div class="order-info">
          <h3>订单号: {{ item.orderId }}</h3>
          <p>商品ID: {{ item.goodsId }}</p>
          <p>成交价: ¥{{ item.dealPrice }}</p>
          <p>订单状态: {{ getStatusText(item.status) }}</p>
          <p>下单时间: {{ item.createTime }}</p>
        </div>
      </div>
    </div>
    <div v-if="orders.length === 0" class="empty">
      <p>暂无订单记录</p>
    </div>
  </div>
</template>

<style scoped>
.order-list-page {
  padding: 20px;
}

.order-list {
  display: flex;
  flex-direction: column;
  gap: 12px;
}

.order-item {
  padding: 16px;
  background: white;
  border-radius: 8px;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.08);
  cursor: pointer;
}

.order-info h3 {
  margin: 0 0 8px 0;
}

.order-info p {
  margin: 0 0 4px 0;
  font-size: 14px;
}

.empty {
  text-align: center;
  padding: 40px;
  color: #999;
}
</style>
