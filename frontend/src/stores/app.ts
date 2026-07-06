import { defineStore } from 'pinia'
import { ref, computed } from 'vue'
import type { Category, Notice } from '@/types'
import { getCategoryList, getNoticeList } from '@/api'

export const useAppStore = defineStore('app', () => {
  const categories = ref<Category[]>([])
  const notices = ref<Notice[]>([])
  const loading = ref(false)
  const sidebarCollapsed = ref(false)

  const categoryTree = computed(() => {
    const tree: Category[] = []
    const map = new Map<number, Category>()

    categories.value.forEach(cat => {
      map.set(cat.categoryId, { ...cat, children: [] as Category[] })
    })

    categories.value.forEach(cat => {
      if (cat.parentId && map.has(cat.parentId)) {
        (map.get(cat.parentId) as Category).children?.push(map.get(cat.categoryId) as Category)
      } else {
        tree.push(map.get(cat.categoryId) as Category)
      }
    })

    return tree
  })

  const recentNotices = computed(() => {
    return [...notices.value].sort((a, b) => 
      new Date(b.publishTime).getTime() - new Date(a.publishTime).getTime()
    ).slice(0, 5)
  })

  const loadCategories = async () => {
    const res = await getCategoryList()
    categories.value = res.data
  }

  const loadNotices = async () => {
    const res = await getNoticeList({ size: 10 })
    notices.value = res.data.list
  }

  const toggleSidebar = () => {
    sidebarCollapsed.value = !sidebarCollapsed.value
  }

  const setLoading = (status: boolean) => {
    loading.value = status
  }

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
