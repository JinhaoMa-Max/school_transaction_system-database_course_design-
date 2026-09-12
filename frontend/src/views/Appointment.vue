<script setup lang="ts">
import { ref, onMounted } from 'vue'
import GoodsImage from '@/components/common/GoodsImage.vue'
import { useRouter } from 'vue-router'
import { getAppointmentList } from '@/api'
import type { Appointment } from '@/types'

const router = useRouter()
const appointments = ref<Appointment[]>([])
const loading = ref(false)
const page = ref(1)
const pageSize = ref(20)
const total = ref(0)

const fetchAppointments = async () => {
  loading.value = true
  try {
    const res = await getAppointmentList({ page: page.value, size: pageSize.value })
    appointments.value = res.data.list
    total.value = res.data.total
  } finally {
    loading.value = false
  }
}

const handlePageChange = (value: number) => {
  page.value = value
  fetchAppointments()
}

const getStatusText = (status: string) => {
  const map: Record<string, string> = {
    pending: '待确认',
    confirmed: '已确认',
    completed: '已完成',
    cancelled: '已取消'
  }
  return map[status] || status
}

onMounted(fetchAppointments)
</script>

<template>
  <div class="appointment-page">
    <h2>我的预约</h2>
    <a-spin :loading="loading" style="width: 100%">
    <div class="appointment-list">
      <div 
        v-for="item in appointments" 
        :key="item.appointmentId" 
        class="appointment-item"
        @click="router.push(`/orders/${item.orderId}`)"
      >
        <div class="appointment-info">
          <GoodsImage :src="item.imageUrl" :alt="item.goodsTitle" style="width: 80px; height: 80px; border-radius: 8px" />
          <h3>{{ item.goodsTitle || '商品信息暂不可用' }}</h3>
          <p>买家：{{ item.buyerName || '用户信息暂不可用' }} · 卖家：{{ item.sellerName || '用户信息暂不可用' }}</p>
          <p>订单编号: {{ item.orderId }}</p>
          <p>面交时间: {{ item.meetTime }}</p>
          <p>面交地点: {{ item.meetLocation }}</p>
          <p>确认码: {{ item.confirmCode }}</p>
          <p>预约状态: {{ getStatusText(item.status) }}</p>
          <p>创建时间: {{ item.createTime }}</p>
        </div>
      </div>
    </div>
    <div v-if="!loading && appointments.length === 0" class="empty">
      <p>暂无预约记录</p>
    </div>
    <a-pagination
      v-if="total > pageSize"
      :current="page"
      :page-size="pageSize"
      :total="total"
      @change="handlePageChange"
    />
    </a-spin>
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
  cursor: pointer;
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
