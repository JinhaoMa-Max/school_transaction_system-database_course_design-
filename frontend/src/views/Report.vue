<script setup lang="ts">
import { ref, reactive, onMounted, watch, computed } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { Message } from '@arco-design/web-vue'
import { createReport, getGoodsById, getUserById, getOrderById } from '@/api'

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
const targetName = ref('')
const targetLoading = ref(false)
const targetId = computed(() => form.reportType === 'goods' ? form.reportedGoodsId : form.reportType === 'user' ? form.reportedUserId : form.reportedOrderId)
const fromContext = computed(() => route.query.type === form.reportType && Number(route.query.id) === targetId.value)
let targetVersion = 0
watch([() => form.reportType, targetId], async ([type, id]) => {
  const version = ++targetVersion
  targetName.value = ''
  targetLoading.value = false
  if (!id || !Number.isInteger(id) || id <= 0) return
  targetLoading.value = true
  try {
    let name = ''
    if (type === 'goods') name = (await getGoodsById(id)).data.title
    else if (type === 'user') {
      const user = (await getUserById(id)).data
      name = user.nickname || user.username
    } else {
      const order = (await getOrderById(id)).data
      name = `${order.goodsTitle || '商品信息暂不可用'}（订单编号 ${order.orderId}）`
    }
    if (version === targetVersion) targetName.value = name
  } catch {
    // 请求失败时由全局拦截器提示，不能把未验证的目标展示为有效对象。
  } finally {
    if (version === targetVersion) targetLoading.value = false
  }
})

const reportTypeOptions = [
  { label: '商品', value: 'goods' },
  { label: '用户', value: 'user' },
  { label: '订单', value: 'order' }
]

const typeLabelMap: Record<string, string> = {
  goods: '商品编号',
  user: '用户编号',
  order: '订单编号'
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

const handleSubmit = async ({ errors }: { errors?: Record<string, any> }) => {
  if (errors || targetLoading.value || !targetName.value) {
    if (!errors) Message.warning('请先确认有效的举报对象')
    return
  }
  submitLoading.value = true
  try {
    const params: {
      reportType: 'goods' | 'user' | 'order'
      reportedGoodsId?: number
      reportedUserId?: number
      reportedOrderId?: number
      reason: string
    } = {
      reportType: form.reportType,
      reason: form.reason
    }

    switch (form.reportType) {
      case 'goods':
        params.reportedGoodsId = form.reportedGoodsId
        break
      case 'user':
        params.reportedUserId = form.reportedUserId
        break
      case 'order':
        params.reportedOrderId = form.reportedOrderId
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
  } else if (!Number.isInteger(value) || value <= 0) {
    callback('请输入有效的正整数编号')
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
              v-if="!fromContext && form.reportType === 'goods'"
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
              v-if="!fromContext && form.reportType === 'user'"
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
              v-if="!fromContext && form.reportType === 'order'"
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

            <a-form-item label="举报对象">
              <a-spin :loading="targetLoading">
                <span>{{ targetName || '请选择举报对象；从商品详情进入可自动带入商品名称' }}</span>
              </a-spin>
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
