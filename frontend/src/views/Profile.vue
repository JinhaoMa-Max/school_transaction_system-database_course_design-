<script setup lang="ts">
import { computed, ref, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { Message } from '@arco-design/web-vue'
import { useUserStore } from '@/stores'
import { uploadAvatar, getReviewList } from '@/api'
import type { Review } from '@/types'

const router = useRouter()
const userStore = useUserStore()
const user = computed(() => userStore.user)
const loading = ref(false)
const avatarInput = ref<HTMLInputElement | null>(null)

// 评价相关
const receivedReviews = ref<Review[]>([])
const reviewLoading = ref(false)
const reviewTotal = ref(0)
const avgRating = ref(0)

const fetchReviews = async () => {
  if (!user.value?.userId) return
  reviewLoading.value = true
  try {
    const res = await getReviewList({
      reviewedUserId: user.value.userId,
      page: 1,
      size: 5
    })
    receivedReviews.value = res.data.list
    reviewTotal.value = res.data.total
    if (res.data.list.length > 0) {
      const sum = res.data.list.reduce((acc, r) => acc + r.rating, 0)
      avgRating.value = Math.round(sum / res.data.list.length * 10) / 10
    }
  } catch {
    // 静默失败，不影响主要信息展示
  } finally {
    reviewLoading.value = false
  }
}

const goToReviews = () => {
  router.push('/reviews')
}

const triggerAvatarInput = () => {
  avatarInput.value?.click()
}

//跳转到主页
const goToHome = () => {
  router.push('/')
}

//跳转到资料编辑页面
const goToEditProfile = () => {
  router.push('/profile/edit')
}

//跳转到学生认证界面（当且仅当用户角色为普通用户时显示）
const goToStudentAuth = () => {
  router.push('/student-auth')
}

//跳转到后台管理界面（当且仅当用户角色为管理员时显示）
const goToAdmin = () => {
  router.push('/admin/dashboard')
}

//跳转到收藏夹
const goToFavorites = () => {
  router.push('/favorites')
}

//跳转到议价管理
const goToBargains = () => {
  router.push('/bargains')
}

//跳转到聊天
const goToChat = () => {
  router.push('/chat')
}

//跳转到我的商品
const goToMyGoods = () => {
  router.push('/my/goods')
}

//跳转到我的订单
const goToOrders = () => {
  router.push('/orders')
}

//退出登录
const handleLogout = () => {
  userStore.logout()
  router.push('/login')
}


//获取角色身份
const getRoleText = (role?: string) => {
  const map: Record<string, string> = {
    user: '普通用户',
    admin: '管理员'
  }
  return role?map[role] || role:'未知角色'
}

//根据用户身份决定tag样式（小巧思之美工优化这一块）
const roleTagClass = computed(() => {
  switch (user.value?.role) {
    case 'user':
      return 'role-tag-user'
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

onMounted(() => {
  fetchReviews()
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
        <a-avatar :size="100" class="profile-avatar" @click="triggerAvatarInput">
            <img v-if="user?.avatarUrl" :src="user?.avatarUrl"/>
            <span v-else>
                 {{ avatarText }}
            </span>
        </a-avatar>
        <input
          id="avatar-input"
          ref="avatarInput"
          type="file"
          accept="image/jpeg,image/png,image/jpg,image/gif,image/webp"
          style="display: none"
          @change="handleAvatarUpload"
        />
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
      <a-descriptions class = "profile-descriptions"  :column="1" bordered size="large">

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

        <!--左边按钮-->
        <a-space class = "profile-actions-left">

          <a-button @click="goToHome" class = "home-button">
             返回主页
          </a-button>

        </a-space>  

        <!--右边按钮-->
        <a-space class = "profile-actions-right">

          <a-button type = "primary" @click="goToEditProfile" class="edit-button">
            编辑资料
          </a-button>

          <a-button @click="goToMyGoods" class="my-goods-button">
            我的商品
          </a-button>

          <a-button @click="goToOrders" class="orders-button">
            我的订单
          </a-button>

          <a-button @click="goToFavorites" class="favorites-button">
            收藏夹
          </a-button>

          <a-button @click="goToBargains" class="bargains-button">
            议价管理
          </a-button>

          <a-button @click="goToChat" class="chat-button">
            聊天
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

      </a-space>

    </a-card>

    <!-- 收到的评价 -->
    <a-card class="review-card" :bordered="false">
      <template #title>
        <div class="review-title-row">
          <span>收到的评价</span>
          <span v-if="receivedReviews.length > 0" class="review-avg-badge">
            <icon-star-fill style="color: #f7ba1e; font-size: 14px;" />
            {{ avgRating }}
          </span>
        </div>
      </template>

      <a-spin :loading="reviewLoading">
        <a-empty v-if="!reviewLoading && receivedReviews.length === 0" description="暂无评价" />

        <div v-else class="review-items">
          <div
            v-for="item in receivedReviews"
            :key="item.reviewId"
            class="review-row"
          >
            <div class="review-row-header">
              <a-avatar :size="36" class="review-row-avatar">
                <span>{{ (item.reviewerName || '用户').slice(0, 1).toUpperCase() }}</span>
              </a-avatar>
              <div class="review-row-meta">
                <span class="review-row-name">{{ item.reviewerName || '用户' }}</span>
                <a-rate
                  :model-value="item.rating"
                  :count="5"
                  disabled
                  size="small"
                  style="font-size: 12px;"
                />
              </div>
              <span class="review-row-time">{{ item.createTime }}</span>
            </div>
            <div v-if="item.content" class="review-row-content">
              {{ item.content }}
            </div>
            <div v-if="item.goodsTitle" class="review-row-goods">
              商品：{{ item.goodsTitle }}
            </div>
          </div>
        </div>

        <div v-if="reviewTotal > 5" class="review-more">
          <a-button type="text" @click="goToReviews">
            查看全部 {{ reviewTotal }} 条评价 →
          </a-button>
        </div>
      </a-spin>
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
  animation: fadeUp 0.6s ease both;
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
  cursor: pointer;
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

.role-tag-user {
  color: #165dff;
  background-color: #e8f3ff;
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
  align-items: center;
  justify-content: space-between;
  margin-top: 24px;
}

.profile-actions-left ,

.profile-actions-right {
  display: flex;
  align-items: center;
}

.home-button{
  background-color: #f8d86f;
  border-radius: 10px;
  color: white;
  transition: all 0.25s ease;
}

.home-button:hover {
  transform: translateY(-4px);
}

.student-auth-button {
  background-color: #7bdfab;
  border-radius: 10px;
  color: white;
  transition: all 0.25s ease;
}

.student-auth-button:hover {
  transform: translateY(-4px);
}

.edit-button{
   border-radius: 10px;
   transition: all 0.25s ease;
}
.edit-button:hover {
  transform: translateY(-4px);
}
.admin-button {
  background-color: #684993;
  border-radius: 10px;
  color: white;
  transition: all 0.25s ease;
}

.admin-button:hover {
  transform: translateY(-4px);
}
.logout-button {
  background-color: #cc5c5d;
  border-radius: 10px;
  color: white;
  transition: all 0.25s ease;
}
.logout-button:hover {
  transform: translateY(-4px);
}

.my-goods-button {
  background-color: #4a90d9;
  color: white;
}

.orders-button {
  background-color: #f0a050;
  color: white;
}

.favorites-button {
  background-color: #e8738a;
  color: white;
}

.bargains-button {
  background-color: #5db39a;
  color: white;
}

.chat-button {
  background-color: #24bac2;
  color: white;
}

/* 评价卡片 */
.review-card {
  max-width: 960px;
  margin: 24px auto 0;
  border-radius: 20px;
  background: var(--color-bg-2);
  box-shadow: 0 8px 24px rgba(45, 54, 142, 0.06);
  animation: fadeUp 0.7s ease both;
  animation-delay: 0.1s;
}

.review-title-row {
  display: flex;
  align-items: center;
  gap: 12px;
}

.review-avg-badge {
  display: inline-flex;
  align-items: center;
  gap: 4px;
  font-size: 14px;
  font-weight: 600;
  color: #1d2129;
  background: #fff7e6;
  padding: 2px 10px;
  border-radius: 999px;
}

.review-items {
  display: flex;
  flex-direction: column;
}

.review-row {
  padding: 16px 0;
  border-bottom: 1px solid var(--color-border-2);
}

.review-row:last-child {
  border-bottom: none;
}

.review-row-header {
  display: flex;
  align-items: center;
  gap: 12px;
}

.review-row-avatar {
  background: #e8f3ff;
  color: #165dff;
  font-weight: 600;
  flex-shrink: 0;
}

.review-row-meta {
  flex: 1;
  display: flex;
  flex-direction: column;
  gap: 2px;
}

.review-row-name {
  font-size: 14px;
  font-weight: 500;
  color: #1d2129;
}

.review-row-time {
  font-size: 12px;
  color: #86909c;
  flex-shrink: 0;
}

.review-row-content {
  margin-top: 10px;
  padding: 10px 14px;
  background: #f7f8fa;
  border-radius: 8px;
  font-size: 13px;
  color: #4e5969;
  line-height: 1.5;
}

.review-row-goods {
  margin-top: 8px;
  font-size: 12px;
  color: #86909c;
}

.review-more {
  text-align: center;
  margin-top: 8px;
}


@keyframes fadeUp {
  from {
    opacity: 0;
    transform: translateY(24px);
  }

  to {
    opacity: 1;
    transform: translateY(0);
  }
}

</style>
