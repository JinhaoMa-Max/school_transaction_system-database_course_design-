<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { Message, Modal } from '@arco-design/web-vue'
import { getOrderList, cancelOrder, completeOrder } from '@/api'
import { verifyConfirmCode } from '@/api'
import { useUserStore } from '@/stores'
import { orderStatusMap } from '@/constants'
import type { TradeOrder } from '@/types'

const router = useRouter()
const userStore = useUserStore()

const loading = ref(false)
const orderList = ref<TradeOrder[]>([])
const total = ref(0)
const page = ref(1)
const pageSize = ref(10)
const activeTab = ref('buy')
const statusFilter = ref('')

const verifyVisible = ref(false)
const verifyCode = ref('')
const currentOrderId = ref<number | null>(null)
const verifyLoading = ref(false)

const statusOptions = [
  { label: '全部', value: '' },
  { label: '待面交', value: 'pending_meet' },
  { label: '面交中', value: 'in_meet' },
  { label: '已完成', value: 'completed' },
  { label: '已取消', value: 'cancelled' }
]



const columns = [
  { title: '商品信息', dataIndex: 'goodsId', width: 280 },
  { title: '成交价', dataIndex: 'dealPrice', width: 120 },
  { title: '订单状态', dataIndex: 'status', width: 100 },
  { title: '对方', dataIndex: 'counterpart', width: 120 },
  { title: '下单时间', dataIndex: 'createTime', width: 170 },
  { title: '操作', dataIndex: 'actions', width: 240 }
]

const fetchOrderList = async () => {
  if (!userStore.user?.userId) return

  loading.value = true
  try {
    const params: Record<string, any> = {
      page: page.value,
      size: pageSize.value
    }
    if (statusFilter.value) {
      params.status = statusFilter.value
    }
    if (activeTab.value === 'buy') {
      params.buyerId = userStore.user.userId
    } else {
      params.sellerId = userStore.user.userId
    }
    const res = await getOrderList(params)
    orderList.value = res.data.list
    total.value = res.data.total
  } catch (error: any) {
    Message.error(error?.response?.data?.message || error?.message || '获取订单列表失败')
  } finally {
    loading.value = false
  }
}

const handleTabChange = (key: string | number) => {
  activeTab.value = String(key)
  page.value = 1
  fetchOrderList()
}

const handleStatusChange = (value: any) => {
  statusFilter.value = value as string
  page.value = 1
  fetchOrderList()
}

const handlePageChange = (pageNum: number) => {
  page.value = pageNum
  fetchOrderList()
}

const handlePageSizeChange = (size: number) => {
  pageSize.value = size
  page.value = 1
  fetchOrderList()
}

const goToGoodsDetail = (goodsId: number) => {
  router.push(`/goods/${goodsId}`)
}

const goToOrderDetail = (orderId: number) => {
  router.push(`/orders/${orderId}`)
}

const goToAppointment = (orderId: number) => {
  router.push(`/appointment/${orderId}`)
}

const goToReview = (orderId: number) => {
  router.push(`/review/${orderId}`)
}

const handleCancel = (orderId: number) => {
  Modal.confirm({
    title: '取消订单',
    content: '确定要取消该订单吗？取消后无法恢复。',
    okText: '确认取消',
    cancelText: '再想想',
    onOk: async () => {
      try {
        await cancelOrder(orderId)
        Message.success('订单已取消')
        fetchOrderList()
      } catch (error: any) {
        Message.error(error?.response?.data?.message || '取消订单失败')
      }
    }
  })
}

const openVerify = (orderId: number) => {
  currentOrderId.value = orderId
  verifyCode.value = ''
  verifyVisible.value = true
}

const handleVerify = async () => {
  if (!currentOrderId.value || !verifyCode.value) {
    Message.warning('请输入确认码')
    return
  }
  verifyLoading.value = true
  try {
    await verifyConfirmCode({
      orderId: currentOrderId.value,
      confirmCode: verifyCode.value
    })
    Message.success('确认码验证成功')
    verifyVisible.value = false
    fetchOrderList()
  } catch (error: any) {
    Message.error(error?.response?.data?.message || '确认码验证失败')
  } finally {
    verifyLoading.value = false
  }
}

const handleComplete = (orderId: number) => {
  Modal.confirm({
    title: '确认完成',
    content: '确认交易已完成吗？确认后将无法撤销。',
    okText: '确认完成',
    cancelText: '取消',
    onOk: async () => {
      try {
        await completeOrder(orderId)
        Message.success('订单已完成')
        fetchOrderList()
      } catch {
        // 错误已由全局拦截器处理
      }
    }
  })
}

onMounted(() => {
  fetchOrderList()
})
</script>

<template>
  <div class="order-list-page">
    <a-card class="content-card">
      <div class="page-header">
        <h2>我的订单</h2>
        <a-select
          v-model="statusFilter"
          :options="statusOptions"
          style="width: 140px"
          @change="handleStatusChange"
        />
      </div>

      <a-tabs
        v-model:active-key="activeTab"
        type="rounded"
        @change="handleTabChange"
      >
        <a-tab-pane key="buy" title="我买到的" />
        <a-tab-pane key="sell" title="我卖出的" />
      </a-tabs>

      <div class="table-container">
        <a-spin :loading="loading" style="width: 100%">
          <a-empty v-if="!loading && orderList.length === 0" description="暂无订单数据" />

          <a-table
            v-else
            :data="orderList"
            :columns="columns"
            :pagination="false"
            :bordered="false"
            size="medium"
            row-key="orderId"
          >
            <template #goodsId="{ record }">
              <div class="goods-info" @click="goToGoodsDetail(record.goodsId)">
                <div class="goods-name">商品 #{{ record.goodsId }}</div>
                <div class="goods-hint">点击查看商品</div>
              </div>
            </template>

            <template #dealPrice="{ record }">
              <span class="price-text">¥{{ record.dealPrice.toFixed(2) }}</span>
            </template>

            <template #status="{ record }">
              <a-tag :color="orderStatusMap[record.status]?.color || 'gray'">
                {{ orderStatusMap[record.status]?.text || record.status }}
              </a-tag>
            </template>

            <template #counterpart="{ record }">
              <span v-if="activeTab === 'buy'">卖家 #{{ record.sellerId }}</span>
              <span v-else>买家 #{{ record.buyerId }}</span>
            </template>

            <template #createTime="{ record }">
              {{ record.createTime }}
            </template>

            <template #actions="{ record }">
              <a-space size="small" wrap>
                <a-button type="text" size="small" @click="goToOrderDetail(record.orderId)">
                  详情
                </a-button>

                <template v-if="record.status === 'pending_meet'">
                  <a-button
                    type="outline"
                    status="warning"
                    size="small"
                    @click="handleCancel(record.orderId)"
                  >
                    取消订单
                  </a-button>
                  <a-button type="primary" size="small" @click="goToAppointment(record.orderId)">
                    预约面交
                  </a-button>
                </template>

                <template v-else-if="record.status === 'in_meet'">
                  <a-button
                    type="outline"
                    size="small"
                    @click="openVerify(record.orderId)"
                  >
                    核销确认码
                  </a-button>
                  <a-button type="primary" size="small" @click="handleComplete(record.orderId)">
                    确认完成
                  </a-button>
                </template>

                <template v-else-if="record.status === 'completed'">
                  <a-button type="outline" size="small" @click="goToReview(record.orderId)">
                    去评价
                  </a-button>
                </template>
              </a-space>
            </template>
          </a-table>

          <div v-if="orderList.length > 0" class="pagination-container">
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
      v-model:visible="verifyVisible"
      title="核销确认码"
      @ok="handleVerify"
      @cancel="verifyVisible = false"
      :confirm-loading="verifyLoading"
      ok-text="确认核销"
    >
      <div class="verify-form">
        <p>请输入对方提供的确认码进行核销：</p>
        <a-input
          v-model="verifyCode"
          placeholder="请输入确认码"
          style="width: 100%"
        />
      </div>
    </a-modal>
  </div>
</template>

<style scoped>
.order-list-page {
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

.pagination-container {
  margin-top: 20px;
  display: flex;
  justify-content: flex-end;
}

.verify-form {
  padding: 8px 0;
}

.verify-form p {
  margin: 0 0 12px 0;
  color: #4e5969;
}
</style>
