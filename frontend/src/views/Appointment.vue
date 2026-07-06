<script setup lang="ts">
import { ref } from 'vue'
import type { Appointment } from '@/types'

const appointments = ref<Appointment[]>([])

const getStatusText = (status: string) => {
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
  <div class="appointment-page">
    <h2>我的预约</h2>
    <div class="appointment-list">
      <div 
        v-for="item in appointments" 
        :key="item.appointmentId" 
        class="appointment-item"
      >
        <div class="appointment-info">
          <h3>预约ID: {{ item.appointmentId }}</h3>
          <p>订单ID: {{ item.orderId }}</p>
          <p>面交时间: {{ item.meetTime }}</p>
          <p>面交地点: {{ item.meetLocation }}</p>
          <p>确认码: {{ item.confirmCode }}</p>
          <p>预约状态: {{ getStatusText(item.status) }}</p>
          <p>创建时间: {{ item.createTime }}</p>
        </div>
      </div>
    </div>
    <div v-if="appointments.length === 0" class="empty">
      <p>暂无预约记录</p>
    </div>
  </div>
</template>

<style scoped>
.appointment-page {
  padding: 20px;
}

.appointment-list {
  display: flex;
  flex-direction: column;
  gap: 12px;
}

.appointment-item {
  padding: 16px;
  background: white;
  border-radius: 8px;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.08);
}

.appointment-info h3 {
  margin: 0 0 8px 0;
}

.appointment-info p {
  margin: 0 0 4px 0;
  font-size: 14px;
}

.empty {
  text-align: center;
  padding: 40px;
  color: #999;
}
</style>
