<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { getAuditLogList } from '@/api'
import type { AuditLog } from '@/types'

const logs = ref<AuditLog[]>([])

onMounted(async () => {
  const res = await getAuditLogList()
  logs.value = res.data.list
})

const getAuditTypeText = (type: string) => {
  const map: Record<string, string> = {
    goods_audit: '商品审核',
    report_handle: '举报处理',
    user_ban: '用户封禁',
    goods_offline: '商品下架'
  }
  return map[type] || type
}

const getActionText = (action: string) => {
  const map: Record<string, string> = {
    approve: '通过',
    reject: '驳回',
    ban: '封禁',
    offline: '下架'
  }
  return map[action] || action
}
</script>

<template>
  <div class="admin-audit-logs-page">
    <h2>审核日志</h2>
    <table class="log-table">
      <thead>
        <tr>
          <th>日志ID</th>
          <th>管理员ID</th>
          <th>审核类型</th>
          <th>目标ID</th>
          <th>处理动作</th>
          <th>处理结果</th>
          <th>备注</th>
          <th>处理时间</th>
        </tr>
      </thead>
      <tbody>
        <tr v-for="item in logs" :key="item.logId">
          <td>{{ item.logId }}</td>
          <td>{{ item.adminId }}</td>
          <td>{{ getAuditTypeText(item.auditType) }}</td>
          <td>{{ item.targetId }}</td>
          <td>{{ getActionText(item.action) }}</td>
          <td>{{ item.result }}</td>
          <td>{{ item.remark || '-' }}</td>
          <td>{{ item.handleTime }}</td>
        </tr>
      </tbody>
    </table>
  </div>
</template>

<style scoped>
.admin-audit-logs-page {
  padding: 20px;
}

.log-table {
  width: 100%;
  border-collapse: collapse;
}

.log-table th,
.log-table td {
  padding: 12px;
  text-align: left;
  border-bottom: 1px solid #e5e5e5;
}

.log-table th {
  background: #f5f5f5;
}
</style>
