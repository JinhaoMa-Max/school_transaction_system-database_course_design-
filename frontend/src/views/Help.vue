<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { Message } from '@arco-design/web-vue'
import { IconLeft, IconBook } from '@arco-design/web-vue/es/icon'
import { getManual } from '@/api/help'
import router from '@/router'

interface InlineSegment {
  text: string
  bold: boolean
}

interface HelpBlock {
  type: 'h1' | 'h2' | 'p' | 'li'
  id: string
  segments: InlineSegment[]
}

const loading = ref(false)
const blocks = ref<HelpBlock[]>([])
const toc = ref<{ id: string; title: string }[]>([])

// 轻量 markdown 解析：手册由本项目维护，语法固定为 # / ## / - / **加粗** / 段落
const parseInline = (text: string): InlineSegment[] => {
  const segments: InlineSegment[] = []
  text.split('**').forEach((part, index) => {
    if (!part) return
    segments.push({ text: part, bold: index % 2 === 1 })
  })
  return segments
}

const parseManual = (md: string): { result: HelpBlock[]; tocItems: { id: string; title: string }[] } => {
  const result: HelpBlock[] = []
  const tocItems: { id: string; title: string }[] = []
  const paragraphBuffer: string[] = []
  let sectionIndex = 0

  const flushParagraph = () => {
    if (paragraphBuffer.length > 0) {
      result.push({ type: 'p', id: '', segments: parseInline(paragraphBuffer.join(' ')) })
      paragraphBuffer.length = 0
    }
  }

  for (const raw of md.split('\n')) {
    const line = raw.trim()
    if (!line) {
      flushParagraph()
      continue
    }

    if (line.startsWith('## ')) {
      flushParagraph()
      const title = line.slice(3).trim()
      const id = `section-${sectionIndex++}`
      tocItems.push({ id, title })
      result.push({ type: 'h2', id, segments: [{ text: title, bold: false }] })
      continue
    }

    if (line.startsWith('# ')) {
      flushParagraph()
      result.push({ type: 'h1', id: '', segments: [{ text: line.slice(2).trim(), bold: false }] })
      continue
    }

    if (line.startsWith('- ')) {
      flushParagraph()
      result.push({ type: 'li', id: '', segments: parseInline(line.slice(2).trim()) })
      continue
    }

    paragraphBuffer.push(line)
  }
  flushParagraph()

  return { result, tocItems }
}

const load = async () => {
  loading.value = true
  try {
    const res = await getManual()
    const md = res.data ?? ''
    if (!md) throw new Error('帮助文档为空')
    const parsed = parseManual(md)
    blocks.value = parsed.result
    toc.value = parsed.tocItems
  } catch {
    Message.error('帮助文档加载失败')
  } finally {
    loading.value = false
  }
}

const jumpTo = (id: string) => {
  document.getElementById(id)?.scrollIntoView({ behavior: 'smooth', block: 'start' })
}

const goBack = () => {
  if (window.history.length > 1) {
    router.back()
  } else {
    router.push('/')
  }
}

onMounted(load)
</script>

<template>
  <div class="help-page">
    <div class="help-topbar">
      <div class="help-topbar-inner">
        <a-button class="help-back" @click="goBack">
          <template #icon><IconLeft /></template>
          返回
        </a-button>
        <div class="help-topbar-title">
          <IconBook />
          <span>帮助中心</span>
        </div>
      </div>
    </div>

    <div class="help-body">
      <a-spin :loading="loading" class="help-spin">
        <div v-if="blocks.length > 0" class="help-layout">
          <aside class="help-toc">
            <div class="help-toc-title">目录</div>
            <a
              v-for="item in toc"
              :key="item.id"
              class="help-toc-item"
              @click.prevent="jumpTo(item.id)"
            >
              {{ item.title }}
            </a>
          </aside>

          <article class="help-content">
            <template v-for="(block, index) in blocks" :key="index">
              <h1 v-if="block.type === 'h1'" class="help-h1">
                <template v-for="(seg, i) in block.segments" :key="i">
                  <strong v-if="seg.bold">{{ seg.text }}</strong>
                  <span v-else>{{ seg.text }}</span>
                </template>
              </h1>
              <h2 v-else-if="block.type === 'h2'" :id="block.id" class="help-h2">
                <template v-for="(seg, i) in block.segments" :key="i">
                  <strong v-if="seg.bold">{{ seg.text }}</strong>
                  <span v-else>{{ seg.text }}</span>
                </template>
              </h2>
              <p v-else-if="block.type === 'p'" class="help-p">
                <template v-for="(seg, i) in block.segments" :key="i">
                  <strong v-if="seg.bold">{{ seg.text }}</strong>
                  <span v-else>{{ seg.text }}</span>
                </template>
              </p>
              <div v-else class="help-li">
                <template v-for="(seg, i) in block.segments" :key="i">
                  <strong v-if="seg.bold">{{ seg.text }}</strong>
                  <span v-else>{{ seg.text }}</span>
                </template>
              </div>
            </template>
            <div class="help-footer">✨ 内容与 AI 助手知识库同步更新</div>
          </article>
        </div>
        <a-empty v-else-if="!loading" description="帮助文档暂不可用，请稍后再试" class="help-empty" />
      </a-spin>
    </div>
  </div>
</template>

<style scoped>
.help-page {
  min-height: 100vh;
  background: #f5f6fb;
  font-family:
    -apple-system,
    BlinkMacSystemFont,
    "Segoe UI",
    "PingFang SC",
    "Hiragino Sans GB",
    "Microsoft YaHei",
    "Helvetica Neue",
    Arial,
    sans-serif;
}

.help-topbar {
  background: linear-gradient(135deg, #253554 0%, #684993 100%);
  padding: 0 24px;
  box-shadow: 0 4px 16px rgba(37, 53, 84, 0.2);
}

.help-topbar-inner {
  max-width: 1080px;
  margin: 0 auto;
  height: 64px;
  display: flex;
  align-items: center;
  gap: 16px;
}

.help-back {
  background: rgba(255, 255, 255, 0.15);
  color: #fff;
  border: none;
  border-radius: 10px;
}

.help-back:hover {
  background: rgba(255, 255, 255, 0.28);
  color: #fff;
}

.help-topbar-title {
  display: flex;
  align-items: center;
  gap: 8px;
  color: #fff;
  font-size: 18px;
  font-weight: 600;
}

.help-body {
  max-width: 1080px;
  margin: 0 auto;
  padding: 24px;
}

.help-spin {
  width: 100%;
}

.help-layout {
  display: flex;
  gap: 20px;
  align-items: flex-start;
}

.help-toc {
  position: sticky;
  top: 24px;
  width: 200px;
  flex-shrink: 0;
  background: #fff;
  border-radius: 20px;
  padding: 16px;
  box-shadow: 0 8px 24px rgba(45, 54, 142, 0.06);
}

.help-toc-title {
  font-size: 13px;
  font-weight: 600;
  color: var(--color-text-3);
  margin-bottom: 10px;
  padding-left: 8px;
}

.help-toc-item {
  display: block;
  padding: 7px 8px;
  border-radius: 8px;
  font-size: 13px;
  color: #4e5969;
  cursor: pointer;
  transition: all 0.2s;
}

.help-toc-item:hover {
  background: rgb(var(--primary-6));
  color: #fff;
}

.help-content {
  flex: 1;
  min-width: 0;
  background: #fff;
  border-radius: 20px;
  padding: 28px 32px;
  box-shadow: 0 8px 24px rgba(45, 54, 142, 0.06);
  animation: fadeUp 0.6s ease both;
}

.help-h1 {
  font-size: 24px;
  font-weight: 700;
  color: #1d2129;
  margin: 0 0 8px;
}

.help-h2 {
  font-size: 18px;
  font-weight: 600;
  color: #1d2129;
  margin: 28px 0 12px;
  padding-bottom: 8px;
  border-bottom: 1px solid var(--color-border-2);
  scroll-margin-top: 24px;
}

.help-h2:first-of-type {
  margin-top: 8px;
}

.help-p {
  margin: 0 0 10px;
  font-size: 14px;
  line-height: 1.8;
  color: #4e5969;
}

.help-li {
  position: relative;
  margin: 0 0 8px;
  padding-left: 16px;
  font-size: 14px;
  line-height: 1.8;
  color: #4e5969;
}

.help-li::before {
  content: '';
  position: absolute;
  left: 2px;
  top: 10px;
  width: 6px;
  height: 6px;
  border-radius: 50%;
  background: rgb(var(--primary-6));
}

.help-footer {
  margin-top: 28px;
  padding-top: 14px;
  border-top: 1px solid var(--color-border-2);
  font-size: 12px;
  color: var(--color-text-3);
  text-align: center;
}

.help-empty {
  background: #fff;
  border-radius: 20px;
  padding: 60px 0;
}

@keyframes fadeUp {
  from {
    opacity: 0;
    transform: translateY(24px);
  }

  to {
    opacity: 1;
    transform: translateY(0);
  }
}

@media (max-width: 768px) {
  .help-toc {
    display: none;
  }

  .help-content {
    padding: 20px 16px;
  }
}
</style>
