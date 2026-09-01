import type { Category } from '@/types'

export const flattenCategories = (categories: Category[]): Category[] => {
  return categories.flatMap(category => [
    category,
    ...flattenCategories(category.children || [])
  ])
}
