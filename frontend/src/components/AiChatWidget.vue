<script setup lang="ts">
import { ref, computed, watch, nextTick, onUnmounted } from 'vue'
import { Message } from '@arco-design/web-vue'
import { IconClose, IconDelete, IconSend, IconBook } from '@arco-design/web-vue/es/icon'
import { streamAiChat } from '@/api/ai'
import type { AiChatMessage } from '@/api/ai'
import router from '@/router'

const WELCOME_MESSAGE = '嗨～我是校园二手小助手 🤖 有什么想问的尽管问我哦！'

const open = ref(false)
const input = ref('')
const streaming = ref(false)
const messages = ref<AiChatMessage[]>([{ role: 'assistant', content: WELCOME_MESSAGE }])
const listRef = ref<HTMLElement | null>(null)
let abortCtrl: AbortController | null = null

const suggestions = [
  { icon: '📦', text: '怎么发布商品？' },
  { icon: '💰', text: '如何砍价？' },
  { icon: '🚨', text: '怎么举报违规商品？' },
  { icon: '🤝', text: '如何线下交易？' }
]

const showSuggestions = computed(() => messages.value.length <= 1)

const togglePanel = () => {
  open.value = !open.value
  if (open.value) scrollToBottom()
}

const goHelp = () => {
  open.value = false
  router.push('/help')
}

const scrollToBottom = () => {
  nextTick(() => {
    const el = listRef.value
    if (el) el.scrollTop = el.scrollHeight
  })
}

watch(
  () => messages.value.map(m => m.content).join('|'),
  () => scrollToBottom()
)

const send = async (text?: string) => {
  const content = (text ?? input.value).trim()
  if (!content) {
    Message.warning('请输入消息内容')
    return
  }
  if (streaming.value) return

  input.value = ''
  messages.value.push({ role: 'user', content })
  const placeholder: AiChatMessage = { role: 'assistant', content: '' }
  messages.value.push(placeholder)
  streaming.value = true
  abortCtrl = new AbortController()

  const history = messages.value.filter(m => m !== placeholder)

  try {
    await streamAiChat(
      history,
      delta => {
        placeholder.content += delta
      },
      abortCtrl.signal
    )
  } catch (e) {
    if ((e as Error)?.name === 'AbortError') return // interrupted by clear/close
    messages.value = messages.value.filter(m => m !== placeholder)
    Message.error((e as Error)?.message || 'AI 服务暂时不可用')
  } finally {
    streaming.value = false
    abortCtrl = null
  }
}

const clearChat = () => {
  abortCtrl?.abort()
  abortCtrl = null
  streaming.value = false
  messages.value = [{ role: 'assistant', content: WELCOME_MESSAGE }]
}

onUnmounted(() => {
  abortCtrl?.abort()
})
</script>

<template>
  <div class="ai-widget">
    <!-- 悬浮入口按钮 -->
    <Transition name="fab">
      <button v-if="!open" class="ai-fab" type="button" aria-label="AI 助手" @click="togglePanel">
        <span class="ai-fab-spark spark-1">✨</span>
        <span class="ai-fab-spark spark-2">✨</span>
        <span class="ai-fab-label">AI 小助手</span>
        <span class="ai-fab-mascot">🤖</span>
      </button>
    </Transition>

    <!-- 聊天面板 -->
    <Transition name="panel">
      <div v-if="open" class="ai-panel">
        <div class="ai-header">
          <div class="ai-header-title">
            <div class="ai-mascot-avatar">🤖</div>
            <div>
              <div class="ai-title-main">AI 小助手</div>
              <div class="ai-title-sub">有问题尽管问我哦～</div>
            </div>
          </div>
          <div class="ai-header-actions">
            <a-tooltip content="帮助中心">
              <a-button shape="circle" size="small" @click="goHelp">
                <template #icon><IconBook /></template>
              </a-button>
            </a-tooltip>
            <a-tooltip content="清空对话">
              <a-button shape="circle" size="small" @click="clearChat">
                <template #icon><IconDelete /></template>
              </a-button>
            </a-tooltip>
            <a-button shape="circle" size="small" @click="togglePanel">
              <template #icon><IconClose /></template>
            </a-button>
          </div>
        </div>

        <div ref="listRef" class="ai-messages">
          <div v-if="showSuggestions" class="ai-suggestions">
            <div class="ai-suggestions-label">你可以这样问 👇</div>
            <div class="ai-suggestion-chips">
              <span
                v-for="s in suggestions"
                :key="s.text"
                class="ai-suggestion-chip"
                @click="send(s.text)"
              >
                <span class="ai-suggestion-emoji">{{ s.icon }}</span>{{ s.text }}
              </span>
            </div>
          </div>

          <div
            v-for="(msg, index) in messages"
            :key="index"
            class="ai-message-row"
            :class="{ mine: msg.role === 'user' }"
          >
            <div v-if="msg.role === 'assistant'" class="ai-msg-avatar">🤖</div>
            <div class="ai-message-bubble">
              <span v-if="msg.content" class="ai-message-content">{{ msg.content }}</span>
              <span
                v-if="streaming && index === messages.length - 1 && !msg.content"
                class="ai-thinking"
              >
                <span class="ai-dot"></span><span class="ai-dot"></span><span class="ai-dot"></span>
              </span>
              <span
                v-if="streaming && index === messages.length - 1 && msg.content"
                class="ai-cursor"
              >▍</span>
            </div>
          </div>
        </div>

        <div class="ai-input">
          <a-textarea
            v-model="input"
            placeholder="想问点什么？Enter 发送～"
            :auto-size="{ minRows: 1, maxRows: 4 }"
            :disabled="streaming"
            @keydown.enter.exact.prevent="send()"
          />
          <a-button
            class="ai-send-btn"
            type="primary"
            shape="circle"
            :loading="streaming"
            :disabled="!input.trim()"
            aria-label="发送"
            @click="send()"
          >
            <template #icon><IconSend /></template>
          </a-button>
        </div>
      </div>
    </Transition>
  </div>
</template>

<style scoped>
/* ============ 悬浮按钮 ============ */
.ai-fab {
  position: fixed;
  right: 24px;
  bottom: 24px;
  z-index: 999;
  width: 56px;
  height: 56px;
  border-radius: 50%;
  border: none;
  cursor: pointer;
  background: linear-gradient(135deg, #7d5fff 0%, #a84fcb 100%);
  color: #fff;
  display: flex;
  align-items: center;
  justify-content: center;
  box-shadow: 0 6px 18px rgba(125, 95, 255, 0.4);
  transition: transform 0.25s cubic-bezier(0.34, 1.56, 0.64, 1);
}

.ai-fab:hover {
  transform: scale(1.1) rotate(-5deg);
}

.ai-fab-mascot {
  font-size: 27px;
  line-height: 1;
  animation: bob 2.6s ease-in-out infinite;
}

.ai-fab-label {
  position: absolute;
  right: 64px;
  top: 50%;
  transform: translateY(-50%) translateX(8px);
  padding: 5px 12px;
  border-radius: 14px;
  background: #fff;
  color: #684993;
  font-size: 12px;
  font-weight: 500;
  white-space: nowrap;
  box-shadow: 0 3px 10px rgba(104, 73, 147, 0.18);
  opacity: 0;
  pointer-events: none;
  transition: opacity 0.25s, transform 0.25s;
}

.ai-fab:hover .ai-fab-label {
  opacity: 1;
  transform: translateY(-50%) translateX(0);
}

.ai-fab-spark {
  position: absolute;
  font-size: 11px;
  animation: twinkle 1.8s ease-in-out infinite;
}

.spark-1 {
  top: 2px;
  right: 4px;
  animation-delay: 0.3s;
}

.spark-2 {
  bottom: 4px;
  left: 2px;
  animation-delay: 1.1s;
}

.fab-enter-active {
  transition: transform 0.3s cubic-bezier(0.34, 1.56, 0.64, 1), opacity 0.3s;
}

.fab-leave-active {
  transition: transform 0.2s ease, opacity 0.2s;
}

.fab-enter-from,
.fab-leave-to {
  transform: scale(0);
  opacity: 0;
}

/* ============ 聊天面板 ============ */
.ai-panel {
  position: fixed;
  right: 24px;
  bottom: 96px;
  z-index: 999;
  width: min(380px, calc(100vw - 32px));
  height: min(520px, 70vh);
  border-radius: 24px;
  border: 1px solid #f0eafc;
  background: #fff;
  box-shadow: 0 12px 40px rgba(104, 73, 147, 0.2);
  display: flex;
  flex-direction: column;
  overflow: hidden;
  transform-origin: bottom right;
}

.panel-enter-active {
  transition: transform 0.35s cubic-bezier(0.34, 1.56, 0.64, 1), opacity 0.35s;
}

.panel-leave-active {
  transition: transform 0.2s ease, opacity 0.2s;
}

.panel-enter-from,
.panel-leave-to {
  transform: translateY(20px) scale(0.92);
  opacity: 0;
}

.ai-header {
  position: relative;
  overflow: hidden;
  flex-shrink: 0;
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 14px 16px;
  background: linear-gradient(135deg, #7d5fff 0%, #a84fcb 100%);
  color: #fff;
}

.ai-header::before {
  content: '';
  position: absolute;
  top: -40px;
  right: -20px;
  width: 90px;
  height: 90px;
  border-radius: 50%;
  background: rgba(255, 255, 255, 0.08);
}

.ai-header::after {
  content: '';
  position: absolute;
  bottom: -30px;
  left: 60px;
  width: 60px;
  height: 60px;
  border-radius: 50%;
  background: rgba(255, 255, 255, 0.06);
}

.ai-header-title {
  display: flex;
  align-items: center;
  gap: 10px;
  z-index: 1;
}

.ai-mascot-avatar {
  width: 38px;
  height: 38px;
  border-radius: 50%;
  background: rgba(255, 255, 255, 0.2);
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 20px;
  animation: wave 1.4s ease-in-out 2;
}

.ai-title-main {
  font-size: 15px;
  font-weight: 600;
  line-height: 20px;
}

.ai-title-sub {
  font-size: 12px;
  opacity: 0.8;
  line-height: 16px;
}

.ai-header-actions {
  display: flex;
  gap: 8px;
  z-index: 1;
}

.ai-header-actions :deep(.arco-btn) {
  background: rgba(255, 255, 255, 0.15);
  color: #fff;
  border: none;
}

.ai-header-actions :deep(.arco-btn:hover) {
  background: rgba(255, 255, 255, 0.28);
  color: #fff;
}

/* ============ 消息区 ============ */
.ai-messages {
  flex: 1;
  min-height: 0;
  overflow-y: auto;
  padding: 12px;
  background: #faf9ff;
}

.ai-messages::-webkit-scrollbar {
  width: 5px;
}

.ai-messages::-webkit-scrollbar-thumb {
  background: #e4dcf7;
  border-radius: 3px;
}

.ai-suggestions {
  margin-bottom: 12px;
  padding: 12px;
  border-radius: 16px;
  background: #fff;
  box-shadow: 0 2px 10px rgba(104, 73, 147, 0.06);
}

.ai-suggestions-label {
  margin-bottom: 8px;
  font-size: 12px;
  color: var(--color-text-3);
}

.ai-suggestion-chips {
  display: flex;
  flex-wrap: wrap;
  gap: 8px;
}

.ai-suggestion-chip {
  display: inline-flex;
  align-items: center;
  gap: 4px;
  padding: 5px 11px;
  border-radius: 14px;
  font-size: 12px;
  background: #f3eefe;
  color: #6d4fc9;
  cursor: pointer;
  user-select: none;
  transition: transform 0.2s cubic-bezier(0.34, 1.56, 0.64, 1), box-shadow 0.2s, background 0.2s;
}

.ai-suggestion-chip:hover {
  transform: translateY(-2px);
  background: #e9dcfe;
  box-shadow: 0 4px 10px rgba(167, 139, 250, 0.28);
}

.ai-suggestion-chip:nth-child(2) {
  background: #fff0f5;
  color: #c75b8e;
}

.ai-suggestion-chip:nth-child(2):hover {
  background: #ffe1ec;
  box-shadow: 0 4px 10px rgba(199, 91, 142, 0.22);
}

.ai-suggestion-chip:nth-child(3) {
  background: #fff6e8;
  color: #c98a3d;
}

.ai-suggestion-chip:nth-child(3):hover {
  background: #ffedd3;
  box-shadow: 0 4px 10px rgba(201, 138, 61, 0.22);
}

.ai-suggestion-chip:nth-child(4) {
  background: #e9f9f1;
  color: #3fa97c;
}

.ai-suggestion-chip:nth-child(4):hover {
  background: #d9f5e7;
  box-shadow: 0 4px 10px rgba(63, 169, 124, 0.22);
}

.ai-message-row {
  display: flex;
  justify-content: flex-start;
  gap: 8px;
  margin-bottom: 12px;
}

.ai-message-row.mine {
  justify-content: flex-end;
}

.ai-msg-avatar {
  flex-shrink: 0;
  width: 28px;
  height: 28px;
  border-radius: 50%;
  background: linear-gradient(135deg, #f3e8ff, #e9d9ff);
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 14px;
  box-shadow: 0 2px 6px rgba(167, 139, 250, 0.2);
}

.ai-message-bubble {
  max-width: 78%;
  padding: 9px 12px;
  font-size: 14px;
  background: #fff;
  border-radius: 14px 14px 14px 4px;
  box-shadow: 0 2px 8px rgba(104, 73, 147, 0.08);
}

.ai-message-row.mine .ai-message-bubble {
  border-radius: 14px 14px 4px 14px;
  background: linear-gradient(135deg, #7d5fff 0%, #a84fcb 100%);
  color: #fff;
  box-shadow: 0 2px 8px rgba(125, 95, 255, 0.25);
}

.ai-message-content {
  white-space: pre-wrap;
  word-break: break-word;
  line-height: 1.6;
}

.ai-thinking {
  display: inline-flex;
  gap: 4px;
  padding: 3px 0;
}

.ai-dot {
  width: 6px;
  height: 6px;
  border-radius: 50%;
  background: #a78bfa;
  animation: dot-bounce 1.1s infinite;
}

.ai-dot:nth-child(2) {
  animation-delay: 0.15s;
}

.ai-dot:nth-child(3) {
  animation-delay: 0.3s;
}

.ai-cursor {
  display: inline-block;
  margin-left: 2px;
  animation: blink 0.8s infinite;
  color: #a78bfa;
}

/* ============ 输入区 ============ */
.ai-input {
  flex-shrink: 0;
  display: flex;
  gap: 10px;
  align-items: flex-end;
  padding: 12px;
  border-top: 1px solid #f0eafc;
  background: #fff;
}

.ai-input :deep(.arco-textarea) {
  flex: 1;
  background: #f5f3ff;
  border-color: #ece7fb;
  border-radius: 16px;
}

.ai-input :deep(.arco-textarea:focus-within) {
  background: #fff;
  border-color: #a78bfa;
}

.ai-send-btn {
  flex-shrink: 0;
  background: linear-gradient(135deg, #7d5fff 0%, #a84fcb 100%);
  border: none;
}

.ai-send-btn:hover {
  background: linear-gradient(135deg, #8b6bff 0%, #b45ce0 100%);
  border: none;
}

.ai-send-btn.arco-btn-disabled {
  background: linear-gradient(135deg, #c9c2e8, #d3c3ea);
  border: none;
}

/* ============ 动画 ============ */
@keyframes bob {
  0%,
  100% {
    transform: translateY(0);
  }

  50% {
    transform: translateY(-4px);
  }
}

@keyframes twinkle {
  0%,
  100% {
    opacity: 0;
    transform: scale(0.6) rotate(0deg);
  }

  50% {
    opacity: 1;
    transform: scale(1) rotate(20deg);
  }
}

@keyframes wave {
  0%,
  100% {
    transform: rotate(0);
  }

  20% {
    transform: rotate(-14deg);
  }

  40% {
    transform: rotate(12deg);
  }

  60% {
    transform: rotate(-8deg);
  }

  80% {
    transform: rotate(5deg);
  }
}

@keyframes dot-bounce {
  0%,
  60%,
  100% {
    transform: translateY(0);
    opacity: 0.45;
  }

  30% {
    transform: translateY(-5px);
    opacity: 1;
  }
}

@keyframes blink {
  0%,
  100% {
    opacity: 1;
  }

  50% {
    opacity: 0;
  }
}
</style>
