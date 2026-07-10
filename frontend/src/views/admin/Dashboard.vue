<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { getAdminStats } from '@/api'

const stats = ref([
  { label: '用户总数', value: 0 },
  { label: '商品总数', value: 0 },
  { label: '订单总数', value: 0 },
  { label: '举报总数', value: 0 }
])

const loading = ref(false)

onMounted(async () => {
  loading.value = true
  try {
    const res = await getAdminStats()
    const data = res.data
    stats.value[0].value = data.userCount
    stats.value[1].value = data.goodsCount
    stats.value[2].value = data.orderCount
    stats.value[3].value = data.reportCount
  } catch {
    // keep default zero values on error
  } finally {
    loading.value = false
  }
})
</script>

<template>
  <div class="dashboard-page">
    <h2>仪表盘</h2>
    <div class="stats-grid">
      <div v-for="item in stats" :key="item.label" class="stat-card">
        <p class="label">{{ item.label }}</p>
        <p class="value">{{ item.value }}</p>
      </div>
    </div>
  </div>
</template>

<style scoped>
.dashboard-page {
  padding: 20px;
}

.stats-grid {
  display: grid;
  grid-template-columns: repeat(4, 1fr);
  gap: 20px;
}

.stat-card {
  padding: 20px;
  background: white;
  border-radius: 8px;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.08);
}

.stat-card .label {
  margin: 0 0 8px 0;
  color: #999;
  font-size: 14px;
}

.stat-card .value {
  margin: 0;
  font-size: 24px;
  font-weight: bold;
}
</style>
