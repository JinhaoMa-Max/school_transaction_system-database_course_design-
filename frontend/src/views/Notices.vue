<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { getPublicNoticeList, getPublicNoticeById } from '@/api/notice'
import type { Notice } from '@/types'

const notices = ref<Notice[]>([])
const loading = ref(false)
const failed = ref(false)
const page = ref(1)
const total = ref(0)
const noticeType = ref('')
const selected = ref<Notice | null>(null)
const detailVisible = ref(false)
const detailLoading = ref(false)
const typeNames: Record<string, string> = { system: '系统公告', transaction: '交易提醒', violation: '违规通报' }
let requestVersion = 0

const loadNotices = async () => {
  const version = ++requestVersion
  loading.value = true
  failed.value = false
  try {
    const res = await getPublicNoticeList({ page: page.value, size: 10, noticeType: noticeType.value || undefined })
    if (version !== requestVersion) return
    notices.value = res.data.list
    total.value = res.data.total
  } catch {
    if (version === requestVersion) failed.value = true
  } finally {
    if (version === requestVersion) loading.value = false
  }
}

const openNotice = async (notice: Notice) => {
  detailLoading.value = true
  selected.value = null
  detailVisible.value = true
  try {
    selected.value = (await getPublicNoticeById(notice.noticeId)).data
  } catch {
    detailVisible.value = false
    loadNotices()
  } finally {
    detailLoading.value = false
  }
}
const changeType = () => { page.value = 1; loadNotices() }
const changePage = (value: number) => { page.value = value; loadNotices() }
const formatTime = (value: string) => value.replace('T', ' ').slice(0, 16)
onMounted(loadNotices)
</script>

<template>
  <main class="notices-page">
    <a-card title="平台公告">
      <p class="intro">查看平台最新消息、交易提醒与违规通报。</p>
      <a-radio-group v-model="noticeType" type="button" @change="changeType">
        <a-radio value="">全部</a-radio>
        <a-radio v-for="(name, type) in typeNames" :key="type" :value="type">{{ name }}</a-radio>
      </a-radio-group>
      <a-spin :loading="loading" style="display: block; min-height: 160px; margin-top: 20px">
        <a-result v-if="failed" status="error" title="公告加载失败">
          <template #extra><a-button @click="loadNotices">重新加载</a-button></template>
        </a-result>
        <a-empty v-else-if="!loading && notices.length === 0" description="暂无公告" />
        <a-list v-else :bordered="false">
          <a-list-item v-for="notice in notices" :key="notice.noticeId">
            <article class="notice-item">
              <div class="notice-meta"><a-tag color="blue">{{ typeNames[notice.noticeType] || '平台公告' }}</a-tag><time>{{ formatTime(notice.publishTime) }}</time></div>
              <button class="notice-title" @click="openNotice(notice)">{{ notice.title }}</button>
              <p class="notice-preview">{{ notice.content }}</p>
              <a-button type="text" @click="openNotice(notice)">阅读全文</a-button>
            </article>
          </a-list-item>
        </a-list>
      </a-spin>
      <a-pagination v-if="!failed && total > 10" :total="total" :current="page" :page-size="10" show-total @change="changePage" />
    </a-card>
    <a-modal v-model:visible="detailVisible" :title="selected?.title || '公告详情'" :footer="false" width="min(680px, 92vw)">
      <a-spin :loading="detailLoading" style="width: 100%">
        <template v-if="selected">
          <div class="notice-meta"><a-tag color="blue">{{ typeNames[selected.noticeType] || '平台公告' }}</a-tag><time>{{ formatTime(selected.publishTime) }}</time></div>
          <p class="notice-content">{{ selected.content }}</p>
        </template>
      </a-spin>
    </a-modal>
  </main>
</template>

<style scoped>
.notices-page { max-width: 1180px; margin: 0 auto; padding: 24px; }
.intro, .notice-meta { color: #86909c; }
.intro { margin: 0 0 20px; }
.notice-item { width: 100%; min-width: 0; }
.notice-meta { display: flex; align-items: center; gap: 12px; font-size: 13px; }
.notice-title { display: block; border: 0; padding: 0; margin: 12px 0 8px; background: none; color: #1d2129; font: inherit; font-size: 18px; font-weight: 600; text-align: left; cursor: pointer; overflow-wrap: anywhere; }
.notice-title:hover { color: #165dff; }
.notice-preview { display: -webkit-box; -webkit-line-clamp: 2; -webkit-box-orient: vertical; overflow: hidden; color: #4e5969; line-height: 1.7; overflow-wrap: anywhere; }
.notice-content { white-space: pre-wrap; overflow-wrap: anywhere; line-height: 1.9; }
</style>
