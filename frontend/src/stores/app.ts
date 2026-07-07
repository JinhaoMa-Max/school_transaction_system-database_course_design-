// 导入Pinia的状态管理定义函数
import { defineStore } from 'pinia'
// 导入Vue的响应式和计算属性API
import { ref, computed } from 'vue'
// 导入分类和通知的类型定义
import type { Category, Notice } from '@/types'
// 导入分类和通知相关的API接口
import { getCategoryList, getNoticeList } from '@/api'

/**
 * 应用全局状态管理Store
 * 管理分类、通知、加载状态、侧边栏状态等全局应用状态
 */
export const useAppStore = defineStore('app', () => {
  // 分类列表
  const categories = ref<Category[]>([])
  // 通知列表
  const notices = ref<Notice[]>([])
  // 全局加载状态
  const loading = ref(false)
  // 侧边栏折叠状态
  const sidebarCollapsed = ref(false)

  /**
   * 计算属性：将分类列表转换为树形结构
   * 通过parentId关联父子分类，构建多级分类树
   */
  const categoryTree = computed(() => {
    // 存储最终的树形结构
    const tree: Category[] = []
    // 使用Map存储分类ID与分类对象的映射，便于快速查找父分类
    const map = new Map<number, Category>()

    // 遍历分类列表，构建映射关系，初始化children数组
    categories.value.forEach(cat => {
      map.set(cat.categoryId, { ...cat, children: [] as Category[] })
    })

    // 再次遍历分类列表，根据parentId将子分类挂载到父分类下
    categories.value.forEach(cat => {
      if (cat.parentId && map.has(cat.parentId)) {
        // 如果存在父分类ID，将当前分类添加到父分类的children中
        (map.get(cat.parentId) as Category).children?.push(map.get(cat.categoryId) as Category)
      } else {
        // 如果没有父分类ID，作为顶级分类添加到树中
        tree.push(map.get(cat.categoryId) as Category)
      }
    })

    // 返回构建完成的分类树
    return tree
  })

  /**
   * 计算属性：获取最近的5条通知
   * 按发布时间倒序排列，取前5条
   */
  const recentNotices = computed(() => {
    // 复制通知数组（避免修改原数组），按发布时间倒序排序
    return [...notices.value].sort((a, b) => 
      new Date(b.publishTime).getTime() - new Date(a.publishTime).getTime()
    ).slice(0, 5) // 取前5条
  })

  /**
   * 加载分类列表
   */
  const loadCategories = async () => {
    // 调用获取分类列表API
    const res = await getCategoryList()
    // 更新分类列表
    categories.value = res.data
  }

  /**
   * 加载通知列表
   * 默认获取10条通知
   */
  const loadNotices = async () => {
    // 调用获取通知列表API，请求10条数据
    const res = await getNoticeList({ size: 10 })
    // 更新通知列表
    notices.value = res.data.list
  }

  /**
   * 切换侧边栏折叠状态
   */
  const toggleSidebar = () => {
    sidebarCollapsed.value = !sidebarCollapsed.value
  }

  /**
   * 设置全局加载状态
   * @param status 加载状态（true为加载中，false为加载完成）
   */
  const setLoading = (status: boolean) => {
    loading.value = status
  }

  // 返回store的状态和方法
  return {
    categories,
    notices,
    loading,
    sidebarCollapsed,
    categoryTree,
    recentNotices,
    loadCategories,
    loadNotices,
    toggleSidebar,
    setLoading
  }
})