<script setup lang="ts">
import { ref, reactive, onMounted } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { Message } from '@arco-design/web-vue'
import { createReport } from '@/api'

const route = useRoute()
const router = useRouter()

const loading = ref(false)
const submitLoading = ref(false)

const form = reactive({
  reportType: 'goods' as 'goods' | 'user' | 'order',
  reportedGoodsId: undefined as number | undefined,
  reportedUserId: undefined as number | undefined,
  reportedOrderId: undefined as number | undefined,
  reason: ''
})

const formRef = ref()

const reportTypeOptions = [
  { label: '商品', value: 'goods' },
  { label: '用户', value: 'user' },
  { label: '订单', value: 'order' }
]

const typeLabelMap: Record<string, string> = {
  goods: '商品ID',
  user: '用户ID',
  order: '订单ID'
}

const initFormFromQuery = () => {
  const type = route.query.type as string
  const id = route.query.id as string

  if (type && ['goods', 'user', 'order'].includes(type)) {
    form.reportType = type as 'goods' | 'user' | 'order'
  }

  if (id) {
    const idNum = Number(id)
    if (!isNaN(idNum)) {
      switch (form.reportType) {
        case 'goods':
          form.reportedGoodsId = idNum
          break
        case 'user':
          form.reportedUserId = idNum
          break
        case 'order':
          form.reportedOrderId = idNum
          break
      }
    }
  }
}

const handleTypeChange = () => {
  form.reportedGoodsId = undefined
  form.reportedUserId = undefined
  form.reportedOrderId = undefined
}

const handleSubmit = async ({ values }: { values: Record<string, any> }) => {
  submitLoading.value = true
  try {
    const params: {
      reportType: 'goods' | 'user' | 'order'
      reportedGoodsId?: number
      reportedUserId?: number
      reportedOrderId?: number
      reason: string
    } = {
      reportType: values.reportType,
      reason: values.reason
    }

    switch (values.reportType) {
      case 'goods':
        params.reportedGoodsId = values.reportedGoodsId
        break
      case 'user':
        params.reportedUserId = values.reportedUserId
        break
      case 'order':
        params.reportedOrderId = values.reportedOrderId
        break
    }

    await createReport(params)
    Message.success('举报提交成功，我们会尽快处理')
    setTimeout(() => {
      handleBack()
    }, 1500)
  } catch {
  } finally {
    submitLoading.value = false
  }
}

const handleBack = () => {
  // 如果有浏览器历史记录则返回上一页，否则跳转到首页
  if (window.history.length > 1) {
    router.back()
  } else {
    router.push('/')
  }
}

const validateReportedId = (value: any, callback: (error?: string) => void) => {
  if (value === undefined || value === null || isNaN(value)) {
    callback(`请输入${typeLabelMap[form.reportType]}`)
  } else if (value <= 0) {
    callback('ID必须大于0')
  } else {
    callback()
  }
}

onMounted(() => {
  initFormFromQuery()
})
</script>

<template>
  <div class="report-page">
    <a-spin :loading="loading" dot>
      <div class="report-container">
        <div class="page-header">
          <a-button type="text" @click="handleBack">
            <icon-left />
            返回
          </a-button>
          <h2 class="page-title">发起举报</h2>
        </div>

        <a-card>
          <a-alert type="warning" style="margin-bottom: 24px">
            <template #content>
              <p>请如实填写举报信息，恶意举报将被扣除信用分。</p>
            </template>
          </a-alert>

          <a-form
            ref="formRef"
            :model="form"
            layout="vertical"
            @submit="handleSubmit"
          >
            <a-form-item
              field="reportType"
              label="举报类型"
              :rules="[{ required: true, message: '请选择举报类型' }]"
            >
              <a-select
                v-model="form.reportType"
                placeholder="请选择举报类型"
                style="width: 100%"
                @change="handleTypeChange"
              >
                <a-option
                  v-for="item in reportTypeOptions"
                  :key="item.value"
                  :value="item.value"
                >
                  {{ item.label }}
                </a-option>
              </a-select>
            </a-form-item>

            <a-form-item
              v-if="form.reportType === 'goods'"
              field="reportedGoodsId"
              :label="typeLabelMap[form.reportType]"
              :rules="[{ validator: validateReportedId }]"
            >
              <a-input-number
                v-model="form.reportedGoodsId"
                :placeholder="`请输入${typeLabelMap[form.reportType]}`"
                style="width: 100%"
                :min="1"
                size="large"
              />
            </a-form-item>

            <a-form-item
              v-if="form.reportType === 'user'"
              field="reportedUserId"
              :label="typeLabelMap[form.reportType]"
              :rules="[{ validator: validateReportedId }]"
            >
              <a-input-number
                v-model="form.reportedUserId"
                :placeholder="`请输入${typeLabelMap[form.reportType]}`"
                style="width: 100%"
                :min="1"
                size="large"
              />
            </a-form-item>

            <a-form-item
              v-if="form.reportType === 'order'"
              field="reportedOrderId"
              :label="typeLabelMap[form.reportType]"
              :rules="[{ validator: validateReportedId }]"
            >
              <a-input-number
                v-model="form.reportedOrderId"
                :placeholder="`请输入${typeLabelMap[form.reportType]}`"
                style="width: 100%"
                :min="1"
                size="large"
              />
            </a-form-item>

            <a-form-item
              field="reason"
              label="举报原因"
              :rules="[{ required: true, message: '请输入举报原因' }]"
            >
              <a-textarea
                v-model="form.reason"
                placeholder="请详细描述举报原因（最多500字）"
                :max-length="500"
                show-word-limit
                :auto-size="{ minRows: 5, maxRows: 10 }"
              />
            </a-form-item>

            <div class="form-actions">
              <a-button size="large" @click="handleBack">
                取消
              </a-button>
              <a-button type="primary" status="danger" html-type="submit" size="large" :loading="submitLoading">
                提交举报
              </a-button>
            </div>
          </a-form>
        </a-card>
      </div>
    </a-spin>
  </div>
</template>

<style scoped>
.report-page {
  max-width: 640px;
  margin: 0 auto;
  padding: 24px;
}

.page-header {
  margin-bottom: 24px;
}

.page-title {
  margin: 16px 0 0 0;
  font-size: 24px;
  font-weight: 600;
}

.form-actions {
  display: flex;
  justify-content: flex-end;
  gap: 12px;
  margin-top: 24px;
}
</style>
