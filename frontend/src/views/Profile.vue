<script setup lang="ts">
import { computed } from 'vue'
import { useRouter } from 'vue-router'
import { useUserStore } from '@/stores'

const router = useRouter()
const userStore = useUserStore()

const user = computed(() => userStore.user)

const goToStudentAuth = () => {
  router.push('/student-auth')
}

const goToAdmin = () => {
  router.push('/admin/dashboard')
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
  <div class="profile-page">
    <h2>个人中心</h2>
    <div v-if="user" class="profile-info">
      <div class="avatar">
        <img :src="user.avatarUrl || 'https://via.placeholder.com/100'" alt="头像" />
      </div>
      <div class="info">
        <h3>{{ user.nickname }}</h3>
        <p>用户名: {{ user.username }}</p>
        <p>角色: {{ getRoleText(user.role) }}</p>
        <p>状态: {{ getStatusText(user.status) }}</p>
        <p>信用分: {{ user.creditScore }}</p>
        <p>手机号: {{ user.phone }}</p>
        <p>邮箱: {{ user.email }}</p>
        <p>注册时间: {{ user.registerTime }}</p>
      </div>
    </div>
    <div class="actions">
      <button v-if="user?.role !== 'admin'" @click="goToStudentAuth">学生认证</button>
      <button v-if="user?.role === 'admin'" @click="goToAdmin">后台管理</button>
      <button @click="userStore.logout">退出登录</button>
    </div>
  </div>
</template>

<style scoped>
.profile-page {
  padding: 20px;
}

.profile-info {
  display: flex;
  gap: 24px;
}

.avatar img {
  width: 100px;
  height: 100px;
  border-radius: 50%;
  object-fit: cover;
}

.info h3 {
  margin: 0 0 16px 0;
  font-size: 24px;
}

.info p {
  margin: 0 0 8px 0;
  font-size: 16px;
}

.actions {
  margin-top: 24px;
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
  background: #165dff;
  color: white;
}

.actions button:nth-child(2) {
  background: #52c41a;
  color: white;
}

.actions button:last-child {
  background: #ff4d4f;
  color: white;
}
</style>
