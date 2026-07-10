<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { getGoodsList, getCategoryList } from '@/api'
import type { Goods, Category } from '@/types'

const router = useRouter()

const goodsList = ref<Goods[]>([])
const loading = ref(false)
const total = ref(0)
const page = ref(1)
const size = ref(12)

const keyword = ref('')
const selectedParentId = ref<number | undefined>(undefined)
const selectedChildId = ref<number | undefined>(undefined)
const minPrice = ref<number | undefined>(undefined)
const maxPrice = ref<number | undefined>(undefined)
const sortBy = ref('created_at_desc')

const categories = ref<Category[]>([])

// 一级分类（顶级分类）
const parentCategories = computed(() => {
  return categories.value
    .filter(c => c.parentId === null)
    .sort((a, b) => a.sortOrder - b.sortOrder)
})

// 当前选中一级分类下的二级分类
const childCategories = computed(() => {
  if (!selectedParentId.value) return []
  return categories.value
    .filter(c => c.parentId === selectedParentId.value)
    .sort((a, b) => a.sortOrder - b.sortOrder)
})

// 当前选中一级分类是否有子分类
const selectedParentHasChildren = computed(() => {
  return childCategories.value.length > 0
})

const conditionMap: Record<string, string> = {
  new: '全新',
  like_new: '几乎全新',
  slight_use: '轻微使用',
  obvious_trace: '明显痕迹'
}

const sortOptions = [
  { label: '最新发布', value: 'created_at_desc' },
  { label: '价格升序', value: 'price_asc' },
  { label: '价格降序', value: 'price_desc' },
  { label: '最多浏览', value: 'view_count_desc' }
]

const fetchCategories = async () => {
  try {
    const res = await getCategoryList()
    categories.value = res.data
  } catch {
    // 错误已由全局拦截器处理
  }
}

const fetchData = async () => {
  loading.value = true
  try {
    let sortByParam: string | undefined
    let ascendingParam: boolean | undefined

    if (sortBy.value) {
      const lastIndex = sortBy.value.lastIndexOf('_')
      if (lastIndex !== -1) {
        sortByParam = sortBy.value.substring(0, lastIndex)
        ascendingParam = sortBy.value.substring(lastIndex + 1) === 'asc'
      }
    }

    const params: Record<string, any> = {
      keyword: keyword.value,
      minPrice: minPrice.value,
      maxPrice: maxPrice.value,
      sortBy: sortByParam,
      ascending: ascendingParam,
      page: page.value,
      size: size.value,
      status: 'approved'
    }

    // 分类筛选逻辑
    if (selectedChildId.value) {
      // 选中了具体二级分类：精确匹配
      params.categoryIds = String(selectedChildId.value)
    } else if (selectedParentId.value) {
      if (selectedParentHasChildren.value) {
        // 只选了一级分类（有子分类）：筛选该一级分类下所有商品
        const childIds = childCategories.value.map(c => c.categoryId)
        params.categoryIds = childIds.join(',')
      } else {
        // 选了一级分类（无子分类）：直接使用该分类ID
        params.categoryIds = String(selectedParentId.value)
      }
    }
    // 都没选则不传 category 参数，显示全部

    const res = await getGoodsList(params)
    goodsList.value = res.data.list
    total.value = res.data.total
  } catch {
    // 错误已由全局拦截器处理
  } finally {
    loading.value = false
  }
}

const handleSearch = () => {
  page.value = 1
  fetchData()
}

const handlePageChange = (pageNum: number) => {
  page.value = pageNum
  fetchData()
}

const handlePageSizeChange = (pageSize: number) => {
  size.value = pageSize
  page.value = 1
  fetchData()
}

const goToDetail = (id: number) => {
  router.push(`/goods/${id}`)
}

// 一级分类变更：清空二级分类，重新筛选
const handleParentChange = (value: any) => {
  selectedParentId.value = typeof value === 'number' ? value : undefined
  selectedChildId.value = undefined  // 清空二级分类
  page.value = 1
  fetchData()
}

// 二级分类变更：重新筛选
const handleChildChange = (value: any) => {
  selectedChildId.value = typeof value === 'number' ? value : undefined
  page.value = 1
  fetchData()
}

const handleSortChange = (value: any) => {
  sortBy.value = String(value || 'created_at_desc')
  page.value = 1
  fetchData()
}

let searchTimer: ReturnType<typeof setTimeout> | null = null
const handleKeywordInput = () => {
  if (searchTimer) clearTimeout(searchTimer)
  searchTimer = setTimeout(() => {
    page.value = 1
    fetchData()
  }, 500)
}

onMounted(() => {
  fetchCategories()
  fetchData()
})
</script>

<template>
  <div class="goods-list-page">
    <div class="page-header">
      <h2>商品广场</h2>
    </div>

    <div class="filter-section">
      <a-card>
        <div class="filter-row">
          <div class="filter-item search-item">
            <a-input-search
              v-model="keyword"
              placeholder="搜索商品关键词"
              @search="handleSearch"
              @input="handleKeywordInput"
              allow-clear
            />
          </div>
          <div class="filter-item category-cascade">
            <a-select
              v-model="selectedParentId"
              placeholder="全部分类"
              allow-clear
              style="width: 150px"
              @change="handleParentChange"
            >
              <a-option
                v-for="cat in parentCategories"
                :key="cat.categoryId"
                :value="cat.categoryId"
              >
                {{ cat.categoryName }}
              </a-option>
            </a-select>
            <a-select
              v-model="selectedChildId"
              :placeholder="selectedParentHasChildren ? '全部子分类' : '无子分类'"
              :disabled="!selectedParentId || !selectedParentHasChildren"
              allow-clear
              style="width: 150px"
              @change="handleChildChange"
            >
              <a-option
                v-for="cat in childCategories"
                :key="cat.categoryId"
                :value="cat.categoryId"
              >
                {{ cat.categoryName }}
              </a-option>
            </a-select>
          </div>
          <div class="filter-item price-range">
            <a-input-number
              v-model="minPrice"
              placeholder="最低价"
              :min="0"
              style="width: 120px"
              @change="handleSearch"
            />
            <span class="price-separator">-</span>
            <a-input-number
              v-model="maxPrice"
              placeholder="最高价"
              :min="0"
              style="width: 120px"
              @change="handleSearch"
            />
          </div>
          <div class="filter-item">
            <a-select
              v-model="sortBy"
              :options="sortOptions"
              style="width: 140px"
              @change="handleSortChange"
            />
          </div>
        </div>
      </a-card>
    </div>

    <div class="goods-section">
      <a-spin :loading="loading" dot>
        <div v-if="goodsList.length > 0" class="goods-grid">
          <a-card
            v-for="item in goodsList"
            :key="item.goodsId"
            class="goods-card"
            hoverable
            @click="goToDetail(item.goodsId)"
          >
            <template #cover>
              <div class="card-image">
                <img
                  :src="item.imageUrl || 'https://via.placeholder.com/300x300?text=No+Image'"
                  :alt="item.title"
                />
                <a-tag class="condition-tag" :color="item.condition === 'new' ? 'green' : 'blue'">
                  {{ conditionMap[item.condition] || item.condition }}
                </a-tag>
              </div>
            </template>
            <a-card-meta :title="item.title">
              <template #title>
                <div class="card-title">{{ item.title }}</div>
              </template>
              <div class="card-meta">
                <div class="price">¥{{ item.price }}</div>
                <div class="meta-info">
                  <span class="seller">{{ item.sellerNickname || '卖家' }}</span>
                  <span class="views">
                    <icon-eye />
                    {{ item.viewCount }}
                  </span>
                </div>
              </div>
            </a-card-meta>
          </a-card>
        </div>
        <a-empty v-else description="暂无商品" />
      </a-spin>
    </div>

    <div class="pagination-section" v-if="total > 0">
      <a-pagination
        :total="total"
        :current="page"
        :page-size="size"
        :page-size-options="[12, 24, 48]"
        show-total
        show-page-size
        show-jumper
        @change="handlePageChange"
        @page-size-change="handlePageSizeChange"
      />
    </div>
  </div>
</template>

<style scoped>
.goods-list-page {
  max-width: 1200px;
  margin: 0 auto;
  padding: 24px;
}

.page-header {
  margin-bottom: 20px;
}

.page-header h2 {
  margin: 0;
  font-size: 24px;
  font-weight: 600;
}

.filter-section {
  margin-bottom: 20px;
}

.filter-row {
  display: flex;
  flex-wrap: wrap;
  gap: 16px;
  align-items: center;
}

.filter-item {
  display: flex;
  align-items: center;
}

.search-item {
  flex: 1;
  min-width: 280px;
}

.category-cascade {
  display: flex;
  gap: 8px;
}

.price-range {
  display: flex;
  align-items: center;
  gap: 8px;
}

.price-separator {
  color: #999;
}

.goods-section {
  margin-bottom: 24px;
}

.goods-grid {
  display: grid;
  grid-template-columns: repeat(4, 1fr);
  gap: 20px;
}

.goods-card {
  cursor: pointer;
}

.card-image {
  position: relative;
  width: 100%;
  height: 200px;
  overflow: hidden;
}

.card-image img {
  width: 100%;
  height: 100%;
  object-fit: cover;
}

.condition-tag {
  position: absolute;
  top: 8px;
  left: 8px;
}

.card-title {
  font-size: 14px;
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
  margin-bottom: 8px;
}

.card-meta {
  width: 100%;
}

.price {
  font-size: 20px;
  font-weight: bold;
  color: #f53f3f;
  margin-bottom: 8px;
}

.meta-info {
  display: flex;
  justify-content: space-between;
  align-items: center;
  font-size: 12px;
  color: #86909c;
}

.seller {
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
  max-width: 100px;
}

.views {
  display: flex;
  align-items: center;
  gap: 4px;
}

.pagination-section {
  display: flex;
  justify-content: center;
  margin-top: 24px;
}

@media (max-width: 1024px) {
  .goods-grid {
    grid-template-columns: repeat(3, 1fr);
  }
}

@media (max-width: 768px) {
  .goods-grid {
    grid-template-columns: repeat(2, 1fr);
  }
}
</style>
