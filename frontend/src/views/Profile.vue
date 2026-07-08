<script setup lang="ts">
import { computed, ref } from 'vue'
import { useRouter } from 'vue-router'
import { Message } from '@arco-design/web-vue'
import { useUserStore } from '@/stores'
import { uploadAvatar } from '@/api'

const router = useRouter()
const userStore = useUserStore()

const user = computed(() => userStore.user)
const loading = ref(false)

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

const handleAvatarUpload = async (e: Event) => {
  const target = e.target as HTMLInputElement
  const file = target.files?.[0]
  if (!file) return

  if (file.size > 5 * 1024 * 1024) {
    Message.warning('头像大小不能超过5MB')
    return
  }

  const allowedTypes = ['image/jpeg', 'image/png', 'image/jpg', 'image/gif', 'image/webp']
  if (!allowedTypes.includes(file.type)) {
    Message.warning('只允许上传JPG、PNG、GIF、WebP格式的图片')
    return
  }

  loading.value = true
  try {
    const res = await uploadAvatar(file)
    userStore.user = res.data
    Message.success('头像上传成功')
  } catch {
    Message.error('头像上传失败')
  } finally {
    loading.value = false
    target.value = ''
  }
}
</script>

<template>
  <div class="profile-page">
    <h2>个人中心</h2>
    <div v-if="user" class="profile-info">
      <div class="avatar" @click="() => document.getElementById('avatar-input')?.click()">
        <img :src="user.avatarUrl || 'https://via.placeholder.com/100'" alt="头像" />
        <div class="avatar-overlay">
          <span>{{ loading ? '上传中...' : '更换头像' }}</span>
        </div>
      </div>
      <input
        id="avatar-input"
        type="file"
        accept="image/jpeg,image/png,image/jpg,image/gif,image/webp"
        style="display: none"
        @change="handleAvatarUpload"
      />
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

.avatar {
  position: relative;
  cursor: pointer;
}

.avatar img {
  width: 100px;
  height: 100px;
  border-radius: 50%;
  object-fit: cover;
}

.avatar-overlay {
  position: absolute;
  bottom: 0;
  left: 0;
  right: 0;
  height: 30px;
  background: rgba(0, 0, 0, 0.6);
  border-radius: 0 0 50% 50%;
  display: flex;
  align-items: center;
  justify-content: center;
  opacity: 0;
  transition: opacity 0.2s;
}

.avatar:hover .avatar-overlay {
  opacity: 1;
}

.avatar-overlay span {
  color: white;
  font-size: 12px;
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
