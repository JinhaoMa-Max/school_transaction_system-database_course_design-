<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { getGoodsList, auditGoods, offlineGoods } from '@/api'
import type { Goods } from '@/types'

const goods = ref<Goods[]>([])

onMounted(async () => {
  const res = await getGoodsList()
  goods.value = res.data.list
})

const handleAudit = async (goodsId: number, status: 'approved' | 'rejected') => {
  await auditGoods(goodsId, { status })
}

const handleOffline = async (goodsId: number) => {
  await offlineGoods(goodsId)
}

const getStatusText = (status: string) => {
  const map: Record<string, string> = {
    pending: '待审核',
    approved: '已审核',
    rejected: '已驳回',
    locked: '已锁定',
    sold: '已售出',
    offline: '已下架'
  }
  return map[status] || status
}
</script>

<template>
  <div class="admin-goods-list-page">
    <h2>商品管理</h2>
    <table class="goods-table">
      <thead>
        <tr>
          <th>商品ID</th>
          <th>标题</th>
          <th>卖家ID</th>
          <th>分类ID</th>
          <th>价格</th>
          <th>状态</th>
          <th>浏览次数</th>
          <th>发布时间</th>
          <th>操作</th>
        </tr>
      </thead>
      <tbody>
        <tr v-for="item in goods" :key="item.goodsId">
          <td>{{ item.goodsId }}</td>
          <td>{{ item.title }}</td>
          <td>{{ item.sellerId }}</td>
          <td>{{ item.categoryId }}</td>
          <td>¥{{ item.price }}</td>
          <td>{{ getStatusText(item.status) }}</td>
          <td>{{ item.viewCount }}</td>
          <td>{{ item.publishTime }}</td>
          <td>
            <button v-if="item.status === 'pending'" @click="handleAudit(item.goodsId, 'approved')">通过</button>
            <button v-if="item.status === 'pending'" @click="handleAudit(item.goodsId, 'rejected')">驳回</button>
            <button v-if="item.status === 'approved'" @click="handleOffline(item.goodsId)">下架</button>
          </td>
        </tr>
      </tbody>
    </table>
  </div>
</template>

<style scoped>
.admin-goods-list-page {
  padding: 20px;
}

.goods-table {
  width: 100%;
  border-collapse: collapse;
}

.goods-table th,
.goods-table td {
  padding: 12px;
  text-align: left;
  border-bottom: 1px solid #e5e5e5;
}

.goods-table th {
  background: #f5f5f5;
}

button {
  padding: 6px 12px;
  margin-right: 8px;
  border: none;
  border-radius: 4px;
  cursor: pointer;
  font-size: 14px;
}

button:nth-child(1) {
  background: #52c41a;
  color: white;
}

button:nth-child(2) {
  background: #ff4d4f;
  color: white;
}

button:nth-child(3) {
  background: #faad14;
  color: white;
}
</style>
