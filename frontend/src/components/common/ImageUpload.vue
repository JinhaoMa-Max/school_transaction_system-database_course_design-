<script setup lang="ts">
import { computed } from 'vue'
import { Message } from '@arco-design/web-vue'
import { uploadImageFile } from '@/api'

interface ImageItem {
  url: string
  uploadedUrl?: string
}

const props = defineProps<{
  modelValue: ImageItem[]
  maxCount?: number
  disabled?: boolean
}>()

const emit = defineEmits<{
  'update:modelValue': [value: ImageItem[]]
}>()

const maxCount = computed(() => props.maxCount || 6)

const handleImageUpload = async (fileList: File[]) => {
  if (props.modelValue.length + fileList.length > maxCount.value) {
    Message.warning(`最多上传${maxCount.value}张图片`)
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
      const newList = [...props.modelValue, { url: base64Url }]
      emit('update:modelValue', newList)
      
      try {
        const uploadRes = await uploadImageFile(file)
        const currentList = [...props.modelValue]
        const imgIndex = currentList.findIndex(img => img.url === base64Url)
        if (imgIndex >= 0) {
          currentList[imgIndex].uploadedUrl = uploadRes.imageUrl
          emit('update:modelValue', [...currentList])
        }
      } catch {
        Message.warning('图片上传失败，请稍后重试')
      }
    }
    reader.readAsDataURL(file)
  }
}

const removeImage = (index: number) => {
  const newList = [...props.modelValue]
  newList.splice(index, 1)
  emit('update:modelValue', newList)
}

const triggerFileInput = () => {
  const input = document.getElementById('image-upload-input')
  input?.click()
}
</script>

<template>
  <div class="image-upload-area">
    <div class="image-list">
      <div
        v-for="(img, index) in modelValue"
        :key="index"
        class="image-item"
      >
        <img :src="img.url" :alt="`图片${index + 1}`" />
        <button
          v-if="!disabled"
          class="remove-btn"
          @click="removeImage(index)"
        >
          <icon-close />
        </button>
      </div>
      <div
        v-if="!disabled && modelValue.length < maxCount"
        class="upload-btn"
        @click="triggerFileInput"
      >
        <icon-plus />
        <span>上传图片</span>
        <span class="upload-hint">最多{{ maxCount }}张</span>
      </div>
    </div>
    <input
      id="image-upload-input"
      type="file"
      accept="image/jpeg,image/png,image/jpg"
      multiple
      style="display: none"
      @change="(e: any) => handleImageUpload(Array.from(e.target.files))"
    />
    <div class="image-tips">
      支持 JPG、PNG 格式，单张不超过 5MB，建议尺寸 800x800 以上
    </div>
  </div>
</template>

<style scoped>
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
  font-size: 14px;
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