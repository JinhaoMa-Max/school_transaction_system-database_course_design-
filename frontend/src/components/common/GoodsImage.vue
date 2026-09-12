<script setup lang="ts">
import { ref, watch } from 'vue'

const props = withDefaults(defineProps<{ src?: string | null; alt?: string | null }>(), {
  src: '',
  alt: '商品图片'
})
const failed = ref(false)
watch(() => props.src, () => { failed.value = false })
</script>

<template>
  <span class="goods-image-frame">
    <img v-if="src && !failed" :src="src" :alt="alt || '商品图片'" @error="failed = true" />
    <span v-else class="goods-image-empty" role="img" :aria-label="`${alt || '商品'}：暂无图片`">暂无图片</span>
  </span>
</template>

<style scoped>
.goods-image-frame { display: inline-flex; width: 100%; height: 100%; overflow: hidden; vertical-align: middle; background: #f2f3f5; }
.goods-image-frame img { width: 100%; height: 100%; object-fit: cover; }
.goods-image-empty { display: flex; align-items: center; justify-content: center; width: 100%; height: 100%; color: #86909c; font-size: 12px; }
</style>
