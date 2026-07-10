<script setup lang="ts">
import { useRouter } from 'vue-router'
import { useUserStore } from '@/stores'
import { getGoodsList } from '@/api/goods'
import { getCategoryList } from '@/api/category'
import type { Goods, Category } from '@/types'
import {ref,onMounted} from 'vue'
import{Message} from '@arco-design/web-vue'

const router = useRouter()
const userStore = useUserStore()

  //商品数据
const hotGoods = ref<Goods[]>([])
const goodsLoading = ref(false)

//分类数据
const categories = ref<Category[]>([])
const categoryLoading = ref(false)

//进入对应分类
const goToCategory = (categoryId: number) => {
  router.push({
    path: '/goods',
    query: {
      categoryId
    }
  })
}
//获取分类名称
const getCategoryName = (category: Category) => {
  return category.categoryName || '其他'
}

const getCategoryNameById = (categoryId?: number | null) => {
  if (!categoryId) return '其他'

  const category = categories.value.find(
    item => item.categoryId === categoryId
  )

  return category ? getCategoryName(category) : '其他'
}

//进入商品广场
const goToGoods = () => {
  router.push('/goods')
}

//进入商品发布页面
const goToPublish = () => {
  router.push('/goods/publish')
}

//进入我的订单
const goToOrders = () => {
  router.push('/orders')
}

//进入个人中心
const goToProfile = () => {
  router.push('/profile')
}

//进入商品页面
const goToGoodsDetail = (goodsId: number) => {
  router.push(`/goods/${goodsId}`)
}

//加载首页热卖商品
const loadHotGoods = async () => {
  try {
    goodsLoading.value = true

    const res = await getGoodsList({
      page: 1,
      size: 8
    } as any)

    const payload = (res as any).data ?? res
    const pageData = payload.list ? payload : payload.data

    hotGoods.value = pageData?.list ?? []
  } catch (error) {
    console.log('首页商品加载失败', error)
    Message.error('商品加载失败')
    hotGoods.value = []
  } finally {
    goodsLoading.value = false
  }
}

//加载首页分类
const loadCategories = async () => {
  try {
    categoryLoading.value = true

    const res = await getCategoryList()

    const payload = (res as any).data ?? res
    const data = payload.data ?? payload

    categories.value = Array.isArray(data) ? data : data?.list ?? []
  } catch (error) {
    console.error('首页分类加载失败', error)
    categories.value = []
  } finally {
    categoryLoading.value = false
  }
}

onMounted(() => {
  loadHotGoods()
  loadCategories()
})

</script>

<template>
  <div class="home-page">

    <!--顶部导航页-->
    <a-card class="home-header-card" :bordered="false">

  <div class="home-header">
        <!-- 标题 -->
        <div class ="home-title">
          校园二手交易平台
        </div>
        <a-space class = "home-nav">

        <a-button type="text" @click="goToGoods">
          商品广场
        </a-button>

        <a-button v-if="userStore.isLoggedIn"  type="text" @click="goToPublish">
          发布商品
        </a-button>

        <a-button v-if="userStore.isLoggedIn" type="text" @click="goToOrders">
          我的订单
        </a-button>

        <a-button v-if="userStore.isLoggedIn" type="text" @click="goToProfile">
          个人中心
        </a-button>

        <a-button v-if="userStore.isLoggedIn" type="text" status="danger" @click="userStore.logout">
          退出登录
        </a-button>
        
        <a-button v-else type="primary" @click="router.push('/login')">
          登录
        </a-button>

        </a-space>

  </div>

    </a-card>

    <main class="home-main">

      <!--标题，介绍-->
      <section class ="hero-card">

         <div class="hero-title">
            发现心仪的二手好物!
          </div>

          <div class="hero-subtitle">
            -安全、便捷、环保的校园交易体验-
          </div>

          <a-button type="primary" class="in-button" size="large" @click="goToGoods">
            立即浏览
          </a-button>

      </section>

      <!--分类导航-->
      <a-card class = "section-card" :bordered="false">

        <template #title>
          分类导航
        </template>

        <a-spin :loading="categoryLoading">
        <div class="category-list">
          <a-button
          v-for="item in categories"
          :key="item.categoryId"
          shape="round"
          @click="goToCategory(item.categoryId)"
          >
          {{ getCategoryName(item) }}
        </a-button>
        </div>
        </a-spin>
      </a-card>


       <!--热门商品-->
      <a-card class = "section-card" :bordered="false">

        <template #title>
          热门商品
        </template>

         <a-spin :loading="goodsLoading">
            <a-empty
              v-if="hotGoods.length === 0"
              description="暂无商品"
            />
            <a-row v-else :gutter="[16,16]">
              <a-col
              v-for="item in hotGoods"
              :key="item.goodsId"
              :xs="24"
              :sm="12"
              :md="8"
              :lg="6"
              >
              <a-card
              class = "goods-card"
              hoverable
              :bordered="false"
              @click="goToGoodsDetail(item.goodsId)"
              >
              <div class = "goods-image">
                暂无图片
              </div>

              <div class = "goods-title">
                {{ item.title }}
              </div>

              <div class = "goods-footer">
                <span class="goods-price">￥{{ item.price }}</span>
                <a-tag size="small">
                  {{ getCategoryNameById(item.categoryId) }}
                </a-tag>
              </div>

              </a-card>

              </a-col>
            </a-row>
         </a-spin>

      </a-card>

    </main>

  </div>

</template>

<style scoped>
.home-page {
  min-height: 100vh;
  background: linear-gradient(180deg, #ffffff 0%, #253554 100%);
  padding: 20px;
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

.home-header-card {
  max-width: 1180px;
  margin: 0 auto 20px;
  border-radius: 20px;
  box-shadow: 0 8px 24px rgba(45, 54, 142, 0.06);
  animation: fadeUp 0.6s ease both;
}

.home-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
}

.home-title {
  font-family:
    "PingFang SC",
    "Microsoft YaHei",
    sans-serif;
  font-size: 35px;
  font-weight: 800;
  letter-spacing: 1.5px;
  color: #253554;
}


.home-nav {
  display: flex;
  gap: 8px;
  align-items:flex-end;
}

.home-nav :deep(.arco-btn) {
  font-size: 15px;
  font-weight: 650;
  letter-spacing: 0.04em;
  color: #253554;
  padding: 6px 12px;
  border-radius: 999px;
  transition:
    color 0.2s ease,
    background-color 0.2s ease,
    transform 0.2s ease;
}

.home-nav :deep(.arco-btn-text:hover) {
  color: #684993;
  background-color: rgba(104, 73, 147, 0.1);
  transform: translateY(-1px);
}

.home-nav :deep(.arco-btn-status-danger) {
  color: #cc5c5d;
}

.home-nav :deep(.arco-btn-status-danger:hover) {
  color: #b83f48;
  background-color: rgba(204, 92, 93, 0.1);
}

.home-nav :deep(.arco-btn-primary) {
  font-weight: 700;
  color: #fff;
  background: linear-gradient(135deg, #253554 0%, #684993 100%);
  border: none;
  box-shadow: 0 6px 14px rgba(104, 73, 147, 0.22);
}

.home-nav :deep(.arco-btn-text) {
  position: relative;
}

.home-nav :deep(.arco-btn-text::after) {
  content: "";
  position: absolute;
  left: 14px;
  right: 14px;
  bottom: 4px;
  height: 2px;
  border-radius: 999px;
  background: linear-gradient(90deg, #253554, #684993);
  transform: scaleX(0);
  transform-origin: center;
  transition: transform 0.2s ease;
}

.home-nav :deep(.arco-btn-text:hover::after) {
  transform: scaleX(1);
}

.home-main {
  max-width: 1180px;
  margin: 0 auto;
}

.hero-card {
  margin-bottom: 20px;
  padding: 70px 40px;
  text-align: center;
  color: white;
  border-radius: 24px;
  background: linear-gradient(135deg, #253554 0%, #684993 100%);
  animation: fadeUp 0.7s ease both;
  animation-delay: 0.1s;
}

.hero-title {
  font-family:
    "PingFang SC",
    "Microsoft YaHei",
    sans-serif;
  font-size: 44px;
  font-weight: 800;
  letter-spacing: 2px;
  margin-bottom: 16px;
  text-shadow: 0 6px 18px rgba(0, 0, 0, 0.22);
}

.hero-subtitle {
  font-size: 18px;
  font-weight: 400;
  letter-spacing: 2px;
  opacity: 0.9;
  margin-bottom: 32px;
}

.in-button {
  background: linear-gradient(135deg, #a84fcb 0%, #7d5fff 100%);
  border-radius: 999px;
  color: white;
  padding: 0 32px;
  font-weight: 600;
  box-shadow: 0 10px 24px rgba(168, 79, 203, 0.35);
  transition: all 0.25s ease;
}

.in-button:hover {
  transform: translateY(-3px);
  box-shadow: 0 14px 30px rgba(168, 79, 203, 0.45);
}

.home-search {
  max-width: 560px;
  margin: 0 auto;
}

.section-card {
  margin-bottom: 20px;
  border-radius: 20px;
  box-shadow: 0 8px 24px rgba(45, 54, 142, 0.06);
  animation: fadeUp 0.7s ease both;
  animation-delay: 0.2s;
}

.category-list {
  display: flex;
  flex-wrap: wrap;
  gap: 12px;
}

.category-list :deep(.arco-btn) {
  transition:
    transform 0.2s ease,
    box-shadow 0.2s ease,
    background-color 0.2s ease;
}

.category-list :deep(.arco-btn:hover) {
  transform: translateY(-3px);
  box-shadow: 0 8px 18px rgba(37, 53, 84, 0.12);
}
.goods-card {
  cursor: pointer;
  border-radius: 16px;
  overflow: hidden;
  transition:
    transform 0.25s ease,
    box-shadow 0.25s ease;
}

.goods-card:hover {
  transform: translateY(-6px);
  box-shadow: 0 14px 32px rgba(37, 53, 84, 0.16);
}


.goods-image {
  height: 130px;
  display: flex;
  align-items: center;
  justify-content: center;
  margin-bottom: 12px;
  border-radius: 12px;
  background: var(--color-fill-2);
  color: var(--color-text-3);
}

.goods-title {
  font-size: 15px;
  font-weight: 600;
  color: var(--color-text-1);
  margin-bottom: 10px;
}

.goods-footer {
  display: flex;
  justify-content: space-between;
  align-items: center;
}

.goods-price {
  font-size: 18px;
  font-weight: 700;
  color: #cc5c5d;
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
