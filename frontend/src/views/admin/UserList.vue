<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { getUserList, banUser, unbanUser } from '@/api'
import type { User } from '@/types'

const users = ref<User[]>([])

onMounted(async () => {
  const res = await getUserList()
  users.value = res.data.list
})

const handleBan = async (userId: number) => {
  await banUser(userId)
}

const handleUnban = async (userId: number) => {
  await unbanUser(userId)
}

const getRoleText = (role: string) => {
  const map: Record<string, string> = {
    buyer: '买家',
    seller: '卖家',
    admin: '管理员'
  }
  return map[role] || role
}

const getStatusText = (status: string) => {
  const map: Record<string, string> = {
    normal: '正常',
    banned: '封禁'
  }
  return map[status] || status
}
</script>

<template>
  <div class="admin-user-list-page">
    <h2>用户管理</h2>
    <table class="user-table">
      <thead>
        <tr>
          <th>用户ID</th>
          <th>用户名</th>
          <th>昵称</th>
          <th>角色</th>
          <th>状态</th>
          <th>信用分</th>
          <th>注册时间</th>
          <th>操作</th>
        </tr>
      </thead>
      <tbody>
        <tr v-for="item in users" :key="item.userId">
          <td>{{ item.userId }}</td>
          <td>{{ item.username }}</td>
          <td>{{ item.nickname }}</td>
          <td>{{ getRoleText(item.role) }}</td>
          <td>{{ getStatusText(item.status) }}</td>
          <td>{{ item.creditScore }}</td>
          <td>{{ item.registerTime }}</td>
          <td>
            <button v-if="item.status === 'normal'" @click="handleBan(item.userId)">封禁</button>
            <button v-else @click="handleUnban(item.userId)">解封</button>
          </td>
        </tr>
      </tbody>
    </table>
  </div>
</template>

<style scoped>
.admin-user-list-page {
  padding: 20px;
}

.user-table {
  width: 100%;
  border-collapse: collapse;
}

.user-table th,
.user-table td {
  padding: 12px;
  text-align: left;
  border-bottom: 1px solid #e5e5e5;
}

.user-table th {
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

button:first-child {
  background: #ff4d4f;
  color: white;
}

button:last-child {
  background: #52c41a;
  color: white;
}
</style>
