<script setup lang="ts">
import { ref, onMounted, reactive } from 'vue'
import { useRouter } from 'vue-router'
import { Message } from '@arco-design/web-vue'
import { useUserStore } from '@/stores'
import { getCategoryList, createGoods, uploadGoodsImage, uploadImage } from '@/api'
import ImageUpload from '@/components/common/ImageUpload.vue'
import { conditionMap } from '@/constants'
import type { Category } from '@/types'

const router = useRouter()
const userStore = useUserStore()
const formRef = ref()
const loading = ref(false)
const categories = ref<Category[]>([])
const imageList = ref<{ url: string; uploadedUrl?: string; file?: File }[]>([])

const form = reactive({
  title: '',
  description: '',
  price: 0,
  condition: 'like_new' as 'new' | 'like_new' | 'slight_use' | 'obvious_trace',
  categoryId: undefined as number | undefined
})

const rules = {
  title: [
    { required: true, message: '请输入商品标题' },
    { maxLength: 100, message: '标题不能超过100个字符' }
  ],
  description: [
    { required: true, message: '请输入商品描述' },
    { maxLength: 2000, message: '描述不能超过2000个字符' }
  ],
  price: [
    { required: true, message: '请输入商品价格' },
    {
      validator: (value: number, callback: (error?: string) => void) => {
        if (value <= 0 || value > 999999) {
          callback('价格必须在0-999999元之间')
        } else {
          callback()
        }
      }
    }
  ],
  condition: [{ required: true, message: '请选择商品成色' }],
  categoryId: [{ required: true, message: '请选择商品分类' }]
}

const conditionOptions = Object.entries(conditionMap).map(([value, label]) => ({
  label,
  value
}))

const fetchCategories = async () => {
  try {
    const res = await getCategoryList()
    categories.value = res.data
  } catch {
    // 错误已由全局拦截器处理
  }
}

const handleSubmit = async () => {
  try {
    await formRef.value.validate()
  } catch {
    Message.error('请检查表单填写是否正确')
    return
  }

  if (imageList.value.length === 0) {
    Message.warning('请至少上传一张商品图片')
    return
  }

  loading.value = true
  try {
    const res = await createGoods({
      sellerId: userStore.user?.userId,
      title: form.title,
      description: form.description,
      price: form.price,
      condition: form.condition,
      categoryId: form.categoryId
    })

    const goodsId = res.data.goodsId

    for (let i = 0; i < imageList.value.length; i++) {
      const item = imageList.value[i]
      let imageUrl = item.uploadedUrl

      if (!imageUrl && item.file) {
        try {
          const uploadRes = await uploadImage(item.file)
          imageUrl = uploadRes.data.url
          item.uploadedUrl = imageUrl
        } catch {
          Message.warning(`第${i + 1}张图片上传失败，已跳过`)
          continue
        }
      }

      try {
        const imageUrl = imageList.value[i].uploadedUrl || imageList.value[i].url
        await uploadGoodsImage(goodsId, {
          imageUrl,
          sortOrder: i + 1
        })
      } catch {
        Message.warning(`第${i + 1}张图片保存失败`)
      }
    }

    Message.success('商品发布成功，等待审核')
    router.push(`/goods/${goodsId}`)
  } catch (error: any) {
    Message.error(error?.response?.data?.message || error?.message || '发布商品失败')
  } finally {
    loading.value = false
  }
}

onMounted(() => {
  fetchCategories()
})
</script>

<template>
  <div class="publish-page">
    <div class="page-header">
      <h2>发布商品</h2>
      <a-tag color="blue">发布后需审核才能上架</a-tag>
    </div>

    <a-card class="form-card">
      <a-form
        ref="formRef"
        :model="form"
        :rules="rules"
        layout="vertical"
        :label-col-props="{ style: { flex: '0 0 120px' } }"
        :wrapper-col-props="{ style: { flex: '1' } }"
      >
        <a-form-item field="title" label="商品标题">
          <a-input
            v-model="form.title"
            placeholder="请输入商品标题（最多100字）"
            :max-length="100"
            show-word-limit
          />
        </a-form-item>

        <a-form-item field="categoryId" label="商品分类">
          <a-select
            v-model="form.categoryId"
            placeholder="请选择商品分类"
            style="width: 100%"
          >
            <a-option
              v-for="cat in categories"
              :key="cat.categoryId"
              :value="cat.categoryId"
            >
              {{ cat.categoryName }}
            </a-option>
          </a-select>
        </a-form-item>

        <a-form-item field="price" label="商品价格">
          <a-input-number
            v-model="form.price"
            placeholder="请输入价格"
            :min="0"
            :max="999999"
            :precision="2"
            style="width: 100%"
          >
            <template #prepend>¥</template>
          </a-input-number>
        </a-form-item>

        <a-form-item field="condition" label="商品成色">
          <a-radio-group v-model="form.condition">
            <a-radio v-for="opt in conditionOptions" :key="opt.value" :value="opt.value">
              {{ opt.label }}
            </a-radio>
          </a-radio-group>
        </a-form-item>

        <a-form-item field="description" label="商品描述">
          <a-textarea
            v-model="form.description"
            placeholder="请详细描述商品的情况，包括使用时长、配件、瑕疵等"
            :auto-size="{ minRows: 4, maxRows: 10 }"
            :max-length="2000"
            show-word-limit
          />
        </a-form-item>

        <a-form-item label="商品图片">
          <ImageUpload v-model="imageList" />
        </a-form-item>

        <a-form-item>
          <a-space>
            <a-button type="primary" size="large" :loading="loading" @click="handleSubmit">
              发布商品
            </a-button>
            <a-button size="large" @click="router.back()">
              取消
            </a-button>
          </a-space>
        </a-form-item>
      </a-form>
    </a-card>
  </div>
</template>

<style scoped>
.publish-page {
  max-width: 800px;
  margin: 0 auto;
  padding: 24px;
}

.page-header {
  display: flex;
  align-items: center;
  gap: 12px;
  margin-bottom: 20px;
}

.page-header h2 {
  margin: 0;
  font-size: 24px;
  font-weight: 600;
}

.form-card {
  border-radius: 8px;
}
</style>
