<script setup lang="ts">
import { ref, onMounted, watch } from 'vue'  
import { useRouter } from 'vue-router'
import { Message } from '@arco-design/web-vue'
import { getGoodsList, getCategoryList } from '@/api'
import type { Goods, Category } from '@/types'

const router = useRouter()

const goodsList = ref<Goods[]>([])
const loading = ref(false)
const total = ref(0)
const page = ref(1)
const size = ref(12)

const keyword = ref('')
const categoryId = ref<number | undefined>(undefined)
const minPrice = ref<number | undefined>(undefined)
const maxPrice = ref<number | undefined>(undefined)
const sortBy = ref('created_at_desc')

const categories = ref<Category[]>([])
const flatCategories = ref<{ label: string; value: number }[]>([])

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
    flatCategories.value = categories.value.map(c => ({
      label: c.categoryName,
      value: c.categoryId
    }))
  } catch {
    // 错误已由全局拦截器处理
  }
}

const fetchData = async () => {
  loading.value = true
  try {
    // 解析排序参数
    let sortByParam: string | undefined
    let ascendingParam: boolean = false
    if (sortBy.value) {
      const lastIndex = sortBy.value.lastIndexOf('_')
      if (lastIndex !== -1) {
        sortByParam = sortBy.value.substring(0, lastIndex)
        ascendingParam = sortBy.value.substring(lastIndex + 1) === 'asc'
      }
    }

    const res = await getGoodsList({
      keyword: keyword.value,
      categoryId: categoryId.value,
      minPrice: minPrice.value,
      maxPrice: maxPrice.value,
      page: page.value,
      size: size.value,
      status: 'approved',
      sortBy: sortByParam,
      ascending: ascendingParam
    })
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

const handleCategoryChange = (value: number | undefined) => {
  categoryId.value = value
  page.value = 1
  fetchData()
}

const handleSortChange = (value: string) => {
  sortBy.value = value
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
          <div class="filter-item">
            <a-select
              v-model="categoryId"
              placeholder="全部分类"
              :options="flatCategories"
              allow-clear
              style="width: 160px"
              @change="handleCategoryChange"
            />
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
