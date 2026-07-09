<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { Message, Modal } from '@arco-design/web-vue'
import { getGoodsList, offlineGoods } from '@/api'
import { useUserStore } from '@/stores'
import { conditionMap, goodsStatusMap } from '@/constants'
import type { Goods } from '@/types'

const router = useRouter()
const userStore = useUserStore()

const loading = ref(false)
const goodsList = ref<Goods[]>([])
const total = ref(0)
const page = ref(1)
const pageSize = ref(10)
const activeTab = ref('all')

const tabOptions = [
  { key: 'all', label: '全部' },
  { key: 'pending', label: '待审核' },
  { key: 'approved', label: '已上架' },
  { key: 'rejected', label: '已驳回' },
  { key: 'sold', label: '已售出' },
  { key: 'offline', label: '已下架' }
]



const columns = [
  { title: '商品图片', dataIndex: 'imageUrl', width: 100 },
  { title: '标题', dataIndex: 'title' },
  { title: '价格', dataIndex: 'price', width: 120 },
  { title: '成色', dataIndex: 'condition', width: 100 },
  { title: '状态', dataIndex: 'status', width: 100 },
  { title: '浏览量', dataIndex: 'viewCount', width: 90 },
  { title: '发布时间', dataIndex: 'publishTime', width: 170 },
  { title: '操作', dataIndex: 'actions', width: 200 }
]

const fetchGoodsList = async () => {
  if (!userStore.user?.userId) return

  loading.value = true
  try {
    const params: Record<string, any> = {
      sellerId: userStore.user.userId,
      page: page.value,
      size: pageSize.value
    }
    if (activeTab.value !== 'all') {
      params.status = activeTab.value
    }
    const res = await getGoodsList(params)
    goodsList.value = res.data.list
    total.value = res.data.total
  } catch {
    // 错误已由全局拦截器处理
  } finally {
    loading.value = false
  }
}

const handleTabChange = (key: string | number) => {
  activeTab.value = String(key)
  page.value = 1
  fetchGoodsList()
}

const handlePageChange = (pageNum: number) => {
  page.value = pageNum
  fetchGoodsList()
}

const handlePageSizeChange = (size: number) => {
  pageSize.value = size
  page.value = 1
  fetchGoodsList()
}

const handleEdit = (goodsId: number) => {
  router.push(`/goods/${goodsId}/edit`)
}

const handleViewDetail = (goodsId: number) => {
  router.push(`/goods/${goodsId}`)
}

const handleOffline = (goodsId: number) => {
  Modal.confirm({
    title: '确认下架',
    content: '确定要下架该商品吗？下架后将无法在商品列表中展示。',
    okText: '确认下架',
    cancelText: '取消',
    onOk: async () => {
      try {
        await offlineGoods(goodsId)
        Message.success('商品下架成功')
        fetchGoodsList()
      } catch {
        // 错误已由全局拦截器处理
      }
    }
  })
}

onMounted(() => {
  fetchGoodsList()
})
</script>

<template>
  <div class="my-goods-page">
    <a-card class="content-card">
      <div class="page-header">
        <h2>我的商品</h2>
        <a-button type="primary" @click="router.push('/goods/publish')">
          发布新商品
        </a-button>
      </div>

      <a-tabs
        v-model:active-key="activeTab"
        type="rounded"
        @change="handleTabChange"
      >
        <a-tab-pane
          v-for="tab in tabOptions"
          :key="tab.key"
          :title="tab.label"
        />
      </a-tabs>

      <div class="table-container">
        <a-spin :loading="loading" style="width: 100%">
          <a-empty v-if="!loading && goodsList.length === 0" description="暂无商品数据" />

          <a-table
            v-else
            :data="goodsList"
            :columns="columns"
            :pagination="false"
            :bordered="false"
            size="medium"
            row-key="goodsId"
          >
            <template #imageUrl="{ record }">
              <div class="image-cell">
                <img
                  v-if="record.imageUrl"
                  :src="record.imageUrl"
                  :alt="record.title"
                  class="goods-image"
                />
                <div v-else class="image-placeholder">
                  无图
                </div>
              </div>
            </template>

            <template #title="{ record }">
              <span class="title-text" @click="handleViewDetail(record.goodsId)">
                {{ record.title }}
              </span>
            </template>

            <template #price="{ record }">
              <span class="price-text">¥{{ record.price.toFixed(2) }}</span>
            </template>

            <template #condition="{ record }">
              {{ conditionMap[record.condition] || record.condition }}
            </template>

            <template #status="{ record }">
              <a-tag :color="goodsStatusMap[record.status]?.color || 'gray'">
                {{ goodsStatusMap[record.status]?.text || record.status }}
              </a-tag>
            </template>

            <template #viewCount="{ record }">
              {{ record.viewCount }}
            </template>

            <template #publishTime="{ record }">
              {{ record.publishTime }}
            </template>

            <template #actions="{ record }">
              <a-space size="small">
                <a-button
                  v-if="record.status === 'pending' || record.status === 'rejected'"
                  type="primary"
                  size="small"
                  @click="handleEdit(record.goodsId)"
                >
                  编辑
                </a-button>
                <a-button
                  v-if="record.status === 'approved'"
                  type="outline"
                  status="warning"
                  size="small"
                  @click="handleOffline(record.goodsId)"
                >
                  下架
                </a-button>
                <a-button type="text" size="small" @click="handleViewDetail(record.goodsId)">
                  查看详情
                </a-button>
              </a-space>
            </template>
          </a-table>

          <div v-if="goodsList.length > 0" class="pagination-container">
            <a-pagination
              :total="total"
              :current="page"
              :page-size="pageSize"
              :page-size-options="[10, 20, 50]"
              show-total
              show-jumper
              show-page-size
              @change="handlePageChange"
              @page-size-change="handlePageSizeChange"
            />
          </div>
        </a-spin>
      </div>
    </a-card>
  </div>
</template>

<style scoped>
.my-goods-page {
  padding: 24px;
  max-width: 1200px;
  margin: 0 auto;
}

.content-card {
  border-radius: 8px;
}

.page-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 20px;
}

.page-header h2 {
  margin: 0;
  font-size: 24px;
  font-weight: 600;
}

.table-container {
  margin-top: 20px;
}

.image-cell {
  width: 72px;
  height: 72px;
  border-radius: 6px;
  overflow: hidden;
  background: #f2f3f5;
}

.goods-image {
  width: 100%;
  height: 100%;
  object-fit: cover;
}

.image-placeholder {
  width: 100%;
  height: 100%;
  display: flex;
  align-items: center;
  justify-content: center;
  color: #c9cdd4;
  font-size: 12px;
}

.title-text {
  cursor: pointer;
  color: #1d2129;
  transition: color 0.2s;
}

.title-text:hover {
  color: #165dff;
}

.price-text {
  color: #f53f3f;
  font-weight: 600;
}

.pagination-container {
  margin-top: 20px;
  display: flex;
  justify-content: flex-end;
}
</style>
