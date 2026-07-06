<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { useUserStore } from '@/stores'
import { submitStudentAuth, getStudentAuth } from '@/api'
import type { StudentAuth } from '@/types'

const userStore = useUserStore()
const auth = ref<StudentAuth | null>(null)

const form = ref({
  studentId: '',
  realName: '',
  college: ''
})

onMounted(async () => {
  if (userStore.user) {
    try {
      const res = await getStudentAuth(userStore.user.userId)
      auth.value = res.data
    } catch {
      auth.value = null
    }
  }
})

const handleSubmit = async () => {
  if (!userStore.user) return
  await submitStudentAuth({ ...form.value, userId: userStore.user.userId })
}

const getStatusText = (status: string) => {
  const map: Record<string, string> = {
    pending: '待审核',
    approved: '已通过',
    rejected: '已驳回'
  }
  return map[status] || status
}
</script>

<template>
  <div class="student-auth-page">
    <h2>学生认证</h2>
    <div v-if="auth" class="auth-status">
      <p>认证状态: {{ getStatusText(auth.authStatus) }}</p>
      <p>学号: {{ auth.studentId }}</p>
      <p>真实姓名: {{ auth.realName }}</p>
      <p>学院: {{ auth.college }}</p>
      <p>认证时间: {{ auth.authTime }}</p>
    </div>
    <div v-else class="auth-form">
      <div class="form-item">
        <label>学号</label>
        <input v-model="form.studentId" type="text" placeholder="请输入学号" />
      </div>
      <div class="form-item">
        <label>真实姓名</label>
        <input v-model="form.realName" type="text" placeholder="请输入真实姓名" />
      </div>
      <div class="form-item">
        <label>学院</label>
        <input v-model="form.college" type="text" placeholder="请输入学院" />
      </div>
      <button @click="handleSubmit">提交认证</button>
    </div>
  </div>
</template>

<style scoped>
.student-auth-page {
  padding: 20px;
}

.auth-status {
  padding: 20px;
  background: white;
  border-radius: 8px;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.08);
}

.auth-status p {
  margin: 0 0 8px 0;
  font-size: 16px;
}

.auth-form {
  padding: 20px;
  background: white;
  border-radius: 8px;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.08);
}

.form-item {
  margin-bottom: 20px;
}

.form-item label {
  display: block;
  margin-bottom: 8px;
  font-weight: 500;
}

.form-item input {
  width: 100%;
  padding: 12px;
  border: 1px solid #e5e5e5;
  border-radius: 4px;
  font-size: 14px;
}

button {
  padding: 12px 24px;
  background: #165dff;
  color: white;
  border: none;
  border-radius: 4px;
  font-size: 16px;
  cursor: pointer;
}
</style>
