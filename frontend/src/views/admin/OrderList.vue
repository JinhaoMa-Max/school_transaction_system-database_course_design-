<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { getOrderList } from '@/api'
import type { TradeOrder } from '@/types'

const orders = ref<TradeOrder[]>([])

onMounted(async () => {
  const res = await getOrderList()
  orders.value = res.data.list
})

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
  <div class="admin-order-list-page">
    <h2>订单管理</h2>
    <table class="order-table">
      <thead>
        <tr>
          <th>订单ID</th>
          <th>商品ID</th>
          <th>买家ID</th>
          <th>卖家ID</th>
          <th>成交价</th>
          <th>状态</th>
          <th>下单时间</th>
        </tr>
      </thead>
      <tbody>
        <tr v-for="item in orders" :key="item.orderId">
          <td>{{ item.orderId }}</td>
          <td>{{ item.goodsId }}</td>
          <td>{{ item.buyerId }}</td>
          <td>{{ item.sellerId }}</td>
          <td>¥{{ item.dealPrice }}</td>
          <td>{{ getStatusText(item.status) }}</td>
          <td>{{ item.createTime }}</td>
        </tr>
      </tbody>
    </table>
  </div>
</template>

<style scoped>
.admin-order-list-page {
  padding: 20px;
}

.order-table {
  width: 100%;
  border-collapse: collapse;
}

.order-table th,
.order-table td {
  padding: 12px;
  text-align: left;
  border-bottom: 1px solid #e5e5e5;
}

.order-table th {
  background: #f5f5f5;
}
</style>
