<script setup lang="ts">
import { ref, reactive } from 'vue'
import { useRouter } from 'vue-router'
import { useUserStore } from '@/stores'
import{Message} from '@arco-design/web-vue'
import { useRoute } from 'vue-router'

const router = useRouter()
const userStore = useUserStore()
const route = useRoute()

//用form对象表示username和password的值
const form = reactive({
  username: '',
  password: ''
})

//组件：保持状态
const rememberMe = ref(true)

//校验规则
const  formRef = ref()

const rules = {
  username: [
      { required: true, message: '请输入用户名(3-20个字符)' },
      { minLength: 3, message: '用户名长度不能小于3位' },
      { maxLength: 20, message: '用户名长度不能大于20位' }
  ],
  password: [
    { required: true, message: '请输入密码(6-20个字符)' },
    { minLength: 6, message: '密码长度不能小于6位' },
    { maxLength: 20, message: '密码长度不能大于20位' }
  ]
}

const loading = ref(false)

//登录逻辑
const handleLogin = async () => {
  const error = await formRef.value?.validate()
  if(error) {
    return
  }
  loading.value = true
  try {
   await userStore.login(form.username, form.password, rememberMe.value)
    Message.success('登录成功')
    const redirect = route.query.redirect as string
    router.push(redirect || '/')
  } catch (error: any) {
  const msg = error?.response?.data?.message || error?.message || ''
  if (msg.includes('密码') || msg.includes('用户名') || msg.includes('账号')) {
    Message.error('用户名或密码错误')
  } else if (msg.includes('网络') || msg.includes('timeout')) {
    Message.error('网络连接失败，请稍后重试')
  } else {
    Message.error('登录失败，请稍后重试')
    console.error('登录失败:', error)
  }
}
   finally {
    loading.value = false
  }
} 

const goToRegister = () => {
  router.push('/register')
}
</script>

<template>
  <div class="login-page">
    <div class="login-card">

      <!-- 登录表单 -->
      <h2>登录</h2>
      <p class="subtitle">欢迎登录校园二手交易平台！</p>

       <!-- a-form 格式的表单 -->
      <a-form ref="formRef"
           :model="form" 
           :rules="rules"
            layout="vertical"
        >
         <!-- 用户名 -->
          <a-form-item  field="username"   label="用户名">
          <a-input v-model="form.username" 
        placeholder="请输入用户名"
        allow-clear 
        />
         </a-form-item>

            <!-- 密码 -->
           <a-form-item field="password" label="密码">
            <a-input-password v-model="form.password" 
        placeholder="请输入密码"
        allow-clear
        @keyup.enter="handleLogin"
         />
           </a-form-item>

           <a-form-item>
              <a-checkbox v-model="rememberMe">记住我</a-checkbox>
           </a-form-item>


           <!-- 登录按钮 -->
           <a-button  type="primary" long :loading="loading" @click="handleLogin">
            登录
          </a-button>

      </a-form>

      <p class="link" @click="goToRegister">没有账号？去注册</p>
    </div>
  </div>
</template>

<style scoped>
.login-page {
  display: flex;
  justify-content: center;
  align-items: center;
  min-height: 100vh;
  background: #f5f5f5;
}

.login-card {
  width: 400px;
  padding: 40px;
  background: white;
  border-radius: 8px;
  box-shadow: 0 2px 12px rgba(0, 0, 0, 0.1);
}

.login-card h2 {
  text-align: center;
  margin-bottom: 30px;
}

.subtitle {
  text-align: center;
  margin-bottom: 28px;
  color: #86909c;
  font-size: 14px;
}

.link {
  text-align: center;
  margin-top: 16px;
  color: #165dff;
  cursor: pointer;
}
</style>
