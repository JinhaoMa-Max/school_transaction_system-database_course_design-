<script setup lang="ts">
import { ref, onMounted, onUnmounted, computed, nextTick } from 'vue'
import { useRoute } from 'vue-router'
import { Message } from '@arco-design/web-vue'
import { useUserStore } from '@/stores'
import { getSessionList, getMessages, sendMessage, markAsRead } from '@/api/chat'
import type { ChatSession, ChatMessage } from '@/types'

const route = useRoute()
const userStore = useUserStore()

const sessions = ref<ChatSession[]>([])
const messages = ref<ChatMessage[]>([])
const selectedSession = ref<ChatSession | null>(null)
const messageContent = ref('')
const messagesContainer = ref<HTMLElement | null>(null)
const sending = ref(false)

let pollTimer: ReturnType<typeof setInterval> | null = null

// 计算当前用户ID
const currentUserId = computed(() => userStore.user?.userId ?? 0)

// 获取对方的名称（优先使用 nickname，回退到 ID）
const getOtherPartyLabel = (session: ChatSession): string => {
  if (currentUserId.value === session.buyerId) {
    return session.sellerName || `卖家 #${session.sellerId}`
  }
  return session.buyerName || `买家 #${session.buyerId}`
}

// 获取消息发送者名称
const getSenderLabel = (msg: ChatMessage): string => {
  if (msg.senderName) return msg.senderName
  if (msg.senderId === currentUserId.value) return '我'
  return `用户 #${msg.senderId}`
}

// 判断消息是否是自己发的
const isMyMessage = (msg: ChatMessage): boolean => {
  return msg.senderId === currentUserId.value
}

const fetchSessions = async () => {
  try {
    const res = await getSessionList()
    sessions.value = res.data || []
  } catch {
    // ignore
  }
}

// 已知消息ID集合，用于轮询时去重
const knownMessageIds = ref<Set<number>>(new Set())

const fetchMessages = async () => {
  if (!selectedSession.value) return
  try {
    const res = await getMessages(selectedSession.value.sessionId)
    const list = res.data?.list || []
    // 记录滚动位置：如果用户在看历史消息，不强制滚到底部
    const container = messagesContainer.value
    const wasAtBottom = container
      ? container.scrollHeight - container.scrollTop - container.clientHeight < 60
      : true

    // 首次加载或完全替换
    messages.value = list
    knownMessageIds.value = new Set(list.map(m => m.messageId))

    if (wasAtBottom) {
      await scrollToBottom()
    }
  } catch {
    // ignore
  }
}

// 轮询时增量合并新消息（不替换已有消息，避免滚动跳动）
const pollMessages = async () => {
  if (!selectedSession.value) return
  try {
    const res = await getMessages(selectedSession.value.sessionId)
    const list = res.data?.list || []
    const container = messagesContainer.value
    const wasAtBottom = container
      ? container.scrollHeight - container.scrollTop - container.clientHeight < 60
      : true

    // 只追加未知的新消息
    let hasNew = false
    for (const msg of list) {
      if (!knownMessageIds.value.has(msg.messageId)) {
        messages.value.push(msg)
        knownMessageIds.value.add(msg.messageId)
        hasNew = true
      }
    }

    if (hasNew && wasAtBottom) {
      await scrollToBottom()
    }
  } catch {
    // ignore
  }
}

const selectSession = async (session: ChatSession) => {
  selectedSession.value = session
  await fetchMessages()
  // 标记已读
  try {
    await markAsRead(session.sessionId)
    // 更新本地未读数
    const found = sessions.value.find(s => s.sessionId === session.sessionId)
    if (found) found.unreadCount = 0
  } catch {
    // ignore
  }
}

const handleSend = async () => {
  if (!selectedSession.value || !messageContent.value.trim()) return
  sending.value = true
  try {
    await sendMessage({
      sessionId: selectedSession.value.sessionId,
      content: messageContent.value.trim()
    })
    messageContent.value = ''
    await fetchMessages()
  } catch {
    Message.error('发送失败')
  } finally {
    sending.value = false
  }
}

const scrollToBottom = async () => {
  await nextTick()
  if (messagesContainer.value) {
    messagesContainer.value.scrollTop = messagesContainer.value.scrollHeight
  }
}

// 处理回车发送
const handleKeydown = (e: KeyboardEvent) => {
  if (e.key === 'Enter' && !e.shiftKey) {
    e.preventDefault()
    handleSend()
  }
}

// 轮询新消息（增量更新，不丢失滚动位置）
const startPolling = () => {
  if (pollTimer) return
  pollTimer = setInterval(async () => {
    await fetchSessions()
    if (selectedSession.value) {
      await pollMessages()
    }
  }, 5000) // 每5秒轮询一次
}

const stopPolling = () => {
  if (pollTimer) {
    clearInterval(pollTimer)
    pollTimer = null
  }
}

onMounted(async () => {
  await fetchSessions()

  // 如果有query参数sessionId，自动选中对应会话
  const querySessionId = route.query.sessionId
  if (querySessionId) {
    const sid = Number(querySessionId)
    const target = sessions.value.find(s => s.sessionId === sid)
    if (target) {
      await selectSession(target)
    }
  }

  startPolling()
})

onUnmounted(() => {
  stopPolling()
})
</script>

<template>
  <div class="chat-page">
    <!-- 左侧会话列表 -->
    <div class="chat-sidebar">
      <div class="sidebar-header">
        <h2>消息</h2>
      </div>
      <div class="session-list">
        <div
          v-for="session in sessions"
          :key="session.sessionId"
          class="session-item"
          :class="{ active: selectedSession?.sessionId === session.sessionId }"
          @click="selectSession(session)"
        >
          <div class="session-avatar">
            <a-avatar :size="44">
              <template #icon>
                <icon-user />
              </template>
            </a-avatar>
          </div>
          <div class="session-info">
            <div class="session-top">
              <span class="session-name">
                {{ session.goodsTitle || '商品 #' + session.goodsId }}
              </span>
              <span class="session-time">{{ session.createTime?.slice(0, 10) }}</span>
            </div>
            <div class="session-bottom">
              <span class="session-party">{{ getOtherPartyLabel(session) }}</span>
              <a-badge
                v-if="session.unreadCount && session.unreadCount > 0"
                :count="session.unreadCount"
                :max-count="99"
                :dot="false"
              />
            </div>
          </div>
        </div>
        <div v-if="sessions.length === 0" class="no-sessions">
          <a-empty description="暂无聊天会话" />
        </div>
      </div>
    </div>

    <!-- 右侧聊天区域 -->
    <div class="chat-content">
      <!-- 未选择会话 -->
      <div v-if="!selectedSession" class="chat-empty">
        <a-empty description="选择一个会话开始聊天" />
      </div>

      <!-- 已选择会话 -->
      <template v-else>
        <!-- 聊天头部 -->
        <div class="chat-header">
          <div class="chat-header-info">
            <div class="chat-header-title">
              {{ selectedSession.goodsTitle || '商品 #' + selectedSession.goodsId }}
            </div>
            <div class="chat-header-sub">
              {{ getOtherPartyLabel(selectedSession) }}
            </div>
          </div>
        </div>

        <!-- 消息列表 -->
        <div ref="messagesContainer" class="chat-messages">
          <div
            v-for="msg in messages"
            :key="msg.messageId"
            class="message-wrapper"
            :class="{ 'message-mine': isMyMessage(msg) }"
          >
            <div class="message-bubble" :class="{ 'bubble-mine': isMyMessage(msg) }">
              <div class="message-sender">{{ getSenderLabel(msg) }}</div>
              <div class="message-text">{{ msg.content }}</div>
              <div class="message-meta">
                <span class="message-time">{{ msg.sendTime?.slice(0, 19).replace('T', ' ') }}</span>
                <span v-if="isMyMessage(msg)" class="message-status">
                  {{ msg.readStatus === 1 ? '已读' : '未读' }}
                </span>
              </div>
            </div>
          </div>
          <div v-if="messages.length === 0" class="no-messages">
            <a-empty description="暂无消息，发送第一条消息吧" />
          </div>
        </div>

        <!-- 输入区域 -->
        <div class="chat-input-area">
          <div class="chat-input-wrapper">
            <a-textarea
              v-model="messageContent"
              placeholder="输入消息内容，按 Enter 发送..."
              :auto-size="{ minRows: 1, maxRows: 4 }"
              :disabled="sending"
              @keydown="handleKeydown"
            />
            <a-button
              type="primary"
              :loading="sending"
              :disabled="!messageContent.trim()"
              @click="handleSend"
            >
              发送
            </a-button>
          </div>
        </div>
      </template>
    </div>
  </div>
</template>

<style scoped>
.chat-page {
  display: flex;
  height: calc(100vh - 64px);
  background: #f5f6f7;
}

/* 左侧会话列表 */
.chat-sidebar {
  width: 340px;
  min-width: 340px;
  background: white;
  border-right: 1px solid #e5e6eb;
  display: flex;
  flex-direction: column;
}

.sidebar-header {
  padding: 20px 20px 16px;
  border-bottom: 1px solid #f0f0f0;
}

.sidebar-header h2 {
  margin: 0;
  font-size: 20px;
  font-weight: 600;
  color: #1d2129;
}

.session-list {
  flex: 1;
  overflow-y: auto;
}

.session-item {
  display: flex;
  align-items: center;
  gap: 12px;
  padding: 14px 20px;
  cursor: pointer;
  transition: background 0.15s;
  border-bottom: 1px solid #f5f5f5;
}

.session-item:hover {
  background: #f7f8fa;
}

.session-item.active {
  background: #e8f3ff;
}

.session-avatar {
  flex-shrink: 0;
}

.session-info {
  flex: 1;
  min-width: 0;
}

.session-top {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 4px;
}

.session-name {
  font-size: 15px;
  font-weight: 500;
  color: #1d2129;
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
  max-width: 180px;
}

.session-time {
  font-size: 12px;
  color: #86909c;
  flex-shrink: 0;
}

.session-bottom {
  display: flex;
  justify-content: space-between;
  align-items: center;
}

.session-party {
  font-size: 13px;
  color: #86909c;
}

.no-sessions {
  padding: 40px 20px;
}

/* 右侧聊天区域 */
.chat-content {
  flex: 1;
  display: flex;
  flex-direction: column;
  background: white;
  margin: 12px;
  border-radius: 8px;
  overflow: hidden;
  box-shadow: 0 1px 4px rgba(0, 0, 0, 0.04);
}

.chat-empty {
  flex: 1;
  display: flex;
  justify-content: center;
  align-items: center;
}

/* 聊天头部 */
.chat-header {
  padding: 16px 24px;
  border-bottom: 1px solid #e5e6eb;
  background: #fafbfc;
}

.chat-header-title {
  font-size: 16px;
  font-weight: 600;
  color: #1d2129;
}

.chat-header-sub {
  font-size: 13px;
  color: #86909c;
  margin-top: 2px;
}

/* 消息列表 */
.chat-messages {
  flex: 1;
  overflow-y: auto;
  padding: 20px 24px;
  background: #fafbfc;
}

.message-wrapper {
  display: flex;
  margin-bottom: 16px;
}

.message-wrapper.message-mine {
  justify-content: flex-end;
}

.message-bubble {
  max-width: 65%;
  padding: 10px 14px;
  border-radius: 12px;
  background: white;
  box-shadow: 0 1px 2px rgba(0, 0, 0, 0.06);
}

.bubble-mine {
  background: #165dff;
  color: white;
}

.message-sender {
  font-size: 12px;
  color: #86909c;
  margin-bottom: 4px;
}

.bubble-mine .message-sender {
  color: rgba(255, 255, 255, 0.75);
}

.message-text {
  font-size: 15px;
  line-height: 1.5;
  word-break: break-word;
}

.message-meta {
  display: flex;
  justify-content: flex-end;
  gap: 8px;
  margin-top: 4px;
  font-size: 11px;
  color: #c9cdd4;
}

.bubble-mine .message-meta {
  color: rgba(255, 255, 255, 0.6);
}

.no-messages {
  padding: 40px;
  text-align: center;
}

/* 输入区域 */
.chat-input-area {
  padding: 12px 20px 20px;
  border-top: 1px solid #e5e6eb;
  background: white;
}

.chat-input-wrapper {
  display: flex;
  gap: 12px;
  align-items: flex-end;
}

.chat-input-wrapper :deep(.arco-textarea) {
  flex: 1;
}

.chat-input-wrapper :deep(.arco-textarea textarea) {
  border-radius: 8px;
  padding: 10px 14px;
  resize: none;
}

.chat-input-wrapper .arco-btn {
  flex-shrink: 0;
  height: 40px;
}
</style>
