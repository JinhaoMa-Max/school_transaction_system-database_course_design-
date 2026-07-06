<script setup lang="ts">
import { ref } from 'vue'

const stats = ref([
  { label: '用户总数', value: 1000 },
  { label: '商品总数', value: 5000 },
  { label: '订单总数', value: 8000 },
  { label: '举报总数', value: 100 }
])
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
