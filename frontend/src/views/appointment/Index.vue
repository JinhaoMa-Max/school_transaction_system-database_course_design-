<script setup lang="ts">
import { ref, reactive, onMounted } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { Message, Modal } from '@arco-design/web-vue'
import {
  getAppointmentByOrderId,
  createAppointment,
  confirmAppointment,
  cancelAppointment,
  startMeet,
  getOrderById,
  getGoodsById
} from '@/api'
import type { Appointment, TradeOrder, Goods } from '@/types'

const route = useRoute()
const router = useRouter()

const orderId = Number(route.params.orderId)

const appointment = ref<Appointment | null>(null)
const order = ref<TradeOrder | null>(null)
const goods = ref<Goods | null>(null)
const loading = ref(false)
const submitLoading = ref(false)
const actionLoading = ref(false)

const form = reactive({
  meetTime: '',
  meetLocation: ''
})

const formRef = ref()

const campusLocations = [
  { label: '图书馆门口', value: '图书馆门口' },
  { label: '食堂一楼', value: '食堂一楼' },
  { label: '教学楼A座大厅', value: '教学楼A座大厅' },
  { label: '宿舍楼栋门口', value: '宿舍楼栋门口' },
  { label: '体育场入口', value: '体育场入口' },
  { label: '校门口', value: '校门口' },
  { label: '其他地点', value: '其他地点' }
]

const appointmentStatusMap: Record<string, { text: string; color: string }> = {
  pending: { text: '待确认', color: 'orange' },
  confirmed: { text: '已确认', color: 'blue' },
  completed: { text: '已完成', color: 'green' },
  cancelled: { text: '已取消', color: 'gray' }
}

const disabledDate = (current?: Date) => {
  if (!current) return false
  return current.getTime() < Date.now() - 24 * 60 * 60 * 1000
}

const fetchData = async () => {
  loading.value = true
  try {
    const [appRes, orderRes] = await Promise.all([
      getAppointmentByOrderId(orderId).catch(() => ({ data: null })),
      getOrderById(orderId).catch(() => ({ data: null }))
    ])
    appointment.value = appRes.data
    order.value = orderRes.data

    if (order.value) {
      const goodsRes = await getGoodsById(order.value.goodsId).catch(() => ({ data: null }))
      goods.value = goodsRes.data
    }
  } catch {
  } finally {
    loading.value = false
  }
}

const handleSubmit = async ({ values }: { values: Record<string, any> }) => {
  submitLoading.value = true
  try {
    await createAppointment({
      orderId,
      meetTime: values.meetTime,
      meetLocation: values.meetLocation
    })
    Message.success('预约创建成功')
    router.push(`/orders/${orderId}`)
  } catch {
  } finally {
    submitLoading.value = false
  }
}

const handleBack = () => {
  router.push(`/orders/${orderId}`)
}

const handleConfirm = async () => {
  if (!appointment.value) return
  actionLoading.value = true
  try {
    await confirmAppointment(appointment.value.appointmentId)
    Message.success('预约已确认')
    await fetchData()
  } finally {
    actionLoading.value = false
  }
}

const handleStartMeet = async () => {
  actionLoading.value = true
  try {
    await startMeet(orderId)
    Message.success('已开始面交，请完成后核销确认码')
    await fetchData()
  } finally {
    actionLoading.value = false
  }
}

const handleCancelAppointment = () => {
  if (!appointment.value) return
  const appointmentId = appointment.value.appointmentId
  Modal.confirm({
    title: '取消预约',
    content: '确定取消本次面交预约吗？取消后仍可重新预约。',
    okText: '确认取消',
    cancelText: '返回',
    onOk: async () => {
      actionLoading.value = true
      try {
        await cancelAppointment(appointmentId)
        Message.success('预约已取消，可以重新选择时间和地点')
        await fetchData()
      } finally {
        actionLoading.value = false
      }
    }
  })
}

onMounted(fetchData)
</script>

<template>
  <div class="appointment-page">
    <a-spin :loading="loading" dot>
      <div class="appointment-container">
        <div class="page-header">
          <a-button type="text" @click="handleBack">
            <icon-left />
            返回订单详情
          </a-button>
          <h2 class="page-title">面交预约</h2>
        </div>

        <div v-if="appointment && appointment.status !== 'cancelled'" class="appointment-detail">
          <a-card>
            <template #title>
              <div class="card-title">
                <span>预约信息</span>
                <a-tag :color="appointmentStatusMap[appointment.status]?.color || 'default'" size="large">
                  {{ appointmentStatusMap[appointment.status]?.text || appointment.status }}
                </a-tag>
              </div>
            </template>

            <div v-if="goods" class="goods-info" @click="router.push(`/goods/${goods.goodsId}`)">
              <div class="goods-image">
                <img :src="goods.imageUrl || 'https://via.placeholder.com/80x80?text=No+Image'" :alt="goods.title" />
              </div>
              <div class="goods-detail">
                <div class="goods-title">{{ goods.title }}</div>
                <div class="goods-price">¥{{ order?.dealPrice || goods.price }}</div>
              </div>
            </div>

            <a-divider />

            <a-descriptions bordered :column="1" size="medium">
              <a-descriptions-item label="面交时间">{{ appointment.meetTime }}</a-descriptions-item>
              <a-descriptions-item label="面交地点">{{ appointment.meetLocation }}</a-descriptions-item>
              <a-descriptions-item label="确认码">
                <span class="confirm-code">{{ appointment.confirmCode }}</span>
              </a-descriptions-item>
              <a-descriptions-item label="创建时间">{{ appointment.createTime }}</a-descriptions-item>
            </a-descriptions>

            <a-alert type="info" style="margin-top: 16px">
              <template #content>
                <p>请妥善保管确认码，面交时需向对方出示此确认码进行核销。</p>
              </template>
            </a-alert>

            <div class="detail-actions">
              <a-button
                v-if="appointment.status === 'pending'"
                type="primary"
                :loading="actionLoading"
                @click="handleConfirm"
              >
                确认预约
              </a-button>
              <a-button
                v-if="appointment.status === 'confirmed' && order?.status === 'pending_meet'"
                type="primary"
                :loading="actionLoading"
                @click="handleStartMeet"
              >
                开始面交
              </a-button>
              <a-button
                v-if="appointment.status === 'confirmed' && order?.status === 'in_meet'"
                type="primary"
                @click="handleBack"
              >
                返回订单核销
              </a-button>
              <a-button
                v-if="appointment.status === 'pending' || appointment.status === 'confirmed'"
                status="danger"
                :disabled="actionLoading"
                @click="handleCancelAppointment"
              >
                取消预约
              </a-button>
            </div>
          </a-card>
        </div>

        <div v-else class="appointment-form">
          <a-card :title="appointment?.status === 'cancelled' ? '重新预约面交' : '预约面交'">
            <a-alert v-if="appointment?.status === 'cancelled'" type="warning" style="margin-bottom: 16px">
              上一次预约已取消，请重新选择面交时间和地点。
            </a-alert>
            <div v-if="goods" class="goods-info">
              <div class="goods-image">
                <img :src="goods.imageUrl || 'https://via.placeholder.com/80x80?text=No+Image'" :alt="goods.title" />
              </div>
              <div class="goods-detail">
                <div class="goods-title">{{ goods.title }}</div>
                <div class="goods-price">¥{{ order?.dealPrice || goods.price }}</div>
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
                field="meetTime"
                label="面交时间"
                :rules="[{ required: true, message: '请选择面交时间' }]"
              >
                <a-date-picker
                  v-model="form.meetTime"
                  type="datetime"
                  style="width: 100%"
                  placeholder="请选择面交时间"
                  :disabled-date="disabledDate"
                  format="YYYY-MM-DD HH:mm"
                />
              </a-form-item>

              <a-form-item
                field="meetLocation"
                label="面交地点"
                :rules="[{ required: true, message: '请选择或输入面交地点' }]"
              >
                <a-select
                  v-model="form.meetLocation"
                  placeholder="请选择面交地点"
                  allow-create
                  allow-search
                >
                  <a-option
                    v-for="item in campusLocations"
                    :key="item.value"
                    :value="item.value"
                  >
                    {{ item.label }}
                  </a-option>
                </a-select>
              </a-form-item>

              <div class="form-actions">
                <a-button size="large" @click="handleBack">
                  取消
                </a-button>
                <a-button type="primary" html-type="submit" size="large" :loading="submitLoading">
                  提交预约
                </a-button>
              </div>
            </a-form>
          </a-card>
        </div>
      </div>
    </a-spin>
  </div>
</template>

<style scoped>
.appointment-page {
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

.detail-actions {
  display: flex;
  gap: 12px;
  justify-content: flex-end;
  margin-top: 20px;
}

.card-title {
  display: flex;
  align-items: center;
  justify-content: space-between;
}

.goods-info {
  display: flex;
  gap: 16px;
  padding: 8px 0;
  cursor: pointer;
  transition: opacity 0.2s;
}

.goods-info:hover {
  opacity: 0.8;
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

.confirm-code {
  font-family: monospace;
  font-size: 18px;
  font-weight: bold;
  color: #165dff;
  letter-spacing: 2px;
}

.form-actions {
  display: flex;
  justify-content: flex-end;
  gap: 12px;
  margin-top: 24px;
}
</style>
