<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { Message, Modal } from '@arco-design/web-vue'
import { getBargainList, handleBargain, handleBargainByBuyer, closeBargain } from '@/api'
import { useUserStore } from '@/stores'
import type { BargainOffer } from '@/types'

const router = useRouter()
const userStore = useUserStore()

const loading = ref(false)
const bargainList = ref<BargainOffer[]>([])
const total = ref(0)
const page = ref(1)
const pageSize = ref(10)
const activeTab = ref('buyer')

const counterVisible = ref(false)
const counterPrice = ref(0)
const currentBargainId = ref<number | null>(null)
const counterLoading = ref(false)

// 买家还价相关状态
const buyerCounterVisible = ref(false)
const buyerCounterPrice = ref(0)
const buyerCurrentBargainId = ref<number | null>(null)
const buyerCounterLoading = ref(false)

const sellerResultMap: Record<string, { text: string; color: string }> = {
  pending: { text: '待处理', color: 'orange' },
  accepted: { text: '已接受', color: 'green' },
  rejected: { text: '已拒绝', color: 'red' },
  countered: { text: '已还价', color: 'blue' }
}

const bargainStatusMap: Record<string, { text: string; color: string }> = {
  active: { text: '进行中', color: 'blue' },
  accepted: { text: '已达成', color: 'green' },
  rejected: { text: '已拒绝', color: 'red' },
  closed: { text: '已关闭', color: 'gray' }
}

const columns = [
  { title: '商品信息', dataIndex: 'goodsId', width: 280 },
  { title: '出价金额', dataIndex: 'offerPrice', width: 120 },
  { title: '卖家处理', dataIndex: 'sellerResult', width: 100 },
  { title: '还价金额', dataIndex: 'counterPrice', width: 120 },
  { title: '议价状态', dataIndex: 'status', width: 100 },
  { title: '发起时间', dataIndex: 'createTime', width: 170 },
  { title: '操作', dataIndex: 'actions', width: 200 }
]

const fetchBargainList = async () => {
  if (!userStore.user?.userId) return

  loading.value = true
  try {
    const params: Record<string, any> = {
      page: page.value,
      size: pageSize.value
    }
    if (activeTab.value === 'buyer') {
      params.buyerId = userStore.user.userId
    } else {
      params.sellerId = userStore.user.userId
    }
    const res = await getBargainList(params)
    bargainList.value = res.data.list
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
  fetchBargainList()
}

const handlePageChange = (pageNum: number) => {
  page.value = pageNum
  fetchBargainList()
}

const handlePageSizeChange = (size: number) => {
  pageSize.value = size
  page.value = 1
  fetchBargainList()
}

const goToGoodsDetail = (goodsId: number) => {
  router.push(`/goods/${goodsId}`)
}

const openCounter = (bargainId: number) => {
  currentBargainId.value = bargainId
  counterPrice.value = 0
  counterVisible.value = true
}

const handleAccept = (bargainId: number) => {
  Modal.confirm({
    title: '确认接受',
    content: '确定接受该议价吗？接受后将生成订单。',
    okText: '确认接受',
    cancelText: '取消',
    onOk: async () => {
      try {
        await handleBargain(bargainId, { sellerResult: 'accepted' })
        Message.success('已接受议价')
        fetchBargainList()
      } catch {
        // 错误已由全局拦截器处理
      }
    }
  })
}

const handleReject = (bargainId: number) => {
  Modal.confirm({
    title: '确认拒绝',
    content: '确定拒绝该议价吗？',
    okText: '确认拒绝',
    cancelText: '取消',
    onOk: async () => {
      try {
        await handleBargain(bargainId, { sellerResult: 'rejected' })
        Message.success('已拒绝议价')
        fetchBargainList()
      } catch {
        // 错误已由全局拦截器处理
      }
    }
  })
}

const handleCounter = async () => {
  if (!currentBargainId.value || counterPrice.value <= 0) {
    Message.warning('请输入有效的还价金额')
    return
  }
  counterLoading.value = true
  try {
    await handleBargain(currentBargainId.value, {
      sellerResult: 'countered',
      counterPrice: counterPrice.value
    })
    Message.success('还价已发送')
    counterVisible.value = false
    fetchBargainList()
  } catch {
    // 错误已由全局拦截器处理
  } finally {
    counterLoading.value = false
  }
}

const handleClose = (bargainId: number) => {
  Modal.confirm({
    title: '关闭议价',
    content: '确定关闭该议价吗？关闭后无法恢复。',
    okText: '确认关闭',
    cancelText: '取消',
    onOk: async () => {
      try {
        await closeBargain(bargainId)
        Message.success('议价已关闭')
        fetchBargainList()
      } catch {
        // 错误已由全局拦截器处理
      }
    }
  })
}

// ---------- 买家操作：对卖家还价做出回应 ----------
const openBuyerCounter = (bargainId: number) => {
  buyerCurrentBargainId.value = bargainId
  buyerCounterPrice.value = 0
  buyerCounterVisible.value = true
}

const handleBuyerAccept = (bargainId: number) => {
  Modal.confirm({
    title: '确认接受',
    content: '确定接受卖家的还价吗？接受后将生成订单。',
    okText: '确认接受',
    cancelText: '取消',
    onOk: async () => {
      try {
        await handleBargainByBuyer(bargainId, { buyerResult: 'accepted' })
        Message.success('已接受卖家还价')
        fetchBargainList()
      } catch {
        // 错误已由全局拦截器处理
      }
    }
  })
}

const handleBuyerReject = (bargainId: number) => {
  Modal.confirm({
    title: '确认拒绝',
    content: '确定拒绝卖家的还价吗？',
    okText: '确认拒绝',
    cancelText: '取消',
    onOk: async () => {
      try {
        await handleBargainByBuyer(bargainId, { buyerResult: 'rejected' })
        Message.success('已拒绝卖家还价')
        fetchBargainList()
      } catch {
        // 错误已由全局拦截器处理
      }
    }
  })
}

const handleBuyerCounter = async () => {
  if (!buyerCurrentBargainId.value || buyerCounterPrice.value <= 0) {
    Message.warning('请输入有效的出价金额')
    return
  }
  buyerCounterLoading.value = true
  try {
    await handleBargainByBuyer(buyerCurrentBargainId.value, {
      buyerResult: 'countered',
      offerPrice: buyerCounterPrice.value
    })
    Message.success('继续还价已发送')
    buyerCounterVisible.value = false
    fetchBargainList()
  } catch {
    // 错误已由全局拦截器处理
  } finally {
    buyerCounterLoading.value = false
  }
}

onMounted(() => {
  fetchBargainList()
})
</script>

<template>
  <div class="bargain-page">
    <a-card class="content-card">
      <div class="page-header">
        <h2>议价管理</h2>
      </div>

      <a-tabs
        v-model:active-key="activeTab"
        type="rounded"
        @change="handleTabChange"
      >
        <a-tab-pane key="buyer" title="我发起的" />
        <a-tab-pane key="seller" title="我收到的" />
      </a-tabs>

      <div class="table-container">
        <a-spin :loading="loading" style="width: 100%">
          <a-empty v-if="!loading && bargainList.length === 0" description="暂无议价记录" />

          <a-table
            v-else
            :data="bargainList"
            :columns="columns"
            :pagination="false"
            :bordered="false"
            size="medium"
            row-key="bargainId"
          >
            <template #goodsId="{ record }">
              <div class="goods-info" @click="goToGoodsDetail(record.goodsId)">
                <div class="goods-name">商品 #{{ record.goodsId }}</div>
                <div class="goods-hint">点击查看详情</div>
              </div>
            </template>

            <template #offerPrice="{ record }">
              <span class="price-text">¥{{ record.offerPrice.toFixed(2) }}</span>
            </template>

            <template #sellerResult="{ record }">
              <a-tag :color="sellerResultMap[record.sellerResult]?.color || 'gray'">
                {{ sellerResultMap[record.sellerResult]?.text || record.sellerResult }}
              </a-tag>
            </template>

            <template #counterPrice="{ record }">
              <span v-if="record.counterPrice" class="counter-price">
                ¥{{ record.counterPrice.toFixed(2) }}
              </span>
              <span v-else class="text-gray">-</span>
            </template>

            <template #status="{ record }">
              <a-tag :color="bargainStatusMap[record.status]?.color || 'gray'">
                {{ bargainStatusMap[record.status]?.text || record.status }}
              </a-tag>
            </template>

            <template #createTime="{ record }">
              {{ record.createTime }}
            </template>

            <template #actions="{ record }">
              <a-space size="small">
                <!-- 卖家操作：待处理 -->
                <template v-if="activeTab === 'seller' && record.status === 'active' && record.sellerResult === 'pending'">
                  <a-button type="primary" size="small" @click="handleAccept(record.bargainId)">
                    接受
                  </a-button>
                  <a-button type="outline" status="danger" size="small" @click="handleReject(record.bargainId)">
                    拒绝
                  </a-button>
                  <a-button type="outline" status="warning" size="small" @click="openCounter(record.bargainId)">
                    还价
                  </a-button>
                </template>
                <!-- 买家操作：卖家已还价，买家做出回应 -->
                <template v-else-if="activeTab === 'buyer' && record.status === 'active' && record.sellerResult === 'countered'">
                  <a-button type="primary" size="small" @click="handleBuyerAccept(record.bargainId)">
                    接受还价
                  </a-button>
                  <a-button type="outline" status="danger" size="small" @click="handleBuyerReject(record.bargainId)">
                    拒绝还价
                  </a-button>
                  <a-button type="outline" status="warning" size="small" @click="openBuyerCounter(record.bargainId)">
                    继续还价
                  </a-button>
                </template>
                <!-- 买家操作：普通进行中（非 countered 状态） -->
                <template v-else-if="activeTab === 'buyer' && record.status === 'active'">
                  <a-button type="text" size="small" status="danger" @click="handleClose(record.bargainId)">
                    关闭议价
                  </a-button>
                </template>
                <a-button type="text" size="small" @click="goToGoodsDetail(record.goodsId)">
                  查看商品
                </a-button>
              </a-space>
            </template>
          </a-table>

          <div v-if="bargainList.length > 0" class="pagination-container">
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

    <a-modal
      v-model:visible="counterVisible"
      title="发起还价"
      @ok="handleCounter"
      @cancel="counterVisible = false"
      :confirm-loading="counterLoading"
      ok-text="发送还价"
    >
      <div class="counter-form">
        <a-form :model="{ counterPrice }" layout="vertical">
          <a-form-item label="还价金额">
            <a-input-number
              v-model="counterPrice"
              :min="0.01"
              :precision="2"
              placeholder="请输入还价金额"
              style="width: 100%"
            >
              <template #prepend>¥</template>
            </a-input-number>
          </a-form-item>
        </a-form>
      </div>
    </a-modal>

    <!-- 买家继续还价弹窗 -->
    <a-modal
      v-model:visible="buyerCounterVisible"
      title="继续还价"
      @ok="handleBuyerCounter"
      @cancel="buyerCounterVisible = false"
      :confirm-loading="buyerCounterLoading"
      ok-text="发送还价"
    >
      <div class="counter-form">
        <a-form :model="{ buyerCounterPrice }" layout="vertical">
          <a-form-item label="您的出价">
            <a-input-number
              v-model="buyerCounterPrice"
              :min="0.01"
              :precision="2"
              placeholder="请输入您的出价"
              style="width: 100%"
            >
              <template #prepend>¥</template>
            </a-input-number>
          </a-form-item>
        </a-form>
      </div>
    </a-modal>
  </div>
</template>

<style scoped>
.bargain-page {
  padding: 24px;
  max-width: 1200px;
  margin: 0 auto;
}

.content-card {
  border-radius: 8px;
}

.page-header {
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

.goods-info {
  cursor: pointer;
}

.goods-name {
  font-size: 14px;
  color: #1d2129;
  margin-bottom: 4px;
  transition: color 0.2s;
}

.goods-info:hover .goods-name {
  color: #165dff;
}

.goods-hint {
  font-size: 12px;
  color: #86909c;
}

.price-text {
  color: #f53f3f;
  font-weight: 600;
}

.counter-price {
  color: #fa9841;
  font-weight: 500;
}

.text-gray {
  color: #c9cdd4;
}

.pagination-container {
  margin-top: 20px;
  display: flex;
  justify-content: flex-end;
}

.counter-form {
  padding: 8px 0;
}
</style>
