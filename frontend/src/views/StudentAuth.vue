<script setup lang="ts">
import { ref,reactive, onMounted, watch } from 'vue'
import { useUserStore } from '@/stores'
import { submitStudentAuth, getStudentAuth } from '@/api'
import type { StudentAuth } from '@/types'
import { Message } from '@arco-design/web-vue'
import router from '@/router'

const userStore = useUserStore()
const formRef = ref()
const auth = ref<StudentAuth | null>(null)
const loading = ref(false)

  //用表单表示各个值
const form = reactive({
  studentId: '',
  realName: '',
  college: ''
})

//输入规则
const rules = {
  studentId: [
    { required: true, message: '请输入学号' },
    { match: /^\d{6,20}$/, message: '学号应为6-20位数字' }
  ],
  realName: [
    { required: true, message: '请输入真实姓名' }
  ],
  college: [
    { required: true, message: '请输入所在的学院' }
  ]
}

//监听一下原字段
watch(
  auth, 
  (val)=>{
    if(!val) return
    Object.assign(form,{
      studentId:val.studentId ?? '',
      realName: val.realName ?? '',
      college: val.college ?? ''
    })
  },
  { immediate: true }
)

onMounted(async () => {
  if (userStore.user) {
    try {
      const res = await getStudentAuth(userStore.user.userId)
      auth.value = res.data
    } catch {
      auth.value = null
    }
  }
})

//处理提交
const handleSubmit = async () => {
  const error = await formRef.value?.validate()
  if(error) { 
    return
  } 
  if (!userStore.user) return
  try{
     loading.value = true
     const res = await submitStudentAuth({ 
      userId: userStore.user.userId,
      studentId:form.studentId.trim(),
      realName:form.realName.trim(),
      college:form.college
     })
    auth.value = res.data
    Message.success('已提交认证')
  }catch(error){
      Message.error('认证提交失败，请稍后重试！')
      console.error('认证提交失败！',error)
  }finally{
    loading.value = false
  }
  
}

//获取审核状态
const getStatusText = (status: string) => {
  const map: Record<string, string> = {
    pending: '待审核',
    approved: '已通过',
    rejected: '已驳回'
  }
  return map[status] || status
}

//跳转到资料展示页面
const goToProfile = () => {
  router.push('/profile')
}

</script>

<template>
  <div class="student-auth-page">

    <a-card class = "student-auth-card">

      <!-- 已提交显示的页面 -->
      <template v-if = "auth && auth.authStatus !== 'rejected'">
      <div class ="auth-result">
        <a-result
          :status="auth.authStatus === 'approved' ? 'success' : 'warning'"
          :title="getStatusText(auth.authStatus)"
          :subtitle="
            auth.authStatus === 'approved' ? '您的学生身份认证已通过' : '您的学生身份认证已提交，请等待管理员审核'"
         />

         <a-descriptions>

         <a-descriptions-item label="学号">
          {{ auth.studentId }}
         </a-descriptions-item>

         <a-descriptions-item label="真实姓名">
          {{ auth.realName }}
         </a-descriptions-item>

         <a-descriptions-item label="学院">
          {{ auth.college }}
         </a-descriptions-item>

         <a-descriptions-item label="认证时间">
          {{ auth.authTime }}
         </a-descriptions-item>

         </a-descriptions>

         <div class="auth-actions">

          <!-- 返回按钮 -->
           <a-button class = "back-button" @click="goToProfile">
            返回
          </a-button>
          
         </div>

      </div>
    </template>

    <!--未提交成功显示的页面-->
    <template v-else>

      <a-alert
        v-if="auth?.authStatus === 'rejected'"
        type="error"
        show-icon
        title="认证已被驳回，请修改后重新提交"
        style="margin-bottom: 20px"
      />

      <a-form ref="formRef"
          :model="form" 
          :rules="rules"
          layout="vertical">

          <!--学号展示（不允许编辑）-->
          <a-form-item label="学号">
            <a-input v-model="form.studentId" disabled/>
          </a-form-item>

          <!-- 真实姓名 -->
          <a-form-item  field="realName"   label="真实姓名">
          <a-input v-model="form.realName" 
        placeholder="请输入您的真实姓名"
        allow-clear 
        />
         </a-form-item>

         <!-- 用户名 -->
          <a-form-item  field="college"   label="学院">
          <a-input v-model="form.college" 
        placeholder="请输入您所属的学院"
        allow-clear 
        />
         </a-form-item>
          
        
      <!-- 操作按钮 (满足条件才显示)-->
      <a-space class = "profile-actions">

        <!--左边按钮-->
        <a-space class = "profile-actions-left">

         <!-- 返回按钮 -->
           <a-button class = "back-button" @click="goToProfile">
            返回
          </a-button>

        </a-space> 

         <!--右边按钮-->
        <a-space class = "profile-actions-right">

        <!-- 认证按钮 -->
           <a-button  type="primary"  class = "ensure-button"  :loading="loading"  @click="handleSubmit">
            认证身份
          </a-button>

        </a-space>

      </a-space>

      </a-form>

      </template>

    </a-card>
   
  </div>
</template>

<style scoped>

.student-auth-page {
  min-height: calc(100vh - 64px);
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

.student-auth-card {
  max-width: 960px;
  margin: 0 auto;
  border-radius: 20px;
  background: var(--color-bg-2);
  box-shadow: 0 8px 24px rgba(45, 54, 142, 0.06);
  animation: fadeUp 0.6s ease both;
}

.profile-actions {
  display: flex;
  align-items: center;
  justify-content: space-between;
  margin-top: 24px;
}

.profile-actions-left ,

.profile-actions-right {
  display: flex;
  align-items: center;
}

.back-button{
  background-color: #24bac2;
  border-radius: 10px;
  color: white;
  transition: all 0.25s ease;
}

.back-button:hover {
  transform: translateY(-4px);
}

.ensure-button{
  border-radius: 10px;
  transition: all 0.25s ease;
}

.ensure-button:hover {
  transform: translateY(-4px);
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
