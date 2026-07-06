<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { getGoodsById, updateGoods } from '@/api'

const route = useRoute()
const router = useRouter()
const goodsId = Number(route.params.id)

const form = ref({
  title: '',
  description: '',
  price: 0,
  condition: 'new' as 'new' | 'like_new' | 'slight_use' | 'obvious_trace',
  categoryId: 1
})

onMounted(async () => {
  const res = await getGoodsById(goodsId)
  const goods = res.data
  form.value = {
    title: goods.title,
    description: goods.description,
    price: goods.price,
    condition: goods.condition,
    categoryId: goods.categoryId
  }
})

const handleSubmit = async () => {
  await updateGoods(goodsId, form.value)
  router.push(`/goods/${goodsId}`)
}
</script>

<template>
  <div class="goods-edit-page">
    <h2>编辑商品</h2>
    <form class="edit-form" @submit.prevent="handleSubmit">
      <div class="form-item">
        <label>商品标题</label>
        <input v-model="form.title" type="text" placeholder="请输入商品标题" required />
      </div>
      <div class="form-item">
        <label>商品描述</label>
        <textarea v-model="form.description" placeholder="请输入商品描述" rows="5" required></textarea>
      </div>
      <div class="form-item">
        <label>商品价格</label>
        <input v-model.number="form.price" type="number" placeholder="请输入商品价格" min="0" required />
      </div>
      <div class="form-item">
        <label>商品成色</label>
        <select v-model="form.condition">
          <option value="new">全新</option>
          <option value="like_new">几乎全新</option>
          <option value="slight_use">轻微使用</option>
          <option value="obvious_trace">明显痕迹</option>
        </select>
      </div>
      <div class="form-item">
        <label>商品分类</label>
        <select v-model="form.categoryId">
          <option value="1">数码产品</option>
          <option value="2">书籍文具</option>
          <option value="3">服饰鞋包</option>
          <option value="4">生活用品</option>
        </select>
      </div>
      <button type="submit">保存修改</button>
    </form>
  </div>
</template>

<style scoped>
.goods-edit-page {
  padding: 20px;
}

.edit-form {
  max-width: 600px;
}

.form-item {
  margin-bottom: 20px;
}

.form-item label {
  display: block;
  margin-bottom: 8px;
  font-weight: 500;
}

.form-item input,
.form-item textarea,
.form-item select {
  width: 100%;
  padding: 12px;
  border: 1px solid #e5e5e5;
  border-radius: 4px;
  font-size: 14px;
}

button {
  padding: 12px 24px;
  background: #165dff;
  color: white;
  border: none;
  border-radius: 4px;
  font-size: 16px;
  cursor: pointer;
}
</style>
