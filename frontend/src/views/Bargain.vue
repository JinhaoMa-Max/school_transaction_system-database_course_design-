<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { getBargainList, handleBargain } from '@/api'
import type { BargainOffer } from '@/types'

const router = useRouter()
const bargains = ref<BargainOffer[]>([])

onMounted(async () => {
  const res = await getBargainList()
  bargains.value = res.data.list
})

const goToGoods = (goodsId: number) => {
  router.push(`/goods/${goodsId}`)
}

const handleAccept = async (bargainId: number) => {
  await handleBargain(bargainId, { sellerResult: 'accepted' })
}

const handleReject = async (bargainId: number) => {
  await handleBargain(bargainId, { sellerResult: 'rejected' })
}

const handleCounter = async (bargainId: number) => {
  const counterPrice = prompt('请输入还价金额')
  if (counterPrice) {
    await handleBargain(bargainId, { sellerResult: 'countered', counterPrice: Number(counterPrice) })
  }
}
</script>

<template>
  <div class="bargain-page">
    <h2>我的议价</h2>
    <div class="bargain-list">
      <div 
        v-for="item in bargains" 
        :key="item.bargainId" 
        class="bargain-item"
      >
        <div class="bargain-info">
          <h3 @click="goToGoods(item.goodsId)">商品ID: {{ item.goodsId }}</h3>
          <p>出价金额: ¥{{ item.offerPrice }}</p>
          <p>处理结果: {{ item.sellerResult }}</p>
          <p>还价金额: {{ item.counterPrice ? '¥' + item.counterPrice : '-' }}</p>
          <p>议价状态: {{ item.status }}</p>
          <p>发起时间: {{ item.createTime }}</p>
        </div>
        <div class="actions">
          <button v-if="item.status === 'active'" @click="handleAccept(item.bargainId)">接受</button>
          <button v-if="item.status === 'active'" @click="handleReject(item.bargainId)">拒绝</button>
          <button v-if="item.status === 'active'" @click="handleCounter(item.bargainId)">还价</button>
        </div>
      </div>
    </div>
    <div v-if="bargains.length === 0" class="empty">
      <p>暂无议价记录</p>
    </div>
  </div>
</template>

<style scoped>
.bargain-page {
  padding: 20px;
}

.bargain-list {
  display: flex;
  flex-direction: column;
  gap: 12px;
}

.bargain-item {
  padding: 16px;
  background: white;
  border-radius: 8px;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.08);
}

.bargain-info h3 {
  margin: 0 0 8px 0;
  cursor: pointer;
  color: #165dff;
}

.bargain-info p {
  margin: 0 0 4px 0;
  font-size: 14px;
}

.actions {
  margin-top: 12px;
}

.actions button {
  padding: 8px 16px;
  margin-right: 8px;
  border: none;
  border-radius: 4px;
  cursor: pointer;
}

.actions button:nth-child(1) {
  background: #52c41a;
  color: white;
}

.actions button:nth-child(2) {
  background: #ff4d4f;
  color: white;
}

.actions button:nth-child(3) {
  background: #faad14;
  color: white;
}

.empty {
  text-align: center;
  padding: 40px;
  color: #999;
}
</style>
