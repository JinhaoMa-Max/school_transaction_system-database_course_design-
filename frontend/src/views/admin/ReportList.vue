<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { getReportList, handleReport } from '@/api'
import type { Report } from '@/types'

const reports = ref<Report[]>([])

onMounted(async () => {
  const res = await getReportList()
  reports.value = res.data.list
})

const handleProcess = async (reportId: number, status: 'processing' | 'resolved' | 'rejected') => {
  await handleReport(reportId, { status })
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

const getTypeText = (type: string) => {
  const map: Record<string, string> = {
    goods: '商品',
    user: '用户',
    order: '订单'
  }
  return map[type] || type
}
</script>

<template>
  <div class="admin-report-list-page">
    <h2>举报管理</h2>
    <table class="report-table">
      <thead>
        <tr>
          <th>举报ID</th>
          <th>举报类型</th>
          <th>举报人ID</th>
          <th>被举报对象</th>
          <th>举报原因</th>
          <th>状态</th>
          <th>举报时间</th>
          <th>操作</th>
        </tr>
      </thead>
      <tbody>
        <tr v-for="item in reports" :key="item.reportId">
          <td>{{ item.reportId }}</td>
          <td>{{ getTypeText(item.reportType) }}</td>
          <td>{{ item.reporterId }}</td>
          <td>
            <span v-if="item.reportedGoodsId">商品ID: {{ item.reportedGoodsId }}</span>
            <span v-else-if="item.reportedUserId">用户ID: {{ item.reportedUserId }}</span>
            <span v-else-if="item.reportedOrderId">订单ID: {{ item.reportedOrderId }}</span>
          </td>
          <td>{{ item.reason }}</td>
          <td>{{ getStatusText(item.status) }}</td>
          <td>{{ item.reportTime }}</td>
          <td>
            <button v-if="item.status === 'pending'" @click="handleProcess(item.reportId, 'processing')">处理中</button>
            <button v-if="item.status !== 'resolved'" @click="handleProcess(item.reportId, 'resolved')">已处理</button>
            <button v-if="item.status !== 'rejected'" @click="handleProcess(item.reportId, 'rejected')">驳回</button>
          </td>
        </tr>
      </tbody>
    </table>
  </div>
</template>

<style scoped>
.admin-report-list-page {
  padding: 20px;
}

.report-table {
  width: 100%;
  border-collapse: collapse;
}

.report-table th,
.report-table td {
  padding: 12px;
  text-align: left;
  border-bottom: 1px solid #e5e5e5;
}

.report-table th {
  background: #f5f5f5;
}

button {
  padding: 6px 12px;
  margin-right: 8px;
  border: none;
  border-radius: 4px;
  cursor: pointer;
  font-size: 14px;
}

button:nth-child(1) {
  background: #faad14;
  color: white;
}

button:nth-child(2) {
  background: #52c41a;
  color: white;
}

button:nth-child(3) {
  background: #ff4d4f;
  color: white;
}
</style>
