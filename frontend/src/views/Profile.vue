<script setup lang="ts">
import { computed } from 'vue'
import { useRouter } from 'vue-router'
import { useUserStore } from '@/stores'

const router = useRouter()
const userStore = useUserStore()

const user = computed(() => userStore.user)


//跳转到资料编辑页面
const goToEditProfile = () => {
  router.push('/profile/edit')
}

//跳转到学生认证界面（当且仅当用户角色为买家或卖家时显示）
const goToStudentAuth = () => {
  router.push('/student-auth')
}

//跳转到后台管理界面（当且仅当用户角色为管理员时显示）
const goToAdmin = () => {
  router.push('/admin/dashboard')
}

//退出登录
const handleLogout = () => {
  userStore.logout()
  router.push('/login')
}


//获取角色身份
const getRoleText = (role?: string) => {
  const map: Record<string, string> = {
    buyer: '买家',
    seller: '卖家',
    admin: '管理员'
  }
  return role?map[role] || role:'未知角色'
}

//根据用户身份决定tag样式（小巧思之美工优化这一块）
const roleTagClass = computed(() => {
  switch (user.value?.role) {
    case 'buyer':
      return 'role-tag-buyer'
    case 'seller':
      return 'role-tag-seller'
    case 'admin':
      return 'role-tag-admin'
    default:
      return 'role-tag-default'
  }
})

//获取用户状态
const getStatusText = (status?: string) => {
  const map: Record<string, string> = {
    normal: '正常',
    banned: '封禁'
  }
  return status?map[status] || status:'未知状态'
}

//当用户图像为空时，用昵称或用户名的首字母作为头像显示
const avatarText = computed(() => {
    const name = user.value?.nickname || user.value?.username || '用户'
    return name.slice(0, 1).toUpperCase()
})
</script>

<template>
  <div class="profile-page">

    <!-- 个人中心表单 -->
    <a-card class="profile-card" :bordered="false">

      <!-- 标题 -->
      <template #title>
        <div class ="profile-title">
          <div class = "profile-title-main">个人中心</div>
          <div class = "profile-title-sub">查看和管理您的认证信息，身份状态和联系方式</div>
        </div>
      </template>

      <div class = "profile-header">
        <a-avatar :size="100" class="profile-avatar">
            <img v-if="user?.avatarUrl" :src="user?.avatarUrl"/>
            <span v-else>
                 {{ avatarText }}
            </span>
        </a-avatar>
        <div class = "profile-basic">

            <div class = "nickname-row">

                <span class = "nickname">
                    {{ user?.nickname || user?.username || '用户' }}
                </span>

                <!-- 角色标签 -->
                <a-tag size = "small" class="role-tag" :class="roleTagClass">
                  {{ getRoleText(user?.role) }}
                </a-tag>

            </div>

            <div class = "username">
                @{{ user?.username || 'unknown' }}
            </div>

        </div>
      </div>

      <!-- 用户信息展示表格 -->
      <a-descriptions class = "profile-descriptions"  :column="1"bordered size="large">

        <a-descriptions-item label="用户名">
          {{ user?.username || '未填写' }}
        </a-descriptions-item>

        <a-descriptions-item label="昵称">
          {{ user?.nickname || '未填写' }}  
        </a-descriptions-item>

        <a-descriptions-item label="身份">
          {{ getRoleText(user?.role) }} 
        </a-descriptions-item>

        <a-descriptions-item label="状态">
          {{ getStatusText(user?.status) }}
        </a-descriptions-item>

        <a-descriptions-item label="信用分">
         {{ user?.creditScore ?? '未填写' }}
        </a-descriptions-item>

        <a-descriptions-item label="手机号">
          {{ user?.phone || '未填写' }}
        </a-descriptions-item>

        <a-descriptions-item label="邮箱">
          {{ user?.email || '未填写' }}
        </a-descriptions-item>

        <a-descriptions-item label="注册时间">
          {{ user?.registerTime || '未填写' }}
        </a-descriptions-item>

      </a-descriptions>

      <!-- 操作按钮 (满足条件才显示)-->
      <a-space class = "profile-actions">

          <a-button type = "primary" @click="goToEditProfile">
            编辑资料
          </a-button>

          <a-button v-if="user?.role !== 'admin'" @click="goToStudentAuth" class="student-auth-button">
            学生认证
          </a-button>

          <a-button v-if="user?.role === 'admin'" @click="goToAdmin" class="admin-button">
            后台管理
          </a-button>

          <a-button @click="handleLogout" class="logout-button">
            退出登录
          </a-button>

      </a-space>

    </a-card>

  </div>
</template>

<!-- 个人中心页面样式 -->
<style scoped>

.profile-page {
  min-height: calc(100vh - 64px);
  padding: 20px;
 background: linear-gradient(180deg, #253554 0%, #ffffff 100%);
  font-family:
    -apple-system,
    BlinkMacSystemFont,
    "Segoe UI",
    "PingFang SC",
    "Hiragino Sans GB",
    "Microsoft YaHei",
    "Helvetica Neue",
    Arial,
    sans-serif;
}

.profile-card {
  max-width: 960px;
  margin: 0 auto;
  border-radius: 20px;
  background: var(--color-bg-2);
  box-shadow: 0 8px 24px rgba(45, 54, 142, 0.06);
}


.profile-title {
  display: flex;
  flex-direction: column;
  gap: 4px;
}

.profile-title-main {
  font-size: 20px;
  font-weight: 600;
  color: var(--color-text-1);
  line-height: 1.4;
}

.profile-title-sub {
  font-size: 13px;
  font-weight: 400;
  color: var(--color-text-3);
}

.profile-header {
  display: flex;
  align-items: center;
  gap: 18px;
  padding: 8px 0 24px;
   margin-top: 24px;
  margin-bottom: 24px;
  border-bottom: 1px solid var(--color-border-2);
}

.profile-avatar {
  flex-shrink: 0;
  font-size: 26px;
  font-weight: 600;
  background-color: var(--color-fill-3);
  color: #253554;
}


.profile-basic {
  flex: 1;
}

.nickname-row {
  display: flex;
  align-items: center;
  gap: 10px;
}

.nickname {
  font-size: 36px;
  font-weight: 600;
  color: var(--color-text-1);
}

.role-tag {
  display: inline-flex;
  align-items: center;
  height: 24px;
  padding: 2px 8px;
  border-radius: 999px;
  font-size: 12px;
  font-weight: 500;
  line-height: 1;
}

.role-tag-buyer {
  color: #165dff;
  background-color: #e8f3ff;
}

.role-tag-seller {
  color: #ff7d00;
  background-color: #fff7e8;
}

.role-tag-admin {
  color: #722ed1;
  background-color: #f5e8ff;
}

.role-tag-default {
  color: var(--color-text-2);
  background-color: var(--color-fill-2);
}


.username {
  margin-top: 6px;
  font-size: 14px;
  color: var(--color-text-3);
}

.profile-descriptions {
  margin-bottom: 28px;
  overflow: hidden;
  border-radius: 10px;
}

:deep(.arco-descriptions-item-label) {
  font-size: 16px;
  width: 120px;
  font-weight: 500;
  color: #fbfbfb;
  background: #565557;
}

:deep(.arco-descriptions-item-value) {
  font-size: 16px;
  color: var(--color-text-1);
  background: var(--color-bg-2);
}


.profile-actions {
  display: flex;
  justify-content: flex-end;
  margin-top: 24px;
}

.student-auth-button {
  background-color: #7bdfab;
  color: white;
}

.admin-button {
  background-color: #684993;
  color: white;
}

.logout-button {
  background-color: #cc5c5d;
  color: white;
}
</style>
