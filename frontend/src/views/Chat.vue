<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { getSessionList, getMessages, sendMessage } from '@/api'
import type { ChatSession, ChatMessage } from '@/types'

const sessions = ref<ChatSession[]>([])
const messages = ref<ChatMessage[]>([])
const selectedSession = ref<ChatSession | null>(null)
const messageContent = ref('')

onMounted(async () => {
  const res = await getSessionList()
  sessions.value = res.data
})

const selectSession = async (session: ChatSession) => {
  selectedSession.value = session
  const res = await getMessages(session.sessionId)
  messages.value = res.data.list
}

const handleSend = async () => {
  if (!selectedSession.value || !messageContent.value) return
  await sendMessage({ sessionId: selectedSession.value.sessionId, content: messageContent.value })
  messageContent.value = ''
}
</script>

<template>
  <div class="chat-page">
    <div class="chat-sidebar">
      <h2>聊天列表</h2>
      <div 
        v-for="item in sessions" 
        :key="item.sessionId" 
        class="session-item"
        :class="{ active: selectedSession?.sessionId === item.sessionId }"
        @click="selectSession(item)"
      >
        <p>会话ID: {{ item.sessionId }}</p>
        <p>商品ID: {{ item.goodsId }}</p>
      </div>
    </div>
    <div class="chat-content">
      <div v-if="selectedSession" class="chat-messages">
        <div 
          v-for="msg in messages" 
          :key="msg.messageId" 
          class="message-item"
        >
          <p class="sender">发送者ID: {{ msg.senderId }}</p>
          <p class="content">{{ msg.content }}</p>
          <p class="time">{{ msg.sendTime }}</p>
        </div>
      </div>
      <div v-else class="empty">
        <p>请选择一个会话</p>
      </div>
      <div v-if="selectedSession" class="chat-input">
        <input v-model="messageContent" type="text" placeholder="输入消息内容" />
        <button @click="handleSend">发送</button>
      </div>
    </div>
  </div>
</template>

<style scoped>
.chat-page {
  display: flex;
  height: calc(100vh - 40px);
  gap: 20px;
  padding: 20px;
}

.chat-sidebar {
  width: 300px;
  background: white;
  border-radius: 8px;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.08);
  padding: 16px;
}

.chat-sidebar h2 {
  margin: 0 0 16px 0;
  font-size: 18px;
}

.session-item {
  padding: 12px;
  margin-bottom: 8px;
  border-radius: 4px;
  cursor: pointer;
}

.session-item:hover {
  background: #f5f5f5;
}

.session-item.active {
  background: #e6f7ff;
}

.session-item p {
  margin: 0 0 4px 0;
  font-size: 14px;
}

.chat-content {
  flex: 1;
  display: flex;
  flex-direction: column;
  background: white;
  border-radius: 8px;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.08);
  padding: 16px;
}

.chat-messages {
  flex: 1;
  overflow-y: auto;
}

.message-item {
  margin-bottom: 16px;
}

.message-item .sender {
  font-size: 12px;
  color: #999;
  margin: 0 0 4px 0;
}

.message-item .content {
  font-size: 16px;
  margin: 0 0 4px 0;
}

.message-item .time {
  font-size: 12px;
  color: #999;
  margin: 0;
}

.empty {
  flex: 1;
  display: flex;
  justify-content: center;
  align-items: center;
  color: #999;
}

.chat-input {
  display: flex;
  gap: 12px;
  margin-top: 16px;
  padding-top: 16px;
  border-top: 1px solid #e5e5e5;
}

.chat-input input {
  flex: 1;
  padding: 12px;
  border: 1px solid #e5e5e5;
  border-radius: 4px;
}

.chat-input button {
  padding: 12px 24px;
  background: #165dff;
  color: white;
  border: none;
  border-radius: 4px;
  cursor: pointer;
}
</style>
