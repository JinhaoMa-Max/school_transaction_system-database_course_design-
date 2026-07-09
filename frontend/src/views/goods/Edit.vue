<script setup lang="ts">
import { ref, reactive, onMounted, computed } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { Message } from '@arco-design/web-vue'
import {
  getGoodsById,
  getGoodsImages,
  getCategoryList,
  updateGoods,
  uploadGoodsImage,
  deleteGoodsImage,
  uploadImageFile
} from '@/api'
import { conditionMap, goodsStatusMap } from '@/constants'
import type { Category, GoodsImage, Goods } from '@/types'

const route = useRoute()
const router = useRouter()
const goodsId = Number(route.params.id)

const formRef = ref()
const loading = ref(false)
const pageLoading = ref(true)
const categories = ref<Category[]>([])
const existingImages = ref<GoodsImage[]>([])
const newImageList = ref<{ url: string; uploadedUrl?: string }[]>([])

const form = reactive({
  title: '',
  description: '',
  price: 0,
  condition: 'like_new' as 'new' | 'like_new' | 'slight_use' | 'obvious_trace',
  categoryId: undefined as number | undefined
})

const goods = ref<Goods | null>(null)

const canEdit = computed(() => {
  return goods.value && (goods.value.status === 'pending' || goods.value.status === 'rejected')
})

const totalImageCount = computed(() => {
  return existingImages.value.length + newImageList.value.length
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

const fetchGoodsDetail = async () => {
  try {
    const res = await getGoodsById(goodsId)
    goods.value = res.data
    form.title = res.data.title
    form.description = res.data.description
    form.price = res.data.price
    form.condition = res.data.condition
    form.categoryId = res.data.categoryId
  } catch {
    // 错误已由全局拦截器处理
  }
}

const fetchGoodsImages = async () => {
  try {
    const res = await getGoodsImages(goodsId)
    existingImages.value = res.data.sort((a, b) => a.sortOrder - b.sortOrder)
  } catch {
    // 错误已由全局拦截器处理
  }
}

const triggerFileInput = () => {
  const input = document.getElementById('edit-file-input')
  input?.click()
}

const handleImageUpload = async (fileList: File[]) => {
  if (totalImageCount.value + fileList.length > 6) {
    Message.warning('最多上传6张图片')
    return
  }
  for (const file of fileList) {
    if (file.size > 5 * 1024 * 1024) {
      Message.warning('单张图片不能超过5MB')
      continue
    }
    const reader = new FileReader()
    reader.onload = async (e) => {
      const base64Url = e.target?.result as string
      newImageList.value.push({ url: base64Url })
      
      try {
        const uploadRes = await uploadImageFile(file)
        const imgIndex = newImageList.value.findIndex(img => img.url === base64Url)
        if (imgIndex >= 0) {
          newImageList.value[imgIndex].uploadedUrl = uploadRes.imageUrl
        }
      } catch {
        Message.warning('图片上传失败，请稍后重试')
      }
    }
    reader.readAsDataURL(file)
  }
}

const removeNewImage = (index: number) => {
  newImageList.value.splice(index, 1)
}

const removeExistingImage = async (imageId: number) => {
  try {
    await deleteGoodsImage(imageId)
    existingImages.value = existingImages.value.filter((img) => img.imageId !== imageId)
    Message.success('图片删除成功')
  } catch {
    // 错误已由全局拦截器处理
  }
}

const handleSubmit = async () => {
  if (!canEdit.value) {
    Message.warning('当前商品状态不允许编辑')
    return
  }

  try {
    await formRef.value.validate()
  } catch {
    Message.error('请检查表单填写是否正确')
    return
  }

  if (totalImageCount.value === 0) {
    Message.warning('请至少保留一张商品图片')
    return
  }

  loading.value = true
  try {
    await updateGoods(goodsId, {
      title: form.title,
      description: form.description,
      price: form.price,
      condition: form.condition,
      categoryId: form.categoryId
    })

    let currentSortOrder = existingImages.value.length
    for (let i = 0; i < newImageList.value.length; i++) {
      try {
        const imageUrl = newImageList.value[i].uploadedUrl || newImageList.value[i].url
        await uploadGoodsImage(goodsId, {
          imageUrl,
          sortOrder: currentSortOrder + i + 1
        })
      } catch {
        // 忽略单张图片上传失败
      }
    }

    Message.success('商品修改成功')
    router.push(`/goods/${goodsId}`)
  } catch (error: any) {
    Message.error(error?.response?.data?.message || error?.message || '修改商品失败')
  } finally {
    loading.value = false
  }
}

onMounted(async () => {
  pageLoading.value = true
  try {
    await Promise.all([fetchGoodsDetail(), fetchGoodsImages(), fetchCategories()])
  } finally {
    pageLoading.value = false
  }
})
</script>

<template>
  <div class="edit-page">
    <a-spin :loading="pageLoading" style="width: 100%">
      <div class="page-header">
        <h2>编辑商品</h2>
        <a-tag v-if="goods" :color="goods.status === 'pending' ? 'orange' : goods.status === 'rejected' ? 'red' : 'green'">
          {{ goodsStatusMap[goods.status] || goods.status }}
        </a-tag>
      </div>

      <a-card v-if="!canEdit && goods" class="form-card">
        <a-empty description="当前商品状态不允许编辑">
          <a-button type="primary" @click="router.push(`/goods/${goodsId}`)">
            返回商品详情
          </a-button>
        </a-empty>
      </a-card>

      <a-card v-else class="form-card">
        <a-form
          ref="formRef"
          :model="form"
          :rules="rules"
          layout="vertical"
          :disabled="!canEdit"
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
            <div class="image-upload-area">
              <div class="image-list">
                <div
                  v-for="img in existingImages"
                  :key="img.imageId"
                  class="image-item"
                >
                  <img :src="img.imageUrl" :alt="`商品图片${img.sortOrder}`" />
                  <button
                    v-if="canEdit"
                    class="remove-btn"
                    @click="removeExistingImage(img.imageId)"
                  >
                    ×
                  </button>
                </div>
                <div
                  v-for="(img, index) in newImageList"
                  :key="'new-' + index"
                  class="image-item"
                >
                  <img :src="img.url" :alt="`新图片${index + 1}`" />
                  <button
                    v-if="canEdit"
                    class="remove-btn"
                    @click="removeNewImage(index)"
                  >
                    ×
                  </button>
                </div>
                <div
                  v-if="canEdit && totalImageCount < 6"
                  class="upload-btn"
                  @click="triggerFileInput"
                >
                  <span class="plus-icon">+</span>
                  <span>上传图片</span>
                  <span class="upload-hint">最多6张</span>
                </div>
              </div>
              <input
                id="edit-file-input"
                type="file"
                accept="image/jpeg,image/png,image/jpg"
                multiple
                style="display: none"
                @change="(e: any) => handleImageUpload(Array.from(e.target.files))"
              />
            </div>
            <div class="image-tips">
              支持 JPG、PNG 格式，单张不超过 5MB，建议尺寸 800x800 以上
            </div>
          </a-form-item>

          <a-form-item v-if="canEdit">
            <a-space>
              <a-button type="primary" size="large" :loading="loading" @click="handleSubmit">
                保存修改
              </a-button>
              <a-button size="large" @click="router.back()">
                取消
              </a-button>
            </a-space>
          </a-form-item>
        </a-form>
      </a-card>
    </a-spin>
  </div>
</template>

<style scoped>
.edit-page {
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

.image-upload-area {
  width: 100%;
}

.image-list {
  display: flex;
  flex-wrap: wrap;
  gap: 12px;
}

.image-item {
  position: relative;
  width: 120px;
  height: 120px;
  border-radius: 6px;
  overflow: hidden;
  border: 1px solid #e5e6eb;
}

.image-item img {
  width: 100%;
  height: 100%;
  object-fit: cover;
}

.remove-btn {
  position: absolute;
  top: 4px;
  right: 4px;
  width: 24px;
  height: 24px;
  border: none;
  border-radius: 50%;
  background: rgba(0, 0, 0, 0.5);
  color: white;
  cursor: pointer;
  display: flex;
  align-items: center;
  justify-content: center;
  padding: 0;
  font-size: 16px;
  line-height: 1;
}

.upload-btn {
  width: 120px;
  height: 120px;
  border: 2px dashed #c9cdd4;
  border-radius: 6px;
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  cursor: pointer;
  transition: border-color 0.2s;
  color: #86909c;
  font-size: 12px;
  gap: 4px;
}

.upload-btn:hover {
  border-color: #165dff;
  color: #165dff;
}

.plus-icon {
  font-size: 28px;
  line-height: 1;
  font-weight: 300;
}

.upload-hint {
  font-size: 10px;
  color: #c9cdd4;
}

.image-tips {
  margin-top: 8px;
  font-size: 12px;
  color: #86909c;
}
</style>
