<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { getReportList, createReport } from '@/api'
import type { Report } from '@/types'

const reports = ref<Report[]>([])
const newReport = ref({
  reportType: 'goods' as 'goods' | 'user' | 'order',
  reportedGoodsId: 0,
  reportedUserId: 0,
  reportedOrderId: 0,
  reason: ''
})

onMounted(async () => {
  const res = await getReportList()
  reports.value = res.data.list
})

const handleCreate = async () => {
  await createReport(newReport.value)
}

const getStatusText = (status: string) => {
  const map: Record<string, string> = {
    pending: '待处理',
    processing: '处理中',
    resolved: '已处理',
    rejected: '已驳回'
  }
  return map[status] || status
}
</script>

<template>
  <div class="report-page">
    <h2>我的举报</h2>
    <div class="report-list">
      <div 
        v-for="item in reports" 
        :key="item.reportId" 
        class="report-item"
      >
        <div class="report-info">
          <h3>举报ID: {{ item.reportId }}</h3>
          <p>举报类型: {{ item.reportType }}</p>
          <p>被举报商品ID: {{ item.reportedGoodsId || '-' }}</p>
          <p>被举报用户ID: {{ item.reportedUserId || '-' }}</p>
          <p>被举报订单ID: {{ item.reportedOrderId || '-' }}</p>
          <p>举报原因: {{ item.reason }}</p>
          <p>举报状态: {{ getStatusText(item.status) }}</p>
          <p>举报时间: {{ item.reportTime }}</p>
        </div>
      </div>
    </div>
    <div v-if="reports.length === 0" class="empty">
      <p>暂无举报记录</p>
    </div>
    <div class="create-report">
      <h3>发起举报</h3>
      <div class="form-item">
        <label>举报类型</label>
        <select v-model="newReport.reportType">
          <option value="goods">商品</option>
          <option value="user">用户</option>
          <option value="order">订单</option>
        </select>
      </div>
      <div v-if="newReport.reportType === 'goods'" class="form-item">
        <label>被举报商品ID</label>
        <input v-model.number="newReport.reportedGoodsId" type="number" />
      </div>
      <div v-if="newReport.reportType === 'user'" class="form-item">
        <label>被举报用户ID</label>
        <input v-model.number="newReport.reportedUserId" type="number" />
      </div>
      <div v-if="newReport.reportType === 'order'" class="form-item">
        <label>被举报订单ID</label>
        <input v-model.number="newReport.reportedOrderId" type="number" />
      </div>
      <div class="form-item">
        <label>举报原因</label>
        <textarea v-model="newReport.reason" rows="3"></textarea>
      </div>
      <button @click="handleCreate">发起举报</button>
    </div>
  </div>
</template>

<style scoped>
.report-page {
  padding: 20px;
}

.report-list {
  display: flex;
  flex-direction: column;
  gap: 12px;
}

.report-item {
  padding: 16px;
  background: white;
  border-radius: 8px;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.08);
}

.report-info h3 {
  margin: 0 0 8px 0;
}

.report-info p {
  margin: 0 0 4px 0;
  font-size: 14px;
}

.empty {
  text-align: center;
  padding: 40px;
  color: #999;
}

.create-report {
  margin-top: 24px;
  padding: 20px;
  background: white;
  border-radius: 8px;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.08);
}

.create-report h3 {
  margin: 0 0 16px 0;
}

.form-item {
  margin-bottom: 16px;
}

.form-item label {
  display: block;
  margin-bottom: 8px;
}

.form-item input,
.form-item select,
.form-item textarea {
  width: 100%;
  padding: 10px;
  border: 1px solid #e5e5e5;
  border-radius: 4px;
}

button {
  padding: 12px 24px;
  background: #165dff;
  color: white;
  border: none;
  border-radius: 4px;
  cursor: pointer;
}
</style>
