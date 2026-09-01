<script setup lang="ts">
import { onMounted, ref } from 'vue'
import { Message } from '@arco-design/web-vue'
import { getStudentAuthList, updateStudentAuth } from '@/api'
import type { StudentAuthAdmin } from '@/types'

const records = ref<StudentAuthAdmin[]>([])
const loading = ref(false)
const status = ref('pending')

const fetchRecords = async () => {
  loading.value = true
  try {
    const res = await getStudentAuthList({
      page: 1,
      size: 100,
      status: status.value || undefined
    })
    records.value = res.data.list
  } finally {
    loading.value = false
  }
}

const handleAudit = async (record: StudentAuthAdmin, authStatus: 'approved' | 'rejected') => {
  await updateStudentAuth(record.authId, { authStatus })
  record.authStatus = authStatus
  Message.success(authStatus === 'approved' ? '学生认证已通过' : '学生认证已驳回')
  if (status.value === 'pending') {
    records.value = records.value.filter(item => item.authId !== record.authId)
  }
}

onMounted(fetchRecords)
</script>

<template>
  <div class="page">
    <div class="header">
      <h2>学生认证审核</h2>
      <select v-model="status" @change="fetchRecords">
        <option value="pending">待审核</option>
        <option value="approved">已通过</option>
        <option value="rejected">已驳回</option>
        <option value="">全部</option>
      </select>
    </div>

    <a-spin :loading="loading">
      <table>
        <thead>
          <tr>
            <th>用户</th>
            <th>学号</th>
            <th>真实姓名</th>
            <th>学院</th>
            <th>状态</th>
            <th>操作</th>
          </tr>
        </thead>
        <tbody>
          <tr v-for="item in records" :key="item.authId">
            <td>{{ item.nickname || item.username }}（{{ item.username }}）</td>
            <td>{{ item.studentId }}</td>
            <td>{{ item.realName }}</td>
            <td>{{ item.college }}</td>
            <td>{{ item.authStatus }}</td>
            <td>
              <template v-if="item.authStatus === 'pending'">
                <button class="approve" @click="handleAudit(item, 'approved')">通过</button>
                <button class="reject" @click="handleAudit(item, 'rejected')">驳回</button>
              </template>
              <span v-else>-</span>
            </td>
          </tr>
          <tr v-if="!loading && records.length === 0">
            <td colspan="6" class="empty">暂无认证记录</td>
          </tr>
        </tbody>
      </table>
    </a-spin>
  </div>
</template>

<style scoped>
.page { padding: 20px; }
.header { display: flex; align-items: center; justify-content: space-between; }
.header select { padding: 8px 12px; border: 1px solid #ddd; border-radius: 4px; }
table { width: 100%; border-collapse: collapse; }
th, td { padding: 12px; text-align: left; border-bottom: 1px solid #e5e5e5; }
th { background: #f5f5f5; }
button { margin-right: 8px; padding: 6px 12px; border: 0; border-radius: 4px; color: white; cursor: pointer; }
.approve { background: #52c41a; }
.reject { background: #ff4d4f; }
.empty { text-align: center; color: #999; }
</style>
