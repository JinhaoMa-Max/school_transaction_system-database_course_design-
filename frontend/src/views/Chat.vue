<script setup lang="ts">
import { ref, onMounted,nextTick,computed } from 'vue'
import { getSessionList, getMessages, sendMessage,getUserById } from '@/api'
import type { ChatSession, ChatMessage } from '@/types'
import router from '@/router'
import{Message} from '@arco-design/web-vue'
import { useUserStore } from '@/stores/user'

const sessions = ref<ChatSession[]>([])
const messages = ref<ChatMessage[]>([])
const selectedSession = ref<ChatSession | null>(null)
const messageContent = ref('')
const sessionLoading = ref(false)
const messageLoading = ref(false)
const sending = ref(false)
const messageListRef = ref<HTMLElement| null>(null)
const userStore = useUserStore()
const userMap = ref<Record<number, any>>({})

//跳转回上一页
const goBack=()=>{
  router.back()
}

//跳转到主页
const goToHome = () => {
  router.push('/')
}

//获取当前用户ID
const currentUserId = computed(() =>{
  return userStore.user?.userId
})

//加载消息发送者信息
const loadMessageUsers = async (list: ChatMessage[]) => {
  const ids = Array.from(
    new Set(list.map(msg => Number(msg.senderId)).filter(Boolean))
  )

  const needLoadIds = ids.filter(id => !userMap.value[id])

  await Promise.all(
    needLoadIds.map(async id => {
      try {
        const res = await getUserById(id)

        userMap.value[id] = res.data ?? res
      } catch (error) {
        console.log(`用户 ${id} 信息加载失败`, error)
      }
    })
  )
}

const getUserAvatar = (userId: number) => {
  const user = userMap.value[Number(userId)]
  return user?.avatar || user?.avatarUrl ||''
}

const getUserName = (userId: number) => {
  const user = userMap.value[Number(userId)]
  return user?.nickname || user?.username || `用户${userId}`
}


//加载聊天列表
const loadsessions = async () =>{
  try{
    sessionLoading.value = true
    const res = await getSessionList()
    sessions.value = res.data
  }
  catch(error){
    Message.error('聊天列表加载失败')
  }finally{
    sessionLoading.value =false
  }
}

onMounted(async () => {
  loadsessions()
})

//加载消息
const selectSession = async (session: ChatSession) => {
  selectedSession.value = session
  try{
    messageLoading.value = true
    const res = await getMessages(session.sessionId)
    messages.value = res.data.list

     await loadMessageUsers(messages.value)
     await scrollToBottom()
  }
  catch(error){
    Message.error('消息加载失败')
  }finally{
    messageLoading.value =false
  }
}

//判断消息归属
const isMine = (msg:ChatMessage)=>{
  return String(msg.senderId)===String(currentUserId.value)
}

//处理发送
const handleSend = async () => {

  const content = messageContent.value.trim()
  if (!selectedSession.value) {
    Message.warning("请先选择一个对话！")
    return
  }
  else if(!content){
    Message.warning("请输入消息内容")
    return
  }
  try{
    sending.value = true
    await sendMessage({ sessionId: selectedSession.value.sessionId, content,senderId:currentUserId.value ?? undefined })
    messageContent.value = ''
    const res = await getMessages(selectedSession.value.sessionId)
    messages.value = res.data.list

    await loadMessageUsers(messages.value)
    await scrollToBottom()
    
  }catch(error){
    Message.error('消息发送失败！')
  }finally{
    sending.value = false
  }
}

//处理消息滚动
const scrollToBottom = async () =>{

  await nextTick()

  if(messageListRef.value){
    messageListRef.value.scrollTop=messageListRef.value.scrollHeight
  }

}

</script>

<template>
  <div class="chat-page">

    <!--顶部-->
    <a-card class = "chat-header" :bordered="false">

      <template #title>
        <div class="chat-title">
          <div class="chat-title-main">会话中心</div>
          <div class="chat-title-sub">选择会话并开始沟通</div>
        </div>
       </template>

      <a-button @click="goBack" class = "back-button">
             返回上一页
      </a-button>

      <a-button @click="goToHome" class = "home-button">
             返回首页
      </a-button>

    </a-card>

    <div class="chat-layout">
    <!--左侧列表-->
    <a-card class = "chat-sidebar" :bordered="false">

       <template #title>
        <div class ="chat-sidebar-title">
        聊天列表
        </div>
       </template>

       <a-spin :loading="sessionLoading">

        <a-list :bordered="false"  >

          <a-list-item
          v-for="item in sessions"
          :key = "item.sessionId"
          class="session-item"
          :class="{active:selectedSession?.sessionId==item.sessionId}"
          @click="selectSession(item)"
          >
            <div>
              对话
              {{ item.sessionId }}
            </div>
            <div class ="session-subtitle">
              商品 ID:
              "{{ item.goodsId }}"
            </div>
          </a-list-item>

        </a-list>

       </a-spin>
       
    </a-card>

    <!--右侧对话区-->
    <a-card class="chat-content" :bordered="false">

      <template #title>
         <div class ="chat-content-title">
          {{ selectedSession ? `会话${selectedSession.sessionId}`:'聊天窗口'}}
        </div>
      </template>

      <a-empty v-if="!selectedSession" description="请选择一个对话"/>

      <template v-else>

        <!--消息区-->
        <div class = "chat-messages" ref="messageListRef">

          <a-spin  class="message-spin" :loading="messageLoading">

            <a-empty v-if="messages.length === 0"
            description="暂无消息"
            />

            <div 
            v-for="msg in messages"
            :key ="msg.messageId"
            class = "message-row"
            :class = "{mine:isMine(msg)}"
            >

               <!-- 对方头像：左边 -->
              <a-avatar
                v-if="!isMine(msg)"
                :size="40"
                class="message-avatar"
              >
                <img
                  v-if="getUserAvatar(msg.senderId)"
                  :src="getUserAvatar(msg.senderId)"
                />
                <span v-else>
                  {{ getUserName(msg.senderId).slice(0, 1) }}
                </span>
              </a-avatar>

              <div class = "message-bubble">

                <div class = "message-meta">
                  {{ getUserName(msg.senderId) }} · {{ msg.sendTime }}
                </div>

                <div class = "message-content">
                  {{ msg.content }}
                </div>
                
              </div>

               <!-- 我的头像：右边 -->
              <a-avatar
                v-if="isMine(msg)"
                :size="40"
                class="message-avatar"
              >
                <img
                  v-if="getUserAvatar(msg.senderId)"
                  :src="getUserAvatar(msg.senderId)"
                />
                <span v-else>
                  {{ getUserName(msg.senderId).slice(0, 1) }}
                </span>
              </a-avatar>

            </div>

          </a-spin>

        </div>
        <!--输入区-->
        <div class="chat-input">
          <a-textarea
          v-model="messageContent"
          placeholder="请输入文本内容"
          :auto-size="{minRows:1,maxRows:4}"
          @keydown.enter.exact.prevent="handleSend"
          />

          <a-button
          type = "primary"
          :loading="sending"
          :disabled="!messageContent.trim()"
          @click="handleSend"
          >
          发送
        </a-button>

        </div>

      </template>

    </a-card>

    </div>

  </div>
</template>

<style scoped>


.chat-page {
  min-height: calc(100vh - 64px);
  padding: 20px;
 background: linear-gradient(180deg, #253554 0%, #ffffff 100%);
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
.chat-layout{
   display: flex;
  gap: 20px;
  max-width: 1180px;
  margin: 0 auto;
  height: calc(100vh - 124px);
}

.chat-header {
   width: 100%;
  max-width: 1180px;
  margin: 24px auto;
  border-radius: 20px;
  box-shadow: 0 8px 24px rgba(45, 54, 142, 0.06);
  animation: fadeUp 0.6s ease both;
}

.chat-title {
  display: flex;
  flex-direction: column;
  gap: 4px;
}

.chat-title-main {
  font-size: 20px;
  font-weight: 600;
  color: #1d2129;
  line-height: 28px;
}

.chat-title-sub {
  font-size: 13px;
  font-weight: 400;
  color: var(--color-text-3);
  line-height: 20px;
}


.home-button{
  background-color: #f8d86f;
  border-radius: 10px;
  color: white;
}
.back-button{
  background-color: #24bac2;
  border-radius: 10px;
  color: white;
}

.chat-sidebar {
  width: 300px;
  flex-shrink: 0;
  border-radius: 20px;
  animation: fadeUp 0.7s ease both;
  animation-delay: 0.1s;
}

.chat-sidebar-title {
  font-size: 20px;
  font-weight: 600;
  color: #1d2129;
  line-height: 28px;
}

.chat-content {
  flex: 1;
  min-width: 0;
  height: 100%;
  border-radius: 20px;
  display: flex;
  flex-direction: column;
  animation: fadeUp 0.7s ease both;
  animation-delay: 0.1s;
}

.chat-content-title {
  font-size: 20px;
  font-weight: 600;
  color: #1d2129;
  line-height: 28px;
}

.chat-content :deep(.arco-card-body) {
  flex: 1;
  min-height: 0;
  display: flex;
  flex-direction: column;
}

.message-spin,
.message-spin :deep(.arco-spin-children) {
  width: 100%;
}

.session-item {
  cursor: pointer;
  border-radius: 6px;
  transition: background 0.2s;
}

.session-item.active {
  background: var(--color-primary-light-1);
}

.session-subtitle {
  margin-top: 4px;
  font-size: 12px;
  color: var(--color-text-3);
}

.chat-messages {
  flex: 1;
  min-height: 0;
  overflow-y: auto;
  padding: 12px 4px;
}

.message-row {
  width: 100%;
  display: flex;
  justify-content: flex-start;
 gap: 8px;
  margin-bottom: 12px;
}

.message-row.mine {
  justify-content: flex-end;
}

.message-avatar {
  flex-shrink: 0;
}

.message-bubble {
  max-width: 60%;
  padding: 10px 12px;
  border-radius: 10px;
  background: var(--color-fill-2);
}

.message-content {
  white-space: pre-wrap;
  word-break: break-word;
  line-height: 1.6;
}

.message-row.mine .message-bubble {
  background: rgb(var(--primary-6));
  color: #fff;
}

.message-meta {
  margin-bottom: 4px;
  font-size: 12px;
  opacity: 0.75;
}

.chat-input {
  flex-shrink: 0;
  display: flex;
  gap: 12px;
  padding-top: 12px;
  border-top: 1px solid var(--color-border-2);
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
</style>
