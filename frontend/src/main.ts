import { createApp } from 'vue'
import ArcoVue from '@arco-design/web-vue'
import '@arco-design/web-vue/dist/arco.css'
import {
  IconEye,
  IconUser,
  IconMessage,
  IconStar,
  IconStarFill,
  IconExclamationCircle,
  IconLeft,
  IconClose,
  IconPlus
} from '@arco-design/web-vue/es/icon'
import { createPinia } from 'pinia'
import router from './router'
import App from './App.vue'

const app = createApp(App)
const pinia = createPinia()

const iconComponents = {
  'icon-eye': IconEye,
  'icon-user': IconUser,
  'icon-message': IconMessage,
  'icon-star': IconStar,
  'icon-star-fill': IconStarFill,
  'icon-exclamation-circle': IconExclamationCircle,
  'icon-left': IconLeft,
  'icon-close': IconClose,
  'icon-plus': IconPlus
}

for (const [name, component] of Object.entries(iconComponents)) {
  app.component(name, component)
}

app.use(ArcoVue)
app.use(pinia)
app.use(router)
app.mount('#app')
