<script setup lang="ts">
import { computed, reactive, ref, watch } from 'vue'
import { useRouter } from 'vue-router'
import { Message } from '@arco-design/web-vue'
import { useUserStore } from '@/stores'
import { updateUser } from '@/api/user'
import type { UpdateUserParams } from '@/types'


const router = useRouter()
const userStore = useUserStore()
const formRef = ref()
const user = computed(() => userStore.user)


//编辑状态字段类型
type EditableField = 'nickname' | 'email' | 'phone' 


const editingField = ref<EditableField|null>(null)

const savingField = ref<EditableField|null>(null)

//用form对象表示各个对象值
const form = reactive({
  username:'',
  nickname: '',
  role:'',
  status:'',
  email: '',
  phone: '',
  creditScore: '',
  registerTime:'',
  avatarUrl: ''
})

//监听一下原字段
watch(
  user, 
  (val)=>{
    if(!val) return
    Object.assign(form,{
      username:val.username ?? '',
      nickname: val.nickname ?? '',
      role:val.role ?? '',
      status:val.status ?? '',
      email: val.email ?? '',
      phone: val.phone ?? '',
      avatarUrl: val.avatarUrl ?? '',
      creditScore:val.creditScore ?? '',
      registerTime:val.registerTime ?? ''
    })
  },
  { immediate: true }
)

//编辑前的字段备份
const editBackup=reactive<Record<EditableField, string>>({
  nickname: '',
  email: '',
  phone: '',
})

//获取编辑字段的值
  const getUpdateParams = (field: EditableField): UpdateUserParams => {
  switch (field) {
    case 'nickname':
      return { nickname: form.nickname.trim() }

    case 'email':
      return { email: form.email.trim() }

    case 'phone':
      return { phone: form.phone.trim() }
  }
}


  const startEdit = (field: EditableField) => {
    if(editingField.value && editingField.value !== field) {
      Message.warning('请先保存或取消当前正在编辑的内容')
      return
    }
    editBackup[field] = form[field]
    editingField.value = field
  }


//保存编辑字段
const saveField = async (field: EditableField) => {

  if(!user.value?.userId) {
    Message.error('用户信息不存在')
    return
  }

  const errors = await formRef.value?.validateField(field)
    if (errors) return

  try {
      savingField.value = field

      const params = getUpdateParams(field)
      const res = await updateUser(user.value.userId, params)

      userStore.setUser(res.data)

      Message.success('保存成功')
      editingField.value = null
  } catch (error) {
      Message.error('保存失败,请稍后重试')
      console.error('保存失败:', error)
    } finally {
      savingField.value = null
    }

}

//取消编辑字段
const cancelEdit = (field: EditableField) => {
  form[field] = editBackup[field]
    editingField.value = null
}

const rules = {
  nickname: [
    { required: true, message: '请输入昵称(少于20个字符)' },
    { maxLength: 20, message: '昵称长度不能大于20位' }
  ],
  email: [
    { required: true, message: '请输入邮箱' },
    { match: /^[^\s@]+@[^\s@]+\.[^\s@]+$/, message: '请输入有效的邮箱地址' }
  ],
  phone: [
    { required: true, message: '请输入手机号' },
    { match: /^\d{10,15}$/, message: '手机号应为10-15位数字' }
  ]
}

//获取角色身份
const getRoleText = (role?: string) => {
  const map: Record<string, string> = {
    user: '普通用户',
    admin: '管理员'
  }
  return role?map[role] || role:'未知角色'
}

//获取用户状态
const getStatusText = (status?: string) => {
  const map: Record<string, string> = {
    normal: '正常',
    banned: '封禁'
  }
  return status?map[status] || status:'未知状态'
}

//读取图片文件
const avatarFile = ref<File | null>(null)

  //保存图像
const savingAvatar = ref(false)

  //处理文字头像
const avatarText = computed(() => {
  const name =
    form.nickname ||
    userStore.user?.nickname ||
    userStore.user?.username ||
    '用户'

  return name.slice(0, 1)
})

  //处理头像上传
const handleAvatarChange = (_: any, currentFile: any) => {
  const file = currentFile?.file
  if (!file) return

  avatarFile.value = file
  form.avatarUrl = URL.createObjectURL(file)

  Message.info('头像已预览')
}

//保存头像
const saveAvatar = async () => {
  if (!avatarFile.value) {
    Message.warning('请先选择头像')
    return
  }

  Message.info('当前仅支持头像预览，暂未接入上传接口')
}



//跳转到资料展示页面
const goToProfile = () => {
  router.push('/profile')
}

</script>

<template>

  <div class="edit-profile-page">

     <a-card class="edit-profile-card" :bordered="false">

     <a-form ref="formRef"
          :model="form" 
          :rules="rules"
          layout="vertical"   
      >

      <!--头像的上传与保存-->
      <a-form-item label="头像">
        <div class="avatar-edit">
          <a-avatar :size="88" class="edit-avatar">
            <img
              v-if="form.avatarUrl"
              :src="form.avatarUrl"
              alt="用户头像"
            />
            <span v-else>
              {{ avatarText }}
            </span>
          </a-avatar>

          <a-space>
            <a-upload
              :show-file-list="false"
              :auto-upload="false"
              accept="image/*"
              @change="handleAvatarChange"
            >
              <a-button>选择头像</a-button>
            </a-upload>

            <a-button 
            type="primary" :disabled="!avatarFile" :loading="savingAvatar" @click="saveAvatar">
              保存头像
            </a-button>
           </a-space>

        </div>
      </a-form-item>


      <!--用户名展示（不允许编辑）-->
      <a-form-item label="用户名">
        <a-input v-model="form.username" disabled/>
      </a-form-item>

      <!-- 昵称编辑 -->
      <a-form-item label="昵称" field="nickname">
          <a-input
            v-model="form.nickname"
            placeholder="请输入昵称"
            :readonly="editingField!== 'nickname'"
            :class ="{ 'readonly-input': editingField !== 'nickname' }"
          >
            <template #append>
              <a-button
                v-if="editingField !== 'nickname'"
                type="text"
                @click="startEdit('nickname')"
              >
                编辑
              </a-button>

              <a-space v-else>

                <a-button 
                type="text" 
                :loading="savingField === 'nickname'"
                @click="saveField('nickname')"
                >
                  保存
                </a-button>

                <a-button 
                type="text" 
                @click="cancelEdit('nickname')"
                >
                  取消
                </a-button>

              </a-space>
            </template>
          </a-input>
        </a-form-item>

        <!--身份展示（不允许编辑）-->
      <a-form-item label="身份">
        <a-input :model-value="getRoleText(form.role)" disabled/>
      </a-form-item>

        <!--状态展示（不允许编辑）-->
      <a-form-item label="状态">
        <a-input :model-value="getStatusText(form.status)" disabled />
      </a-form-item>

      <a-form-item label="信用分">
        <a-input :model-value="String(form.creditScore)" disabled />
      </a-form-item>


       <!-- 手机号编辑 -->
      <a-form-item label="手机号" field="phone">
          <a-input
            v-model="form.phone"
            placeholder="请输入手机号"
            :readonly="editingField!== 'phone'"
            :class ="{ 'readonly-input': editingField !== 'phone' }"
          >
            <template #append>
              <a-button
                v-if="editingField !== 'phone'"
                type="text"
                @click="startEdit('phone')"
              >
                编辑
              </a-button>

              <a-space v-else>
                <a-button 
                type="text" 
                :loading="savingField === 'phone'"
                @click="saveField('phone')"
                >
                  保存
                </a-button>
                <a-button 
                type="text" 
                @click="cancelEdit('phone')"
                >
                  取消
                </a-button>
              </a-space>
            </template>
          </a-input>
        </a-form-item>

        <!-- 邮箱编辑 -->
      <a-form-item label="邮箱" field="email">
          <a-input
            v-model="form.email"
            placeholder="请输入邮箱"
            :readonly="editingField!== 'email'"
            :class ="{ 'readonly-input': editingField !== 'email' }"
          >
            <template #append>
              <a-button
                v-if="editingField !== 'email'"
                type="text"
                @click="startEdit('email')"
              >
                编辑
              </a-button>

              <a-space v-else>
                <a-button 
                type="text" 
                :loading="savingField === 'email'"
                @click="saveField('email')"
                >
                  保存
                </a-button>
                <a-button 
                type="text" 
                @click="cancelEdit('email')"
                >
                  取消
                </a-button>
              </a-space>
            </template>
          </a-input>
        </a-form-item>

       <a-form-item label="注册时间">
          <a-input v-model="form.registerTime" disabled />
        </a-form-item>




    <a-button type = "primary" @click="goToProfile">
      返回个人资料
    </a-button>

     </a-form>

     </a-card>

  </div>

</template>

<!-- 编辑资料页面样式 -->
<style scoped>

.edit-profile-page {
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

.edit-profile-card {
  max-width: 960px;
  margin: 0 auto;
  border-radius: 20px;
  background: var(--color-bg-2);
  box-shadow: 0 8px 24px rgba(45, 54, 142, 0.06);
}

.avatar-edit {
  display: flex;
  align-items: center;
  gap: 16px;
}

.edit-avatar {
  flex-shrink: 0;
  font-size: 28px;
  font-weight: 600;
  background-color: var(--color-fill-3);
  color: var(--color-text-2);
}

.readonly-input :deep(.arco-input-wrapper) {
  background-color: var(--color-fill-2);
}

.readonly-input :deep(.arco-input) {
  color: var(--color-text-2);
  cursor: default;
}


</style>
