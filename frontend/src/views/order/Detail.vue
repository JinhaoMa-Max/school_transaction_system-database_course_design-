<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { getOrderById, cancelOrder, completeOrder, startMeet } from '@/api'
import { getAppointmentByOrderId, createAppointment, verifyConfirmCode } from '@/api'
import type { TradeOrder, Appointment } from '@/types'

const route = useRoute()
const router = useRouter()
const orderId = Number(route.params.id)

const order = ref<TradeOrder | null>(null)
const appointment = ref<Appointment | null>(null)
const meetTime = ref('')
const meetLocation = ref('')
const confirmCode = ref('')

onMounted(async () => {
  const res = await getOrderById(orderId)
  order.value = res.data
  
  try {
    const appRes = await getAppointmentByOrderId(orderId)
    appointment.value = appRes.data
  } catch {
    appointment.value = null
  }
})

const handleCancel = async () => {
  await cancelOrder(orderId)
  router.push('/orders')
}

const handleStartMeet = async () => {
  await startMeet(orderId)
}

const handleComplete = async () => {
  await completeOrder(orderId)
  router.push('/orders')
}

const handleCreateAppointment = async () => {
  await createAppointment({ orderId, meetTime: meetTime.value, meetLocation: meetLocation.value })
}

const handleVerifyCode = async () => {
  await verifyConfirmCode({ orderId, confirmCode: confirmCode.value })
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

const getAppointmentStatusText = (status: string) => {
  const map: Record<string, string> = {
    pending: '待确认',
    confirmed: '已确认',
    completed: '已完成',
    cancelled: '已取消'
  }
  return map[status] || status
}
</script>

<template>
  <div class="order-detail-page">
    <div v-if="order" class="order-detail">
      <h2>订单详情</h2>
      <div class="order-info">
        <p>订单号: {{ order.orderId }}</p>
        <p>商品ID: {{ order.goodsId }}</p>
        <p>买家ID: {{ order.buyerId }}</p>
        <p>卖家ID: {{ order.sellerId }}</p>
        <p>成交价: ¥{{ order.dealPrice }}</p>
        <p>订单状态: {{ getStatusText(order.status) }}</p>
        <p>下单时间: {{ order.createTime }}</p>
      </div>
      <div class="actions">
        <button v-if="order.status === 'pending_meet'" @click="handleStartMeet">开始面交</button>
        <button v-if="order.status === 'in_meet'" @click="handleComplete">确认完成</button>
        <button v-if="order.status !== 'completed' && order.status !== 'cancelled'" @click="handleCancel">取消订单</button>
      </div>
      <div class="appointment-section">
        <h3>面交预约</h3>
        <div v-if="appointment" class="appointment-info">
          <p>预约时间: {{ appointment.meetTime }}</p>
          <p>预约地点: {{ appointment.meetLocation }}</p>
          <p>确认码: {{ appointment.confirmCode }}</p>
          <p>预约状态: {{ getAppointmentStatusText(appointment.status) }}</p>
        </div>
        <div v-else-if="order.status === 'pending_meet'" class="create-appointment">
          <input v-model="meetTime" type="datetime-local" placeholder="面交时间" />
          <input v-model="meetLocation" type="text" placeholder="面交地点" />
          <button @click="handleCreateAppointment">创建预约</button>
        </div>
        <div v-if="order.status === 'in_meet'" class="verify-section">
          <input v-model="confirmCode" type="text" placeholder="输入确认码" />
          <button @click="handleVerifyCode">核销确认码</button>
        </div>
      </div>
    </div>
  </div>
</template>

<style scoped>
.order-detail-page {
  padding: 20px;
}

.order-detail {
  max-width: 600px;
}

.order-info p {
  margin: 0 0 8px 0;
  font-size: 16px;
}

.actions {
  margin: 24px 0;
}

.actions button {
  padding: 12px 24px;
  margin-right: 12px;
  border: none;
  border-radius: 4px;
  font-size: 16px;
  cursor: pointer;
}

.actions button:nth-child(1) {
  background: #52c41a;
  color: white;
}

.actions button:nth-child(2) {
  background: #165dff;
  color: white;
}

.actions button:nth-child(3) {
  background: #ff4d4f;
  color: white;
}

.appointment-section {
  margin-top: 24px;
  padding-top: 24px;
  border-top: 1px solid #e5e5e5;
}

.appointment-section h3 {
  margin: 0 0 16px 0;
}

.appointment-info p {
  margin: 0 0 8px 0;
  font-size: 14px;
}

.create-appointment input {
  padding: 10px;
  margin-right: 12px;
  border: 1px solid #e5e5e5;
  border-radius: 4px;
}

.create-appointment button,
.verify-section button {
  padding: 10px 20px;
  background: #165dff;
  color: white;
  border: none;
  border-radius: 4px;
  cursor: pointer;
}

.verify-section {
  margin-top: 16px;
}

.verify-section input {
  padding: 10px;
  width: 200px;
  border: 1px solid #e5e5e5;
  border-radius: 4px;
  margin-right: 12px;
}
</style>
