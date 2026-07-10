<script setup lang="ts">
import { ref, reactive } from 'vue'
import { useRouter } from 'vue-router'
import{Message} from '@arco-design/web-vue'
import{register} from '@/api/auth'

const router = useRouter()
const formRef = ref()
const loading = ref(false)

//用form对象表示各个值
const form = reactive({
  studentId: '',
  username: '',
  password: '',
  confirmPassword: '',
  nickname: '',
  email: '',
  phone: ''
})


//校验规则
const rules = {

studentId: [
  { required: true, message: '请输入学号' },
  { match: /^\d{6,20}$/, message: '学号应为6-20位数字' }
],

  username: [
    { required: true, message: '请输入用户名(3-20个字符)' },
    { minLength: 3, message: '用户名长度不能小于3位' },
    { maxLength: 20, message: '用户名长度不能大于20位' },
    {
      match: /^(?!\d+$)[A-Za-z0-9_]{3,20}$/,
      message: '用户名可包含字母、数字和下划线，但不能为纯数字'
    }
  ],

  password: [
    { required: true, message: '请输入密码(6-20个字符)' },
    { 
       match: /^(?=.*\d)(?=.*[a-z])(?=.*[A-Z])(?=.*[!@#$%^&*])[A-Za-z0-9!@#$%^&*]{6,20}$/,
       message: '密码需包含数字、小写字母、大写字母和特殊字符，长度 6-20 位'
    }
  ],

confirmPassword: [
  { required: true, message: '请再次输入密码' },
  {
    validator: (value: string, callback: (msg?: string) => void) => {
      if (value !== form.password) {
        callback('两次输入的密码不一致')
      } else {
        callback()
      }
    }
  }
]
,

  nickname: [
     { maxLength: 20, message: '昵称不能超过20个字符' }
  ],

  email: [
     {message: '请输入正确的邮箱格式' }
  ],

  phone: [
    { required: true, message: '请输入手机号' },
    { match: /^1[3-9]\d{9}$/, message: '请输入正确的手机号' }
  ]

}

//注册逻辑
const handleRegister = async () => {
  const error = await formRef.value?.validate()
  if(error) { 
    return
  } 
  try {
        loading.value = true
        await register({
          studentId: form.studentId.trim(),
          username: form.username.trim(),
          password: form.password,
          nickname: form.nickname.trim(),
          email: form.email.trim(),
          phone: form.phone.trim()
        })  
      Message.success('注册成功！')
      router.push('/login')
 } catch (error: any) {
  const msg = error?.response?.data?.message || error?.message || ''
  if (msg.includes('学号') && msg.includes('已存在')) {
  Message.error('该学号已被绑定，请检查后重新填写')
}else if (msg.includes('用户名') && msg.includes('已存在')) {
    Message.error('该用户名已被注册，请更换')
  } else if (msg.includes('网络') || msg.includes('timeout')) {
    Message.error('网络连接失败，请稍后重试')
  } else {
    // 
    console.error('注册失败:', error)
  }
} finally {
    loading.value = false
  }
}

//跳转到登录页面
const goToLogin = () => {
  router.push('/login')
}
</script>

<template>
  <div class="register-page">
    <div class="register-card">

      <!-- 注册表单 -->
      <h2>注册</h2>
      <p class="subtitle">欢迎注册校园二手交易平台！</p>

      <a-form ref="formRef"
          :model="form" 
          :rules="rules"
          layout="vertical"   
        >

        <!-- 学号 -->
        <a-form-item field="studentId" label="学号">
          <a-input
            v-model="form.studentId"
            placeholder="请输入学号"
            allow-clear
          />
        </a-form-item>

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
              />
           </a-form-item>

           <!-- 确认密码 -->
          <a-form-item field="confirmPassword" label="确认密码">
            <a-input-password v-model="form.confirmPassword" 
          placeholder="请再次输入密码"
          allow-clear
             />
          </a-form-item>


            <!-- 昵称 -->
          <a-form-item  field="nickname"   label="昵称(选填)">
          <a-input v-model="form.nickname" 
        placeholder="请输入昵称"
        allow-clear 
        />
         </a-form-item>

          <!-- 手机号 -->
          <a-form-item  field="phone"   label="手机号">
          <a-input v-model="form.phone" 
        placeholder="请输入手机号"
        allow-clear 
        />
         </a-form-item>

           <!-- 邮箱号 -->
          <a-form-item  field="email"   label="邮箱（选填）">
          <a-input v-model="form.email" 
        placeholder="请输入邮箱"
        allow-clear 
        />
         </a-form-item>

           <!-- 注册按钮 -->
           <a-button  type="primary" long  :loading="loading"  @click="handleRegister">
            注册
          </a-button>

      </a-form>
      <p class="link" @click="goToLogin">已有账号？去登录</p>
    </div>
  </div>
</template>

<style scoped>
.register-page {
  display: flex;
  min-height: 100vh;
  padding: 20px;
  background: linear-gradient(180deg, #253554 0%, #ffffff 100%);
  justify-content: center;
  align-items: center;
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

.register-card {
  width: 400px;
  padding: 40px;
  background: white;
  border-radius: 8px;
  box-shadow: 0 2px 12px rgba(0, 0, 0, 0.1);
  animation: fadeUp 0.6s ease both;
}

.register-card h2 {
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

:deep(input::-ms-reveal),
:deep(input::-ms-clear) {
  display: none;
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
