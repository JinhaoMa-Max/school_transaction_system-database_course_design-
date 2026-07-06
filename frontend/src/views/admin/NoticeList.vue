<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { getNoticeList, createNotice, deleteNotice } from '@/api'
import type { Notice } from '@/types'

const notices = ref<Notice[]>([])
const newNotice = ref({
  title: '',
  content: '',
  noticeType: 'system' as const
})

onMounted(async () => {
  const res = await getNoticeList()
  notices.value = res.data.list
})

const handleCreate = async () => {
  await createNotice(newNotice.value)
}

const handleUpdate = async (_noticeId: number) => {
  
}

const handleDelete = async (noticeId: number) => {
  await deleteNotice(noticeId)
}

const getTypeText = (type: string) => {
  const map: Record<string, string> = {
    system: '系统公告',
    transaction: '交易须知',
    violation: '违规提醒'
  }
  return map[type] || type
}
</script>

<template>
  <div class="admin-notice-list-page">
    <h2>公告管理</h2>
    <div class="create-section">
      <h3>发布公告</h3>
      <div class="form-item">
        <label>标题</label>
        <input v-model="newNotice.title" type="text" />
      </div>
      <div class="form-item">
        <label>类型</label>
        <select v-model="newNotice.noticeType">
          <option value="system">系统公告</option>
          <option value="transaction">交易须知</option>
          <option value="violation">违规提醒</option>
        </select>
      </div>
      <div class="form-item">
        <label>内容</label>
        <textarea v-model="newNotice.content" rows="5"></textarea>
      </div>
      <button @click="handleCreate">发布</button>
    </div>
    <table class="notice-table">
      <thead>
        <tr>
          <th>公告ID</th>
          <th>标题</th>
          <th>类型</th>
          <th>发布者ID</th>
          <th>发布时间</th>
          <th>操作</th>
        </tr>
      </thead>
      <tbody>
        <tr v-for="item in notices" :key="item.noticeId">
          <td>{{ item.noticeId }}</td>
          <td>{{ item.title }}</td>
          <td>{{ getTypeText(item.noticeType) }}</td>
          <td>{{ item.publisherId }}</td>
          <td>{{ item.publishTime }}</td>
          <td>
            <button @click="handleUpdate(item.noticeId)">编辑</button>
            <button @click="handleDelete(item.noticeId)">删除</button>
          </td>
        </tr>
      </tbody>
    </table>
  </div>
</template>

<style scoped>
.admin-notice-list-page {
  padding: 20px;
}

.create-section {
  padding: 20px;
  background: white;
  border-radius: 8px;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.08);
  margin-bottom: 20px;
}

.create-section h3 {
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

.create-section button {
  padding: 12px 24px;
  background: #165dff;
  color: white;
  border: none;
  border-radius: 4px;
  cursor: pointer;
}

.notice-table {
  width: 100%;
  border-collapse: collapse;
}

.notice-table th,
.notice-table td {
  padding: 12px;
  text-align: left;
  border-bottom: 1px solid #e5e5e5;
}

.notice-table th {
  background: #f5f5f5;
}

.notice-table button {
  padding: 6px 12px;
  margin-right: 8px;
  border: none;
  border-radius: 4px;
  cursor: pointer;
  font-size: 14px;
}

.notice-table button:first-child {
  background: #165dff;
  color: white;
}

.notice-table button:last-child {
  background: #ff4d4f;
  color: white;
}
</style>
